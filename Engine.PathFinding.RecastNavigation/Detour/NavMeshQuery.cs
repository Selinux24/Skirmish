using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Provides the ability to perform pathfinding related queries against a navigation mesh.
    /// </summary>
    public class NavMeshQuery : IDisposable
    {
        /// <summary>
        /// Limit raycasting during any angle pahfinding
        /// The limit is given as a multiple of the character radius
        /// </summary>
        const float DT_RAY_CAST_LIMIT_PROPORTIONS = 50.0f;
        /// <summary>
        /// Parent node bits
        /// </summary>
        const int DT_NODE_PARENT_BITS = 24;
        /// <summary>
        /// State node bits
        /// </summary>
        const int DT_NODE_STATE_BITS = 2;
        /// <summary>
        /// Number of extra states per node. See dtNode::state
        /// </summary>
        const int DT_MAX_STATES_PER_NODE = 1 << DT_NODE_STATE_BITS;
        /// <summary>
        /// Search heuristic scale.
        /// </summary>
        const float H_SCALE = 0.999f;
        /// <summary>
        /// Maximum polygon count in the query
        /// </summary>
        const int MAX_POLYS = 256;
        /// <summary>
        /// Maximum smooth points in the query
        /// </summary>
        const int MAX_SMOOTH = 2048;

        /// <summary>
        /// Navmesh data.
        /// </summary>
        private readonly NavMesh m_nav = null;
        /// <summary>
        /// Node pool.
        /// </summary>
        private readonly NodePool m_nodePool = null;
        /// <summary>
        /// Small node pool.
        /// </summary>
        private readonly NodePool m_tinyNodePool = null;
        /// <summary>
        /// Open list queue.
        /// </summary>
        private readonly NodeQueue m_openList = null;
        /// <summary>
        /// Sliced query state.
        /// </summary>
        private QueryData m_query = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nav">Pointer to the dtNavMesh object to use for all queries.</param>
        /// <param name="maxNodes">Maximum number of search nodes.</param>
        public NavMeshQuery(NavMesh nav, int maxNodes)
        {
            m_query = new QueryData();

            if (maxNodes > (1 << DT_NODE_PARENT_BITS) - 1)
            {
                throw new ArgumentException("Invalid maximum nodes value.", nameof(maxNodes));
            }

            m_nav = nav;

            if (m_nodePool == null || m_nodePool.GetMaxNodes() < maxNodes)
            {
                if (m_nodePool != null)
                {
                    m_nodePool.Dispose();
                    m_nodePool = null;
                }

                m_nodePool = new NodePool(maxNodes, Helper.NextPowerOfTwo(maxNodes / 4));
            }
            else
            {
                m_nodePool.Clear();
            }

            if (m_tinyNodePool == null)
            {
                m_tinyNodePool = new NodePool(64, 32);
            }
            else
            {
                m_tinyNodePool.Clear();
            }

            if (m_openList == null || m_openList.GetCapacity() < maxNodes)
            {
                if (m_openList != null)
                {
                    m_openList.Dispose();
                    m_openList = null;
                }

                m_openList = new NodeQueue(maxNodes);
            }
            else
            {
                m_openList.Clear();
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~NavMeshQuery()
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
                m_tinyNodePool?.Dispose();
                m_openList?.Dispose();
            }
        }

        /// <summary>
        /// Selects a tile reference
        /// </summary>
        /// <param name="best">Best tile</param>
        /// <param name="tile">Candidate tile</param>
        /// <param name="areaSum">Area sumatory</param>
        /// <returns>Returns the selected tile</returns>
        private static TileRef SelectBestTile(TileRef best, TileRef tile, ref float areaSum)
        {
            // Place random locations on on ground.
            if (best.Poly.Type == PolyTypes.Ground)
            {
                // Calc area of the polygon.
                float polyArea = best.Tile.GetPolyArea(best.Poly);
                // Choose random polygon weighted by area, using reservoi sampling.
                areaSum += polyArea;
                float u = Helper.RandomGenerator.NextFloat(0, 1);
                if (u * areaSum <= polyArea)
                {
                    return best;
                }
            }

            return tile;
        }
        /// <summary>
        /// Returns portal points between two polygons.
        /// </summary>
        private static Status GetPortalPoints(TileRef from, TileRef to, out Vector3 left, out Vector3 right)
        {
            left = new();
            right = new();

            // Find the link that points to the 'to' polygon.
            Link? link = null;
            for (int i = from.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = from.Tile.Links[i].Next)
            {
                if (from.Tile.Links[i].NRef == to.Ref)
                {
                    link = from.Tile.Links[i];
                    break;
                }
            }
            if (!link.HasValue)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Handle off-mesh connections.
            if (from.Poly.Type == PolyTypes.OffmeshConnection)
            {
                // Find link that points to first vertex.
                return from.Tile.FindLinkToNeighbour(from.Poly, to.Ref, out left, out right);
            }

            if (to.Poly.Type == PolyTypes.OffmeshConnection)
            {
                return to.Tile.FindLinkToNeighbour(to.Poly, from.Ref, out left, out right);
            }

            // Find portal vertices.
            int v0 = from.Poly.Verts[link.Value.Edge];
            int v1 = from.Poly.Verts[(link.Value.Edge + 1) % from.Poly.VertCount];
            left = from.Tile.Verts[v0];
            right = from.Tile.Verts[v1];

            // If the link is at tile boundary, dtClamp the vertices to
            // the link width.
            if (link.Value.Side != 0xff && (link.Value.BMin != 0 || link.Value.BMax != 255))
            {
                // Unpack portal limits.
                float s = 1.0f / 255.0f;
                float tmin = link.Value.BMin * s;
                float tmax = link.Value.BMax * s;
                left = Vector3.Lerp(from.Tile.Verts[v0], from.Tile.Verts[v1], tmin);
                right = Vector3.Lerp(from.Tile.Verts[v0], from.Tile.Verts[v1], tmax);
            }

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Returns edge mid point between two polygons.
        /// </summary>
        private static Status GetEdgeMidPoint(TileRef from, TileRef to, out Vector3 mid)
        {
            mid = new();

            if (GetPortalPoints(from, to, out var left, out var right).HasFlag(Status.DT_FAILURE))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            mid = (left + right) * 0.5f;

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Finds movement delta
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="targetPos">Target position</param>
        /// <param name="overMesh">Over mesh flag</param>
        private static Vector3 FindMovementDelta(Vector3 position, Vector3 targetPos, bool overMesh)
        {
            const float STEP_SIZE = 0.5f;

            // Find movement delta.
            var delta = Vector3.Subtract(targetPos, position);
            float len = delta.Length();

            // If the steer target is end of path or off-mesh link, do not move past the location.
            if (overMesh && len < STEP_SIZE)
            {
                len = 1;
            }
            else
            {
                len = STEP_SIZE / len;
            }

            return Vector3.Add(position, delta * len);
        }

        /// <summary>
        /// Finds a path from the start polygon to the end polygon.
        /// </summary>
        /// <param name="startRef">The refrence id of the start polygon.</param>
        /// <param name="endRef">The reference id of the end polygon.</param>
        /// <param name="startPos">A position within the start polygon.</param>
        /// <param name="endPos">A position within the end polygon.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxPath">The maximum number of polygons the @p path array can hold.</param>
        /// <param name="resultPath">Result path</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindPath(PathPoint start, PathPoint end, QueryFilter filter, int maxPath, out SimplePath resultPath)
        {
            resultPath = new(maxPath);

            // Validate input
            if (filter == null || maxPath <= 0 || !start.IsValid(m_nav) || !end.IsValid(m_nav))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            if (start.Ref == end.Ref)
            {
                resultPath.StartPath(start.Ref);
                return Status.DT_SUCCESS;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            var startNode = m_nodePool.GetNode(start.Ref, 0);
            startNode.Pos = start.Pos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = Vector3.Distance(start.Pos, end.Pos) * H_SCALE;
            startNode.Id = start.Ref;
            startNode.Flags = NodeFlagTypes.Open;
            m_openList.Push(startNode);

            var lastBestNode = startNode;
            float lastBestNodeCost = startNode.Total;

            bool outOfNodes = false;

            while (!m_openList.Empty())
            {
                // Remove node from open list and put it in closed list.
                var bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Reached the goal, stop searching.
                if (bestNode.Id == end.Ref)
                {
                    lastBestNode = bestNode;
                    break;
                }

                // Get current poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var best = m_nav.GetTileAndPolyByRefUnsafe(bestNode.Id);

                // Get parent poly and tile.
                int parentRef = 0;
                TileRef parent = TileRef.Null;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef != 0)
                {
                    parent = m_nav.GetTileAndPolyByRefUnsafe(parentRef);
                }

                for (int i = best.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = best.Tile.Links[i].Next)
                {
                    int neighbourRef = best.Tile.Links[i].NRef;

                    // Skip invalid ids and do not expand back to where we came from.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Get neighbour poly and tile.
                    // The API input has been cheked already, skip checking internal data.
                    var neighbour = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);

                    if (!filter.PassFilter(neighbour.Poly.Flags))
                    {
                        continue;
                    }

                    // deal explicitly with crossing tile boundaries
                    int crossSide = 0;
                    if (best.Tile.Links[i].Side != 0xff)
                    {
                        crossSide = best.Tile.Links[i].Side >> 1;
                    }

                    // get the node
                    var neighbourNode = m_nodePool.GetNode(neighbourRef, crossSide);
                    if (neighbourNode == null)
                    {
                        outOfNodes = true;
                        continue;
                    }

                    // If the node is visited the first time, calculate node position.
                    if (neighbourNode.Flags == NodeFlagTypes.None)
                    {
                        var midPointRes = GetEdgeMidPoint(best, neighbour, out var pos);

                        if (midPointRes != Status.DT_SUCCESS)
                        {
                            Logger.WriteWarning(this, $"FindPath GetEdgeMidPoint result: {midPointRes}");
                            return midPointRes;
                        }

                        neighbourNode.Pos = pos;
                    }

                    // Calculate cost and heuristic.
                    float cost;
                    float heuristic;

                    // Special case for last node.
                    if (neighbourRef == end.Ref)
                    {
                        // Cost
                        float curCost = filter.GetCost(bestNode.Pos, neighbourNode.Pos, parent, best, neighbour);
                        float endCost = filter.GetCost(neighbourNode.Pos, end.Pos, best, neighbour, TileRef.Null);

                        cost = bestNode.Cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        // Cost
                        float curCost = filter.GetCost(bestNode.Pos, neighbourNode.Pos, parent, best, neighbour);

                        cost = bestNode.Cost + curCost;
                        heuristic = Vector3.Distance(neighbourNode.Pos, end.Pos) * H_SCALE;
                    }

                    float total = cost + heuristic;

                    // The node is already in open list and the new result is worse, skip.
                    if (neighbourNode.IsOpen && total >= neighbourNode.Total)
                    {
                        continue;
                    }

                    // The node is already visited and process, and the new result is worse, skip.
                    if (neighbourNode.IsClosed && total >= neighbourNode.Total)
                    {
                        continue;
                    }

                    // Add or update the node.
                    neighbourNode.PIdx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.Id = neighbourRef;
                    neighbourNode.Flags &= ~NodeFlagTypes.Closed;
                    neighbourNode.Cost = cost;
                    neighbourNode.Total = total;

                    if (neighbourNode.IsOpen)
                    {
                        // Already in open, update node location.
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        // Put the node in open list.
                        neighbourNode.Flags |= NodeFlagTypes.Open;
                        m_openList.Push(neighbourNode);
                    }

                    // Update nearest node to target so far.
                    if (heuristic < lastBestNodeCost)
                    {
                        lastBestNodeCost = heuristic;
                        lastBestNode = neighbourNode;
                    }
                }
            }

            Status status = GetPathToNode(lastBestNode, maxPath, out resultPath);

            if (lastBestNode.Id != end.Ref)
            {
                status |= Status.DT_PARTIAL_RESULT;
            }

            if (outOfNodes)
            {
                status |= Status.DT_OUT_OF_NODES;
            }

            return status;
        }
        /// <summary>
        /// Finds the straight path from the start to the end position within the polygon corridor.
        /// </summary>
        /// <param name="startPos">Path start position.</param>
        /// <param name="endPos">Path end position.</param>
        /// <param name="path">An array of polygon references that represent the path corridor.</param>
        /// <param name="maxStraightPath">The maximum number of points the straight path arrays can hold.</param>
        /// <param name="options">Query options.</param>
        /// <param name="resultPath">Result path</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindStraightPath(Vector3 startPos, Vector3 endPos, SimplePath path, int maxStraightPath, StraightPathOptions options, out StraightPath resultPath)
        {
            resultPath = new(maxStraightPath);

            if (path == null || path.Count <= 0 || maxStraightPath <= 0 || startPos.IsInfinity() || endPos.IsInfinity())
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            var fpStatus = ClosestPointOnPolyBoundary(path.Start, startPos, out Vector3 closestStartPos);
            if (fpStatus.HasFlag(Status.DT_FAILURE))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            var lpStatus = ClosestPointOnPolyBoundary(path.End, endPos, out Vector3 closestEndPos);
            if (lpStatus.HasFlag(Status.DT_FAILURE))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Add start point.
            var startPStatus = resultPath.AppendVertex(closestStartPos, StraightPathFlagTypes.DT_STRAIGHTPATH_START, path.Start, maxStraightPath);
            if (startPStatus != Status.DT_IN_PROGRESS)
            {
                return startPStatus;
            }

            if (path.Count > 1)
            {
                var portalApex = closestStartPos;
                var portalLeft = portalApex;
                var portalRight = portalApex;
                int apexIndex = 0;
                int leftIndex = 0;
                int rightIndex = 0;

                PolyTypes leftPolyType = 0;
                PolyTypes rightPolyType = 0;

                int leftPolyRef = path.Start;
                int rightPolyRef = path.Start;

                var pathNodes = path.GetPath();

                for (int i = 0; i < path.Count; ++i)
                {
                    Vector3 left;
                    Vector3 right;
                    PolyTypes toType;

                    if (i + 1 < path.Count)
                    {
                        // Next portal.
                        var ppStatus = GetPortalPoints(
                            pathNodes[i],
                            pathNodes[i + 1],
                            out left, out right, out _, out toType);

                        if (ppStatus.HasFlag(Status.DT_FAILURE))
                        {
                            // Failed to get portal points, in practice this means that path[i+1] is invalid polygon.
                            // Clamp the end point to path[i], and return the path so far.

                            var cpBoundaryStatus = ClosestPointOnPolyBoundary(pathNodes[i], endPos, out closestEndPos);
                            if (cpBoundaryStatus.HasFlag(Status.DT_FAILURE))
                            {
                                // This should only happen when the first polygon is invalid.
                                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
                            }

                            // Apeend portals along the current straight path segment.
                            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
                            {
                                // Ignore status return value as we're just about to return anyway.
                                AppendPortals(
                                    apexIndex, i, closestEndPos, pathNodes, maxStraightPath, options,
                                    ref resultPath);
                            }

                            // Ignore status return value as we're just about to return anyway.
                            resultPath.AppendVertex(closestEndPos, 0, pathNodes[i], maxStraightPath);

                            return Status.DT_SUCCESS | Status.DT_PARTIAL_RESULT | ((resultPath.Count >= maxStraightPath) ? Status.DT_BUFFER_TOO_SMALL : 0);
                        }

                        // If starting really close the portal, advance.
                        if (i == 0 && Utils.DistancePtSegSqr2D(portalApex, left, right) < (0.001f * 0.001f))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // End of the path.
                        left = closestEndPos;
                        right = closestEndPos;

                        toType = PolyTypes.Ground;
                    }

                    // Right vertex.
                    if (Utils.TriArea2D(portalApex, portalRight, right) <= 0.0f)
                    {
                        if (Utils.VClosest(portalApex, portalRight) || Utils.TriArea2D(portalApex, portalLeft, right) > 0.0f)
                        {
                            portalRight = right;
                            rightPolyRef = (i + 1 < path.Count) ? pathNodes[i + 1] : 0;
                            rightPolyType = toType;
                            rightIndex = i;
                        }
                        else
                        {
                            // Append portals along the current straight path segment.
                            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
                            {
                                var appendStatus = AppendPortals(
                                    apexIndex, leftIndex, portalLeft, pathNodes, maxStraightPath, options,
                                    ref resultPath);
                                if (appendStatus != Status.DT_IN_PROGRESS)
                                {
                                    return appendStatus;
                                }
                            }

                            portalApex = portalLeft;
                            apexIndex = leftIndex;

                            StraightPathFlagTypes flags = 0;
                            if (leftPolyRef == 0)
                            {
                                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_END;
                            }
                            else if (leftPolyType == PolyTypes.OffmeshConnection)
                            {
                                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION;
                            }
                            int r = leftPolyRef;

                            // Append or update vertex
                            var stat = resultPath.AppendVertex(portalApex, flags, r, maxStraightPath);
                            if (stat != Status.DT_IN_PROGRESS)
                            {
                                return stat;
                            }

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            // Restart
                            i = apexIndex;

                            continue;
                        }
                    }

                    // Left vertex.
                    if (Utils.TriArea2D(portalApex, portalLeft, left) >= 0.0f)
                    {
                        if (Utils.VClosest(portalApex, portalLeft) || Utils.TriArea2D(portalApex, portalRight, left) < 0.0f)
                        {
                            portalLeft = left;
                            leftPolyRef = (i + 1 < path.Count) ? pathNodes[i + 1] : 0;
                            leftPolyType = toType;
                            leftIndex = i;
                        }
                        else
                        {
                            // Append portals along the current straight path segment.
                            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
                            {
                                var appendStatus = AppendPortals(
                                    apexIndex, rightIndex, portalRight, pathNodes, maxStraightPath, options,
                                    ref resultPath);

                                if (appendStatus != Status.DT_IN_PROGRESS)
                                {
                                    return appendStatus;
                                }
                            }

                            portalApex = portalRight;
                            apexIndex = rightIndex;

                            StraightPathFlagTypes flags = 0;
                            if (rightPolyRef == 0)
                            {
                                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_END;
                            }
                            else if (rightPolyType == PolyTypes.OffmeshConnection)
                            {
                                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION;
                            }
                            int r = rightPolyRef;

                            // Append or update vertex
                            var stat = resultPath.AppendVertex(portalApex, flags, r, maxStraightPath);
                            if (stat != Status.DT_IN_PROGRESS)
                            {
                                return stat;
                            }

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            // Restart
                            i = apexIndex;
                        }
                    }
                }

                // Append portals along the current straight path segment.
                if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
                {
                    var stat = AppendPortals(
                        apexIndex, path.Count - 1, closestEndPos, pathNodes, maxStraightPath, options,
                        ref resultPath);

                    if (stat != Status.DT_IN_PROGRESS)
                    {
                        return stat;
                    }
                }
            }

            // Ignore status return value as we're just about to return anyway.
            resultPath.AppendVertex(closestEndPos, StraightPathFlagTypes.DT_STRAIGHTPATH_END, 0, maxStraightPath);

            return Status.DT_SUCCESS | ((resultPath.Count >= maxStraightPath) ? Status.DT_BUFFER_TOO_SMALL : 0);
        }
        /// <summary>
        /// Intializes a sliced path query.
        /// </summary>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="start">A position within the start polygon.</param>
        /// <param name="end">A position within the end polygon.</param>
        /// <param name="options">Query options</param>
        /// <returns>The status flags for the query.</returns>
        /// <example>
        /// Common use case:
        /// -# Call InitSlicedFindPath() to initialize the sliced path query.
        /// -# Call UpdateSlicedFindPath() until it returns complete.
        /// -# Call FinalizeSlicedFindPath() to get the path.
        /// </example>
        public Status InitSlicedFindPath(QueryFilter filter, PathPoint start, PathPoint end, FindPathOptions options = FindPathOptions.AnyAngle)
        {
            // Init path state.
            m_query = new()
            {
                Status = 0,
                StartRef = start.Ref,
                EndRef = end.Ref,
                StartPos = start.Pos,
                EndPos = end.Pos,
                Filter = filter,
                Options = options,
                RaycastLimitSqr = float.MaxValue
            };

            // Validate input
            if (filter == null || !start.IsValid(m_nav) || !end.IsValid(m_nav))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // trade quality with performance?
            if ((options & FindPathOptions.AnyAngle) != 0)
            {
                // limiting to several times the character radius yields nice results. It is not sensitive 
                // so it is enough to compute it from the first tile.
                var tile = m_nav.GetTileByRef(start.Ref);
                float agentRadius = tile.Header.WalkableRadius;
                m_query.RaycastLimitSqr = (float)Math.Pow(agentRadius * DT_RAY_CAST_LIMIT_PROPORTIONS, 2);
            }

            if (start.Ref == end.Ref)
            {
                m_query.Status = Status.DT_SUCCESS;
                return Status.DT_SUCCESS;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            var startNode = m_nodePool.GetNode(start.Ref, 0);
            startNode.Pos = start.Pos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = Vector3.Distance(start.Pos, end.Pos) * H_SCALE;
            startNode.Id = start.Ref;
            startNode.Flags = NodeFlagTypes.Open;
            m_openList.Push(startNode);

            m_query.Status = Status.DT_IN_PROGRESS;
            m_query.LastBestNode = startNode;
            m_query.LastBestNodeCost = startNode.Total;

            return m_query.Status;
        }
        /// <summary>
        /// Updates an in-progress sliced path query.
        /// </summary>
        /// <param name="maxIter">The maximum number of iterations to perform.</param>
        /// <param name="doneIters">The actual number of iterations completed.</param>
        /// <returns>The status flags for the query.</returns>
        public Status UpdateSlicedFindPath(int maxIter, out int doneIters)
        {
            doneIters = 0;

            if (!m_query.Status.HasFlag(Status.DT_IN_PROGRESS))
            {
                return m_query.Status;
            }

            // Make sure the request is still valid.
            if (!m_nav.IsValidPolyRef(m_query.StartRef) || !m_nav.IsValidPolyRef(m_query.EndRef))
            {
                m_query.Status = Status.DT_FAILURE;
                return Status.DT_FAILURE;
            }

            int iter = 0;
            while (iter < maxIter && !m_openList.Empty())
            {
                iter++;

                // Remove node from open list and put it in closed list.
                Node bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Reached the goal, stop searching.
                if (bestNode.Id == m_query.EndRef)
                {
                    m_query.LastBestNode = bestNode;
                    Status details = m_query.Status & Status.DT_STATUS_DETAIL_MASK;
                    m_query.Status = Status.DT_SUCCESS | details;
                    doneIters = iter;

                    return m_query.Status;
                }

                // Get current poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var best = m_nav.GetTileAndPolyByNode(bestNode);
                if (best.Ref == 0)
                {
                    // The polygon has disappeared during the sliced query, fail.
                    m_query.Status = Status.DT_FAILURE;
                    doneIters = iter;

                    return m_query.Status;
                }

                // Get parent and grand parent poly and tile.
                var (found, parent, parentNode, grandpaRef) = GetParents(bestNode);
                if (!found)
                {
                    // The polygon has disappeared during the sliced query, fail.
                    m_query.Status = Status.DT_FAILURE;
                    doneIters = iter;

                    return m_query.Status;
                }

                EvaluateLinks(best, bestNode, parent, parentNode, grandpaRef);
            }

            // Exhausted all nodes, but could not find path.
            if (m_openList.Empty())
            {
                Status details = m_query.Status & Status.DT_STATUS_DETAIL_MASK;
                m_query.Status = Status.DT_SUCCESS | details;
            }

            doneIters = iter;

            return m_query.Status;
        }
        /// <summary>
        /// Gets the parent and grand parent of the specified node
        /// </summary>
        /// <param name="node">Node</param>
        private (bool Found, TileRef Parent, Node ParentNode, int? PrandpaRef) GetParents(Node node)
        {
            Node parentNode = null;
            int? grandpaRef = null;
            if (node.PIdx != 0)
            {
                parentNode = m_nodePool.GetNodeAtIdx(node.PIdx);
                if (parentNode.PIdx != 0)
                {
                    grandpaRef = m_nodePool.GetNodeAtIdx(parentNode.PIdx).Id;
                }
            }

            TileRef parent = TileRef.Null;
            if (parentNode?.Id > 0)
            {
                parent = m_nav.GetTileAndPolyByNode(parentNode);
                if (parent.Ref == 0 || (grandpaRef.HasValue && !m_nav.IsValidPolyRef(grandpaRef.Value)))
                {
                    return (false, default, default, default);
                }
            }

            return (true, parent, parentNode, grandpaRef);
        }
        /// <summary>
        /// Decides whether to test raycast to previous nodes
        /// </summary>
        /// <param name="parentNode">Parent node</param>
        /// <param name="node">Node</param>
        /// <returns></returns>
        private bool UseLOS(Node parentNode, Node node)
        {
            if ((m_query.Options & FindPathOptions.AnyAngle) != 0 &&
                ((parentNode?.Id ?? 0) != 0) &&
                (Vector3.DistanceSquared(parentNode.Pos, node.Pos) < m_query.RaycastLimitSqr))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Evaluates the node links
        /// </summary>
        /// <param name="best">Tile</param>
        /// <param name="bestNode">Node</param>
        /// <param name="parent">Parent tile</param>
        /// <param name="parentNode">Parent node</param>
        /// <param name="grandpaRef">Grand parent reference</param>
        private void EvaluateLinks(TileRef best, Node bestNode, TileRef parent, Node parentNode, int? grandpaRef)
        {
            // decide whether to test raycast to previous nodes
            bool tryLOS = UseLOS(parentNode, bestNode);

            for (int i = best.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = best.Tile.Links[i].Next)
            {
                int neighbourRef = best.Tile.Links[i].NRef;

                // Skip invalid ids and do not expand back to where we came from.
                if (neighbourRef == 0 || neighbourRef == parentNode?.Id)
                {
                    continue;
                }

                // Get neighbour poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var neighbour = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);

                if (!m_query.Filter.PassFilter(neighbour.Poly.Flags))
                {
                    continue;
                }

                // get the neighbor node
                Node neighbourNode = m_nodePool.GetNode(neighbourRef, 0);
                if (neighbourNode == null)
                {
                    m_query.Status |= Status.DT_OUT_OF_NODES;
                    continue;
                }

                // do not expand to nodes that were already visited from the same parent
                if (neighbourNode.PIdx != 0 && neighbourNode.PIdx == bestNode.PIdx)
                {
                    continue;
                }

                neighbour.Node = neighbourNode;

                UpdateNeighbourNode(tryLOS, neighbour, parent, best, ref grandpaRef);
            }
        }
        /// <summary>
        /// Updates the neighbour node cost
        /// </summary>
        /// <param name="tryLOS">Try line of sight</param>
        /// <param name="neighbour">Neighbour</param>
        /// <param name="parent">Parent</param>
        /// <param name="best">Best</param>
        /// <param name="grandpaRef">Grandparent reference</param>
        private void UpdateNeighbourNode(bool tryLOS, TileRef neighbour, TileRef parent, TileRef best, ref int? grandpaRef)
        {
            // If the node is visited the first time, calculate node position.
            if (neighbour.Node.Flags == NodeFlagTypes.None)
            {
                var midPointRes = GetEdgeMidPoint(best, neighbour, out var pos);
                if (midPointRes != Status.DT_SUCCESS)
                {
                    Logger.WriteWarning(this, $"UpdateSlicedFindPath GetEdgeMidPoint result: {midPointRes}");
                    return;
                }

                neighbour.Node.Pos = pos;
            }

            // Calculate cost and heuristic.
            float cost;
            bool foundShortCut = false;
            if (tryLOS)
            {
                cost = CalcCostLOS(neighbour, parent, best, ref grandpaRef, out foundShortCut);
            }
            else
            {
                cost = CalcCost(neighbour, parent, best);
            }

            float heuristic = CalcHeuristic(ref cost, neighbour, best);

            float total = cost + heuristic;

            // The node is already in open list and the new result is worse, skip.
            if (neighbour.Node.IsOpen && total >= neighbour.Node.Total)
            {
                return;
            }

            // The node is already visited and process, and the new result is worse, skip.
            if (neighbour.Node.IsClosed && total >= neighbour.Node.Total)
            {
                return;
            }

            // Add or update the node.
            neighbour.Node.PIdx = foundShortCut ? best.Node.PIdx : m_nodePool.GetNodeIdx(best.Node);
            neighbour.Node.Id = neighbour.Ref;
            neighbour.Node.Flags = neighbour.Node.Flags & ~(NodeFlagTypes.Closed | NodeFlagTypes.ParentDetached);
            neighbour.Node.Cost = cost;
            neighbour.Node.Total = total;

            if (foundShortCut)
            {
                neighbour.Node.Flags = neighbour.Node.Flags | NodeFlagTypes.ParentDetached;
            }

            if ((neighbour.Node.Flags & NodeFlagTypes.Open) != 0)
            {
                // Already in open, update node location.
                m_openList.Modify(neighbour.Node);
            }
            else
            {
                // Put the node in open list.
                neighbour.Node.Flags |= NodeFlagTypes.Open;
                m_openList.Push(neighbour.Node);
            }

            // Update nearest node to target so far.
            if (heuristic < m_query.LastBestNodeCost)
            {
                m_query.LastBestNodeCost = heuristic;
                m_query.LastBestNode = neighbour.Node;
            }
        }
        /// <summary>
        /// Calculates the node cost
        /// </summary>
        /// <param name="neighbour">Neighbour</param>
        /// <param name="parent">Parent</param>
        /// <param name="best">Best</param>
        /// <returns>Returns the cost value</returns>
        private float CalcCost(TileRef neighbour, TileRef parent, TileRef best)
        {
            // update move cost
            float curCost = m_query.Filter.GetCost(best.Node.Pos, neighbour.Node.Pos, parent, best, neighbour);

            return best.Node.Cost + curCost;
        }
        /// <summary>
        /// Calculates the node cost using line of sight
        /// </summary>
        /// <param name="neighbour">Neighbour</param>
        /// <param name="parent">Parent</param>
        /// <param name="best">Best</param>
        /// <param name="grandpaRef">Grandparent reference</param>
        /// <param name="foundShortCut">Returns true if a shortcut was found</param>
        /// <returns>Returns the cost value</returns>
        private float CalcCostLOS(TileRef neighbour, TileRef parent, TileRef best, ref int? grandpaRef, out bool foundShortCut)
        {
            var request = new RaycastRequest
            {
                StartRef = parent.Ref,
                StartPos = parent.Node.Pos,
                EndPos = neighbour.Node.Pos,
                Filter = m_query.Filter,
                Options = RaycastOptions.DT_RAYCAST_USE_COSTS,
                MaxPath = 0,
                PrevReference = grandpaRef,
            };

            Raycast(request, out var rayHit);

            // raycast parent
            grandpaRef = rayHit.PrevReference;
            foundShortCut = rayHit.T >= 1.0f;

            // update move cost
            if (foundShortCut)
            {
                // shortcut found using raycast. Using shorter cost instead
                return parent.Node.Cost + rayHit.PathCost;
            }
            else
            {
                // No shortcut found.
                float curCost = m_query.Filter.GetCost(best.Node.Pos, neighbour.Node.Pos, parent, best, neighbour);

                return best.Node.Cost + curCost;
            }
        }
        /// <summary>
        /// Calculates the node heuristic value
        /// </summary>
        /// <param name="cost">Node cost</param>
        /// <param name="neighbour">Neighbour</param>
        /// <param name="best">Best</param>
        /// <returns>Returns the heuristic value</returns>
        private float CalcHeuristic(ref float cost, TileRef neighbour, TileRef best)
        {
            // Special case for last node.
            float heuristic;
            if (neighbour.Ref == m_query.EndRef)
            {
                float endCost = m_query.Filter.GetCost(neighbour.Node.Pos, m_query.EndPos, best, neighbour, TileRef.Null);
                cost += endCost;
                heuristic = 0;
            }
            else
            {
                heuristic = Vector3.Distance(neighbour.Node.Pos, m_query.EndPos) * H_SCALE;
            }

            return heuristic;
        }
        /// <summary>
        /// Finalizes and returns the results of a sliced path query.
        /// </summary>
        /// <param name="maxPath">The max number of polygons the path array can hold.</param>
        /// <param name="path">An ordered list of polygon references representing the path. (Start to end.)</param>
        /// <returns>The status flags for the query.</returns>
        public Status FinalizeSlicedFindPath(int maxPath, out SimplePath path)
        {
            path = null;

            if (maxPath <= 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            List<int> pathList = [];

            if (m_query.Status.HasFlag(Status.DT_FAILURE))
            {
                // Reset query.
                m_query = null;
                return Status.DT_FAILURE;
            }

            if (m_query.StartRef == m_query.EndRef)
            {
                // Special case: the search starts and ends at same poly.
                pathList.Add(m_query.StartRef);
            }
            else
            {
                // Reverse the path.
                if (m_query.LastBestNode.Id != m_query.EndRef)
                {
                    m_query.Status |= Status.DT_PARTIAL_RESULT;
                }

                var node = ReversePath(m_query.LastBestNode);

                pathList.AddRange(StorePath(node, maxPath));
            }

            Status details = m_query.Status & Status.DT_STATUS_DETAIL_MASK;

            // Reset query.
            m_query = new();

            path = new(maxPath);
            path.StartPath(pathList);

            return Status.DT_SUCCESS | details;
        }
        /// <summary>
        /// Finalizes and returns the results of an incomplete sliced path query, returning the path to the furthest polygon on the existing path that was visited during the search.
        /// </summary>
        /// <param name="existing">An array of polygon references for the existing path.</param>
        /// <param name="maxPath">The max number of polygons the @p path array can hold.</param>
        /// <param name="path">An ordered list of polygon references representing the path. (Start to end.)</param>
        /// <returns>The status flags for the query.</returns>
        public Status FinalizeSlicedFindPathPartial(int maxPath, int[] existing, out SimplePath path)
        {
            path = null;

            if ((existing?.Length ?? 0) == 0 || maxPath <= 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            if (m_query.Status.HasFlag(Status.DT_FAILURE))
            {
                // Reset query.
                m_query = new();
                return Status.DT_FAILURE;
            }

            List<int> pathList = [];

            if (m_query.StartRef == m_query.EndRef)
            {
                // Special case: the search starts and ends at same poly.
                pathList.Add(m_query.StartRef);
            }
            else
            {
                // Find furthest existing node that was visited.
                Node node = null;
                for (int i = existing.Length - 1; i >= 0; --i)
                {
                    m_nodePool.FindNodes(existing[i], 1, out Node[] nodes);
                    if (nodes != null)
                    {
                        node = nodes[0];
                        break;
                    }
                }

                if (node == null)
                {
                    m_query.Status |= Status.DT_PARTIAL_RESULT;
                    node = m_query.LastBestNode;
                }

                // Reverse the path.
                node = ReversePath(node);

                // Store path
                pathList.AddRange(StorePath(node, maxPath));
            }

            Status details = m_query.Status & Status.DT_STATUS_DETAIL_MASK;

            // Reset query.
            m_query = new();

            path = new(maxPath);
            path.StartPath(pathList);

            return Status.DT_SUCCESS | details;
        }
        /// <summary>
        /// Reverses the path updating node PIdx references
        /// </summary>
        /// <param name="node">First node</param>
        /// <returns>Returns the new first node</returns>
        private Node ReversePath(Node node)
        {
            Node prev = null;
            NodeFlagTypes prevRay = 0;
            do
            {
                var next = m_nodePool.GetNodeAtIdx(node.PIdx);
                node.PIdx = m_nodePool.GetNodeIdx(prev);
                prev = node;
                var nextRay = node.Flags & NodeFlagTypes.ParentDetached; // keep track of whether parent is not adjacent (i.e. due to raycast shortcut)
                node.Flags = (node.Flags & ~NodeFlagTypes.ParentDetached) | prevRay; // and store it in the reversed path's node
                prevRay = nextRay;
                node = next;
            }
            while (node != null);
            node = prev;

            return node;
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
                var next = m_tinyNodePool.GetNodeAtIdx(node.PIdx);
                node.PIdx = m_tinyNodePool.GetNodeIdx(prev);
                prev = node;
                node = next;
            }
            while (node != null);

            // Store result
            node = prev;
            do
            {
                if (!visited.Add(node.Id))
                {
                    return false;
                }
                node = m_tinyNodePool.GetNodeAtIdx(node.PIdx);
            }
            while (node != null);

            return true;
        }

        /// <summary>
        /// Stores the path following the node chain
        /// </summary>
        /// <param name="node">First Node</param>
        /// <param name="maxPath">Max path length</param>
        /// <returns>Returns the reference list path</returns>
        private int[] StorePath(Node node, int maxPath)
        {
            List<int> pathList = [];

            // Store path
            do
            {
                var next = m_nodePool.GetNodeAtIdx(node.PIdx);

                Status status = BuildNodePath(node, next, maxPath - pathList.Count, out var partialPath);

                pathList.AddRange(partialPath);

                if ((status & Status.DT_STATUS_DETAIL_MASK) != 0)
                {
                    m_query.Status |= status & Status.DT_STATUS_DETAIL_MASK;
                    break;
                }

                node = next;
            }
            while (node != null);

            return [.. pathList];
        }
        /// <summary>
        /// Builds the node partial path from node to next
        /// </summary>
        /// <param name="node">Current node</param>
        /// <param name="next">Next node</param>
        /// <param name="maxPath">Maximum partial path items</param>
        /// <param name="partialPath">Resulting partial path node list</param>
        /// <returns>Returns true </returns>
        private Status BuildNodePath(Node node, Node next, int maxPath, out int[] partialPath)
        {
            Status status = 0;

            List<int> pathList = [];

            if (node.Flags.HasFlag(NodeFlagTypes.ParentDetached))
            {
                var request = new RaycastRequest
                {
                    StartRef = node.Id,
                    StartPos = node.Pos,
                    EndPos = next.Pos,
                    Filter = m_query.Filter,
                    MaxPath = maxPath,
                };

                status = Raycast(request, out var hit);
                if (status.HasFlag(Status.DT_SUCCESS))
                {
                    var rPath = hit.CreateSimplePath();
                    pathList.AddRange(rPath.GetPath());
                }

                // raycast ends on poly boundary and the path might include the next poly boundary.
                if (pathList.Count > 0 && pathList[^1] == next.Id)
                {
                    pathList.RemoveAt(pathList.Count - 1); // remove to avoid duplicates
                }
            }
            else
            {
                pathList.Add(node.Id);

                if (pathList.Count >= maxPath)
                {
                    status = Status.DT_BUFFER_TOO_SMALL;
                }
            }

            partialPath = [.. pathList];

            return status;
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

            var startNode = m_nodePool.GetNode(startRef, 0);
            startNode.Pos = centerPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = NodeFlagTypes.Open;
            m_openList.Push(startNode);

            Status status = Status.DT_SUCCESS;

            int n = 0;

            while (!m_openList.Empty())
            {
                var bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var best = m_nav.GetTileAndPolyByRefUnsafe(bestNode.Id);

                // Get parent poly and tile.
                int parentRef = 0;
                var parent = TileRef.Null;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef != 0)
                {
                    parent = m_nav.GetTileAndPolyByRefUnsafe(parentRef);
                }

                if (n < maxResult)
                {
                    result.Refs[n] = best.Ref;
                    result.Parents[n] = parentRef;
                    result.Costs[n] = bestNode.Total;
                    ++n;
                }
                else
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }

                var query = new FindPolysAroundCircleQuery()
                {
                    Center = centerPos,
                    Radius = radius,
                };
                ProcessTileLinksQuery(best, parent, query, filter, out bool outOfNodes);
                if (outOfNodes)
                {
                    status |= Status.DT_OUT_OF_NODES;
                }
            }

            result.Count = n;

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

            var startNode = m_nodePool.GetNode(startRef, 0);
            startNode.Pos = centerPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = NodeFlagTypes.Open;
            m_openList.Push(startNode);

            Status status = Status.DT_SUCCESS;

            int n = 0;

            while (!m_openList.Empty())
            {
                var bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var best = m_nav.GetTileAndPolyByRefUnsafe(bestNode.Id);

                // Get parent poly and tile.
                int parentRef = 0;
                var parent = TileRef.Null;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef != 0)
                {
                    parent = m_nav.GetTileAndPolyByRefUnsafe(parentRef);
                }

                if (n < maxResult)
                {
                    result.Refs[n] = best.Ref;
                    result.Parents[n] = parentRef;
                    result.Costs[n] = bestNode.Total;
                    ++n;
                }
                else
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

            result.Count = n;

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
            var portalStatus = GetPortalPoints(best, neighbour, out Vector3 va, out Vector3 vb);
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
            neighbour.Node = m_nodePool.GetNode(neighbour.Ref, 0);
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

            neighbour.Node.Id = neighbour.Ref;
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
        /// Gets a path from the explored nodes in the previous search.
        /// </summary>
        /// <param name="endRef">The reference id of the end polygon.</param>
        /// <param name="maxPath">The maximum number of polygons the path array can hold.</param>
        /// <param name="path">An ordered list of polygon references representing the path. (Start to end.)</param>
        /// <returns>
        /// The status flags. Returns DT_FAILURE | DT_INVALID_PARAM if any parameter is wrong, or if
        /// endRef was not explored in the previous search. Returns DT_SUCCESS | DT_BUFFER_TOO_SMALL
        /// if path cannot contain the entire path. In this case it is filled to capacity with a partial path.
        /// Otherwise returns DT_SUCCESS.
        /// </returns>
        /// <remarks>
        /// The result of this function depends on the state of the query object. For that reason it should only
        /// be used immediately after one of the two Dijkstra searches, findPolysAroundCircle or findPolysAroundShape.
        /// </remarks>
        public Status GetPathFromDijkstraSearch(int endRef, int maxPath, out SimplePath path)
        {
            path = null;

            if (!m_nav.IsValidPolyRef(endRef) || maxPath < 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            if (m_nodePool.FindNodes(endRef, 1, out var endNodes) != 1 || (endNodes[0].Flags & NodeFlagTypes.Closed) == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            return GetPathToNode(endNodes[0], maxPath, out path);
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

            var query = new FindNearestPolyQuery(this, center);

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
        public Status QueryPolygons(Vector3 center, Vector3 halfExtents, QueryFilter filter, IPolyQuery query)
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

            int MAX_NEIS = 32;

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
        /// Finds the non-overlapping navigation polygons in the local neighbourhood around the center position.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="centerPos">The center of the query circle.</param>
        /// <param name="radius">The radius of the query circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="maxResult">The maximum number of polygons the result arrays can hold.</param>
        /// <param name="result">The polygons in the local neighbourhood.</param>
        /// <returns>The status flags for the query.</returns>
        public Status FindLocalNeighbourhood(int startRef, Vector3 centerPos, float radius, QueryFilter filter, int maxResult, out PolyRefs result)
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

            int MAX_STACK = 48;
            Node[] stack = new Node[MAX_STACK];
            int nstack = 0;

            m_tinyNodePool.Clear();

            Node startNode = m_tinyNodePool.GetNode(startRef, 0);
            startNode.PIdx = 0;
            startNode.Id = startRef;
            startNode.Flags = NodeFlagTypes.Closed;
            stack[nstack++] = startNode;

            float radiusSqr = (radius * radius);

            Vector3[] pa = new Vector3[IndexedPolygon.DT_VERTS_PER_POLYGON];
            Vector3[] pb = new Vector3[IndexedPolygon.DT_VERTS_PER_POLYGON];

            Status status = Status.DT_SUCCESS;

            int n = 0;
            if (n < maxResult)
            {
                result.Refs[n] = startNode.Id;
                result.Parents[n] = 0;
                ++n;
            }
            else
            {
                status |= Status.DT_BUFFER_TOO_SMALL;
            }

            while (nstack != 0)
            {
                // Pop front.
                Node curNode = stack[0];
                for (int i = 0; i < nstack - 1; ++i)
                {
                    stack[i] = stack[i + 1];
                }
                nstack--;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var cur = m_nav.GetTileAndPolyByRefUnsafe(curNode.Id);

                for (int i = cur.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = cur.Tile.Links[i].Next)
                {
                    var link = cur.Tile.Links[i];
                    int neighbourRef = link.NRef;
                    // Skip invalid neighbours.
                    if (neighbourRef == 0)
                    {
                        continue;
                    }

                    // Skip if cannot alloca more nodes.
                    var neighbourNode = m_tinyNodePool.GetNode(neighbourRef, 0);
                    if (neighbourNode == null)
                    {
                        continue;
                    }
                    // Skip visited.
                    if (neighbourNode.IsClosed)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    var neighbour = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);

                    // Skip off-mesh connections.
                    if (neighbour.Poly.Type == PolyTypes.OffmeshConnection)
                    {
                        continue;
                    }

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbour.Poly.Flags))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    if (GetPortalPoints(cur, neighbour, out var va, out var vb).HasFlag(Status.DT_FAILURE))
                    {
                        continue;
                    }

                    // If the circle is not touching the next polygon, skip it.
                    float distSqr = Utils.DistancePtSegSqr2D(centerPos, va, vb, out _);
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    // Mark node visited, this is done before the overlap test so that
                    // we will not visit the poly again if the test fails.
                    neighbourNode.Flags |= NodeFlagTypes.Closed;
                    neighbourNode.PIdx = m_tinyNodePool.GetNodeIdx(curNode);

                    // Check that the polygon does not collide with existing polygons.

                    // Collect vertices of the neighbour poly.
                    int npa = neighbour.Poly.VertCount;
                    for (int k = 0; k < npa; ++k)
                    {
                        pa[k] = neighbour.Tile.Verts[neighbour.Poly.Verts[k]];
                    }

                    bool overlap = false;
                    for (int j = 0; j < n; ++j)
                    {
                        int pastRef = result.Refs[j];

                        // Connected polys do not overlap.
                        bool connected = false;
                        for (int k = cur.Poly.FirstLink; k != MeshTile.DT_NULL_LINK; k = cur.Tile.Links[k].Next)
                        {
                            if (cur.Tile.Links[k].NRef == pastRef)
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
                        int npb = past.Poly.VertCount;
                        for (int k = 0; k < npb; ++k)
                        {
                            pb[k] = past.Tile.Verts[past.Poly.Verts[k]];
                        }

                        if (Utils.OverlapPolyPoly2D(pa, npa, pb, npb))
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (overlap) continue;

                    // This poly is fine, store and advance to the poly.
                    if (n < maxResult)
                    {
                        result.Refs[n] = neighbourRef;
                        result.Parents[n] = cur.Ref;
                        ++n;
                    }
                    else
                    {
                        status |= Status.DT_BUFFER_TOO_SMALL;
                    }

                    if (nstack < MAX_STACK)
                    {
                        stack[nstack++] = neighbourNode;
                    }
                }
            }

            result.Count = n;

            return status;
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

            int MAX_STACK = 48;
            var stack = new List<Node>(MAX_STACK);

            m_tinyNodePool.Clear();

            var startNode = m_tinyNodePool.GetNode(startRef, 0);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = NodeFlagTypes.Closed;

            stack.Add(startNode);

            var bestPos = startPos;
            float bestDist = float.MaxValue;
            Node bestNode = null;

            // Search constraints
            var searchPos = Vector3.Lerp(startPos, endPos, 0.5f);
            float searchRadSqr = (float)Math.Pow(Vector3.Distance(startPos, endPos) / 2.0f + 0.001f, 2);

            while (stack.Count != 0)
            {
                var curNode = stack[0];

                // Pop front.
                stack.RemoveAt(0);

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var cur = m_nav.GetTileAndPolyByRefUnsafe(curNode.Id);

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
                    int MAX_NEIS = 8;
                    var neis = new List<int>(MAX_NEIS);

                    if (cur.Poly.NeighbourIsExternalLink(j))
                    {
                        // Tile border.
                        for (int k = cur.Poly.FirstLink; k != MeshTile.DT_NULL_LINK; k = cur.Tile.Links[k].Next)
                        {
                            Link link = cur.Tile.Links[k];
                            if (link.Edge == j && link.NRef != 0)
                            {
                                var nei = m_nav.GetTileAndPolyByRefUnsafe(link.NRef);
                                if (filter.PassFilter(nei.Poly.Flags) && neis.Count < MAX_NEIS)
                                {
                                    neis.Add(link.NRef);
                                }
                            }
                        }
                    }
                    else if (cur.Poly.Neis[j] != 0)
                    {
                        int idx = cur.Poly.Neis[j] - 1;
                        int r = m_nav.GetTileRef(cur.Tile) | idx;
                        if (filter.PassFilter(cur.Tile.Polys[idx].Flags))
                        {
                            // Internal edge, encode id.
                            neis.Add(r);
                        }
                    }

                    if (neis.Count == 0)
                    {
                        // Wall edge, calc distance.
                        var vj = verts[j];
                        var vi = verts[i];
                        float distSqr = Utils.DistancePtSegSqr2D(endPos, vj, vi, out float tseg);
                        if (distSqr < bestDist)
                        {
                            // Update nearest distance.
                            bestPos = Vector3.Lerp(vj, vi, tseg);
                            bestDist = distSqr;
                            bestNode = curNode;
                        }
                    }
                    else
                    {
                        foreach (var nei in neis)
                        {
                            // Skip if no node can be allocated.
                            var neighbourNode = m_tinyNodePool.GetNode(nei, 0);
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
                            var vj = verts[j];
                            var vi = verts[i];
                            float distSqr = Utils.DistancePtSegSqr2D(searchPos, vj, vi, out _);
                            if (distSqr > searchRadSqr)
                            {
                                continue;
                            }

                            // Mark as the node as visited and push to queue.
                            if (stack.Count < MAX_STACK)
                            {
                                neighbourNode.PIdx = m_tinyNodePool.GetNodeIdx(curNode);
                                neighbourNode.Flags |= NodeFlagTypes.Closed;
                                stack.Add(neighbourNode);
                            }
                        }
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
        /// Casts a 'walkability' ray along the surface of the navigation mesh from the start position toward the end position.
        /// </summary>
        /// <param name="request">Ray cast request</param>
        /// <param name="hit">Pointer to a raycast hit structure which will be filled by the results.</param>
        public Status Raycast(RaycastRequest request, out RaycastHit hit)
        {
            hit = new()
            {
                HitNormal = Vector3.Zero,
                MaxPath = request.MaxPath,
                T = 0,
                PathCost = 0
            };

            // Validate input
            if (!request.IsValid(m_nav))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            var startPos = request.StartPos;
            var endPos = request.EndPos;
            var filter = request.Filter;
            bool useCosts = request.Options.HasFlag(RaycastOptions.DT_RAYCAST_USE_COSTS);

            var curPos = startPos;
            var dir = Vector3.Subtract(endPos, startPos);

            Status status = Status.DT_SUCCESS;
            Vector3 lastPos;
            int n = 0;

            // The API input has been checked already, skip checking internal data.
            var (cur, prev, next) = request.GetTiles(m_nav);

            while (cur.Ref != 0)
            {
                // Cast ray against current polygon.
                next.Poly = cur.Poly;

                // Collect vertices.
                var verts = cur.Tile.GetPolyVerts(cur.Poly);

                if (!Utils.IntersectSegmentPoly2D(startPos, endPos, verts, out _, out float tmax, out _, out int segMax))
                {
                    // Could not hit the polygon, keep the old t and report hit.
                    hit.Cut(n);

                    return status;
                }

                if (!hit.PrepareHitData(ref n, cur, tmax, segMax))
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }

                // Ray end is completely inside the polygon.
                if (segMax == -1)
                {
                    hit.T = float.MaxValue;
                    hit.Cut(n);

                    // add the cost
                    if (useCosts)
                    {
                        hit.PathCost += filter.GetCost(curPos, endPos, prev, cur, cur);
                    }

                    return status;
                }

                // Follow neighbours.
                next.Ref = 0;

                RayCastLinks(cur, filter, startPos, endPos, tmax, segMax, ref next);

                // add the cost
                if (useCosts)
                {
                    // compute the intersection point at the furthest end of the polygon
                    // and correct the height (since the raycast moves in 2d)
                    lastPos = curPos;
                    curPos = Vector3.Add(startPos, dir) * hit.T;
                    curPos.Y = CalculateHeight(curPos, verts, segMax);

                    hit.PathCost += filter.GetCost(lastPos, curPos, prev, cur, next);
                }

                if (next.Ref == 0)
                {
                    // No neighbour, we hit a wall.

                    // Calculate hit normal.
                    hit.HitNormal = CalculateHitNormal(verts, segMax);
                    hit.Cut(n);

                    return status;
                }

                // No hit, advance to neighbour polygon.
                prev = cur;
                cur = next;

                // Maintain reference
                hit.PrevReference = prev.Ref;
            }

            hit.Cut(n);

            return status;
        }
        /// <summary>
        /// Calculates the position height
        /// </summary>
        /// <param name="pos">Position (2D)</param>
        /// <param name="verts">Vertex list</param>
        /// <param name="index">Vertex index</param>
        /// <returns>Returns the Y value</returns>
        private static float CalculateHeight(Vector3 pos, Vector3[] verts, int index)
        {
            var e1 = verts[index];
            var e2 = verts[(index + 1) % verts.Length];
            var eDir = Vector3.Subtract(e2, e1);
            var diff = Vector3.Subtract(pos, e1);
            float s = (eDir.X * eDir.X) > (eDir.Z * eDir.Z) ? diff.X / eDir.X : diff.Z / eDir.Z;
            return e1.Y + eDir.Y * s;
        }
        /// <summary>
        /// Calculates the hit normal
        /// </summary>
        /// <param name="verts">Vertex list</param>
        /// <param name="index">Vertex index</param>
        /// <returns>Returns the hit normal (XZ)</returns>
        private static Vector3 CalculateHitNormal(Vector3[] verts, int index)
        {
            var e1 = verts[index];
            var e2 = verts[(index + 1) % verts.Length];
            float dx = e2.X - e1.X;
            float dz = e2.Z - e1.Z;
            return Vector3.Normalize(new(dz, 0, -dx));
        }
        /// <summary>
        /// Ray cast to the linked tiles
        /// </summary>
        /// <param name="cur">Tile to iterate links</param>
        /// <param name="filter">Query filter</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="tmax">Maximum distance</param>
        /// <param name="segMax">Maximum segment</param>
        /// <param name="next">Updates the next tile to test</param>
        private void RayCastLinks(TileRef cur, QueryFilter filter, Vector3 startPos, Vector3 endPos, float tmax, int segMax, ref TileRef next)
        {
            for (int i = cur.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = cur.Tile.Links[i].Next)
            {
                var link = cur.Tile.Links[i];

                // Find link which contains this edge.
                if (link.Edge != segMax)
                {
                    continue;
                }

                // Get pointer to the next polygon.
                next = m_nav.GetTileAndPolyByRefUnsafe(link.NRef);

                // Skip off-mesh connections.
                if (next.Poly.Type == PolyTypes.OffmeshConnection)
                {
                    continue;
                }

                // Skip links based on filter.
                if (!filter.PassFilter(next.Poly.Flags))
                {
                    continue;
                }

                // If the link is internal, just return the ref.
                if (link.Side == 0xff)
                {
                    next.Ref = link.NRef;
                    break;
                }

                // If the link is at tile boundary,

                // Check if the link spans the whole edge, and accept.
                if (link.ExcedBoundaries())
                {
                    next.Ref = link.NRef;
                    break;
                }

                // Check for partial edge links.
                if (!CheckEdgeLinks(link, cur, startPos, endPos, tmax))
                {
                    next.Ref = link.NRef;
                    break;
                }
            }
        }
        /// <summary>
        /// Checks edge links
        /// </summary>
        /// <param name="link">Link</param>
        /// <param name="cur">Tile</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="tmax">Maximum distance</param>
        private static bool CheckEdgeLinks(Link link, TileRef cur, Vector3 startPos, Vector3 endPos, float tmax)
        {
            // Check for partial edge links.
            int v0 = cur.Poly.Verts[link.Edge];
            int v1 = cur.Poly.Verts[(link.Edge + 1) % cur.Poly.VertCount];
            var left = cur.Tile.Verts[v0];
            var right = cur.Tile.Verts[v1];

            // Check that the intersection lies inside the link portal.
            if (link.Side == 0 || link.Side == 4)
            {
                // Calculate link size.
                const float s = 1.0f / 255.0f;
                float lmin = left.Z + (right.Z - left.Z) * (link.BMin * s);
                float lmax = left.Z + (right.Z - left.Z) * (link.BMax * s);
                if (lmin > lmax)
                {
                    Helper.Swap(ref lmin, ref lmax);
                }

                // Find Z intersection.
                float z = startPos.Z + (endPos.Z - startPos.Z) * tmax;
                if (z >= lmin && z <= lmax)
                {
                    return false;
                }
            }
            else if (link.Side == 2 || link.Side == 6)
            {
                // Calculate link size.
                const float s = 1.0f / 255.0f;
                float lmin = left.X + (right.X - left.X) * (link.BMin * s);
                float lmax = left.X + (right.X - left.X) * (link.BMax * s);
                if (lmin > lmax)
                {
                    Helper.Swap(ref lmin, ref lmax);
                }

                // Find X intersection.
                float x = startPos.X + (endPos.X - startPos.X) * tmax;
                if (x >= lmin && x <= lmax)
                {
                    return false;
                }
            }

            return true;
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
        public Status FindDistanceToWall(int startRef, Vector3 centerPos, float maxRadius, QueryFilter filter, out float hitDist, out Vector3 hitPos, out Vector3 hitNormal)
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

            var startNode = m_nodePool.GetNode(startRef, 0);
            startNode.Pos = centerPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
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
                var best = m_nav.GetTileAndPolyByRefUnsafe(bestNode.Id);

                // Get parent poly and tile.
                int parentRef = 0;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }

                // Hit test walls.
                hitPos = HitTestWalls(best, filter, centerPos, radiusSqr);

                // Process the links
                status = ProcessLinksFindDistance(best, parentRef, centerPos, radiusSqr, filter);
            }

            // Calc hit normal.
            hitNormal = Vector3.Subtract(centerPos, hitPos);
            hitNormal.Normalize();

            hitDist = (float)Math.Sqrt(radiusSqr);

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
        private Vector3 HitTestWalls(TileRef best, QueryFilter filter, Vector3 centerPos, float radiusSqr)
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
        private bool BorderIsSolid(TileRef best, int j, QueryFilter filter)
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
        private Status ProcessLinksFindDistance(TileRef best, int parentRef, Vector3 centerPos, float radiusSqr, QueryFilter filter)
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
        private Status ProcessLinksNeighbourFindDistance(Link link, TileRef best, Vector3 centerPos, float radiusSqr, QueryFilter filter)
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

            neighbour.Node = m_nodePool.GetNode(neighbour.Ref, 0);
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
                var midPointRes = GetEdgeMidPoint(best, neighbour, out var pos);
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

            neighbour.Node.Id = neighbour.Ref;
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
        public Status GetPolyWallSegments(int r, QueryFilter filter, int maxSegments, out Segment[] segmentsRes)
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
                if (cur.Poly.NeighbourIsExternalLink(i))
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
        private void SkipExternalLink(QueryFilter filter, int j, TileRef cur, List<SegInterval> ints, int maxInterval)
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
        private bool SkipInternalEdge(QueryFilter filter, int j, TileRef cur, Vector3 vi, Vector3 vj, List<Segment> segments, int maxSegments)
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

        /// <summary>
        /// Returns random location on navmesh.
        /// </summary>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="randomRef">The reference id of the random location.</param>
        /// <param name="randomPt">The random location. </param>
        /// <returns>The status flags for the query.</returns>
        /// <remarks>
        /// Polygons are chosen weighted by area. The search runs in linear related to number of polygon.
        /// </remarks>
        public Status FindRandomPoint(QueryFilter filter, out int randomRef, out Vector3 randomPt)
        {
            randomRef = -1;
            randomPt = Vector3.Zero;

            if (filter == null)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Randomly pick one tile. Assume that all tiles cover roughly the same area.
            var tile = PickTile();
            if (tile == null)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick one polygon weighted by polygon area.
            Poly bestPoly = null;
            int bestPolyRef = 0;
            int bse = m_nav.GetTileRef(tile);

            float areaSum = 0.0f;
            var polys = tile
                .GetPolys()
                .Where(p => p.Type != PolyTypes.OffmeshConnection && filter.PassFilter(p.Flags))
                .ToArray();

            for (int i = 0; i < polys.Length; ++i)
            {
                var poly = polys[i];
                int r = bse | i;

                // Calc area of the polygon.
                float polyArea = tile.GetPolyArea(poly);

                // Choose random polygon weighted by area, using reservoi sampling.
                areaSum += polyArea;
                float u = Helper.RandomGenerator.NextFloat(0, 1);
                if (u * areaSum <= polyArea)
                {
                    bestPoly = poly;
                    bestPolyRef = r;
                }
            }

            if (bestPoly == null)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick point on polygon.
            var verts = tile.GetPolyVerts(bestPoly);

            var pt = Utils.RandomPointInConvexPoly(verts);

            Status status = GetPolyHeight(bestPolyRef, pt, out float h);
            if (status.HasFlag(Status.DT_FAILURE))
            {
                return status;
            }
            pt.Y = h;

            randomPt = pt;
            randomRef = bestPolyRef;

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Pick random tile
        /// </summary>
        /// <returns>Returns the picked tile</returns>
        public MeshTile PickTile()
        {
            MeshTile tile = null;
            float areaSum = 0.0f;

            for (int i = 0; i < m_nav.MaxTiles; i++)
            {
                var tl = m_nav.Tiles[i];
                if (tl == null || !tl.Header.IsValid())
                {
                    continue;
                }

                // Choose random tile using reservoi sampling.
                float area = 1.0f; // Could be tile area too.
                areaSum += area;
                float u = Helper.RandomGenerator.NextFloat(0, 1);
                if (u * areaSum <= area)
                {
                    tile = tl;
                }
            }

            return tile;
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

            var startNode = m_nodePool.GetNode(startRef, 0);
            startNode.Pos = centerPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = NodeFlagTypes.Open;

            m_openList.Push(startNode);

            float radiusSqr = maxRadius * maxRadius;
            var random = new TileRef();
            float areaSum = 0.0f;

            while (!m_openList.Empty())
            {
                var bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                var best = m_nav.GetTileAndPolyByNodeUnsafe(bestNode);

                random = SelectBestTile(best, random, ref areaSum);

                EvaluateTiles(best, centerPos, radiusSqr, filter);
            }

            if (random.Ref == TileRef.Null.Ref)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick point on polygon.
            var verts = random.Tile.GetPolyVerts(random.Poly);

            var pt = Utils.RandomPointInConvexPoly(verts);

            Status stat = GetPolyHeight(random.Ref, pt, out float h);
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
        /// Evaluates the tile list
        /// </summary>
        /// <param name="best">Best tile</param>
        /// <param name="centerPos">Center position</param>
        /// <param name="radiusSqr">Squared radius</param>
        /// <param name="filter">Query filter</param>
        private void EvaluateTiles(TileRef best, Vector3 centerPos, float radiusSqr, QueryFilter filter)
        {
            // Get parent poly and tile.
            int parentRef = m_nodePool.GetNodeAtIdx(best.Node.PIdx)?.Id ?? 0;

            for (int i = best.Poly.FirstLink; i != MeshTile.DT_NULL_LINK; i = best.Tile.Links[i].Next)
            {
                var link = best.Tile.Links[i];
                int neighbourRef = link.NRef;

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
                if (GetPortalPoints(best, neighbour, out Vector3 va, out Vector3 vb).HasFlag(Status.DT_FAILURE))
                {
                    continue;
                }

                // If the circle is not touching the next polygon, skip it.
                float distSqr = Utils.DistancePtSegSqr2D(centerPos, va, vb, out _);
                if (distSqr > radiusSqr)
                {
                    continue;
                }

                var neighbourNode = m_nodePool.GetNode(neighbour.Ref, 0);
                if (neighbourNode == null)
                {
                    continue;
                }

                if (neighbourNode.IsClosed)
                {
                    continue;
                }

                neighbour.Node = neighbourNode;

                EvaluateTile(neighbour, best, va, vb);
            }
        }
        /// <summary>
        /// Evaluates the tile
        /// </summary>
        /// <param name="neighbour">Neighbour tile</param>
        /// <param name="best">Best tile</param>
        /// <param name="va">Segment A position</param>
        /// <param name="vb">Segment B position</param>
        private void EvaluateTile(TileRef neighbour, TileRef best, Vector3 va, Vector3 vb)
        {
            // Cost
            if (neighbour.Node.Flags == NodeFlagTypes.None)
            {
                neighbour.Node.Pos = Vector3.Lerp(va, vb, 0.5f);
            }

            float total = best.Node.Total + Vector3.Distance(best.Node.Pos, neighbour.Node.Pos);

            // The node is already in open list and the new result is worse, skip.
            if (neighbour.Node.IsOpen && total >= neighbour.Node.Total)
            {
                return;
            }

            neighbour.Node.Id = neighbour.Ref;
            neighbour.Node.Flags = (neighbour.Node.Flags & ~NodeFlagTypes.Closed);
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
        /// Finds the closest point on the specified polygon.
        /// </summary>
        /// <param name="r">The reference id of the polygon.</param>
        /// <param name="pos">The position to check.</param>
        /// <param name="closest">The closest point on the polygon.</param>
        /// <returns>The status flags for the query.</returns>
        public Status ClosestPointOnPoly(int r, Vector3 pos, out Vector3 closest)
        {
            return ClosestPointOnPoly(r, pos, out closest, out _);
        }
        /// <summary>
        /// Finds the closest point on the specified polygon.
        /// </summary>
        /// <param name="r">The reference id of the polygon.</param>
        /// <param name="pos">The position to check.</param>
        /// <param name="closest">The closest point on the polygon.</param>
        /// <param name="posOverPoly">True of the position is over the polygon.</param>
        /// <returns>The status flags for the query.</returns>
        public Status ClosestPointOnPoly(int r, Vector3 pos, out Vector3 closest, out bool posOverPoly)
        {
            closest = Vector3.Zero;
            posOverPoly = false;

            if (!m_nav.IsValidPolyRef(r) || pos.IsInfinity())
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            m_nav.ClosestPointOnPoly(r, pos, out closest, out posOverPoly);

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Returns a point on the boundary closest to the source point if the source point is outside the polygon's xz-bounds.
        /// </summary>
        /// <param name="r">The reference id to the polygon.</param>
        /// <param name="pos">The position to check.</param>
        /// <param name="closest">The closest point.</param>
        /// <returns>The status flags for the query.</returns>
        public Status ClosestPointOnPolyBoundary(int r, Vector3 pos, out Vector3 closest)
        {
            closest = new Vector3();

            var cur = m_nav.GetTileAndPolyByRef(r);
            if (cur.Ref == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }
            if (pos.IsInfinity())
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Collect vertices.
            var verts = cur.Tile.GetPolyVerts(cur.Poly);

            bool inside = Utils.PointInPolygon2D(pos, verts, out var edged, out var edget);
            if (inside)
            {
                // Point is inside the polygon, return the point.
                closest = pos;
            }
            else
            {
                // Point is outside the polygon, dtClamp to nearest edge.
                closest = Utils.ClosestPointOutsidePoly(verts, edged, edget);
            }

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Gets the height of the polygon at the provided position using the height detail. (Most accurate.)
        /// </summary>
        /// <param name="r">The reference id of the polygon.</param>
        /// <param name="pos">A position within the xz-bounds of the polygon.</param>
        /// <param name="height">The height at the surface of the polygon.</param>
        /// <returns>The status flags for the query.</returns>
        public Status GetPolyHeight(int r, Vector3 pos, out float height)
        {
            height = 0;

            var cur = m_nav.GetTileAndPolyByRef(r);
            if (cur.Ref == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }
            if (pos.XZ().IsInfinity())
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // We used to return success for offmesh connections, but the
            // getPolyHeight in DetourNavMesh does not do this, so special
            // case it here.
            if (cur.Poly.Type == PolyTypes.OffmeshConnection)
            {
                var v0 = cur.Tile.Verts[cur.Poly.Verts[0]];
                var v1 = cur.Tile.Verts[cur.Poly.Verts[1]];
                Utils.DistancePtSegSqr2D(pos, v0, v1, out float t);
                height = v0.Y + (v1.Y - v0.Y) * t;
                return Status.DT_SUCCESS;
            }

            return cur.Tile.GetPolyHeight(cur.Poly, pos, out height) ?
                Status.DT_SUCCESS :
                Status.DT_FAILURE | Status.DT_INVALID_PARAM;
        }
        /// <summary>
        /// Returns true if the polygon reference is valid and passes the filter restrictions.
        /// </summary>
        /// <param name="r">The polygon reference to check.</param>
        /// <param name="filter">The filter to apply.</param>
        /// <returns>Returns true if the polygon reference is valid and passes the filter restrictions.</returns>
        public bool IsValidPolyRef(int r, QueryFilter filter)
        {
            var cur = m_nav.GetTileAndPolyByRef(r);
            if (cur.Ref == 0)
            {
                // If cannot get polygon, assume it does not exists and boundary is invalid.
                return false;
            }
            if (!filter.PassFilter(cur.Poly.Flags))
            {
                // If cannot pass filter, assume flags has changed and boundary is invalid.
                return false;
            }
            return true;
        }
        /// <summary>
        ///  Returns true if the polygon reference is in the closed list. 
        /// </summary>
        /// <param name="r">The reference id of the polygon to check.</param>
        /// <returns>True if the polygon is in closed list.</returns>
        public bool IsInClosedList(int r)
        {
            if (m_nodePool == null) return false;

            int n = m_nodePool.FindNodes(r, DT_MAX_STATES_PER_NODE, out Node[] nodes);

            for (int i = 0; i < n; i++)
            {
                if (nodes[i].IsClosed)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets the node pool.
        /// </summary>
        /// <returns>The node pool.</returns>
        public NodePool GetNodePool() { return m_nodePool; }
        /// <summary>
        /// Gets the navigation mesh the query object is using.
        /// </summary>
        /// <returns>The navigation mesh the query object is using.</returns>
        public NavMesh GetAttachedNavMesh() { return m_nav; }

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
        /// <summary>
        /// Returns portal points between two polygons.
        /// </summary>
        private Status GetPortalPoints(int from, int to, out Vector3 left, out Vector3 right, out PolyTypes fromType, out PolyTypes toType)
        {
            left = new();
            right = new();
            fromType = PolyTypes.Ground;
            toType = PolyTypes.Ground;

            var fromT = m_nav.GetTileAndPolyByRef(from);
            if (fromT.Ref == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            fromType = fromT.Poly.Type;

            var toT = m_nav.GetTileAndPolyByRef(to);
            if (toT.Ref == 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            toType = toT.Poly.Type;

            return GetPortalPoints(fromT, toT, out left, out right);
        }
        /// <summary>
        /// Appends intermediate portal points to a straight path.
        /// </summary>
        private Status AppendPortals(int startIdx, int endIdx, Vector3 endPos, int[] path, int maxStraightPath, StraightPathOptions options, ref StraightPath straightPath)
        {
            var startPos = straightPath.EndPath;

            // Append or update last vertex
            for (int i = startIdx; i < endIdx; i++)
            {
                // Calculate portal
                int from = path[i];
                var fromT = m_nav.GetTileAndPolyByRef(from);
                if (fromT.Ref == 0)
                {
                    return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
                }

                int to = path[i + 1];
                var toT = m_nav.GetTileAndPolyByRef(to);
                if (toT.Ref == 0)
                {
                    return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
                }

                if (GetPortalPoints(fromT, toT, out var left, out var right).HasFlag(Status.DT_FAILURE))
                {
                    break;
                }

                if (options.HasFlag(StraightPathOptions.AreaCrossings) && fromT.Poly.Area == toT.Poly.Area)
                {
                    // Skip intersection if only area crossings are requested.
                    continue;
                }

                // Append intersection
                if (Utils.IntersectSegments2D(startPos, endPos, left, right, out _, out float t))
                {
                    var pt = Vector3.Lerp(left, right, t);

                    var stat = straightPath.AppendVertex(pt, 0, path[i + 1], maxStraightPath);
                    if (stat != Status.DT_IN_PROGRESS)
                    {
                        return stat;
                    }
                }
            }
            return Status.DT_IN_PROGRESS;
        }
        /// <summary>
        /// Gets the path leading to the specified end node.
        /// </summary>
        private Status GetPathToNode(Node endNode, int maxPath, out SimplePath path)
        {
            path = new(maxPath);

            // Find the length of the entire path.
            Node curNode = endNode;
            int length = 0;
            do
            {
                length++;
                curNode = m_nodePool.GetNodeAtIdx(curNode.PIdx);
            }
            while (curNode != null);

            // If the path cannot be fully stored then advance to the last node we will be able to store.
            curNode = endNode;
            int writeCount;
            for (writeCount = length; writeCount > maxPath; writeCount--)
            {
                curNode = m_nodePool.GetNodeAtIdx(curNode.PIdx);
            }

            // Write path
            List<int> tmp = [];
            for (int i = writeCount - 1; i >= 0; i--)
            {
                tmp.Insert(0, curNode.Id);
                curNode = m_nodePool.GetNodeAtIdx(curNode.PIdx);
            }

            path.Clear();
            path.StartPath(tmp);

            if (length > maxPath)
            {
                return Status.DT_SUCCESS | Status.DT_BUFFER_TOO_SMALL;
            }

            return Status.DT_SUCCESS;
        }

        /// <summary>
        /// Calcs a path
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="polyPickExt">Extensions</param>
        /// <param name="mode">Path mode</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="resultPath">Result path</param>
        /// <returns>Returns the status of the path calculation</returns>
        public Status CalcPath(QueryFilter filter, Vector3 polyPickExt, PathFindingMode mode, Vector3 startPos, Vector3 endPos, out IEnumerable<Vector3> resultPath)
        {
            resultPath = null;

            FindNearestPoly(startPos, polyPickExt, filter, out int startRef, out _);
            FindNearestPoly(endPos, polyPickExt, filter, out int endRef, out _);

            var endPointsDefined = startRef != 0 && endRef != 0;
            if (!endPointsDefined)
            {
                return Status.DT_FAILURE;
            }

            PathPoint start = new() { Ref = startRef, Pos = startPos };
            PathPoint end = new() { Ref = endRef, Pos = endPos };

            if (mode == PathFindingMode.Follow)
            {
                if (CalcPathFollow(filter, start, end, out var path))
                {
                    resultPath = path;

                    return Status.DT_SUCCESS;
                }
            }
            else if (mode == PathFindingMode.Straight)
            {
                if (CalcPathStraigh(filter, start, end, out var path))
                {
                    resultPath = path;

                    return Status.DT_SUCCESS;
                }
            }
            else if (mode == PathFindingMode.Sliced)
            {
                var status = InitSlicedFindPath(filter, start, end);
                if (status != Status.DT_SUCCESS)
                {
                    return status;
                }

                return UpdateSlicedFindPath(20, out _);
            }

            return Status.DT_FAILURE;
        }
        /// <summary>
        /// Calculates the result path
        /// </summary>
        /// <param name="filter">Query filter</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="resultPath">Resulting path</param>
        private bool CalcPathFollow(QueryFilter filter, PathPoint start, PathPoint end, out List<Vector3> resultPath)
        {
            resultPath = null;

            FindPath(start, end, filter, MAX_POLYS, out var iterPath);
            if (iterPath.Count <= 0)
            {
                return false;
            }

            // Iterate over the path to find smooth path on the detail mesh surface.
            ClosestPointOnPoly(start.Ref, start.Pos, out var iterPos, out _);
            ClosestPointOnPoly(iterPath.End, end.Pos, out var targetPos, out _);

            List<Vector3> smoothPath = [iterPos];

            // Move towards target a small advancement at a time until target reached or
            // when ran out of memory to store the path.
            while (iterPath.Count != 0 && smoothPath.Count < MAX_SMOOTH)
            {
                if (IterPathFollow(filter, targetPos, smoothPath, iterPath, ref iterPos))
                {
                    //End reached
                    break;
                }
            }

            resultPath = smoothPath;

            return smoothPath.Count > 0;
        }
        /// <summary>
        /// Smooths the path
        /// </summary>
        /// <param name="filter">Query filter</param>
        /// <param name="targetPos">Target position</param>
        /// <param name="smoothPath">Smooth path</param>
        /// <param name="iterPath">Path to iterate</param>
        /// <param name="iterPos">Current iteration position</param>
        private bool IterPathFollow(QueryFilter filter, Vector3 targetPos, List<Vector3> smoothPath, SimplePath iterPath, ref Vector3 iterPos)
        {
            float SLOP = 0.01f;

            // Find location to steer towards.
            if (!GetSteerTarget(iterPos, targetPos, SLOP, iterPath, out var target))
            {
                return true;
            }

            bool endOfPath = (target.Flag & StraightPathFlagTypes.DT_STRAIGHTPATH_END) != 0;
            bool offMeshConnection = (target.Flag & StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0;

            // Find movement delta.
            Vector3 moveTgt = FindMovementDelta(iterPos, target.Position, endOfPath || offMeshConnection);

            // Move
            MoveAlongSurface(
                iterPath.Start, iterPos, moveTgt, filter, 16,
                out var result, out var visited);

            SimplePath.FixupCorridor(iterPath, visited);
            SimplePath.FixupShortcuts(iterPath, this);

            GetPolyHeight(iterPath.Start, result, out float h);
            result.Y = h;
            iterPos = result;

            bool inRange = Utils.InRange(iterPos, target.Position, SLOP, 1.0f);
            if (!inRange)
            {
                // Store results.
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return false;
            }

            // Handle end of path and off-mesh links when close enough.
            if (endOfPath)
            {
                // Reached end of path.
                iterPos = targetPos;

                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return true;
            }

            if (offMeshConnection)
            {
                // Reached off-mesh connection.
                HandleOffMeshConnection(target, smoothPath, iterPath, ref iterPos);

                // Store results.
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return false;
            }

            // Store results.
            if (smoothPath.Count < MAX_SMOOTH)
            {
                smoothPath.Add(iterPos);
            }

            return false;
        }
        /// <summary>
        /// Handle off-mesh connection
        /// </summary>
        /// <param name="target">Target position</param>
        /// <param name="smoothPath">Smooth path</param>
        /// <param name="iterPath">Path to iterate</param>
        /// <param name="iterPos">Current iteration position</param>
        private void HandleOffMeshConnection(SteerTarget target, List<Vector3> smoothPath, SimplePath iterPath, ref Vector3 iterPos)
        {
            // Advance the path up to and over the off-mesh connection.
            int prevRef = 0;
            int polyRef = iterPath.Start;
            int npos = 0;
            var iterNodes = iterPath.GetPath();
            while (npos < iterPath.Count && polyRef != target.Ref)
            {
                prevRef = polyRef;
                polyRef = iterNodes[npos];
                npos++;
            }
            iterPath.Prune(npos);

            // Handle the connection.
            if (GetAttachedNavMesh().GetOffMeshConnectionPolyEndPoints(prevRef, polyRef, out var sPos, out var ePos))
            {
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(sPos);
                }

                // Move position at the other side of the off-mesh link.
                iterPos = ePos;
                GetPolyHeight(iterPath.Start, iterPos, out float eh);
                iterPos.Y = eh;
            }
        }
        /// <summary>
        /// Calculates straigh path
        /// </summary>
        /// <param name="filter">Query filter</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="resultPath">Resulting path</param>
        private bool CalcPathStraigh(QueryFilter filter, PathPoint start, PathPoint end, out Vector3[] resultPath)
        {
            FindPath(start, end, filter, MAX_POLYS, out var polys);
            if (polys.Count < 0)
            {
                resultPath = [];

                return false;
            }

            // In case of partial path, make sure the end point is clamped to the last polygon.
            var epos = end.Pos;
            if (polys.End != end.Ref)
            {
                ClosestPointOnPoly(polys.End, end.Pos, out epos, out _);
            }

            FindStraightPath(
                start.Pos, epos, polys,
                MAX_POLYS, StraightPathOptions.AllCrossings,
                out var straightPath);

            resultPath = straightPath.GetPath();

            return straightPath.Count > 0;
        }
        /// <summary>
        /// Gets a steer target
        /// </summary>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="minTargetDist">Miminum tangent distance</param>
        /// <param name="path">Current path</param>
        /// <param name="target">Out target</param>
        private bool GetSteerTarget(Vector3 startPos, Vector3 endPos, float minTargetDist, SimplePath path, out SteerTarget target)
        {
            target = new SteerTarget
            {
                Position = Vector3.Zero,
                Flag = 0,
                Ref = 0,
                Points = null,
                PointCount = 0
            };

            // Find steer target.
            int MAX_STEER_POINTS = 3;
            FindStraightPath(
                startPos, endPos, path,
                MAX_STEER_POINTS, StraightPathOptions.None,
                out var steerPath);

            if (steerPath.Count == 0)
            {
                return false;
            }

            target.PointCount = steerPath.Count;
            target.Points = steerPath.GetPath();

            // Find vertex far enough to steer to.
            int ns = 0;
            while (ns < steerPath.Count)
            {
                // Stop at Off-Mesh link or when point is further than slop away.
                if ((steerPath.GetFlag(ns) & StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
                    !Utils.InRange(steerPath.GetPathPosition(ns), startPos, minTargetDist, 1000.0f))
                {
                    break;
                }
                ns++;
            }
            // Failed to find good point to steer to.
            if (ns >= steerPath.Count)
            {
                return false;
            }

            var pos = steerPath.GetPathPosition(ns);
            pos.Y = startPos.Y;

            target.Position = pos;
            target.Flag = steerPath.GetFlag(ns);
            target.Ref = steerPath.GetRef(ns);

            return true;
        }
    }
}
