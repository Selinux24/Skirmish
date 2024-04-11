using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Find polygons helper
    /// </summary>
    class FindPolysHelper : IDisposable
    {
        /// <summary>
        /// Navmesh data.
        /// </summary>
        private readonly NavMesh m_nav = null;
        /// <summary>
        /// Node pool.
        /// </summary>
        private readonly NodePool m_nodePool = null;
        /// <summary>
        /// Open list queue.
        /// </summary>
        private readonly NodeQueue m_openList = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nav">Navigation mesh</param>
        /// <param name="navQuery">Navigation mesh query</param>
        /// <param name="maxNodes">Maximum number of nodes in the search list</param>
        public FindPolysHelper(NavMesh nav, int maxNodes)
        {
            ArgumentNullException.ThrowIfNull(nav);

            m_nav = nav;
            m_nodePool = new(maxNodes, Helper.NextPowerOfTwo(maxNodes / 4));
            m_openList = new(maxNodes);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FindPolysHelper()
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
                m_openList?.Dispose();
            }
        }

        /// <summary>
        /// Finds the polygons along the navigation graph that touch the specified circle.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="centerPos">The center of the search circle.</param>
        /// <param name="radius">The radius of the search circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxResult">The maximum number of polygons the result arrays can hold.</param>
        /// <param name="result">The polygons touched by the circle.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindPolysAroundCircle(int startRef, Vector3 centerPos, float radius, QueryFilter filter, int maxResult, out PolyRefs result)
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

            m_nodePool.Clear();
            m_openList.Clear();

            var startNode = m_nodePool.AllocateNode(startRef, 0);
            startNode.Pos = centerPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Ref = startRef;
            startNode.Flags = NodeFlagTypes.Open;
            m_openList.Push(startNode);

            Status status = Status.DT_SUCCESS;

            while (!m_openList.Empty())
            {
                var bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var best = m_nav.GetTileAndPolyByRefUnsafe(bestNode.Ref);

                // Get parent poly and tile.
                int parentRef = 0;
                var parent = TileRef.Null;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Ref;
                }
                if (parentRef != 0)
                {
                    parent = m_nav.GetTileAndPolyByRefUnsafe(parentRef);
                }

                if (!result.Append(best.Ref, parentRef, bestNode.Total))
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }

                var query = new FindPolysAroundCircleQuery(centerPos, radius);
                ProcessTileLinksQuery(best, parent, query, filter, out bool outOfNodes);
                if (outOfNodes)
                {
                    status |= Status.DT_OUT_OF_NODES;
                }
            }

            return status;
        }
        /// <summary>
        /// Finds the polygons along the naviation graph that touch the specified convex polygon.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="verts">The vertices describing the convex polygon.</param>
        /// <param name="nverts">The number of vertices in the polygon.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxResult">The maximum number of polygons the result arrays can hold.</param>
        /// <param name="result">The polygons touched by the circle.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindPolysAroundShape(int startRef, Vector3[] verts, QueryFilter filter, int maxResult, out PolyRefs result)
        {
            result = new(maxResult);

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) ||
                verts == null || verts.Length < 3 ||
                filter == null ||
                maxResult < 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            var centerPos = Vector3.Zero;
            foreach (var v in verts)
            {
                centerPos += v;
            }
            centerPos *= 1.0f / verts.Length;

            var startNode = m_nodePool.AllocateNode(startRef, 0);
            startNode.Pos = centerPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Ref = startRef;
            startNode.Flags = NodeFlagTypes.Open;
            m_openList.Push(startNode);

            Status status = Status.DT_SUCCESS;

            while (!m_openList.Empty())
            {
                var bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var best = m_nav.GetTileAndPolyByRefUnsafe(bestNode.Ref);

                // Get parent poly and tile.
                int parentRef = 0;
                var parent = TileRef.Null;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Ref;
                }
                if (parentRef != 0)
                {
                    parent = m_nav.GetTileAndPolyByRefUnsafe(parentRef);
                }

                if (!result.Append(best.Ref, parentRef, bestNode.Total))
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }

                var query = new FindPolysAroundShapeQuery(verts);
                ProcessTileLinksQuery(best, parent, query, filter, out bool outOfNodes);
                if (outOfNodes)
                {
                    status |= Status.DT_OUT_OF_NODES;
                }
            }

            return status;
        }
        /// <summary>
        /// Process tile links
        /// </summary>
        /// <param name="best">Tile to process</param>
        /// <param name="parent">Parent tile</param>
        /// <param name="query">Query</param>
        /// <param name="filter">Query filter</param>
        /// <param name="outOfNodes">Return true if the query is out of available nodes</param>
        private void ProcessTileLinksQuery(TileRef best, TileRef parent, IFindPolysQuery query, QueryFilter filter, out bool outOfNodes)
        {
            outOfNodes = false;

            for (int i = best.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = best.Tile.Links[i].Next)
            {
                var link = best.Tile.Links[i];

                // Skip invalid neighbours and do not follow back to parent.
                if (link.NRef == 0 || link.NRef == parent.Ref)
                {
                    continue;
                }

                ProcessTileLinkNeighbourQuery(link, best, parent, query, filter, out bool neiOutOfNodes);
                if (neiOutOfNodes)
                {
                    outOfNodes = true;
                }
            }
        }
        /// <summary>
        /// Process tile link neighbour
        /// </summary>
        /// <param name="link">Link to process</param>
        /// <param name="best">Link's tile</param>
        /// <param name="parent">Parent</param>
        /// <param name="query">Query</param>
        /// <param name="filter">Query filter</param>
        /// <param name="outOfNodes">Return true if the query is out of available nodes</param>
        private void ProcessTileLinkNeighbourQuery(Link link, TileRef best, TileRef parent, IFindPolysQuery query, QueryFilter filter, out bool outOfNodes)
        {
            outOfNodes = false;

            // Expand to neighbour
            var neighbour = m_nav.GetTileAndPolyByRefUnsafe(link.NRef);

            // Do not advance if the polygon is excluded by the filter.
            if (!filter.PassFilter(neighbour.Poly.Flags))
            {
                return;
            }

            // Find edge and calc distance to the edge.
            var portalStatus = TileRef.GetPortalPoints(best, neighbour, out Vector3 va, out Vector3 vb);
            if (portalStatus.HasFlag(Status.DT_FAILURE))
            {
                return;
            }

            // Process query
            if (!query.Contains(va, vb))
            {
                return;
            }

            // Get the node
            neighbour.Node = m_nodePool.AllocateNode(neighbour.Ref, 0);
            if (neighbour.Node == null)
            {
                outOfNodes = true;
                return;
            }

            if (neighbour.Node.IsClosed)
            {
                return;
            }

            // Cost
            if (neighbour.Node.Flags == 0)
            {
                neighbour.Node.Pos = Vector3.Lerp(va, vb, 0.5f);
            }

            float cost = filter.GetCost(best.Node.Pos, neighbour.Node.Pos, parent, best, neighbour);

            float total = best.Node.Total + cost;

            // The node is already in open list and the new result is worse, skip.
            if (neighbour.Node.IsOpen && total >= neighbour.Node.Total)
            {
                return;
            }

            neighbour.Node.Ref = neighbour.Ref;
            neighbour.Node.PIdx = m_nodePool.GetNodeIdx(best.Node);
            neighbour.Node.Total = total;

            if (neighbour.Node.IsOpen)
            {
                m_openList.Modify(neighbour.Node);
            }
            else
            {
                neighbour.Node.Flags = NodeFlagTypes.Open;
                m_openList.Push(neighbour.Node);
            }
        }
        /// <summary>
        /// Finds the polygon nearest to the specified center point.
        /// </summary>
        /// <param name="center">The center of the search box.</param>
        /// <param name="halfExtents">The search distance along each axis.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="nearestRef">The reference id of the nearest polygon.</param>
        /// <param name="nearestPt">The nearest point on the polygon.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindNearestPoly(Vector3 center, Vector3 halfExtents, QueryFilter filter, out int nearestRef, out Vector3 nearestPt)
        {
            nearestRef = 0;
            nearestPt = Vector3.Zero;

            // queryPolygons below will check rest of params

            var query = new FindNearestPolyQuery(m_nav, center);

            Status status = QueryPolygons(center, halfExtents, filter, query);
            if (status.HasFlag(Status.DT_FAILURE))
            {
                return status;
            }

            nearestRef = query.NearestRef();
            // Only override nearestPt if we actually found a poly so the nearest point is valid.
            if (nearestRef != 0)
            {
                nearestPt = query.NearestPoint();

                return Status.DT_SUCCESS;
            }

            return Status.DT_FAILURE;
        }
        /// <summary>
        /// Finds polygons that overlap the search box.
        /// </summary>
        /// <param name="center">The center of the search box.</param>
        /// <param name="halfExtents">The search distance along each axis.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="query">The query. Polygons found will be batched together and passed to this query.</param>
        /// <returns>The status flags for the query.</returns>
        private Status QueryPolygons(Vector3 center, Vector3 halfExtents, QueryFilter filter, IPolyQuery query)
        {
            if (center.IsInfinity() ||
                halfExtents.IsInfinity() ||
                filter == null || query == null)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            var bmin = Vector3.Subtract(center, halfExtents);
            var bmax = Vector3.Add(center, halfExtents);
            var bounds = new BoundingBox(bmin, bmax);

            // Find tiles the query touches.
            m_nav.CalcTileLoc(bmin, out int minx, out int miny);
            m_nav.CalcTileLoc(bmax, out int maxx, out int maxy);

            const int MAX_NEIS = 32;

            for (int y = miny; y <= maxy; y++)
            {
                for (int x = minx; x <= maxx; x++)
                {
                    var neis = m_nav.GetTilesAt(x, y, MAX_NEIS);
                    foreach (var nei in neis)
                    {
                        QueryPolygonsInTile(nei, bounds, filter, query);
                    }
                }
            }

            return Status.DT_SUCCESS;
        }

        /// <summary>
        ///  Queries polygons within a tile.
        /// </summary>
        private void QueryPolygonsInTile(MeshTile tile, BoundingBox bounds, QueryFilter filter, IPolyQuery query)
        {
            if (tile.BvTree?.Length > 0)
            {
                QueryPolygonsInTileBVTree(tile, bounds, filter, query);
            }
            else
            {
                QueryPolygonsInTileByRef(tile, bounds, filter, query);
            }
        }
        /// <summary>
        ///  Queries polygons within a tile using a BVtree.
        /// </summary>
        private void QueryPolygonsInTileBVTree(MeshTile tile, BoundingBox bounds, QueryFilter filter, IPolyQuery query, int batchSize = 32)
        {
            List<int> polyRefs = new(batchSize);

            // Calculate quantized box
            tile.CalculateQuantizedBox(bounds, out var bmin, out var bmax);

            // Traverse tree
            int bse = m_nav.GetTileRef(tile);
            int nodeIndex = 0;
            int endIndex = tile.Header.BvNodeCount;

            while (nodeIndex < endIndex)
            {
                var node = nodeIndex < tile.BvTree.Length ? tile.BvTree[nodeIndex] : new();

                bool overlap = Utils.OverlapBounds(bmin, bmax, node.BMin, node.BMax);
                bool isLeafNode = node.I >= 0;

                if (isLeafNode && overlap && filter.PassFilter(tile.Polys[node.I].Flags))
                {
                    int r = bse | node.I;

                    polyRefs.Add(r);

                    if (polyRefs.Count == batchSize)
                    {
                        query.Process(tile, polyRefs);
                        polyRefs.Clear();
                    }
                }

                if (overlap || isLeafNode)
                {
                    nodeIndex++;

                    continue;
                }

                int escapeIndex = -node.I;
                nodeIndex += escapeIndex;
            }

            // Process the last polygons that didn't make a full batch.
            query.Process(tile, polyRefs);
        }
        /// <summary>
        ///  Queries polygons within a tile reference by reference.
        /// </summary>
        private void QueryPolygonsInTileByRef(MeshTile tile, BoundingBox bounds, QueryFilter filter, IPolyQuery query)
        {
            int batchSize = 32;
            List<int> polyRefs = new(batchSize);

            int bse = m_nav.GetTileRef(tile);

            for (int i = 0; i < tile.Header.PolyCount; ++i)
            {
                var p = tile.Polys[i];

                // Do not return off-mesh connection polygons.
                if (p.Type == PolyTypes.OffmeshConnection)
                {
                    continue;
                }

                // Must pass filter
                if (!filter.PassFilter(p.Flags))
                {
                    continue;
                }

                // Calc polygon bounds.
                var tileBounds = tile.GetPolyBounds(p);

                if (bounds.Contains(tileBounds) != ContainmentType.Disjoint)
                {
                    int r = bse | i;

                    polyRefs.Add(r);

                    if (polyRefs.Count == batchSize)
                    {
                        query.Process(tile, polyRefs);
                        polyRefs.Clear();
                    }
                }
            }

            // Process the last polygons that didn't make a full batch.
            query.Process(tile, polyRefs);
        }
    }
}
