using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Distance to wall helper
    /// </summary>
    class WallHelper : IDisposable
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
        /// <param name="maxNodes">Maximum number of nodes in the search list</param>
        public WallHelper(NavMesh nav, int maxNodes)
        {
            ArgumentNullException.ThrowIfNull(nav);

            m_nav = nav;
            m_nodePool = new(maxNodes, Helper.NextPowerOfTwo(maxNodes / 4));
            m_openList = new(maxNodes);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~WallHelper()
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
        /// Finds the distance from the specified position to the nearest polygon wall.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon containing centerPos.</param>
        /// <param name="centerPos">The center of the search circle.</param>
        /// <param name="maxRadius">The radius of the search circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="hitDist">The distance to the nearest wall from centerPos.</param>
        /// <param name="hitPos">The nearest position on the wall that was hit.</param>
        /// <param name="hitNormal">The normalized ray formed from the wall point to the source point.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindDistanceToWall(int startRef, Vector3 centerPos, float maxRadius, IGraphQueryFilter filter, out float hitDist, out Vector3 hitPos, out Vector3 hitNormal)
        {
            hitDist = 0;
            hitPos = Vector3.Zero;
            hitNormal = Vector3.Zero;

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) ||
                centerPos.IsInfinity() ||
                maxRadius < 0 || float.IsInfinity(maxRadius) ||
                filter == null)
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

            float radiusSqr = (maxRadius * maxRadius);

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
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Ref;
                }

                // Hit test walls.
                hitPos = HitTestWalls(best, filter, centerPos, radiusSqr);

                // Process the links
                status = ProcessLinksFindDistance(best, parentRef, centerPos, radiusSqr, filter);
            }

            // Calc hit normal.
            hitNormal = Vector3.Subtract(centerPos, hitPos);
            hitNormal.Normalize();

            hitDist = MathF.Sqrt(radiusSqr);

            return status;
        }
        /// <summary>
        /// Gets the closest hit position against a wall
        /// </summary>
        /// <param name="best">Tile</param>
        /// <param name="filter">Query filter</param>
        /// <param name="centerPos">Circle center position</param>
        /// <param name="radiusSqr">Circle squared radius</param>
        /// <returns>Returns the closest hit position</returns>
        private Vector3 HitTestWalls(TileRef best, IGraphQueryFilter filter, Vector3 centerPos, float radiusSqr)
        {
            var hitPos = Vector3.Zero;

            for (int i = 0, j = best.Poly.VertCount - 1; i < best.Poly.VertCount; j = i++)
            {
                // Skip non-solid edges.
                if (best.Poly.NeighbourIsExternalLink(j))
                {
                    // Tile border.
                    bool solid = BorderIsSolid(best, j, filter);
                    if (!solid)
                    {
                        continue;
                    }
                }
                else if (best.Poly.Neis[j] != 0)
                {
                    // Internal edge
                    int idx = best.Poly.Neis[j] - 1;
                    if (filter.PassFilter(best.Tile.Polys[idx].Flags))
                    {
                        continue;
                    }
                }

                // Calc distance to the edge.
                var vj = best.Tile.Verts[best.Poly.Verts[j]];
                var vi = best.Tile.Verts[best.Poly.Verts[i]];
                float distSqr = Utils.DistancePtSegSqr2D(centerPos, vj, vi, out float tseg);

                // Edge is too far, skip.
                if (distSqr > radiusSqr)
                {
                    continue;
                }

                // Hit wall, update radius.
                radiusSqr = distSqr;
                // Calculate hit pos.
                hitPos = vj + (vi - vj) * tseg;
            }

            return hitPos;
        }
        /// <summary>
        /// Gets whether a border is solid or not
        /// </summary>
        /// <param name="best">Tile</param>
        /// <param name="j">Border neighbour</param>
        /// <param name="filter">Query filter</param>
        /// <returns>Returns true if the border is solid</returns>
        private bool BorderIsSolid(TileRef best, int j, IGraphQueryFilter filter)
        {
            bool solid = true;

            for (int k = best.Poly.FirstLink; k != MeshTile.DT_NULL_LINK; k = best.Tile.Links[k].Next)
            {
                var link = best.Tile.Links[k];
                if (link.Edge == j)
                {
                    if (link.NRef != 0)
                    {
                        var nei = m_nav.GetTileAndPolyByRefUnsafe(link.NRef);
                        if (filter.PassFilter(nei.Poly.Flags))
                        {
                            solid = false;
                        }
                    }
                    break;
                }
            }

            return solid;
        }
        /// <summary>
        /// Process links
        /// </summary>
        /// <param name="best">Tile</param>
        /// <param name="parentRef">Parent reference</param>
        /// <param name="centerPos">Center position</param>
        /// <param name="radiusSqr">Squared radius</param>
        /// <param name="filter">Query filter</param>
        /// <returns>Returns the partial status</returns>
        private Status ProcessLinksFindDistance(TileRef best, int parentRef, Vector3 centerPos, float radiusSqr, IGraphQueryFilter filter)
        {
            Status status = Status.DT_SUCCESS;

            for (int i = best.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = best.Tile.Links[i].Next)
            {
                var link = best.Tile.Links[i];

                // Skip invalid neighbours and do not follow back to parent.
                if (link.NRef == 0 || link.NRef == parentRef)
                {
                    return status;
                }

                Status neiStatus = ProcessLinksNeighbourFindDistance(link, best, centerPos, radiusSqr, filter);
                if (neiStatus != Status.DT_SUCCESS)
                {
                    status = neiStatus;
                }
            }

            return status;
        }
        /// <summary>
        /// Process neighbour link
        /// </summary>
        /// <param name="link">Link</param>
        /// <param name="best">Tile</param>
        /// <param name="centerPos">Center position</param>
        /// <param name="radiusSqr">Squared radius</param>
        /// <param name="filter">Query filter</param>
        /// <returns>Returns the partial result</returns>
        private Status ProcessLinksNeighbourFindDistance(Link link, TileRef best, Vector3 centerPos, float radiusSqr, IGraphQueryFilter filter)
        {
            Status status = Status.DT_SUCCESS;

            // Expand to neighbour.
            var neighbour = m_nav.GetTileAndPolyByRefUnsafe(link.NRef);

            // Do not advance if the polygon is excluded by the filter.
            if (!filter.PassFilter(neighbour.Poly.Flags))
            {
                return status;
            }

            // Skip off-mesh connections.
            if (neighbour.Poly.Type == PolyTypes.OffmeshConnection)
            {
                return status;
            }

            // Calc distance to the edge.
            var va = best.Tile.Verts[best.Poly.Verts[link.Edge]];
            var vb = best.Tile.Verts[best.Poly.Verts[(link.Edge + 1) % best.Poly.VertCount]];
            float distSqr = Utils.DistancePtSegSqr2D(centerPos, va, vb, out _);
            if (distSqr > radiusSqr)
            {
                // If the circle is not touching the next polygon, skip it.
                return status;
            }

            neighbour.Node = m_nodePool.AllocateNode(neighbour.Ref, 0);
            if (neighbour.Node == null)
            {
                status |= Status.DT_OUT_OF_NODES;
                return status;
            }

            if (neighbour.Node.IsClosed)
            {
                return status;
            }

            // Cost
            if (neighbour.Node.Flags == 0)
            {
                var midPointRes = TileRef.GetEdgeMidPoint(best, neighbour, out var pos);
                if (midPointRes != Status.DT_SUCCESS)
                {
                    Logger.WriteWarning(this, $"FindPath GetEdgeMidPoint result: {midPointRes}");
                    status = midPointRes;
                    return status;
                }

                neighbour.Node.Pos = pos;
            }

            float total = best.Node.Total + Vector3.Distance(best.Node.Pos, neighbour.Node.Pos);

            // The node is already in open list and the new result is worse, skip.
            if (neighbour.Node.IsOpen && total >= neighbour.Node.Total)
            {
                return status;
            }

            neighbour.Node.Ref = neighbour.Ref;
            neighbour.Node.Flags = neighbour.Node.Flags & ~NodeFlagTypes.Closed;
            neighbour.Node.PIdx = m_nodePool.GetNodeIdx(best.Node);
            neighbour.Node.Total = total;

            if (neighbour.Node.IsOpen)
            {
                m_openList.Modify(neighbour.Node);
            }
            else
            {
                neighbour.Node.Flags |= NodeFlagTypes.Open;
                m_openList.Push(neighbour.Node);
            }

            return status;
        }

        /// <summary>
        /// Returns the segments for the specified polygon, optionally including portals.
        /// </summary>
        /// <param name="r">The reference id of the polygon.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="segmentVerts">The segments.</param>
        /// <param name="segmentRefs">The reference ids of each segment's neighbor polygon. Or zero if the segment is a wall.</param>
        /// <param name="segmentCount">The number of segments returned.</param>
        /// <param name="maxSegments">The maximum number of segments the result arrays can hold.</param>
        /// <returns>The status flags for the query.</returns>
        public Status GetPolyWallSegments(int r, IGraphQueryFilter filter, int maxSegments, out Segment[] segmentsRes)
        {
            segmentsRes = [];

            var cur = m_nav.GetTileAndPolyByRef(r);
            if (cur.Ref == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            if (filter == null || maxSegments < 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            int MAX_INTERVAL = 16;
            List<SegInterval> ints = [];
            List<Segment> segments = [];

            Status status = Status.DT_SUCCESS;

            for (int i = 0, j = cur.Poly.VertCount - 1; i < cur.Poly.VertCount; j = i++)
            {
                var vj = cur.Tile.Verts[cur.Poly.Verts[j]];
                var vi = cur.Tile.Verts[cur.Poly.Verts[i]];

                // Skip non-solid edges.
                if (cur.Poly.NeighbourIsExternalLink(j))
                {
                    SkipExternalLink(filter, j, cur, ints, MAX_INTERVAL);
                }
                else
                {
                    if (!SkipInternalEdge(filter, j, cur, vi, vj, segments, maxSegments))
                    {
                        status |= Status.DT_BUFFER_TOO_SMALL;
                    }

                    continue;
                }

                // Add sentinels
                SegInterval.InsertInterval(ints, MAX_INTERVAL, -1, 0, 0);
                SegInterval.InsertInterval(ints, MAX_INTERVAL, 255, 256, 0);

                // Store segments.
                if (!StoreSegments(vi, vj, ints, segments, maxSegments))
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }
            }

            segmentsRes = [.. segments];

            return status;
        }
        /// <summary>
        /// Skips external link
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="j">Neighbour index</param>
        /// <param name="cur">Current tile reference</param>
        /// <param name="ints">Interval list</param>
        /// <param name="maxInterval">Maximum number of intervals in the list</param>
        private void SkipExternalLink(IGraphQueryFilter filter, int j, TileRef cur, List<SegInterval> ints, int maxInterval)
        {
            // Tile border.
            for (int k = cur.Poly.FirstLink; k != MeshTile.DT_NULL_LINK; k = cur.Tile.Links[k].Next)
            {
                var link = cur.Tile.Links[k];
                if (link.Edge == j && link.NRef != 0)
                {
                    var nei = m_nav.GetTileAndPolyByRefUnsafe(link.NRef);
                    if (filter.PassFilter(nei.Poly.Flags))
                    {
                        SegInterval.InsertInterval(ints, maxInterval, link.BMin, link.BMax, link.NRef);
                    }
                }
            }
        }
        /// <summary>
        /// Skips internal edge
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="j">Neighbour index</param>
        /// <param name="cur">Current tile reference</param>
        /// <param name="vi">Segment point i</param>
        /// <param name="vj">Segment point j</param>
        /// <param name="segments">Segment list</param>
        /// <param name="maxSegments">Maximum segments in the list</param>
        /// <returns>Returns false if the segment buffer is too small</returns>
        private bool SkipInternalEdge(IGraphQueryFilter filter, int j, TileRef cur, Vector3 vi, Vector3 vj, List<Segment> segments, int maxSegments)
        {
            // Internal edge
            int neij = cur.Poly.Neis[j];
            int neiRef = 0;
            if (neij != 0)
            {
                int idx = neij - 1;
                neiRef = m_nav.GetTileRef(cur.Tile) | idx;
                if (!filter.PassFilter(cur.Tile.Polys[idx].Flags))
                {
                    neiRef = 0;
                }
            }

            // If the edge leads to another polygon and portals are not stored, skip.
            if (neiRef != 0)
            {
                return true;
            }

            if (segments.Count < maxSegments)
            {
                segments.Add(new()
                {
                    S1 = vj,
                    S2 = vi,
                    R = neiRef,
                });
            }
            else
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Stores the specified segment in the list
        /// </summary>
        /// <param name="vi">Segment point i</param>
        /// <param name="vj">Segment point j</param>
        /// <param name="ints">Interval list</param>
        /// <param name="segments">Segment list</param>
        /// <param name="maxSegments">Maximum segments in the list</param>
        /// <returns>Returns false if the segment buffer is too small</returns>
        private static bool StoreSegments(Vector3 vi, Vector3 vj, List<SegInterval> ints, List<Segment> segments, int maxSegments)
        {
            for (int k = 1; k < ints.Count; ++k)
            {
                if (segments.Count >= maxSegments)
                {
                    return false;
                }

                // Portal segment.
                if (ints[k].R != 0)
                {
                    float tmin = ints[k].TMin / 255.0f;
                    float tmax = ints[k].TMax / 255.0f;

                    segments.Add(new()
                    {
                        S1 = Vector3.Lerp(vj, vi, tmin),
                        S2 = Vector3.Lerp(vj, vi, tmax),
                        R = ints[k].R,
                    });
                }

                if (segments.Count >= maxSegments)
                {
                    return false;
                }

                // Wall segment.
                int imin = ints[k - 1].TMax;
                int imax = ints[k].TMin;
                if (imin == imax)
                {
                    continue;
                }

                float min = imin / 255.0f;
                float max = imax / 255.0f;

                segments.Add(new()
                {
                    S1 = Vector3.Lerp(vj, vi, min),
                    S2 = Vector3.Lerp(vj, vi, max),
                    R = 0,
                });
            }

            return true;
        }
    }
}
