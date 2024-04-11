using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Random point around circle helper
    /// </summary>
    class RandomPointAroundCircleHelper : IDisposable
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
        public RandomPointAroundCircleHelper(NavMesh nav, int maxNodes)
        {
            ArgumentNullException.ThrowIfNull(nav);

            m_nav = nav;
            m_nodePool = new(maxNodes, Helper.NextPowerOfTwo(maxNodes / 4));
            m_openList = new(maxNodes);
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
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_nodePool?.Dispose();
                m_openList?.Dispose();
            }
        }

        /// <summary>
        /// Returns random location on navmesh within the reach of specified location.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="centerPos">The center of the search circle.</param>
        /// <param name="maxRadius">The radius of the search circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="randomRef">The reference id of the random location.</param>
        /// <param name="randomPt">The random location.</param>
        /// <returns>The status flags for the query.</returns>
        /// <remarks>
        /// Polygons are chosen weighted by area. The search runs in linear related to number of polygon.
        /// The location is not exactly constrained by the circle, but it limits the visited polygons.
        /// </remarks>
        public Status FindRandomPointAroundCircle(int startRef, Vector3 centerPos, float maxRadius, QueryFilter filter, out int randomRef, out Vector3 randomPt)
        {
            randomRef = 0;
            randomPt = new Vector3();

            // Validate input
            if (startRef == 0 || !m_nav.IsValidPolyRef(startRef) ||
                centerPos.IsInfinity() ||
                maxRadius < 0 || float.IsNaN(maxRadius) ||
                filter == null)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            var start = m_nav.GetTileAndPolyByRefUnsafe(startRef);
            if (!filter.PassFilter(start.Poly.Flags))
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

            float radiusSqr = maxRadius * maxRadius;
            var random = TileRef.Null;
            float areaSum = 0.0f;

            while (!m_openList.Empty())
            {
                var bestNode = m_openList.Pop();
                bestNode.SetClosed();

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var best = m_nav.GetTileAndPolyByNodeUnsafe(bestNode);

                random = SelectBestTile(best, random, ref areaSum);

                EvaluateTileLinks(best, centerPos, radiusSqr, filter);
            }

            if (random.Ref == TileRef.Null.Ref)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick point on polygon.
            var verts = random.Tile.GetPolyVerts(random.Poly);

            var pt = Utils.RandomPointInConvexPoly(verts);

            var stat = m_nav.GetPolyHeight(random.Ref, pt, out float h);
            if (stat.HasFlag(Status.DT_FAILURE))
            {
                return stat;
            }
            pt.Y = h;

            randomPt = pt;
            randomRef = random.Ref;

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Selects a tile reference
        /// </summary>
        /// <param name="tileFrom">Tile from</param>
        /// <param name="tileTo">Tile to</param>
        /// <param name="areaSum">Area sumatory</param>
        /// <returns>Returns the selected tile</returns>
        private static TileRef SelectBestTile(TileRef tileFrom, TileRef tileTo, ref float areaSum)
        {
            // Place random locations on on ground.
            if (tileFrom.Poly.Type != PolyTypes.Ground)
            {
                return tileTo;
            }

            // Calc area of the polygon.
            float polyArea = tileFrom.Tile.GetPolyArea(tileFrom.Poly);

            // Choose random polygon weighted by area, using reservoi sampling.
            areaSum += polyArea;
            float u = Helper.RandomGenerator.NextFloat(0, 1);
            if (u * areaSum <= polyArea)
            {
                return tileFrom;
            }

            return tileTo;
        }
        /// <summary>
        /// Evaluates the tile link list
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <param name="centerPos">Center position</param>
        /// <param name="radiusSqr">Squared radius</param>
        /// <param name="filter">Query filter</param>
        private void EvaluateTileLinks(TileRef tile, Vector3 centerPos, float radiusSqr, QueryFilter filter)
        {
            // Get parent poly and tile.
            int parentRef = m_nodePool.GetNodeAtIdx(tile.Node.PIdx)?.Ref ?? 0;

            foreach (var neighbourRef in tile.IteratePolygonLinks().Select(link => link.NRef))
            {
                // Skip invalid neighbours and do not follow back to parent.
                if (neighbourRef == 0 || neighbourRef == parentRef)
                {
                    continue;
                }

                // Expand to neighbour
                var neighbour = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);

                // Do not advance if the polygon is excluded by the filter.
                if (!filter.PassFilter(neighbour.Poly.Flags))
                {
                    continue;
                }

                // Find edge and calc distance to the edge.
                if (TileRef.GetPortalPoints(tile, neighbour, out Vector3 va, out Vector3 vb).HasFlag(Status.DT_FAILURE))
                {
                    continue;
                }

                // If the circle is not touching the next polygon, skip it.
                float distSqr = Utils.DistancePtSegSqr2D(centerPos, va, vb, out _);
                if (distSqr > radiusSqr)
                {
                    continue;
                }

                var neighbourNode = m_nodePool.AllocateNode(neighbour.Ref, 0);
                if (neighbourNode == null)
                {
                    continue;
                }

                if (neighbourNode.IsClosed)
                {
                    continue;
                }

                neighbour.Node = neighbourNode;

                EvaluateCost(neighbour, tile, va, vb);
            }
        }
        /// <summary>
        /// Evaluates the tile
        /// </summary>
        /// <param name="tileFrom">Tile from</param>
        /// <param name="tileTo">Tile to</param>
        /// <param name="va">Segment A position</param>
        /// <param name="vb">Segment B position</param>
        private void EvaluateCost(TileRef tileFrom, TileRef tileTo, Vector3 va, Vector3 vb)
        {
            // Cost
            if (tileFrom.Node.Flags == NodeFlagTypes.None)
            {
                tileFrom.Node.Pos = Vector3.Lerp(va, vb, 0.5f);
            }

            float total = tileTo.Node.Total + Vector3.Distance(tileTo.Node.Pos, tileFrom.Node.Pos);

            // The node is already in open list and the new result is worse, skip.
            if (tileFrom.Node.IsOpen && total >= tileFrom.Node.Total)
            {
                return;
            }

            tileFrom.Node.PIdx = m_nodePool.GetNodeIdx(tileTo.Node);
            tileFrom.Node.Ref = tileFrom.Ref;
            tileFrom.Node.Flags &= ~NodeFlagTypes.Closed;
            tileFrom.Node.Total = total;

            if (tileFrom.Node.IsOpen)
            {
                m_openList.Modify(tileFrom.Node);
            }
            else
            {
                tileFrom.Node.Flags = NodeFlagTypes.Open;
                m_openList.Push(tileFrom.Node);
            }
        }
    }
}
