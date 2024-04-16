using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Find local neighbourhood helper
    /// </summary>
    class FindLocalNeighbourhoodHelper : IDisposable
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
        public FindLocalNeighbourhoodHelper(NavMesh nav)
        {
            ArgumentNullException.ThrowIfNull(nav);

            m_nav = nav;
            m_nodePool = new(64, 32);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FindLocalNeighbourhoodHelper()
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
        /// Finds the non-overlapping navigation polygons in the local neighbourhood around the center position.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="centerPos">The center of the query circle.</param>
        /// <param name="radius">The radius of the query circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxResult">The maximum number of polygons the result arrays can hold.</param>
        /// <param name="result">The polygons in the local neighbourhood.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindLocalNeighbourhood(int startRef, Vector3 centerPos, float radius, IGraphQueryFilter filter, int maxResult, out PolyRefs result)
        {
            result = new(maxResult);

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) ||
                centerPos.IsInfinity() ||
                radius < 0 || float.IsInfinity(radius) ||
                filter == null ||
                maxResult < 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            FixedStack<Node> stack = new(48);

            m_nodePool.Clear();

            var startNode = m_nodePool.AllocateNode(startRef, 0);
            startNode.PIdx = 0;
            startNode.Ref = startRef;
            startNode.Flags = NodeFlagTypes.Closed;
            stack.Push(startNode);

            float radiusSqr = radius * radius;

            Status status = Status.DT_SUCCESS;

            result.Append(startNode.Ref, 0, 0f);

            while (stack.Count > 0)
            {
                if (!ProcessFindLocalNeighbourhoodStack(stack, result, centerPos, radiusSqr, filter))
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }
            }

            return status;
        }
        /// <summary>
        /// Process the stack
        /// </summary>
        /// <param name="stack">Stack</param>
        /// <param name="maxStackCount">Maximum stack count</param>
        /// <param name="result">Polygon reference result</param>
        /// <param name="centerPos">Center position</param>
        /// <param name="radiusSqr">Squared radius</param>
        /// <param name="filter">Query filter</param>
        private bool ProcessFindLocalNeighbourhoodStack(FixedStack<Node> stack, PolyRefs result, Vector3 centerPos, float radiusSqr, IGraphQueryFilter filter)
        {
            bool gResult = true;

            // Pop front.
            var curNode = stack.Pop();

            // Get poly and tile.
            // The API input has been cheked already, skip checking internal data.
            var cur = m_nav.GetTileAndPolyByRefUnsafe(curNode.Ref);

            foreach (var link in cur.IteratePolygonLinks())
            {
                if (!GetNeighbour(link.NRef, filter, out var neighbour, out var neighbourNode))
                {
                    continue;
                }

                // Find edge and calc distance to the edge.
                if (TileRef.GetPortalPoints(cur, neighbour, out var va, out var vb).HasFlag(Status.DT_FAILURE))
                {
                    continue;
                }

                float distSqr = Utils.DistancePtSegSqr2D(centerPos, va, vb, out _);
                if (distSqr > radiusSqr)
                {
                    // If the circle is not touching the next polygon, skip it.
                    continue;
                }

                // Mark node visited, this is done before the overlap test so that
                // we will not visit the poly again if the test fails.
                neighbourNode.Flags |= NodeFlagTypes.Closed;
                neighbourNode.PIdx = m_nodePool.GetNodeIdx(curNode);

                // Check that the polygon does not collide with existing polygons.

                // Collect vertices of the neighbour poly.
                var pa = neighbour.GetPolyVertices();

                if (TestOverlap(cur, pa, result))
                {
                    continue;
                }

                // This poly is fine, store and advance to the poly.
                if (!result.Append(neighbour.Ref, cur.Ref, 0f))
                {
                    gResult = false;
                }

                stack.Push(neighbourNode);
            }

            return gResult;
        }
        /// <summary>
        /// Gets the neighbour data
        /// </summary>
        /// <param name="nref">Neighbour reference</param>
        /// <param name="filter">Query filter</param>
        /// <param name="neighbour">Neighbour tile</param>
        /// <param name="neighbourNode">Neighbour node</param>
        private bool GetNeighbour(int nref, IGraphQueryFilter filter, out TileRef neighbour, out Node neighbourNode)
        {
            neighbour = TileRef.Null;
            neighbourNode = null;

            int neighbourRef = nref;

            if (neighbourRef == 0)
            {
                // Skip invalid neighbours.
                return false;
            }

            neighbourNode = m_nodePool.AllocateNode(neighbourRef, 0);
            if (neighbourNode == null)
            {
                // Skip if cannot alloca more nodes.
                return false;
            }

            if (neighbourNode.IsClosed)
            {
                // Skip visited.
                return false;
            }

            // Expand to neighbour
            neighbour = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);

            if (neighbour.Poly.Type == PolyTypes.OffmeshConnection)
            {
                // Skip off-mesh connections.
                return false;
            }

            if (!filter.PassFilter(neighbour.Poly.Flags))
            {
                // Do not advance if the polygon is excluded by the filter.
                return false;
            }

            return true;
        }
        /// <summary>
        /// Tests overlaping
        /// </summary>
        /// <param name="cur">Tile</param>
        /// <param name="pa">Vertices</param>
        /// <param name="result">Result polygon references</param>
        private bool TestOverlap(TileRef cur, Vector3[] pa, PolyRefs result)
        {
            bool overlap = false;
            for (int j = 0; j < result.Count; ++j)
            {
                int pastRef = result.GetReference(j);

                // Connected polys do not overlap.
                bool connected = false;
                foreach (var link in cur.IteratePolygonLinks())
                {
                    if (link.NRef == pastRef)
                    {
                        connected = true;
                        break;
                    }
                }
                if (connected)
                {
                    continue;
                }

                // Potentially overlapping.
                var past = m_nav.GetTileAndPolyByRefUnsafe(pastRef);

                // Get vertices and test overlap
                var pb = past.GetPolyVertices();

                if (Utils.OverlapPolyPoly2D(pa, pa.Length, pb, pb.Length))
                {
                    overlap = true;
                    break;
                }
            }

            return overlap;
        }
    }
}
