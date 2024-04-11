using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Move along surface helper
    /// </summary>
    class MoveAlongSurfaceHelper : IDisposable
    {
        /// <summary>
        /// Navmesh data.
        /// </summary>
        private readonly NavMesh m_nav = null;
        /// <summary>
        /// Small node pool.
        /// </summary>
        private readonly NodePool m_nodePool = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nav">Navigation mesh</param>
        public MoveAlongSurfaceHelper(NavMesh nav)
        {
            ArgumentNullException.ThrowIfNull(nav);

            m_nav = nav;
            m_nodePool = new(64, 32);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~MoveAlongSurfaceHelper()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_nodePool?.Dispose();
            }
        }

        /// <summary>
        /// Moves from the start to the end position constrained to the navigation mesh.
        /// </summary>
        /// <param name="startRef">The reference id of the start polygon.</param>
        /// <param name="startPos">A position of the mover within the start polygon.</param>
        /// <param name="endPos">The desired end position of the mover.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="resultPos">The result position of the mover.</param>
        /// <param name="visited">The reference ids of the polygons visited during the move.</param>
        /// <param name="visitedCount">The number of polygons visited during the move.</param>
        /// <param name="maxVisitedSize">The maximum number of polygons the visited array can hold.</param>
        /// <returns>The status flags for the query.</returns>
        public Status MoveAlongSurface(int startRef, Vector3 startPos, Vector3 endPos, QueryFilter filter, int maxVisitedSize, out Vector3 resultPos, out SimplePath visited)
        {
            resultPos = Vector3.Zero;
            visited = new(maxVisitedSize);

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) ||
                startPos.IsInfinity() ||
                endPos.IsInfinity() ||
                filter == null ||
                maxVisitedSize <= 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            Status status = Status.DT_SUCCESS;

            FixedStack<Node> stack = new(48);

            m_nodePool.Clear();

            var startNode = m_nodePool.AllocateNode(startRef, 0);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Ref = startRef;
            startNode.Flags = NodeFlagTypes.Closed;
            stack.Push(startNode);

            var bestPos = startPos;
            float bestDist = float.MaxValue;
            Node bestNode = null;

            // Search constraints
            var searchPos = Vector3.Lerp(startPos, endPos, 0.5f);
            float searchRadSqr = (float)Math.Pow(Vector3.Distance(startPos, endPos) / 2.0f + 0.001f, 2);

            while (stack.Count != 0)
            {
                // Pop front.
                var curNode = stack.Pop();

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var cur = m_nav.GetTileAndPolyByRefUnsafe(curNode.Ref);

                // Collect vertices.
                var verts = cur.Tile.GetPolyVerts(cur.Poly);

                // If target is inside the poly, stop search.
                if (Utils.PointInPolygon2D(endPos, verts))
                {
                    bestNode = curNode;
                    bestPos = endPos;

                    break;
                }

                // Find wall edges and find nearest point inside the walls.
                for (int i = 0, j = cur.Poly.VertCount - 1; i < cur.Poly.VertCount; j = i++)
                {
                    // Find links to neighbours.
                    var neis = FindTileNeigbours(cur, j, filter);
                    var vj = verts[j];
                    var vi = verts[i];

                    if (neis.Count != 0)
                    {
                        ProcessTileNeighbours(stack, neis, vi, vj, searchRadSqr, searchPos, curNode);
                        continue;
                    }

                    // Wall edge, calc distance.
                    float distSqr = Utils.DistancePtSegSqr2D(endPos, vj, vi, out float tseg);
                    if (distSqr < bestDist)
                    {
                        // Update nearest distance.
                        bestPos = Vector3.Lerp(vj, vi, tseg);
                        bestDist = distSqr;
                        bestNode = curNode;
                    }
                }
            }

            // Reverse the path.
            if (!ReversePath(bestNode, visited))
            {
                status |= Status.DT_BUFFER_TOO_SMALL;
            }

            resultPos = bestPos;

            return status;
        }
        /// <summary>
        /// Finds the tile neighbour references
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <param name="polyIndex">Polygon index</param>
        /// <param name="filter">Query filter</param>
        /// <returns>Return the neighbour references list</returns>
        private List<int> FindTileNeigbours(TileRef tile, int polyIndex, QueryFilter filter)
        {
            const int MAX_NEIS = 8;

            var neis = new List<int>(MAX_NEIS);

            if (tile.Poly.NeighbourIsExternalLink(polyIndex))
            {
                // Tile border.
                foreach (var link in tile.IteratePolygonLinks())
                {
                    if (link.Edge == polyIndex && link.NRef != 0)
                    {
                        var nei = m_nav.GetTileAndPolyByRefUnsafe(link.NRef);
                        if (filter.PassFilter(nei.Poly.Flags) && neis.Count < MAX_NEIS)
                        {
                            neis.Add(link.NRef);
                        }
                    }
                }
            }
            else if (tile.Poly.Neis[polyIndex] != 0)
            {
                int idx = tile.Poly.Neis[polyIndex] - 1;
                int r = m_nav.GetTileRef(tile.Tile) | idx;
                if (filter.PassFilter(tile.Tile.Polys[idx].Flags))
                {
                    // Internal edge, encode id.
                    neis.Add(r);
                }
            }

            return neis;
        }
        /// <summary>
        /// Process the node stack
        /// </summary>
        /// <param name="stack">Stack to process</param>
        /// <param name="neis">Neighbour reference list</param>
        /// <param name="vi">Segment point i</param>
        /// <param name="vj">Segment point j</param>
        /// <param name="searchRadSqr">Squared search radius</param>
        /// <param name="searchPos">Search position</param>
        /// <param name="curNode">Current node</param>
        private void ProcessTileNeighbours(FixedStack<Node> stack, List<int> neis, Vector3 vi, Vector3 vj, float searchRadSqr, Vector3 searchPos, Node curNode)
        {
            foreach (var nei in neis)
            {
                // Skip if no node can be allocated.
                var neighbourNode = m_nodePool.AllocateNode(nei, 0);
                if (neighbourNode == null)
                {
                    continue;
                }
                // Skip if already visited.
                if ((neighbourNode.Flags & NodeFlagTypes.Closed) != 0)
                {
                    continue;
                }

                // Skip the link if it is too far from search constraint.
                float distSqr = Utils.DistancePtSegSqr2D(searchPos, vj, vi, out _);
                if (distSqr > searchRadSqr)
                {
                    continue;
                }

                // Mark as the node as visited and push to queue.
                neighbourNode.PIdx = m_nodePool.GetNodeIdx(curNode);
                neighbourNode.Flags |= NodeFlagTypes.Closed;

                stack.Push(neighbourNode);
            }
        }
        /// <summary>
        /// Reverses the path updating node PIdx references, and updates the visited path
        /// </summary>
        /// <param name="bestNode">First node</param>
        /// <param name="visited">Visited nodes</param>
        /// <returns>Returns true if the visited path was correctly updated or not updated at all</returns>
        private bool ReversePath(Node bestNode, SimplePath visited)
        {
            if (bestNode == null)
            {
                return true;
            }

            Node prev = null;
            var node = bestNode;
            do
            {
                var next = m_nodePool.GetNodeAtIdx(node.PIdx);
                node.PIdx = m_nodePool.GetNodeIdx(prev);
                prev = node;
                node = next;
            }
            while (node != null);

            // Store result
            node = prev;
            do
            {
                if (!visited.Add(node.Ref))
                {
                    return false;
                }
                node = m_nodePool.GetNodeAtIdx(node.PIdx);
            }
            while (node != null);

            return true;
        }
    }
}
