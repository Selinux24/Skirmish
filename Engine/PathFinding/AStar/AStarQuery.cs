using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.AStar
{
    using Engine.Collections;
    using Engine.PathFinding;

    /// <summary>
    /// Path finder class
    /// </summary>
    public static class AStarQuery
    {
        /// <summary>
        /// Constant for second diagonal distance method
        /// </summary>
        private static readonly float ChebisevCnt = (float)Math.Sqrt(2) - 2f;

        /// <summary>
        /// Cached paths
        /// </summary>
        private static List<PathCache> Cache = new List<PathCache>();

        /// <summary>
        /// Gets the path from start to end
        /// </summary>
        /// <param name="graph">Grid</param>
        /// <param name="startPosition">Start point</param>
        /// <param name="endPosition">End point</param>
        /// <param name="heuristicMethod">Heuristic metod (Diagonal distance 2 by default)</param>
        /// <param name="heuristicEstimateValue">Heuristic estimate value (8 by default)</param>
        /// <returns>Returns the path from start to end</returns>
        public static Vector3[] FindPath(Grid graph, Vector3 startPosition, Vector3 endPosition, HeuristicMethods heuristicMethod = HeuristicMethods.DiagonalDistance2, int heuristicEstimateValue = 8)
        {
            GridNode start = graph.FindNode(startPosition);
            GridNode end = graph.FindNode(endPosition);
            if (start != null && end != null)
            {
                PathCache cachedPath = Cache.Find(p => p.Start == start && p.End == end);
                if (cachedPath != null)
                {
                    //Return path
                    return cachedPath.Path;
                }
                else
                {
                    //Calculate return path
                    Vector3[] solvedList = CalcReturnPath(start, end, heuristicMethod, heuristicEstimateValue);
                    if (solvedList != null && solvedList.Length > 0)
                    {
                        //Generate path
                        var path = solvedList;

                        //Update queue
                        if (Cache.Count >= 10) Cache.RemoveAt(0);

                        //Add path to cache
                        Cache.Add(new PathCache()
                        {
                            Path = path,
                            Start = start,
                            End = end,
                        });

                        return path;
                    }
                }
            }

            return new Vector3[] { };
        }
        /// <summary>
        /// Gets the path from start to end
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="end">End node</param>
        /// <param name="heuristicMethod">Heuristic metod</param>
        /// <param name="heuristicEstimateValue">Heuristic estimate value</param>
        /// <returns>Returns the path from start to end</returns>
        private static Vector3[] CalcReturnPath(GridNode start, GridNode end, HeuristicMethods heuristicMethod, int heuristicEstimateValue)
        {
            //New queue
            PriorityDictionary<GridNode, float> openPathsQueue = new PriorityDictionary<GridNode, float>();
            //Data dictionary
            Dictionary<GridNode, AStarQueryData> nodesData = new Dictionary<GridNode, AStarQueryData>();

            //Add first node
            openPathsQueue.Enqueue(start, 1);
            nodesData.Add(start, new AStarQueryData());

            bool nodeFound = false;
            while (openPathsQueue.Count > 0)
            {
                //Dequeue the node with lower priority
                var item = openPathsQueue.Dequeue();

                var currentNode = item.Value;
                var currentNodeData = nodesData[currentNode];

                //If the node is not closed to continue the process
                if (currentNodeData.State != GridNodeStates.Closed)
                {
                    //Set the node status Closed
                    currentNodeData.State = GridNodeStates.Closed;

                    //If the current node is the destination node has found the way
                    if (currentNode == end)
                    {
                        currentNodeData.State = GridNodeStates.Closed;
                        nodeFound = true;

                        break;
                    }
                    else
                    {
                        //Process neigbors
                        ProcessNeighbors(
                            openPathsQueue, nodesData,
                            currentNode, end,
                            heuristicMethod, heuristicEstimateValue);
                    }
                }
            }

            if (nodeFound)
            {
                //We found a valid path
                List<Vector3> solvedList = new List<Vector3>();

                var node = end;
                while (node != null)
                {
                    solvedList.Insert(0, node.Center);

                    node = nodesData[node].NextNode;
                }

                return solvedList.ToArray();
            }
            else
            {
                //If no result...
                return new Vector3[] { };
            }
        }
        /// <summary>
        /// Adds nodes to queue by priority, processing the specified node neighbors
        /// </summary>
        /// <param name="openPathsQueue">Queue</param>
        /// <param name="nodesData">Nodes data dictionary</param>
        /// <param name="currentNode">Current node</param>
        /// <param name="end">End node</param>
        /// <param name="heuristicMethod">Heuristic metod</param>
        /// <param name="heuristicEstimateValue">Heuristic estimate value</param>
        private static void ProcessNeighbors(PriorityDictionary<GridNode, float> openPathsQueue, Dictionary<GridNode, AStarQueryData> nodesData, GridNode currentNode, GridNode end, HeuristicMethods heuristicMethod, int heuristicEstimateValue)
        {
            //Search every possible direction from the current node
            for (int i = 1; i < currentNode.Connections.Length; i++)
            {
                var nextNode = currentNode[i];
                if (nextNode == null)
                {
                    continue;
                }

                if (!nodesData.ContainsKey(nextNode))
                {
                    nodesData.Add(nextNode, new AStarQueryData());
                }

                if (nextNode.State == GridNodeStates.Closed)
                {
                    //Impassable node
                    continue;
                }

                var nextNodeData = nodesData[nextNode];
                if (nextNodeData.State == GridNodeStates.Closed)
                {
                    //Closed node
                    continue;
                }

                float newGone = currentNode.TotalCost + ((int)nextNodeData.State);

                if (nextNodeData.State == GridNodeStates.Clear && nextNode.TotalCost < newGone)
                {
                    continue;
                }

                nextNodeData.NextNode = currentNode;
                nextNodeData.Cost = newGone;
                nextNodeData.State = GridNodeStates.Clear;

                //Calculate priority from next to end
                float heuristicValue = CalcHeuristic(
                    nextNode.Center,
                    end.Center,
                    heuristicMethod);

                openPathsQueue.Enqueue(nextNode, newGone + (heuristicEstimateValue * heuristicValue));
            }
        }
        /// <summary>
        /// Calculate the heuristic value as the start and end positions
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="heuristicMethod">Calculation method</param>
        /// <returns>Returns the heuristic value according to the start and end positions</returns>
        private static float CalcHeuristic(Vector3 start, Vector3 end, HeuristicMethods heuristicMethod)
        {
            if (heuristicMethod == HeuristicMethods.Euclidean)
            {
                float dx = (end.X - start.X);
                float dz = (end.Z - start.Z);
                float h = (float)Math.Sqrt(dx * dx + dz * dz);

                return h;
            }
            else if (heuristicMethod == HeuristicMethods.Manhattan)
            {
                float dx = Math.Abs(start.X - end.X);
                float dz = Math.Abs(start.Z - end.Z);
                float h = dx + dz;

                return h;
            }
            else if (heuristicMethod == HeuristicMethods.DiagonalDistance1)
            {
                float dx = Math.Abs(start.X - end.X);
                float dz = Math.Abs(start.Z - end.Z);
                float h = Math.Max(dx, dz);

                return h;
            }
            else if (heuristicMethod == HeuristicMethods.DiagonalDistance2)
            {
                float dx = Math.Abs(start.X - end.X);
                float dz = Math.Abs(start.Z - end.Z);
                float h = (dx + dz) + ChebisevCnt * Math.Min(dx, dz);

                return h;
            }
            else if (heuristicMethod == HeuristicMethods.HexDistance)
            {
                float dx = start.X - end.X;
                float dy = start.Y - end.Y;
                float dz = dx - dy;
                float h = Math.Max(Math.Abs(dx), Math.Max(Math.Abs(dy), Math.Abs(dz)));

                return h;
            }
            else
            {
                throw new ArgumentException(string.Format("Calculation method {0} not valid.", heuristicMethod));
            }
        }

        /// <summary>
        /// Cached path
        /// </summary>
        class PathCache
        {
            /// <summary>
            /// Start node
            /// </summary>
            public GridNode Start;
            /// <summary>
            /// End node
            /// </summary>
            public GridNode End;
            /// <summary>
            /// Path
            /// </summary>
            public Vector3[] Path;
        }
    }
}
