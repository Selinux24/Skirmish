using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Provides the ability to perform pathfinding related queries against a navigation mesh.
    /// </summary>
    public class NavMeshQuery : IDisposable
    {
        /// <summary>
        /// Navmesh data.
        /// </summary>
        private NavMesh m_nav = null;
        /// <summary>
        /// Node pool.
        /// </summary>
        private NodePool m_nodePool = null;
        /// <summary>
        /// Small node pool.
        /// </summary>
        private NodePool m_tinyNodePool = null;
        /// <summary>
        /// Open list queue.
        /// </summary>
        private NodeQueue m_openList = null;
        /// <summary>
        /// Sliced query state.
        /// </summary>
        private QueryData m_query = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public NavMeshQuery()
        {
            m_query = new QueryData();
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
                if (m_nodePool != null)
                {
                    m_nodePool.Dispose();
                    m_nodePool = null;
                }
                if (m_tinyNodePool != null)
                {
                    m_tinyNodePool.Dispose();
                    m_tinyNodePool = null;
                }
                if (m_openList != null)
                {
                    m_openList.Dispose();
                    m_openList = null;
                }
            }
        }

        /// <summary>
        /// Initializes the query object.
        /// </summary>
        /// <param name="nav">Pointer to the dtNavMesh object to use for all queries.</param>
        /// <param name="maxNodes">Maximum number of search nodes.</param>
        /// <returns>The status flags for the query.</returns>
        public Status Init(NavMesh nav, int maxNodes)
        {
            if (maxNodes > (1 << DetourUtils.DT_NODE_PARENT_BITS) - 1)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
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

            return Status.DT_SUCCESS;
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
        public Status FindPath(int startRef, int endRef, Vector3 startPos, Vector3 endPos, QueryFilter filter, int maxPath, out SimplePath resultPath)
        {
            resultPath = new SimplePath(maxPath);

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) ||
                !m_nav.IsValidPolyRef(endRef) ||
                startPos.IsInfinity() ||
                endPos.IsInfinity() ||
                filter == null ||
                maxPath <= 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            if (startRef == endRef)
            {
                resultPath.StartPath(startRef);
                return Status.DT_SUCCESS;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            var startNode = m_nodePool.GetNode(startRef, 0);
            startNode.Pos = startPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = Vector3.Distance(startPos, endPos) * DetourUtils.H_SCALE;
            startNode.Id = startRef;
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
                if (bestNode.Id == endRef)
                {
                    lastBestNode = bestNode;
                    break;
                }

                // Get current poly and tile.
                // The API input has been cheked already, skip checking internal data.
                int bestRef = bestNode.Id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out MeshTile bestTile, out Poly bestPoly);

                // Get parent poly and tile.
                int parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef != 0)
                {
                    m_nav.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);
                }

                for (int i = bestPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = bestTile.Links[i].Next)
                {
                    int neighbourRef = bestTile.Links[i].NRef;

                    // Skip invalid ids and do not expand back to where we came from.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Get neighbour poly and tile.
                    // The API input has been cheked already, skip checking internal data.
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out MeshTile neighbourTile, out Poly neighbourPoly);

                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // deal explicitly with crossing tile boundaries
                    int crossSide = 0;
                    if (bestTile.Links[i].Side != 0xff)
                    {
                        crossSide = bestTile.Links[i].Side >> 1;
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
                        var midPointRes = GetEdgeMidPoint(
                            bestRef, bestPoly, bestTile,
                            neighbourRef, neighbourPoly, neighbourTile,
                            out var pos);

                        if (midPointRes != Status.DT_SUCCESS)
                        {
                            Console.WriteLine($"FindPath GetEdgeMidPoint result: {midPointRes}");
                            return midPointRes;
                        }

                        neighbourNode.Pos = pos;
                    }

                    // Calculate cost and heuristic.
                    float cost;
                    float heuristic;

                    // Special case for last node.
                    if (neighbourRef == endRef)
                    {
                        // Cost
                        float curCost = filter.GetCost(
                            bestNode.Pos, neighbourNode.Pos,
                            parentRef, parentTile, parentPoly,
                            bestRef, bestTile, bestPoly,
                            neighbourRef, neighbourTile, neighbourPoly);
                        float endCost = filter.GetCost(
                            neighbourNode.Pos, endPos,
                            bestRef, bestTile, bestPoly,
                            neighbourRef, neighbourTile, neighbourPoly,
                            0, null, null);

                        cost = bestNode.Cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        // Cost
                        float curCost = filter.GetCost(
                            bestNode.Pos, neighbourNode.Pos,
                            parentRef, parentTile, parentPoly,
                            bestRef, bestTile, bestPoly,
                            neighbourRef, neighbourTile, neighbourPoly);
                        cost = bestNode.Cost + curCost;
                        heuristic = Vector3.Distance(neighbourNode.Pos, endPos) * DetourUtils.H_SCALE;
                    }

                    float total = cost + heuristic;

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0 && total >= neighbourNode.Total)
                    {
                        continue;
                    }
                    // The node is already visited and process, and the new result is worse, skip.
                    if ((neighbourNode.Flags & NodeFlagTypes.Closed) != 0 && total >= neighbourNode.Total)
                    {
                        continue;
                    }

                    // Add or update the node.
                    neighbourNode.PIdx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.Id = neighbourRef;
                    neighbourNode.Flags = (neighbourNode.Flags & ~NodeFlagTypes.Closed);
                    neighbourNode.Cost = cost;
                    neighbourNode.Total = total;

                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0)
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

            if (lastBestNode.Id != endRef)
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
            resultPath = new StraightPath(maxStraightPath);

            if (startPos.IsInfinity() ||
                endPos.IsInfinity() ||
                path == null || path.Count <= 0 ||
                maxStraightPath <= 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // TODO: Should this be callers responsibility?
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
            var startPStatus = AppendVertex(
                closestStartPos, StraightPathFlagTypes.DT_STRAIGHTPATH_START, path.Start, maxStraightPath,
                ref resultPath);
            if (startPStatus != Status.DT_IN_PROGRESS)
            {
                return startPStatus;
            }

            if (path.Count > 1)
            {
                Vector3 portalApex = closestStartPos;
                Vector3 portalLeft = portalApex;
                Vector3 portalRight = portalApex;
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
                            pathNodes.ElementAt(i),
                            pathNodes.ElementAt(i + 1),
                            out left, out right, out _, out toType);

                        if (ppStatus.HasFlag(Status.DT_FAILURE))
                        {
                            // Failed to get portal points, in practice this means that path[i+1] is invalid polygon.
                            // Clamp the end point to path[i], and return the path so far.

                            var cpBoundaryStatus = ClosestPointOnPolyBoundary(pathNodes.ElementAt(i), endPos, out closestEndPos);
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
                                    apexIndex, i, closestEndPos, pathNodes.ToArray(), maxStraightPath, options,
                                    ref resultPath);
                            }

                            // Ignore status return value as we're just about to return anyway.
                            AppendVertex(
                                closestEndPos, 0, pathNodes.ElementAt(i), maxStraightPath,
                                ref resultPath);

                            return Status.DT_SUCCESS | Status.DT_PARTIAL_RESULT | ((resultPath.Count >= maxStraightPath) ? Status.DT_BUFFER_TOO_SMALL : 0);
                        }

                        // If starting really close the portal, advance.
                        if (i == 0 && DetourUtils.DistancePtSegSqr2D(portalApex, left, right) < (0.001f * 0.001f))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // End of the path.
                        left = closestEndPos;
                        right = closestEndPos;

                        toType = PolyTypes.DT_POLYTYPE_GROUND;
                    }

                    // Right vertex.
                    if (DetourUtils.TriArea2D(portalApex, portalRight, right) <= 0.0f)
                    {
                        if (DetourUtils.Vequal(portalApex, portalRight) || DetourUtils.TriArea2D(portalApex, portalLeft, right) > 0.0f)
                        {
                            portalRight = right;
                            rightPolyRef = (i + 1 < path.Count) ? pathNodes.ElementAt(i + 1) : 0;
                            rightPolyType = toType;
                            rightIndex = i;
                        }
                        else
                        {
                            // Append portals along the current straight path segment.
                            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
                            {
                                var appendStatus = AppendPortals(
                                    apexIndex, leftIndex, portalLeft, pathNodes.ToArray(), maxStraightPath, options,
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
                            else if (leftPolyType == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
                            {
                                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION;
                            }
                            int r = leftPolyRef;

                            // Append or update vertex
                            var stat = AppendVertex(
                                portalApex, flags, r, maxStraightPath,
                                ref resultPath);

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
                    if (DetourUtils.TriArea2D(portalApex, portalLeft, left) >= 0.0f)
                    {
                        if (DetourUtils.Vequal(portalApex, portalLeft) || DetourUtils.TriArea2D(portalApex, portalRight, left) < 0.0f)
                        {
                            portalLeft = left;
                            leftPolyRef = (i + 1 < path.Count) ? pathNodes.ElementAt(i + 1) : 0;
                            leftPolyType = toType;
                            leftIndex = i;
                        }
                        else
                        {
                            // Append portals along the current straight path segment.
                            if ((options & (StraightPathOptions.AreaCrossings | StraightPathOptions.AllCrossings)) != 0)
                            {
                                var appendStatus = AppendPortals(
                                    apexIndex, rightIndex, portalRight, pathNodes.ToArray(), maxStraightPath, options,
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
                            else if (rightPolyType == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
                            {
                                flags = StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION;
                            }
                            int r = rightPolyRef;

                            // Append or update vertex
                            var stat = AppendVertex(
                                portalApex, flags, r, maxStraightPath,
                                ref resultPath);

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
                        apexIndex, path.Count - 1, closestEndPos, pathNodes.ToArray(), maxStraightPath, options,
                        ref resultPath);

                    if (stat != Status.DT_IN_PROGRESS)
                    {
                        return stat;
                    }
                }
            }

            // Ignore status return value as we're just about to return anyway.
            AppendVertex(
                closestEndPos, StraightPathFlagTypes.DT_STRAIGHTPATH_END, 0, maxStraightPath,
                ref resultPath);

            return Status.DT_SUCCESS | ((resultPath.Count >= maxStraightPath) ? Status.DT_BUFFER_TOO_SMALL : 0);
        }
        /// <summary>
        /// Intializes a sliced path query.
        /// </summary>
        /// <param name="startRef">The refrence id of the start polygon.</param>
        /// <param name="endRef">The reference id of the end polygon.</param>
        /// <param name="startPos">A position within the start polygon.</param>
        /// <param name="endPos">A position within the end polygon.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="options">Query options</param>
        /// <returns>The status flags for the query.</returns>
        /// <example>
        /// Common use case:
        /// -# Call InitSlicedFindPath() to initialize the sliced path query.
        /// -# Call UpdateSlicedFindPath() until it returns complete.
        /// -# Call FinalizeSlicedFindPath() to get the path.
        /// </example>
        public Status InitSlicedFindPath(int startRef, int endRef, Vector3 startPos, Vector3 endPos, QueryFilter filter, FindPathOptions options = FindPathOptions.DT_FINDPATH_ANY_ANGLE)
        {
            // Init path state.
            m_query = new QueryData
            {
                Status = 0,
                StartRef = startRef,
                EndRef = endRef,
                StartPos = startPos,
                EndPos = endPos,
                Filter = filter,
                Options = options,
                RaycastLimitSqr = float.MaxValue
            };

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) ||
                !m_nav.IsValidPolyRef(endRef) ||
                startPos.IsInfinity() ||
                endPos.IsInfinity() ||
                filter == null)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // trade quality with performance?
            if ((options & FindPathOptions.DT_FINDPATH_ANY_ANGLE) != 0)
            {
                // limiting to several times the character radius yields nice results. It is not sensitive 
                // so it is enough to compute it from the first tile.
                MeshTile tile = m_nav.GetTileByRef(startRef);
                float agentRadius = tile.Header.WalkableRadius;
                m_query.RaycastLimitSqr = (float)Math.Pow(agentRadius * DetourUtils.DT_RAY_CAST_LIMIT_PROPORTIONS, 2);
            }

            if (startRef == endRef)
            {
                m_query.Status = Status.DT_SUCCESS;
                return Status.DT_SUCCESS;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            Node startNode = m_nodePool.GetNode(startRef, 0);
            startNode.Pos = startPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = Vector3.Distance(startPos, endPos) * DetourUtils.H_SCALE;
            startNode.Id = startRef;
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
                int bestRef = bestNode.Id;
                if (!m_nav.GetTileAndPolyByRef(bestRef, out MeshTile bestTile, out Poly bestPoly))
                {
                    // The polygon has disappeared during the sliced query, fail.
                    m_query.Status = Status.DT_FAILURE;
                    doneIters = iter;
                    return m_query.Status;
                }

                // Get parent and grand parent poly and tile.
                int parentRef = 0;
                int? grandpaRef = null;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                Node parentNode = null;
                if (bestNode.PIdx != 0)
                {
                    parentNode = m_nodePool.GetNodeAtIdx(bestNode.PIdx);
                    parentRef = parentNode.Id;
                    if (parentNode.PIdx != 0)
                    {
                        grandpaRef = m_nodePool.GetNodeAtIdx(parentNode.PIdx).Id;
                    }
                }
                if (parentRef != 0)
                {
                    bool invalidParent = !m_nav.GetTileAndPolyByRef(parentRef, out parentTile, out parentPoly);
                    if (invalidParent || (grandpaRef.HasValue && !m_nav.IsValidPolyRef(grandpaRef.Value)))
                    {
                        // The polygon has disappeared during the sliced query, fail.
                        m_query.Status = Status.DT_FAILURE;
                        doneIters = iter;
                        return m_query.Status;
                    }
                }

                // decide whether to test raycast to previous nodes
                bool tryLOS = false;
                if ((m_query.Options & FindPathOptions.DT_FINDPATH_ANY_ANGLE) != 0 &&
                    (parentNode != null) &&
                    (parentRef != 0) &&
                    (Vector3.DistanceSquared(parentNode.Pos, bestNode.Pos) < m_query.RaycastLimitSqr))
                {
                    tryLOS = true;
                }

                for (int i = bestPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = bestTile.Links[i].Next)
                {
                    int neighbourRef = bestTile.Links[i].NRef;

                    // Skip invalid ids and do not expand back to where we came from.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Get neighbour poly and tile.
                    // The API input has been cheked already, skip checking internal data.
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out MeshTile neighbourTile, out Poly neighbourPoly);

                    if (!m_query.Filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
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

                    // If the node is visited the first time, calculate node position.
                    if (neighbourNode.Flags == NodeFlagTypes.None)
                    {
                        var midPointRes = GetEdgeMidPoint(
                            bestRef, bestPoly, bestTile,
                            neighbourRef, neighbourPoly, neighbourTile,
                            out var pos);

                        if (midPointRes != Status.DT_SUCCESS)
                        {
                            Console.WriteLine($"UpdateSlicedFindPath GetEdgeMidPoint result: {midPointRes}");
                            return midPointRes;
                        }

                        neighbourNode.Pos = pos;
                    }

                    // Calculate cost and heuristic.
                    float cost;
                    float heuristic;

                    // raycast parent
                    bool foundShortCut = false;

                    RaycastHit rayHit = new RaycastHit
                    {
                        MaxPath = 0,
                        PathCost = 0,
                        T = 0,
                    };
                    if (tryLOS)
                    {
                        Raycast(
                            parentRef, parentNode.Pos, neighbourNode.Pos, m_query.Filter, RaycastOptions.DT_RAYCAST_USE_COSTS, 0,
                            ref grandpaRef,
                            out rayHit);
                        foundShortCut = rayHit.T >= 1.0f;
                    }

                    // update move cost
                    if (foundShortCut)
                    {
                        // shortcut found using raycast. Using shorter cost instead
                        cost = parentNode.Cost + rayHit.PathCost;
                    }
                    else
                    {
                        // No shortcut found.
                        float curCost = m_query.Filter.GetCost(
                            bestNode.Pos, neighbourNode.Pos,
                            parentRef, parentTile, parentPoly,
                            bestRef, bestTile, bestPoly,
                            neighbourRef, neighbourTile, neighbourPoly);

                        cost = bestNode.Cost + curCost;
                    }

                    // Special case for last node.
                    if (neighbourRef == m_query.EndRef)
                    {
                        float endCost = m_query.Filter.GetCost(
                            neighbourNode.Pos, m_query.EndPos,
                            bestRef, bestTile, bestPoly,
                            neighbourRef, neighbourTile, neighbourPoly,
                            0, null, null);

                        cost += endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        heuristic = Vector3.Distance(neighbourNode.Pos, m_query.EndPos) * DetourUtils.H_SCALE;
                    }

                    float total = cost + heuristic;

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0 && total >= neighbourNode.Total)
                    {
                        continue;
                    }
                    // The node is already visited and process, and the new result is worse, skip.
                    if ((neighbourNode.Flags & NodeFlagTypes.Closed) != 0 && total >= neighbourNode.Total)
                    {
                        continue;
                    }

                    // Add or update the node.
                    neighbourNode.PIdx = foundShortCut ? bestNode.PIdx : m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.Id = neighbourRef;
                    neighbourNode.Flags = (neighbourNode.Flags & ~(NodeFlagTypes.Closed | NodeFlagTypes.ParentDetached));
                    neighbourNode.Cost = cost;
                    neighbourNode.Total = total;
                    if (foundShortCut)
                    {
                        neighbourNode.Flags = (neighbourNode.Flags | NodeFlagTypes.ParentDetached);
                    }

                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0)
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
                    if (heuristic < m_query.LastBestNodeCost)
                    {
                        m_query.LastBestNodeCost = heuristic;
                        m_query.LastBestNode = neighbourNode;
                    }
                }
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

            List<int> pathList = new List<int>();

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

                Node prev = null;
                Node node = m_query.LastBestNode;
                NodeFlagTypes prevRay = 0;
                do
                {
                    Node next = m_nodePool.GetNodeAtIdx(node.PIdx);
                    node.PIdx = m_nodePool.GetNodeIdx(prev);
                    prev = node;
                    NodeFlagTypes nextRay = node.Flags & NodeFlagTypes.ParentDetached; // keep track of whether parent is not adjacent (i.e. due to raycast shortcut)
                    node.Flags = (node.Flags & ~NodeFlagTypes.ParentDetached) | prevRay; // and store it in the reversed path's node
                    prevRay = nextRay;
                    node = next;
                }
                while (node != null);

                // Store path
                node = prev;
                do
                {
                    Node next = m_nodePool.GetNodeAtIdx(node.PIdx);
                    Status status = 0;
                    if ((node.Flags & NodeFlagTypes.ParentDetached) != 0)
                    {
                        status = Raycast(
                            node.Id, node.Pos, next.Pos, m_query.Filter, maxPath - pathList.Count,
                            out _, out _, out var rpath);
                        if (status.HasFlag(Status.DT_SUCCESS))
                        {
                            pathList.AddRange(rpath.GetPath());
                        }
                        // raycast ends on poly boundary and the path might include the next poly boundary.
                        if (pathList[pathList.Count - 1] == next.Id)
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

                    if ((status & Status.DT_STATUS_DETAIL_MASK) != 0)
                    {
                        m_query.Status |= status & Status.DT_STATUS_DETAIL_MASK;
                        break;
                    }
                    node = next;
                }
                while (node != null);
            }

            Status details = m_query.Status & Status.DT_STATUS_DETAIL_MASK;

            // Reset query.
            m_query = new QueryData();

            path = new SimplePath(maxPath);
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
        public Status FinalizeSlicedFindPathPartial(int maxPath, IEnumerable<int> existing, out SimplePath path)
        {
            path = null;

            if (existing?.Any() != true || maxPath <= 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            if (m_query.Status.HasFlag(Status.DT_FAILURE))
            {
                // Reset query.
                m_query = new QueryData();
                return Status.DT_FAILURE;
            }

            List<int> pathList = new List<int>();

            if (m_query.StartRef == m_query.EndRef)
            {
                // Special case: the search starts and ends at same poly.
                pathList.Add(m_query.StartRef);
            }
            else
            {
                // Find furthest existing node that was visited.
                Node prev = null;
                Node node = null;
                for (int i = existing.Count() - 1; i >= 0; --i)
                {
                    m_nodePool.FindNodes(existing.ElementAt(i), out Node[] nodes, 1);
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
                NodeFlagTypes prevRay = 0;
                do
                {
                    Node next = m_nodePool.GetNodeAtIdx(node.PIdx);
                    node.PIdx = m_nodePool.GetNodeIdx(prev);
                    prev = node;
                    NodeFlagTypes nextRay = node.Flags & NodeFlagTypes.ParentDetached; // keep track of whether parent is not adjacent (i.e. due to raycast shortcut)
                    node.Flags = (node.Flags & ~NodeFlagTypes.ParentDetached) | prevRay; // and store it in the reversed path's node
                    prevRay = nextRay;
                    node = next;
                }
                while (node != null);

                // Store path
                node = prev;
                do
                {
                    Node next = m_nodePool.GetNodeAtIdx(node.PIdx);
                    Status status = 0;
                    if ((node.Flags & NodeFlagTypes.ParentDetached) != 0)
                    {
                        status = Raycast(
                            node.Id, node.Pos, next.Pos, m_query.Filter, maxPath - pathList.Count,
                            out _, out _, out var rpath);
                        if (status.HasFlag(Status.DT_SUCCESS))
                        {
                            pathList.AddRange(rpath.GetPath());
                        }
                        // raycast ends on poly boundary and the path might include the next poly boundary.
                        if (pathList[pathList.Count - 1] == next.Id)
                        {
                            pathList.RemoveAt(pathList.Count); // remove to avoid duplicates
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

                    if ((status & Status.DT_STATUS_DETAIL_MASK) != 0)
                    {
                        m_query.Status |= status & Status.DT_STATUS_DETAIL_MASK;
                        break;
                    }
                    node = next;
                }
                while (node != null);
            }

            Status details = m_query.Status & Status.DT_STATUS_DETAIL_MASK;

            // Reset query.
            m_query = new QueryData();

            path = new SimplePath(maxPath);
            path.StartPath(pathList);

            return Status.DT_SUCCESS | details;
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
            result = new PolyRefs(maxResult);

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

            Node startNode = m_nodePool.GetNode(startRef, 0);
            startNode.Pos = centerPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = NodeFlagTypes.Open;
            m_openList.Push(startNode);

            Status status = Status.DT_SUCCESS;

            int n = 0;

            float radiusSqr = (radius * radius);

            while (!m_openList.Empty())
            {
                Node bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                int bestRef = bestNode.Id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out MeshTile bestTile, out Poly bestPoly);

                // Get parent poly and tile.
                int parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef != 0)
                {
                    m_nav.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);
                }

                if (n < maxResult)
                {
                    result.Refs[n] = bestRef;
                    result.Parents[n] = parentRef;
                    result.Costs[n] = bestNode.Total;
                    ++n;
                }
                else
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }

                for (int i = bestPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = bestTile.Links[i].Next)
                {
                    Link link = bestTile.Links[i];
                    int neighbourRef = link.NRef;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out MeshTile neighbourTile, out Poly neighbourPoly);

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    if (GetPortalPoints(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly, neighbourTile, out Vector3 va, out Vector3 vb).HasFlag(Status.DT_FAILURE))
                    {
                        continue;
                    }

                    // If the circle is not touching the next polygon, skip it.
                    float distSqr = DetourUtils.DistancePtSegSqr2D(centerPos, va, vb, out _);
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    Node neighbourNode = m_nodePool.GetNode(neighbourRef, 0);
                    if (neighbourNode == null)
                    {
                        status |= Status.DT_OUT_OF_NODES;
                        continue;
                    }

                    if ((neighbourNode.Flags & NodeFlagTypes.Closed) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.Flags == 0)
                    {
                        neighbourNode.Pos = Vector3.Lerp(va, vb, 0.5f);
                    }

                    float cost = filter.GetCost(
                        bestNode.Pos, neighbourNode.Pos,
                        parentRef, parentTile, parentPoly,
                        bestRef, bestTile, bestPoly,
                        neighbourRef, neighbourTile, neighbourPoly);

                    float total = bestNode.Total + cost;

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0 && total >= neighbourNode.Total)
                    {
                        continue;
                    }

                    neighbourNode.Id = neighbourRef;
                    neighbourNode.PIdx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.Total = total;

                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.Flags = NodeFlagTypes.Open;
                        m_openList.Push(neighbourNode);
                    }
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
        public Status FindPolysAroundShape(int startRef, Vector3[] verts, int nverts, QueryFilter filter, int maxResult, out PolyRefs result)
        {
            result = new PolyRefs(maxResult);

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) ||
                verts == null || nverts < 3 ||
                filter == null ||
                maxResult < 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            Vector3 centerPos = Vector3.Zero;
            for (int i = 0; i < nverts; ++i)
            {
                centerPos += verts[i];
            }
            centerPos *= (1.0f / nverts);

            Node startNode = m_nodePool.GetNode(startRef, 0);
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
                Node bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                int bestRef = bestNode.Id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out MeshTile bestTile, out Poly bestPoly);

                // Get parent poly and tile.
                int parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef != 0)
                {
                    m_nav.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);
                }

                if (n < maxResult)
                {
                    result.Refs[n] = bestRef;
                    result.Parents[n] = parentRef;
                    result.Costs[n] = bestNode.Total;
                    ++n;
                }
                else
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }

                for (int i = bestPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = bestTile.Links[i].Next)
                {
                    Link link = bestTile.Links[i];
                    int neighbourRef = link.NRef;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out MeshTile neighbourTile, out Poly neighbourPoly);

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    if (GetPortalPoints(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly, neighbourTile, out Vector3 va, out Vector3 vb).HasFlag(Status.DT_FAILURE))
                    {
                        continue;
                    }

                    // If the poly is not touching the edge to the next polygon, skip the connection it.
                    if (!DetourUtils.IntersectSegmentPoly2D(va, vb, verts, nverts, out float tmin, out float tmax, out _, out _))
                    {
                        continue;
                    }
                    if (tmin > 1.0f || tmax < 0.0f)
                    {
                        continue;
                    }

                    Node neighbourNode = m_nodePool.GetNode(neighbourRef, 0);
                    if (neighbourNode == null)
                    {
                        status |= Status.DT_OUT_OF_NODES;
                        continue;
                    }

                    if ((neighbourNode.Flags & NodeFlagTypes.Closed) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.Flags == 0)
                    {
                        neighbourNode.Pos = Vector3.Lerp(va, vb, 0.5f);
                    }

                    float cost = filter.GetCost(
                        bestNode.Pos, neighbourNode.Pos,
                        parentRef, parentTile, parentPoly,
                        bestRef, bestTile, bestPoly,
                        neighbourRef, neighbourTile, neighbourPoly);

                    float total = bestNode.Total + cost;

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0 && total >= neighbourNode.Total)
                    {
                        continue;
                    }

                    neighbourNode.Id = neighbourRef;
                    neighbourNode.PIdx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.Total = total;

                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.Flags = NodeFlagTypes.Open;
                        m_openList.Push(neighbourNode);
                    }
                }
            }

            result.Count = n;

            return status;
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

            if (m_nodePool.FindNodes(endRef, out Node[] endNodes, 1) != 1 || (endNodes[0].Flags & NodeFlagTypes.Closed) == 0)
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
            }

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Finds polygons that overlap the search box.
        /// </summary>
        /// <param name="center">The center of the search box.</param>
        /// <param name="halfExtents">The search distance along each axis.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="polys">The reference ids of the polygons that overlap the query box.</param>
        /// <param name="polyCount">The number of polygons in the search result.</param>
        /// <param name="maxPolys">The maximum number of polygons the search result can hold.</param>
        /// <returns>The status flags for the query.</returns>
        public Status QueryPolygons(Vector3 center, Vector3 halfExtents, QueryFilter filter, int[] polys, int polyCount, int maxPolys)
        {
            if (polys == null || polyCount == 0 || maxPolys < 0)
            {
                return Status.DT_FAILURE;
            }

            CollectPolysQuery collector = new CollectPolysQuery(polys, maxPolys);

            if (QueryPolygons(center, halfExtents, filter, collector).HasFlag(Status.DT_FAILURE))
            {
                return Status.DT_FAILURE;
            }

            return collector.Overflow ? Status.DT_SUCCESS | Status.DT_BUFFER_TOO_SMALL : Status.DT_SUCCESS;
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

            Vector3 bmin = Vector3.Subtract(center, halfExtents);
            Vector3 bmax = Vector3.Add(center, halfExtents);

            // Find tiles the query touches.
            m_nav.CalcTileLoc(bmin, out int minx, out int miny);
            m_nav.CalcTileLoc(bmax, out int maxx, out int maxy);

            int MAX_NEIS = 32;
            MeshTile[] neis = new MeshTile[MAX_NEIS];

            for (int y = miny; y <= maxy; ++y)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    int nneis = m_nav.GetTilesAt(x, y, neis, MAX_NEIS);
                    for (int j = 0; j < nneis; ++j)
                    {
                        QueryPolygonsInTile(neis[j], bmin, bmax, filter, query);
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
            result = new PolyRefs(maxResult);

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

            Vector3[] pa = new Vector3[DetourUtils.DT_VERTS_PER_POLYGON];
            Vector3[] pb = new Vector3[DetourUtils.DT_VERTS_PER_POLYGON];

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
                int curRef = curNode.Id;
                m_nav.GetTileAndPolyByRefUnsafe(curRef, out MeshTile curTile, out Poly curPoly);

                for (int i = curPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = curTile.Links[i].Next)
                {
                    Link link = curTile.Links[i];
                    int neighbourRef = link.NRef;
                    // Skip invalid neighbours.
                    if (neighbourRef == 0)
                    {
                        continue;
                    }

                    // Skip if cannot alloca more nodes.
                    Node neighbourNode = m_tinyNodePool.GetNode(neighbourRef, 0);
                    if (neighbourNode == null)
                    {
                        continue;
                    }
                    // Skip visited.
                    if ((neighbourNode.Flags & NodeFlagTypes.Closed) != 0)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out MeshTile neighbourTile, out Poly neighbourPoly);

                    // Skip off-mesh connections.
                    if (neighbourPoly.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    if (GetPortalPoints(curRef, curPoly, curTile, neighbourRef, neighbourPoly, neighbourTile, out Vector3 va, out Vector3 vb).HasFlag(Status.DT_FAILURE))
                    {
                        continue;
                    }

                    // If the circle is not touching the next polygon, skip it.
                    float distSqr = DetourUtils.DistancePtSegSqr2D(centerPos, va, vb, out _);
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
                    int npa = neighbourPoly.VertCount;
                    for (int k = 0; k < npa; ++k)
                    {
                        pa[k] = neighbourTile.Verts[neighbourPoly.Verts[k]];
                    }

                    bool overlap = false;
                    for (int j = 0; j < n; ++j)
                    {
                        int pastRef = result.Refs[j];

                        // Connected polys do not overlap.
                        bool connected = false;
                        for (int k = curPoly.FirstLink; k != DetourUtils.DT_NULL_LINK; k = curTile.Links[k].Next)
                        {
                            if (curTile.Links[k].NRef == pastRef)
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
                        m_nav.GetTileAndPolyByRefUnsafe(pastRef, out MeshTile pastTile, out Poly pastPoly);

                        // Get vertices and test overlap
                        int npb = pastPoly.VertCount;
                        for (int k = 0; k < npb; ++k)
                        {
                            pb[k] = pastTile.Verts[pastPoly.Verts[k]];
                        }

                        if (DetourUtils.OverlapPolyPoly2D(pa, npa, pb, npb))
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (overlap)
                        continue;

                    // This poly is fine, store and advance to the poly.
                    if (n < maxResult)
                    {
                        result.Refs[n] = neighbourRef;
                        result.Parents[n] = curRef;
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
            visited = new SimplePath(maxVisitedSize);

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
            Node[] stack = new Node[MAX_STACK];
            int nstack = 0;

            m_tinyNodePool.Clear();

            Node startNode = m_tinyNodePool.GetNode(startRef, 0);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = NodeFlagTypes.Closed;
            stack[nstack++] = startNode;

            Vector3 bestPos = startPos;
            float bestDist = float.MaxValue;
            Node bestNode = null;

            // Search constraints
            Vector3 searchPos = Vector3.Lerp(startPos, endPos, 0.5f);
            float searchRadSqr = (float)Math.Pow(Vector3.Distance(startPos, endPos) / 2.0f + 0.001f, 2);

            Vector3[] verts = new Vector3[DetourUtils.DT_VERTS_PER_POLYGON];

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
                int curRef = curNode.Id;
                m_nav.GetTileAndPolyByRefUnsafe(curRef, out MeshTile curTile, out Poly curPoly);

                // Collect vertices.
                int nverts = curPoly.VertCount;
                for (int i = 0; i < nverts; ++i)
                {
                    verts[i] = curTile.Verts[curPoly.Verts[i]];
                }

                // If target is inside the poly, stop search.
                if (DetourUtils.PointInPolygon(endPos, verts, nverts))
                {
                    bestNode = curNode;
                    bestPos = endPos;
                    break;
                }

                // Find wall edges and find nearest point inside the walls.
                for (int i = 0, j = curPoly.VertCount - 1; i < curPoly.VertCount; j = i++)
                {
                    // Find links to neighbours.
                    int MAX_NEIS = 8;
                    int nneis = 0;
                    int[] neis = new int[MAX_NEIS];

                    if ((curPoly.Neis[j] & DetourUtils.DT_EXT_LINK) != 0)
                    {
                        // Tile border.
                        for (int k = curPoly.FirstLink; k != DetourUtils.DT_NULL_LINK; k = curTile.Links[k].Next)
                        {
                            Link link = curTile.Links[k];
                            if (link.Edge == j && link.NRef != 0)
                            {
                                m_nav.GetTileAndPolyByRefUnsafe(link.NRef, out MeshTile neiTile, out Poly neiPoly);
                                if (filter.PassFilter(link.NRef, neiTile, neiPoly) && nneis < MAX_NEIS)
                                {
                                    neis[nneis++] = link.NRef;
                                }
                            }
                        }
                    }
                    else if (curPoly.Neis[j] != 0)
                    {
                        int idx = (curPoly.Neis[j] - 1);
                        int r = m_nav.GetTileRef(curTile) | idx;
                        if (filter.PassFilter(r, curTile, curTile.Polys[idx]))
                        {
                            // Internal edge, encode id.
                            neis[nneis++] = r;
                        }
                    }

                    if (nneis == 0)
                    {
                        // Wall edge, calc distance.
                        Vector3 vj = verts[j];
                        Vector3 vi = verts[i];
                        float distSqr = DetourUtils.DistancePtSegSqr2D(endPos, vj, vi, out float tseg);
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
                        for (int k = 0; k < nneis; ++k)
                        {
                            // Skip if no node can be allocated.
                            Node neighbourNode = m_tinyNodePool.GetNode(neis[k], 0);
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
                            // TODO: Maybe should use getPortalPoints(), but this one is way faster.
                            Vector3 vj = verts[j];
                            Vector3 vi = verts[i];
                            float distSqr = DetourUtils.DistancePtSegSqr2D(searchPos, vj, vi, out _);
                            if (distSqr > searchRadSqr)
                            {
                                continue;
                            }

                            // Mark as the node as visited and push to queue.
                            if (nstack < MAX_STACK)
                            {
                                neighbourNode.PIdx = m_tinyNodePool.GetNodeIdx(curNode);
                                neighbourNode.Flags |= NodeFlagTypes.Closed;
                                stack[nstack++] = neighbourNode;
                            }
                        }
                    }
                }
            }

            if (bestNode != null)
            {
                // Reverse the path.
                Node prev = null;
                Node node = bestNode;
                do
                {
                    Node next = m_tinyNodePool.GetNodeAtIdx(node.PIdx);
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
                        status |= Status.DT_BUFFER_TOO_SMALL;
                        break;
                    }
                    node = m_tinyNodePool.GetNodeAtIdx(node.PIdx);
                }
                while (node != null);
            }

            resultPos = bestPos;

            return status;
        }
        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh from the start position toward the end position.
        /// </summary>
        /// <param name="startRef">The reference id of the start polygon.</param>
        /// <param name="startPos">A position within the start polygon representing the start of the ray.</param>
        /// <param name="endPos">The position to cast the ray toward.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="t">The hit parameter. (FLT_MAX if no wall hit.)</param>
        /// <param name="hitNormal">The normal of the nearest wall hit.</param>
        /// <param name="path">The reference ids of the visited polygons.</param>
        /// <param name="maxPath">The maximum number of polygons the path array can hold.</param>
        /// <returns>The status flags for the query.</returns>
        public Status Raycast(int startRef, Vector3 startPos, Vector3 endPos, QueryFilter filter, int maxPath, out float t, out Vector3 hitNormal, out SimplePath path)
        {
            int? prevRef = null;
            Status status = Raycast(startRef, startPos, endPos, filter, 0, maxPath, ref prevRef, out RaycastHit hit);

            t = hit.T;
            hitNormal = hit.HitNormal;
            path = new SimplePath(maxPath);
            path.StartPath(hit.Path);

            return status;
        }
        /// <summary>
        /// Casts a 'walkability' ray along the surface of the navigation mesh from the start position toward the end position.
        /// </summary>
        /// <param name="startRef">The reference id of the start polygon.</param>
        /// <param name="startPos">A position within the start polygon representing the start of the ray.</param>
        /// <param name="endPos">The position to cast the ray toward.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="options">Govern how the raycast behaves. See dtRaycastOptions</param>
        /// <param name="maxPath">The maximum number of polygons the path array can hold.</param>
        /// <param name="hit">Pointer to a raycast hit structure which will be filled by the results.</param>
        /// <param name="prevRef">parent of start ref. Used during for cost calculation</param>
        /// <returns></returns>
        public Status Raycast(int startRef, Vector3 startPos, Vector3 endPos, QueryFilter filter, RaycastOptions options, int maxPath, ref int? prevRef, out RaycastHit hit)
        {
            hit = new RaycastHit
            {
                MaxPath = maxPath,
                T = 0,
                PathCost = 0
            };

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) ||
                startPos.IsInfinity() ||
                endPos.IsInfinity() ||
                filter == null ||
                prevRef.HasValue && !m_nav.IsValidPolyRef(prevRef.Value))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            Vector3 dir, curPos, lastPos;
            Vector3[] verts = new Vector3[DetourUtils.DT_VERTS_PER_POLYGON + 3];
            int n = 0;

            curPos = startPos;
            dir = Vector3.Subtract(endPos, startPos);
            hit.HitNormal = Vector3.Zero;

            Status status = Status.DT_SUCCESS;

            // The API input has been checked already, skip checking internal data.
            int curRef = startRef;
            m_nav.GetTileAndPolyByRefUnsafe(curRef, out MeshTile tile, out Poly poly);
            MeshTile prevTile = tile;
            MeshTile nextTile = tile;
            Poly prevPoly = poly;
            Poly nextPoly = poly;
            if (prevRef.HasValue)
            {
                m_nav.GetTileAndPolyByRefUnsafe(prevRef.Value, out prevTile, out prevPoly);
            }

            while (curRef != 0)
            {
                // Cast ray against current polygon.

                // Collect vertices.
                int nv = 0;
                for (int i = 0; i < poly.VertCount; ++i)
                {
                    verts[nv] = tile.Verts[poly.Verts[i]];
                    nv++;
                }

                if (!DetourUtils.IntersectSegmentPoly2D(startPos, endPos, verts, nv, out _, out float tmax, out _, out int segMax))
                {
                    // Could not hit the polygon, keep the old t and report hit.
                    hit.Cut(n);
                    return status;
                }

                hit.HitEdgeIndex = segMax;

                // Keep track of furthest t so far.
                if (tmax > hit.T)
                {
                    hit.T = tmax;
                }

                // Store visited polygons.
                if (n < hit.MaxPath)
                {
                    hit.Add(curRef);

                    n++;
                }
                else
                {
                    status |= Status.DT_BUFFER_TOO_SMALL;
                }

                // Ray end is completely inside the polygon.
                if (segMax == -1)
                {
                    hit.T = float.MaxValue;
                    hit.Cut(n);

                    // add the cost
                    if ((options & RaycastOptions.DT_RAYCAST_USE_COSTS) != 0)
                    {
                        hit.PathCost += filter.GetCost(curPos, endPos, prevRef, prevTile, prevPoly, curRef, tile, poly, curRef, tile, poly);
                    }

                    return status;
                }

                // Follow neighbours.
                int nextRef = 0;

                for (int i = poly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = tile.Links[i].Next)
                {
                    Link link = tile.Links[i];

                    // Find link which contains this edge.
                    if (link.Edge != segMax)
                    {
                        continue;
                    }

                    // Get pointer to the next polygon.
                    m_nav.GetTileAndPolyByRefUnsafe(link.NRef, out nextTile, out nextPoly);

                    // Skip off-mesh connections.
                    if (nextPoly.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Skip links based on filter.
                    if (!filter.PassFilter(link.NRef, nextTile, nextPoly))
                    {
                        continue;
                    }

                    // If the link is internal, just return the ref.
                    if (link.Side == 0xff)
                    {
                        nextRef = link.NRef;
                        break;
                    }

                    // If the link is at tile boundary,

                    // Check if the link spans the whole edge, and accept.
                    if (link.BMin == 0 && link.BMax == 255)
                    {
                        nextRef = link.NRef;
                        break;
                    }

                    // Check for partial edge links.
                    int v0 = poly.Verts[link.Edge];
                    int v1 = poly.Verts[(link.Edge + 1) % poly.VertCount];
                    Vector3 left = tile.Verts[v0];
                    Vector3 right = tile.Verts[v1];

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
                            nextRef = link.NRef;
                            break;
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
                            nextRef = link.NRef;
                            break;
                        }
                    }
                }

                // add the cost
                if ((options & RaycastOptions.DT_RAYCAST_USE_COSTS) != 0)
                {
                    // compute the intersection point at the furthest end of the polygon
                    // and correct the height (since the raycast moves in 2d)
                    lastPos = curPos;
                    curPos = Vector3.Add(startPos, dir) * hit.T;
                    var e1 = verts[segMax];
                    var e2 = verts[((segMax + 1) % nv)];
                    Vector3 eDir;
                    Vector3 diff;
                    eDir = Vector3.Subtract(e2, e1);
                    diff = Vector3.Subtract(curPos, e1);
                    float s = (eDir.X * eDir.X) > (eDir.Z * eDir.Z) ? diff.X / eDir.X : diff.Z / eDir.Z;
                    curPos.Y = e1.Y + eDir.Y * s;

                    hit.PathCost += filter.GetCost(lastPos, curPos, prevRef, prevTile, prevPoly, curRef, tile, poly, nextRef, nextTile, nextPoly);
                }

                if (nextRef == 0)
                {
                    // No neighbour, we hit a wall.

                    // Calculate hit normal.
                    int a = segMax;
                    int b = segMax + 1 < nv ? segMax + 1 : 0;
                    var va = verts[a];
                    var vb = verts[b];
                    float dx = vb.X - va.X;
                    float dz = vb.Z - va.Z;
                    hit.HitNormal = Vector3.Normalize(new Vector3(dz, 0, -dx));
                    hit.Cut(n);
                    return status;
                }

                // No hit, advance to neighbour polygon.
                prevRef = curRef;
                curRef = nextRef;
                prevTile = tile;
                tile = nextTile;
                prevPoly = poly;
                poly = nextPoly;
            }

            hit.Cut(n);

            return status;
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

            Node startNode = m_nodePool.GetNode(startRef, 0);
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
                Node bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                int bestRef = bestNode.Id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out MeshTile bestTile, out Poly bestPoly);

                // Get parent poly and tile.
                int parentRef = 0;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef != 0)
                {
                    m_nav.GetTileAndPolyByRefUnsafe(parentRef, out _, out _);
                }

                // Hit test walls.
                for (int i = 0, j = bestPoly.VertCount - 1; i < bestPoly.VertCount; j = i++)
                {
                    // Skip non-solid edges.
                    if ((bestPoly.Neis[j] & DetourUtils.DT_EXT_LINK) != 0)
                    {
                        // Tile border.
                        bool solid = true;
                        for (int k = bestPoly.FirstLink; k != DetourUtils.DT_NULL_LINK; k = bestTile.Links[k].Next)
                        {
                            Link link = bestTile.Links[k];
                            if (link.Edge == j)
                            {
                                if (link.NRef != 0)
                                {
                                    m_nav.GetTileAndPolyByRefUnsafe(link.NRef, out MeshTile neiTile, out Poly neiPoly);
                                    if (filter.PassFilter(link.NRef, neiTile, neiPoly))
                                    {
                                        solid = false;
                                    }
                                }
                                break;
                            }
                        }
                        if (!solid)
                        {
                            continue;
                        }
                    }
                    else if (bestPoly.Neis[j] != 0)
                    {
                        // Internal edge
                        int idx = bestPoly.Neis[j] - 1;
                        int r = m_nav.GetTileRef(bestTile) | idx;
                        if (filter.PassFilter(r, bestTile, bestTile.Polys[idx]))
                        {
                            continue;
                        }
                    }

                    // Calc distance to the edge.
                    Vector3 vj = bestTile.Verts[bestPoly.Verts[j]];
                    Vector3 vi = bestTile.Verts[bestPoly.Verts[i]];
                    float distSqr = DetourUtils.DistancePtSegSqr2D(centerPos, vj, vi, out float tseg);

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

                for (int i = bestPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = bestTile.Links[i].Next)
                {
                    Link link = bestTile.Links[i];
                    int neighbourRef = link.NRef;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour.
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out MeshTile neighbourTile, out Poly neighbourPoly);

                    // Skip off-mesh connections.
                    if (neighbourPoly.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Calc distance to the edge.
                    Vector3 va = bestTile.Verts[bestPoly.Verts[link.Edge]];
                    Vector3 vb = bestTile.Verts[bestPoly.Verts[(link.Edge + 1) % bestPoly.VertCount]];
                    float distSqr = DetourUtils.DistancePtSegSqr2D(centerPos, va, vb, out _);

                    // If the circle is not touching the next polygon, skip it.
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    Node neighbourNode = m_nodePool.GetNode(neighbourRef, 0);
                    if (neighbourNode == null)
                    {
                        status |= Status.DT_OUT_OF_NODES;
                        continue;
                    }

                    if ((neighbourNode.Flags & NodeFlagTypes.Closed) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.Flags == 0)
                    {
                        var midPointRes = GetEdgeMidPoint(
                            bestRef, bestPoly, bestTile,
                            neighbourRef, neighbourPoly, neighbourTile,
                            out var pos);

                        if (midPointRes != Status.DT_SUCCESS)
                        {
                            Console.WriteLine($"FindPath GetEdgeMidPoint result: {midPointRes}");
                            return midPointRes;
                        }

                        neighbourNode.Pos = pos;
                    }

                    float total = bestNode.Total + Vector3.Distance(bestNode.Pos, neighbourNode.Pos);

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0 && total >= neighbourNode.Total)
                    {
                        continue;
                    }

                    neighbourNode.Id = neighbourRef;
                    neighbourNode.Flags = (neighbourNode.Flags & ~NodeFlagTypes.Closed);
                    neighbourNode.PIdx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.Total = total;

                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.Flags |= NodeFlagTypes.Open;
                        m_openList.Push(neighbourNode);
                    }
                }
            }

            // Calc hit normal.
            hitNormal = Vector3.Subtract(centerPos, hitPos);
            hitNormal.Normalize();

            hitDist = (float)Math.Sqrt(radiusSqr);

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
            segmentsRes = new Segment[] { };

            List<Segment> segments = new List<Segment>();

            if (!m_nav.GetTileAndPolyByRef(r, out MeshTile tile, out Poly poly))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            if (filter == null || maxSegments < 0)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            int MAX_INTERVAL = 16;
            List<SegInterval> ints = new List<SegInterval>();

            bool storePortals = false;

            Status status = Status.DT_SUCCESS;

            for (int i = 0, j = poly.VertCount - 1; i < poly.VertCount; j = i++)
            {
                // Skip non-solid edges.
                if ((poly.Neis[j] & DetourUtils.DT_EXT_LINK) != 0)
                {
                    // Tile border.
                    for (int k = poly.FirstLink; k != DetourUtils.DT_NULL_LINK; k = tile.Links[k].Next)
                    {
                        Link link = tile.Links[k];
                        if (link.Edge == j && link.NRef != 0)
                        {
                            m_nav.GetTileAndPolyByRefUnsafe(link.NRef, out MeshTile neiTile, out Poly neiPoly);
                            if (filter.PassFilter(link.NRef, neiTile, neiPoly))
                            {
                                SegInterval.InsertInterval(ints, MAX_INTERVAL, link.BMin, link.BMax, link.NRef);
                            }
                        }
                    }
                }
                else
                {
                    // Internal edge
                    int neiRef = 0;
                    if (poly.Neis[j] != 0)
                    {
                        int idx = (poly.Neis[j] - 1);
                        neiRef = m_nav.GetTileRef(tile) | idx;
                        if (!filter.PassFilter(neiRef, tile, tile.Polys[idx]))
                        {
                            neiRef = 0;
                        }
                    }

                    // If the edge leads to another polygon and portals are not stored, skip.
                    if (neiRef != 0 && !storePortals)
                    {
                        continue;
                    }

                    if (segments.Count < maxSegments)
                    {
                        segments.Add(new Segment
                        {
                            S1 = tile.Verts[poly.Verts[j]],
                            S2 = tile.Verts[poly.Verts[i]],
                            R = neiRef,
                        });
                    }
                    else
                    {
                        status |= Status.DT_BUFFER_TOO_SMALL;
                    }

                    continue;
                }

                // Add sentinels
                SegInterval.InsertInterval(ints, MAX_INTERVAL, -1, 0, 0);
                SegInterval.InsertInterval(ints, MAX_INTERVAL, 255, 256, 0);

                // Store segments.
                Vector3 vj = tile.Verts[poly.Verts[j]];
                Vector3 vi = tile.Verts[poly.Verts[i]];
                for (int k = 1; k < ints.Count; ++k)
                {
                    // Portal segment.
                    if (storePortals && ints[k].R != 0)
                    {
                        float tmin = ints[k].TMin / 255.0f;
                        float tmax = ints[k].TMax / 255.0f;
                        if (segments.Count < maxSegments)
                        {
                            segments.Add(new Segment
                            {
                                S1 = Vector3.Lerp(vj, vi, tmin),
                                S2 = Vector3.Lerp(vj, vi, tmax),
                                R = ints[k].R,
                            });
                        }
                        else
                        {
                            status |= Status.DT_BUFFER_TOO_SMALL;
                        }
                    }

                    // Wall segment.
                    int imin = ints[k - 1].TMax;
                    int imax = ints[k].TMin;
                    if (imin != imax)
                    {
                        float tmin = imin / 255.0f;
                        float tmax = imax / 255.0f;
                        if (segments.Count < maxSegments)
                        {
                            segments.Add(new Segment
                            {
                                S1 = Vector3.Lerp(vj, vi, tmin),
                                S2 = Vector3.Lerp(vj, vi, tmax),
                                R = 0,
                            });
                        }
                        else
                        {
                            status |= Status.DT_BUFFER_TOO_SMALL;
                        }
                    }
                }
            }

            segmentsRes = segments.ToArray();

            return status;
        }
        /// <summary>
        /// Returns random location on navmesh.
        /// </summary>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="frand">Function returning a random number [0..1).</param>
        /// <param name="randomRef">The reference id of the random location.</param>
        /// <param name="randomPt">The random location. </param>
        /// <returns>The status flags for the query.</returns>
        /// <remarks>
        /// Polygons are chosen weighted by area. The search runs in linear related to number of polygon.
        /// </remarks>
        public Status FindRandomPoint(QueryFilter filter, Random frand, out int randomRef, out Vector3 randomPt)
        {
            randomRef = -1;
            randomPt = Vector3.Zero;

            if (filter == null || frand == null)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Randomly pick one tile. Assume that all tiles cover roughly the same area.
            MeshTile tile = null;
            float tsum = 0.0f;
            for (int i = 0; i < m_nav.MaxTiles; i++)
            {
                MeshTile tl = m_nav.Tiles[i];
                if (tl == null || tl.Header.Magic != DetourUtils.DT_NAVMESH_MAGIC)
                {
                    continue;
                }

                // Choose random tile using reservoi sampling.
                float area = 1.0f; // Could be tile area too.
                tsum += area;
                float u = frand.NextFloat(0, 1);
                if (u * tsum <= area)
                {
                    tile = tl;
                }
            }
            if (tile == null)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick one polygon weighted by polygon area.
            Poly poly = null;
            int polyRef = 0;
            int bse = m_nav.GetTileRef(tile);

            float areaSum = 0.0f;
            for (int i = 0; i < tile.Header.PolyCount; ++i)
            {
                Poly p = tile.Polys[i];
                // Do not return off-mesh connection polygons.
                if (p.Type != PolyTypes.DT_POLYTYPE_GROUND)
                {
                    continue;
                }
                // Must pass filter
                int r = bse | i;
                if (!filter.PassFilter(r, tile, p))
                {
                    continue;
                }

                // Calc area of the polygon.
                float polyArea = tile.GetPolyArea(p);

                // Choose random polygon weighted by area, using reservoi sampling.
                areaSum += polyArea;
                float u = frand.NextFloat(0, 1);
                if (u * areaSum <= polyArea)
                {
                    poly = p;
                    polyRef = r;
                }
            }

            if (poly == null)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick point on polygon.
            Vector3[] verts = tile.GetPolyVerts(poly);

            float s = frand.NextFloat(0, 1);
            float t = frand.NextFloat(0, 1);

            DetourUtils.RandomPointInConvexPoly(verts, out _, s, t, out Vector3 pt);

            Status status = GetPolyHeight(polyRef, pt, out float h);
            if (status.HasFlag(Status.DT_FAILURE))
            {
                return status;
            }
            pt.Y = h;

            randomPt = pt;

            randomRef = polyRef;

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Returns random location on navmesh within the reach of specified location.
        /// </summary>
        /// <param name="startRef">The reference id of the polygon where the search starts.</param>
        /// <param name="centerPos">The center of the search circle.</param>
        /// <param name="maxRadius">The radius of the search circle.</param>
        /// <param name="filter">The polygon filter to apply to the query.</param>
        /// <param name="frand">Function returning a random number [0..1).</param>
        /// <param name="randomRef">The reference id of the random location.</param>
        /// <param name="randomPt">The random location.</param>
        /// <returns>The status flags for the query.</returns>
        /// <remarks>
        /// Polygons are chosen weighted by area. The search runs in linear related to number of polygon.
        /// The location is not exactly constrained by the circle, but it limits the visited polygons.
        /// </remarks>
        public Status FindRandomPointAroundCircle(int startRef, Vector3 centerPos, float maxRadius, QueryFilter filter, Random frand, out int randomRef, out Vector3 randomPt)
        {
            randomRef = 0;
            randomPt = new Vector3();

            // Validate input
            if (startRef == 0 || !m_nav.IsValidPolyRef(startRef) ||
                centerPos.IsInfinity() ||
                maxRadius < 0 || float.IsNaN(maxRadius) ||
                filter == null || frand == null)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            m_nav.GetTileAndPolyByRefUnsafe(startRef, out MeshTile startTile, out Poly startPoly);
            if (!filter.PassFilter(startRef, startTile, startPoly))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            Node startNode = m_nodePool.GetNode(startRef, 0);
            startNode.Pos = centerPos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = NodeFlagTypes.Open;
            m_openList.Push(startNode);

            Status status = Status.DT_SUCCESS;

            float radiusSqr = maxRadius * maxRadius;
            float areaSum = 0.0f;

            MeshTile randomTile = null;
            Poly randomPoly = null;
            int randomPolyRef = 0;

            while (!m_openList.Empty())
            {
                Node bestNode = m_openList.Pop();
                bestNode.Flags &= ~NodeFlagTypes.Open;
                bestNode.Flags |= NodeFlagTypes.Closed;

                // Get poly and tile.
                // The API input has been cheked already, skip checking internal data.
                int bestRef = bestNode.Id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out MeshTile bestTile, out Poly bestPoly);

                // Place random locations on on ground.
                if (bestPoly.Type == PolyTypes.DT_POLYTYPE_GROUND)
                {
                    // Calc area of the polygon.
                    float polyArea = bestTile.GetPolyArea(bestPoly);
                    // Choose random polygon weighted by area, using reservoi sampling.
                    areaSum += polyArea;
                    float u = frand.NextFloat(0, 1);
                    if (u * areaSum <= polyArea)
                    {
                        randomTile = bestTile;
                        randomPoly = bestPoly;
                        randomPolyRef = bestRef;
                    }
                }

                // Get parent poly and tile.
                int parentRef = 0;
                if (bestNode.PIdx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef != 0)
                {
                    m_nav.GetTileAndPolyByRefUnsafe(parentRef, out _, out _);
                }

                for (int i = bestPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = bestTile.Links[i].Next)
                {
                    Link link = bestTile.Links[i];
                    int neighbourRef = link.NRef;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out MeshTile neighbourTile, out Poly neighbourPoly);

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    if (GetPortalPoints(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly, neighbourTile, out Vector3 va, out Vector3 vb).HasFlag(Status.DT_FAILURE))
                    {
                        continue;
                    }

                    // If the circle is not touching the next polygon, skip it.
                    float distSqr = DetourUtils.DistancePtSegSqr2D(centerPos, va, vb, out _);
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    var neighbourNode = m_nodePool.GetNode(neighbourRef, 0);
                    if (neighbourNode == null)
                    {
                        status |= Status.DT_OUT_OF_NODES;
                        continue;
                    }

                    if ((neighbourNode.Flags & NodeFlagTypes.Closed) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.Flags == NodeFlagTypes.None)
                    {
                        neighbourNode.Pos = Vector3.Lerp(va, vb, 0.5f);
                    }

                    float total = bestNode.Total + Vector3.Distance(bestNode.Pos, neighbourNode.Pos);

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0 && total >= neighbourNode.Total)
                    {
                        continue;
                    }

                    neighbourNode.Id = neighbourRef;
                    neighbourNode.Flags = (neighbourNode.Flags & ~NodeFlagTypes.Closed);
                    neighbourNode.PIdx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.Total = total;

                    if ((neighbourNode.Flags & NodeFlagTypes.Open) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.Flags = NodeFlagTypes.Open;
                        m_openList.Push(neighbourNode);
                    }
                }
            }

            if (randomPoly == null)
            {
                return Status.DT_FAILURE;
            }

            // Randomly pick point on polygon.
            Vector3[] verts = randomTile.GetPolyVerts(randomPoly);

            float s = frand.NextFloat(0, 1);
            float t = frand.NextFloat(0, 1);

            DetourUtils.RandomPointInConvexPoly(verts, out _, s, t, out Vector3 pt);

            Status stat = GetPolyHeight(randomPolyRef, pt, out float h);
            if (stat.HasFlag(Status.DT_FAILURE))
            {
                return stat;
            }
            pt.Y = h;

            randomPt = pt;
            randomRef = randomPolyRef;

            return Status.DT_SUCCESS;
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

            this.m_nav.ClosestPointOnPoly(r, pos, out closest, out posOverPoly);

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

            if (!m_nav.GetTileAndPolyByRef(r, out MeshTile tile, out Poly poly))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }
            if (pos.IsInfinity())
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Collect vertices.
            Vector3[] verts = new Vector3[DetourUtils.DT_VERTS_PER_POLYGON];
            int nv = 0;
            for (int i = 0; i < poly.VertCount; ++i)
            {
                verts[nv] = tile.Verts[poly.Verts[i]];
                nv++;
            }

            bool inside = DetourUtils.DistancePtPolyEdgesSqr(pos, verts, nv, out float[] edged, out float[] edget);
            if (inside)
            {
                // Point is inside the polygon, return the point.
                closest = pos;
            }
            else
            {
                // Point is outside the polygon, dtClamp to nearest edge.
                float dmin = edged[0];
                int imin = 0;
                for (int i = 1; i < nv; ++i)
                {
                    if (edged[i] < dmin)
                    {
                        dmin = edged[i];
                        imin = i;
                    }
                }
                var va = verts[imin];
                var vb = verts[((imin + 1) % nv)];
                closest = Vector3.Lerp(va, vb, edget[imin]);
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

            if (!m_nav.GetTileAndPolyByRef(r, out MeshTile tile, out Poly poly))
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
            if (poly.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                var v0 = tile.Verts[poly.Verts[0]];
                var v1 = tile.Verts[poly.Verts[1]];
                DetourUtils.DistancePtSegSqr2D(pos, v0, v1, out float t);
                height = v0.Y + (v1.Y - v0.Y) * t;
                return Status.DT_SUCCESS;
            }

            return m_nav.GetPolyHeight(tile, poly, pos, out height) ?
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
            bool status = m_nav.GetTileAndPolyByRef(r, out MeshTile tile, out Poly poly);
            // If cannot get polygon, assume it does not exists and boundary is invalid.
            if (!status)
            {
                return false;
            }
            // If cannot pass filter, assume flags has changed and boundary is invalid.
            if (!filter.PassFilter(r, tile, poly))
            {
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

            int n = m_nodePool.FindNodes(r, out Node[] nodes, DetourUtils.DT_MAX_STATES_PER_NODE);

            for (int i = 0; i < n; i++)
            {
                if ((nodes[i].Flags & NodeFlagTypes.Closed) != 0)
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
        private void QueryPolygonsInTile(MeshTile tile, Vector3 qmin, Vector3 qmax, QueryFilter filter, IPolyQuery query)
        {
            int batchSize = 32;
            int[] polyRefs = new int[batchSize];
            Poly[] polys = new Poly[batchSize];
            int n = 0;

            if (tile.BvTree != null && tile.BvTree.Length > 0)
            {
                int nodeIndex = 0;
                int endIndex = tile.Header.BvNodeCount;
                var tbmin = tile.Header.BMin;
                var tbmax = tile.Header.BMax;
                float qfac = tile.Header.BvQuantFactor;

                // Calculate quantized box
                // dtClamp query box to world box.
                float minx = MathUtil.Clamp(qmin.X, tbmin.X, tbmax.X) - tbmin.X;
                float miny = MathUtil.Clamp(qmin.Y, tbmin.Y, tbmax.Y) - tbmin.Y;
                float minz = MathUtil.Clamp(qmin.Z, tbmin.Z, tbmax.Z) - tbmin.Z;
                float maxx = MathUtil.Clamp(qmax.X, tbmin.X, tbmax.X) - tbmin.X;
                float maxy = MathUtil.Clamp(qmax.Y, tbmin.Y, tbmax.Y) - tbmin.Y;
                float maxz = MathUtil.Clamp(qmax.Z, tbmin.Z, tbmax.Z) - tbmin.Z;
                // Quantize
                Int3 bmin = new Int3();
                Int3 bmax = new Int3();
                bmin.X = (int)(qfac * minx) & 0xfffe;
                bmin.Y = (int)(qfac * miny) & 0xfffe;
                bmin.Z = (int)(qfac * minz) & 0xfffe;
                bmax.X = (int)(qfac * maxx + 1) | 1;
                bmax.Y = (int)(qfac * maxy + 1) | 1;
                bmax.Z = (int)(qfac * maxz + 1) | 1;

                // Traverse tree
                int bse = m_nav.GetTileRef(tile);
                while (nodeIndex < endIndex)
                {
                    var node = nodeIndex < tile.BvTree.Length ? tile.BvTree[nodeIndex] : new BVNode();

                    bool overlap = DetourUtils.OverlapQuantBounds(bmin, bmax, node.BMin, node.BMax);
                    bool isLeafNode = node.I >= 0;

                    if (isLeafNode && overlap)
                    {
                        int r = bse | node.I;
                        if (filter.PassFilter(r, tile, tile.Polys[node.I]))
                        {
                            polyRefs[n] = r;
                            polys[n] = tile.Polys[node.I];

                            if (n == batchSize - 1)
                            {
                                query.Process(tile, polys, polyRefs, batchSize);
                                n = 0;
                            }
                            else
                            {
                                n++;
                            }
                        }
                    }

                    if (overlap || isLeafNode)
                    {
                        nodeIndex++;
                    }
                    else
                    {
                        int escapeIndex = -node.I;
                        nodeIndex += escapeIndex;
                    }
                }
            }
            else
            {
                int bse = m_nav.GetTileRef(tile);
                for (int i = 0; i < tile.Header.PolyCount; ++i)
                {
                    var p = tile.Polys[i];

                    // Do not return off-mesh connection polygons.
                    if (p.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Must pass filter
                    int r = bse | i;
                    if (!filter.PassFilter(r, tile, p))
                    {
                        continue;
                    }

                    // Calc polygon bounds.
                    var v = tile.Verts[p.Verts[0]];
                    Vector3 bmin = v;
                    Vector3 bmax = v;
                    for (int j = 1; j < p.VertCount; ++j)
                    {
                        v = tile.Verts[p.Verts[j]];
                        bmin = Vector3.Min(bmin, v);
                        bmax = Vector3.Max(bmax, v);
                    }

                    if (RecastUtils.OverlapBounds(qmin, qmax, bmin, bmax))
                    {
                        polyRefs[n] = r;
                        polys[n] = p;

                        if (n == batchSize - 1)
                        {
                            query.Process(tile, polys, polyRefs, batchSize);
                            n = 0;
                        }
                        else
                        {
                            n++;
                        }
                    }
                }
            }

            // Process the last polygons that didn't make a full batch.
            if (n > 0)
            {
                query.Process(tile, polys, polyRefs, n);
            }
        }
        /// <summary>
        /// Returns portal points between two polygons.
        /// </summary>
        private Status GetPortalPoints(int from, int to, out Vector3 left, out Vector3 right, out PolyTypes fromType, out PolyTypes toType)
        {
            left = new Vector3();
            right = new Vector3();
            fromType = PolyTypes.DT_POLYTYPE_GROUND;
            toType = PolyTypes.DT_POLYTYPE_GROUND;

            if (!m_nav.GetTileAndPolyByRef(from, out MeshTile fromTile, out Poly fromPoly))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            fromType = fromPoly.Type;

            if (!m_nav.GetTileAndPolyByRef(to, out MeshTile toTile, out Poly toPoly))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            toType = toPoly.Type;

            return GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out left, out right);
        }
        /// <summary>
        /// Returns portal points between two polygons.
        /// </summary>
        private Status GetPortalPoints(int from, Poly fromPoly, MeshTile fromTile, int to, Poly toPoly, MeshTile toTile, out Vector3 left, out Vector3 right)
        {
            left = new Vector3();
            right = new Vector3();

            // Find the link that points to the 'to' polygon.
            Link? link = null;
            for (int i = fromPoly.FirstLink; i != DetourUtils.DT_NULL_LINK; i = fromTile.Links[i].Next)
            {
                if (fromTile.Links[i].NRef == to)
                {
                    link = fromTile.Links[i];
                    break;
                }
            }
            if (!link.HasValue)
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            // Handle off-mesh connections.
            if (fromPoly.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                // Find link that points to first vertex.
                return fromTile.FindLinkToNeighbour(fromPoly, to, out left, out right);
            }

            if (toPoly.Type == PolyTypes.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                return toTile.FindLinkToNeighbour(toPoly, from, out left, out right);
            }

            // Find portal vertices.
            int v0 = fromPoly.Verts[link.Value.Edge];
            int v1 = fromPoly.Verts[(link.Value.Edge + 1) % fromPoly.VertCount];
            left = fromTile.Verts[v0];
            right = fromTile.Verts[v1];

            // If the link is at tile boundary, dtClamp the vertices to
            // the link width.
            if (link.Value.Side != 0xff && (link.Value.BMin != 0 || link.Value.BMax != 255))
            {
                // Unpack portal limits.
                float s = 1.0f / 255.0f;
                float tmin = link.Value.BMin * s;
                float tmax = link.Value.BMax * s;
                left = Vector3.Lerp(fromTile.Verts[v0], fromTile.Verts[v1], tmin);
                right = Vector3.Lerp(fromTile.Verts[v0], fromTile.Verts[v1], tmax);
            }

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Returns edge mid point between two polygons.
        /// </summary>
        private Status GetEdgeMidPoint(int from, Poly fromPoly, MeshTile fromTile, int to, Poly toPoly, MeshTile toTile, out Vector3 mid)
        {
            mid = new Vector3();

            if (GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out Vector3 left, out Vector3 right).HasFlag(Status.DT_FAILURE))
            {
                return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
            }

            mid = (left + right) * 0.5f;

            return Status.DT_SUCCESS;
        }
        /// <summary>
        /// Appends vertex to a straight path
        /// </summary>
        private Status AppendVertex(Vector3 pos, StraightPathFlagTypes flags, int r, int maxStraightPath, ref StraightPath straightPath)
        {
            if (straightPath.Count > 0 && DetourUtils.Vequal(straightPath.EndPath, pos))
            {
                // The vertices are equal, update flags and poly.
                straightPath.SetFlags(straightPath.Count - 1, flags);
                straightPath.SetRef(straightPath.Count - 1, r);
            }
            else
            {
                // Append new vertex.
                straightPath.Append(pos, flags, r);

                // If there is no space to append more vertices, return.
                if (straightPath.Count >= maxStraightPath)
                {
                    return Status.DT_SUCCESS | Status.DT_BUFFER_TOO_SMALL;
                }

                // If reached end of path, return.
                if (flags == StraightPathFlagTypes.DT_STRAIGHTPATH_END)
                {
                    return Status.DT_SUCCESS;
                }
            }
            return Status.DT_IN_PROGRESS;
        }
        /// <summary>
        /// Appends intermediate portal points to a straight path.
        /// </summary>
        private Status AppendPortals(int startIdx, int endIdx, Vector3 endPos, int[] path, int maxStraightPath, StraightPathOptions options, ref StraightPath straightPath)
        {
            Vector3 startPos = straightPath.EndPath;
            // Append or update last vertex
            for (int i = startIdx; i < endIdx; i++)
            {
                // Calculate portal
                int from = path[i];
                if (!m_nav.GetTileAndPolyByRef(from, out MeshTile fromTile, out Poly fromPoly))
                {
                    return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
                }

                int to = path[i + 1];
                if (!m_nav.GetTileAndPolyByRef(to, out MeshTile toTile, out Poly toPoly))
                {
                    return Status.DT_FAILURE | Status.DT_INVALID_PARAM;
                }

                if (GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out Vector3 left, out Vector3 right).HasFlag(Status.DT_FAILURE))
                {
                    break;
                }

                if (options.HasFlag(StraightPathOptions.AreaCrossings) &&
                    fromPoly.Area == toPoly.Area)
                {
                    // Skip intersection if only area crossings are requested.
                    continue;
                }

                // Append intersection
                if (DetourUtils.IntersectSegSeg2D(startPos, endPos, left, right, out _, out float t))
                {
                    Vector3 pt = Vector3.Lerp(left, right, t);

                    Status stat = AppendVertex(
                        pt, 0, path[i + 1], maxStraightPath,
                        ref straightPath);
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
            path = new SimplePath(maxPath);

            // Find the length of the entire path.
            Node curNode = endNode;
            int length = 0;
            do
            {
                length++;
                curNode = m_nodePool.GetNodeAtIdx(curNode.PIdx);
            } while (curNode != null);

            // If the path cannot be fully stored then advance to the last node we will be able to store.
            curNode = endNode;
            int writeCount;
            for (writeCount = length; writeCount > maxPath; writeCount--)
            {
                curNode = m_nodePool.GetNodeAtIdx(curNode.PIdx);
            }

            // Write path
            List<int> tmp = new List<int>();
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
    }
}
