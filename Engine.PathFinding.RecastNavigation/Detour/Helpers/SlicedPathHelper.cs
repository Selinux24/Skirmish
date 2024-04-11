using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Sliced find path helper
    /// </summary>
    class SlicedPathHelper : IDisposable
    {
        /// <summary>
        /// Limit raycasting during any angle pahfinding
        /// The limit is given as a multiple of the character radius
        /// </summary>
        const float DT_RAY_CAST_LIMIT_PROPORTIONS = 50.0f;
        /// <summary>
        /// Search heuristic scale.
        /// </summary>
        const float H_SCALE = 0.999f;

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
        /// Sliced query state.
        /// </summary>
        private QueryData m_query = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nav">Navigation mesh</param>
        /// <param name="maxNodes">Maximum number of nodes in the search list</param>
        public SlicedPathHelper(NavMesh nav, int maxNodes)
        {
            ArgumentNullException.ThrowIfNull(nav);

            m_nav = nav;
            m_nodePool = new(maxNodes, Helper.NextPowerOfTwo(maxNodes / 4));
            m_openList = new(maxNodes);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SlicedPathHelper()
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
                m_query.RaycastLimitSqr = MathF.Pow(agentRadius * DT_RAY_CAST_LIMIT_PROPORTIONS, 2);
            }

            if (start.Ref == end.Ref)
            {
                m_query.Status = Status.DT_SUCCESS;
                return Status.DT_SUCCESS;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            var startNode = m_nodePool.AllocateNode(start.Ref, 0);
            startNode.Pos = start.Pos;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = Vector3.Distance(start.Pos, end.Pos) * H_SCALE;
            startNode.Ref = start.Ref;
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
                if (bestNode.Ref == m_query.EndRef)
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
                    grandpaRef = m_nodePool.GetNodeAtIdx(parentNode.PIdx).Ref;
                }
            }

            TileRef parent = TileRef.Null;
            if (parentNode?.Ref > 0)
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
                ((parentNode?.Ref ?? 0) != 0) &&
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
                if (neighbourRef == 0 || neighbourRef == parentNode?.Ref)
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
                Node neighbourNode = m_nodePool.AllocateNode(neighbourRef, 0);
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
                var midPointRes = TileRef.GetEdgeMidPoint(best, neighbour, out var pos);
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
            neighbour.Node.Ref = neighbour.Ref;
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

            m_nav.Raycast(request, out var rayHit);

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
                if (m_query.LastBestNode.Ref != m_query.EndRef)
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
                    var (nodes, _) = m_nodePool.FindNodes(existing[i], 1);
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
                    StartRef = node.Ref,
                    StartPos = node.Pos,
                    EndPos = next.Pos,
                    Filter = m_query.Filter,
                    MaxPath = maxPath,
                };

                status = m_nav.Raycast(request, out var hit);
                if (status.HasFlag(Status.DT_SUCCESS))
                {
                    var rPath = hit.CreateSimplePath();
                    pathList.AddRange(rPath.GetPath());
                }

                // raycast ends on poly boundary and the path might include the next poly boundary.
                if (pathList.Count > 0 && pathList[^1] == next.Ref)
                {
                    pathList.RemoveAt(pathList.Count - 1); // remove to avoid duplicates
                }
            }
            else
            {
                pathList.Add(node.Ref);

                if (pathList.Count >= maxPath)
                {
                    status = Status.DT_BUFFER_TOO_SMALL;
                }
            }

            partialPath = [.. pathList];

            return status;
        }
    }
}
