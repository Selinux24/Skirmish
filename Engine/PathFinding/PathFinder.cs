using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.PathFinding
{
    using Engine.Collections;
    using Engine.Common;

    /// <summary>
    /// Path finder class
    /// </summary>
    public static class PathFinder
    {
        /// <summary>
        /// Cached paths
        /// </summary>
        private static List<PathCache> Cache = new List<PathCache>(10);
        /// <summary>
        /// Paths to validate
        /// </summary>
        private static PriorityQueue<GridNode, float> OpenPathsQueue = new PriorityQueue<GridNode, float>();

        /// <summary>
        /// Heuristic estimate value
        /// </summary>
        public static int HeuristicEstimateValue = 8;
        /// <summary>
        /// Calc method
        /// </summary>
        public static HeuristicMethods HeuristicMethod = HeuristicMethods.Manhattan;

        /// <summary>
        /// Gets the path from start to end
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        /// <returns>Returns the path from start to end</returns>
        public static Path FindPath(GridNode start, GridNode end)
        {
            PathCache cachedPath = Cache.Find(p => p.Start == start && p.End == end);
            if (cachedPath != null)
            {
                return cachedPath.Path;
            }
            else
            {
                Path path = CalcReturnPath(start, end);

                if (Cache.Count >= 10)
                {
                    Cache.RemoveAt(0);
                }

                Cache.Add(new PathCache()
                {
                    Path = path,
                    Start = start,
                    End = end,
                });

                return path;
            }
        }
        /// <summary>
        /// Gets the path from start to end
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        /// <returns>Returns the path from start to end</returns>
        private static Path CalcReturnPath(GridNode start, GridNode end)
        {
            //Set dictionary
            Dictionary<GridNode, PathFinderData> nodes = new Dictionary<GridNode, PathFinderData>();

            //Clear queue
            OpenPathsQueue.Clear();

            //Add first node
            OpenPathsQueue.Enqueue(start, 1);
            nodes.Add(start, new PathFinderData());

            bool nodeFound = false;
            while (OpenPathsQueue.Count > 0)
            {
                //Dequeue the node with lower priority
                PriorityQueueItem<GridNode, float> item = OpenPathsQueue.Dequeue();

                GridNode currentNode = item.Value;
                PathFinderData currentNodeData = nodes[currentNode];

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
                        //Search every possible direction from the current node
                        for (int i = 1; i < 9; i++)
                        {
                            Headings heading = (Headings)i;

                            if (currentNode.ConnectedNodes.ContainsKey(heading))
                            {
                                GridNode nextNode = currentNode.ConnectedNodes[heading];

                                if (!nodes.ContainsKey(nextNode))
                                {
                                    nodes.Add(nextNode, new PathFinderData());
                                }

                                PathFinderData nextNodeData = nodes[nextNode];

                                if (nextNode.State == GridNodeStates.Closed)
                                {
                                    //Impassable node
                                    continue;
                                }

                                if (nextNodeData.State == GridNodeStates.Closed)
                                {
                                    //Closed node
                                    continue;
                                }

                                float newGone = currentNode.Cost + ((int)nextNodeData.State);

                                if (nextNodeData.State == GridNodeStates.Clear)
                                {
                                    if (nextNode.Cost < newGone)
                                    {
                                        continue;
                                    }
                                }

                                nextNodeData.NextNode = currentNode;
                                nextNodeData.Cost = newGone;
                                nextNodeData.State = GridNodeStates.Clear;

                                //Calculate priority
                                float heuristicValue = CalcHeuristic(
                                    HeuristicMethod,
                                    HeuristicEstimateValue,
                                    nextNode.Center,
                                    end.Center);

                                OpenPathsQueue.Enqueue(nextNode, newGone + heuristicValue);
                            }
                        }
                    }
                }
            }

            if (nodeFound)
            {
                //We found a valid path
                List<GridNode> solvedList = new List<GridNode>();

                GridNode node = end;
                while (node != null)
                {
                    solvedList.Insert(0, node);

                    node = nodes[node].NextNode;
                }

                return new Path(solvedList.ToArray());
            }
            else
            {
                //If no result...
                return null;
            }
        }
        /// <summary>
        /// Calculate the heuristic value as the start and end positions
        /// </summary>
        /// <param name="method">Calculation method</param>
        /// <param name="heuristicEstimateValue">Estimated value</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <returns>Returns the heuristic value according to the start and end positions</returns>
        private static float CalcHeuristic(HeuristicMethods method, float heuristicEstimateValue, Vector3 start, Vector3 end)
        {
            if (method == HeuristicMethods.Euclidean)
            {
                //Euclidean
                return heuristicEstimateValue * (float)(Math.Sqrt(Math.Pow((start.X - end.X), 2) + Math.Pow((start.Y - end.Y), 2) + Math.Pow((start.Z - end.Z), 2)));
            }
            else if (method == HeuristicMethods.Manhattan)
            {
                //Manhattan
                return heuristicEstimateValue * (Math.Abs(end.X - start.X) + Math.Abs(end.Y - start.Y) + Math.Abs(end.Z - start.Z));
            }
            else if (method == HeuristicMethods.DiagonalDistance)
            {
                //Diagonal distance
                return heuristicEstimateValue * (Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.X - end.Z)));
            }
            else
            {
                throw new ArgumentException(string.Format("Calculation method {0} not valid.", method));
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
            public Path Path;
        }
    }
}
