using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Find path helper
    /// </summary>
    class FindPathHelper : IDisposable
    {
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
        /// Constructor
        /// </summary>
        /// <param name="nav">Navigation mesh</param>
        /// <param name="maxNodes">Maximum number of nodes in the search list</param>
        public FindPathHelper(NavMesh nav, int maxNodes)
        {
            ArgumentNullException.ThrowIfNull(nav);

            m_nav = nav;
            m_nodePool = new(maxNodes, Helper.NextPowerOfTwo(maxNodes / 4));
            m_openList = new(maxNodes);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FindPathHelper()
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

            var startNode = m_nodePool.AllocateNode(start.Ref, 0);
            startNode.Ref = start.Ref;
            startNode.Pos = start.Pos;
            startNode.Flags = NodeFlagTypes.Open;
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = Vector3.Distance(start.Pos, end.Pos) * H_SCALE;
            m_openList.Push(startNode);

            var lastBestNode = startNode;
            float lastBestNodeCost = startNode.Total;

            bool outOfNodes = false;

            while (!m_openList.Empty())
            {
                // Remove node from open list and put it in closed list.
                var bestNode = m_openList.Pop();
                bestNode.SetClosed();

                // Reached the goal, stop searching.
                if (bestNode.Ref == end.Ref)
                {
                    lastBestNode = bestNode;
                    break;
                }

                Status pStatus = ProcessOpenList(filter, end, bestNode, ref lastBestNode, ref lastBestNodeCost, out outOfNodes);
                if (pStatus == Status.DT_IN_PROGRESS)
                {
                    continue;
                }

                return pStatus;
            }

            Status status = GetPathToNode(lastBestNode, maxPath, out resultPath);

            if (lastBestNode.Ref != end.Ref)
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
        /// Process open list
        /// </summary>
        /// <param name="filter">Query filter</param>
        /// <param name="target">Target point</param>
        /// <param name="bestNode">Best node</param>
        /// <param name="lastBestNode">Updates the last best node</param>
        /// <param name="lastBestNodeCost">Updates the last best node cost</param>
        /// <param name="outOfNodes">Returns whether the path is out of nodes</param>
        /// <returns>Process results</returns>
        private Status ProcessOpenList(QueryFilter filter, PathPoint target, Node bestNode, ref Node lastBestNode, ref float lastBestNodeCost, out bool outOfNodes)
        {
            outOfNodes = false;

            // Get current poly and tile.
            // The API input has been cheked already, skip checking internal data.
            var best = m_nav.GetTileAndPolyByRefUnsafe(bestNode.Ref);

            // Get parent poly and tile.
            TileRef parent = GetParentTileRef(bestNode);

            foreach (var link in best.IteratePolygonLinks())
            {
                if (!ValidateNeighbourNode(link, filter, parent, out var neighbour, out var neighbourNode, out outOfNodes))
                {
                    continue;
                }

                // If the node is visited the first time, calculate node position.
                if (neighbourNode.Flags == NodeFlagTypes.None)
                {
                    var midPointRes = TileRef.GetEdgeMidPoint(best, neighbour, out var pos);
                    if (midPointRes != Status.DT_SUCCESS)
                    {
                        Logger.WriteWarning(this, $"FindPath GetEdgeMidPoint result: {midPointRes}");
                        return midPointRes;
                    }

                    neighbourNode.Pos = pos;
                }

                // Calculate cost and heuristic.
                var (isBest, heuristic) = CalculateCostAndHeuristics(filter, target, bestNode, neighbourNode, parent, best, neighbour);
                if (!isBest)
                {
                    continue;
                }

                // Update nearest node to target so far.
                if (heuristic < lastBestNodeCost)
                {
                    lastBestNodeCost = heuristic;
                    lastBestNode = neighbourNode;
                }
            }

            return Status.DT_IN_PROGRESS;
        }
        /// <summary>
        /// Gets and validates the neighbours from the specified link
        /// </summary>
        /// <param name="link">Link</param>
        /// <param name="filter">Query filter</param>
        /// <param name="parent">Parent tile</param>
        /// <param name="neighbour">Resulting neighbour tile</param>
        /// <param name="neighbourNode">Resulting neighbour node</param>
        /// <param name="outOfNodes">Gets whether the pool is out of nodes</param>
        private bool ValidateNeighbourNode(Link link, QueryFilter filter, TileRef parent, out TileRef neighbour, out Node neighbourNode, out bool outOfNodes)
        {
            neighbour = TileRef.Null;
            neighbourNode = null;
            outOfNodes = false;

            int neighbourRef = link.NRef;

            // Skip invalid ids
            if (neighbourRef == 0)
            {
                return false;
            }

            // Do not expand back to where we came from.
            if (neighbourRef == parent.Ref)
            {
                return false;
            }

            // Get neighbour poly and tile.
            // The API input has been cheked already, skip checking internal data.
            neighbour = m_nav.GetTileAndPolyByRefUnsafe(neighbourRef);

            if (!filter.PassFilter(neighbour.Poly.Flags))
            {
                return false;
            }

            // deal explicitly with crossing tile boundaries
            int crossSide = 0;
            if (link.Side != 0xff)
            {
                crossSide = link.Side >> 1;
            }

            // get the node
            neighbourNode = m_nodePool.AllocateNode(neighbourRef, crossSide);
            if (neighbourNode == null)
            {
                outOfNodes = true;

                return false;
            }

            return true;
        }
        /// <summary>
        /// Calculates cost and heuristics
        /// </summary>
        /// <param name="filter">Query filter</param>
        /// <param name="target">Target point</param>
        /// <param name="pa">Current node</param>
        /// <param name="pb">Next node</param>
        /// <param name="prev">Previuos tile</param>
        /// <param name="cur">Current tile</param>
        /// <param name="next">Next tile</param>
        /// <returns>Returns whether the next node has best cost, and the heuristics value</returns>
        private (bool isBest, float heuristic) CalculateCostAndHeuristics(QueryFilter filter, PathPoint target, Node pa, Node pb, TileRef prev, TileRef cur, TileRef next)
        {
            float curCost = filter.GetCost(pa.Pos, pb.Pos, prev, cur, next);
            float cost = pa.Cost + curCost;
            float heuristic = Vector3.Distance(pb.Pos, target.Pos) * H_SCALE;

            if (next.Ref == target.Ref)
            {
                // Special case for last node.
                float endCost = filter.GetCost(pb.Pos, target.Pos, cur, next, TileRef.Null);

                // Cost
                cost += endCost;
                heuristic = 0;
            }

            float total = cost + heuristic;

            // The node is already in open list and the new result is worse, skip.
            if (pb.IsOpen && total >= pb.Total)
            {
                return (false, 0);
            }

            // The node is already visited and process, and the new result is worse, skip.
            if (pb.IsClosed && total >= pb.Total)
            {
                return (false, 0);
            }

            // Add or update the node.
            pb.PIdx = m_nodePool.GetNodeIdx(pa);
            pb.Ref = next.Ref;
            pb.Cost = cost;
            pb.Total = total;
            pb.RemoveClosed();

            if (pb.IsOpen)
            {
                // Already in open, update node location.
                m_openList.Modify(pb);
            }
            else
            {
                // Put the node in open list.
                pb.SetOpened();
                m_openList.Push(pb);
            }

            return (true, heuristic);
        }
        /// <summary>
        /// Gets the parent tile reference of the specified node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the parent tile reference</returns>
        private TileRef GetParentTileRef(Node node)
        {
            // Get parent poly and tile.
            if (node.PIdx == 0)
            {
                return TileRef.Null;
            }

            int parentRef = m_nodePool.GetNodeAtIdx(node.PIdx).Ref;
            if (parentRef == 0)
            {
                return TileRef.Null;
            }

            return m_nav.GetTileAndPolyByRefUnsafe(parentRef);
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
                tmp.Insert(0, curNode.Ref);
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
