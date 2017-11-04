using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Collections;
    using Engine.Common;

    /// <summary>
    /// Do pathfinding calculations on the TiledNavMesh
    /// </summary>
    class NavigationMeshQuery : IDisposable
    {
        /// <summary>
        /// Heuristic scale
        /// </summary>
        private const float HeuristicScale = 0.999f;

        private const int MaximumInterval = 16;

        private const int MaximumStack = 48;

        #region Helper classes

        /// <summary>
        /// Determine which list the node is in.
        /// </summary>
        [Flags]
        enum NodeFlags
        {
            /// <summary>
            /// None
            /// </summary>
            None = 0,
            /// <summary>
            /// Open list contains nodes to examine.
            /// </summary>
            Open = 0x01,
            /// <summary>
            /// Closed list stores path.
            /// </summary>
            Closed = 0x02,
            /// <summary>
            /// Parent of the node is not adjacent. Found using raycast.
            /// </summary>
            ParentDetached = 0x04
        }
        /// <summary>
        /// Every polygon becomes a Node, which contains a position and cost.
        /// </summary>
        class Node : IValueWithCost
        {
            /// <summary>
            /// Node position
            /// </summary>
            public Vector3 Position;
            /// <summary>
            /// Node cost
            /// </summary>
            public float Cost;
            /// <summary>
            /// Index to parent node
            /// </summary>
            public int ParentIndex = 30;
            /// <summary>
            /// Node flags 
            /// </summary>
            /// <remarks>0/open/closed</remarks>
            public NodeFlags Flags = 0;
            /// <summary>
            /// Polygon reference the node corresponds to
            /// </summary>
            public PolyId Id;

            /// <summary>
            /// Total cost
            /// </summary>
            public float TotalCost { get; set; }
            /// <summary>
            /// Gets if the node is into an open list
            /// </summary>
            public bool IsInOpenList
            {
                get
                {
                    return (this.Flags & NodeFlags.Open) != 0;
                }
            }
            /// <summary>
            /// Gets if the node is into a closed list
            /// </summary>
            public bool IsInClosedList
            {
                get
                {
                    return (this.Flags & NodeFlags.Closed) != 0;
                }
            }

            /// <summary>
            /// Opens the node
            /// </summary>
            public void SetNodeFlagOpen()
            {
                this.Flags |= NodeFlags.Open;
            }
            /// <summary>
            /// Closes the node
            /// </summary>
            public void SetNodeFlagClosed()
            {
                this.Flags &= ~NodeFlags.Open;
                this.Flags |= NodeFlags.Closed;
            }
            /// <summary>
            /// Removes closed flag
            /// </summary>
            /// <returns>Returns the resulting flag</returns>
            public NodeFlags RemoveNodeFlagClosed()
            {
                return this.Flags & ~NodeFlags.Closed;
            }
        }
        /// <summary>
        /// Link all nodes together. Store indices in hash map.
        /// </summary>
        class NodePool
        {
            private List<Node> nodes;
            private Dictionary<PolyId, Node> nodeDict;
            private int maxNodes;

            /// <summary>
            /// Initializes a new instance of the <see cref="NodePool"/> class.
            /// </summary>
            /// <param name="maxNodes">The maximum number of nodes that can be stored</param>
            /// <param name="hashSize">The maximum number of elements in the hash table</param>
            public NodePool(int maxNodes)
            {
                this.maxNodes = maxNodes;

                nodes = new List<Node>(maxNodes);
                nodeDict = new Dictionary<PolyId, Node>();
            }

            /// <summary>
            /// Reset all the data.
            /// </summary>
            public void Clear()
            {
                nodes.Clear();
                nodeDict.Clear();
            }
            /// <summary>
            /// Try to find a node.
            /// </summary>
            /// <param name="id">Node's id</param>
            /// <returns>The node, if found. Null, if otherwise.</returns>
            public Node FindNode(PolyId id)
            {
                Node node;
                if (nodeDict.TryGetValue(id, out node))
                {
                    return node;
                }

                return null;
            }
            /// <summary>
            /// Try to find the node. If it doesn't exist, create a new node.
            /// </summary>
            /// <param name="id">Node's id</param>
            /// <returns>The node</returns>
            public Node GetNode(PolyId id)
            {
                Node node;
                if (nodeDict.TryGetValue(id, out node))
                {
                    return node;
                }

                if (nodes.Count >= maxNodes)
                {
                    return null;
                }

                Node newNode = new Node();
                newNode.ParentIndex = 0;
                newNode.Cost = 0;
                newNode.TotalCost = 0;
                newNode.Id = id;
                newNode.Flags = 0;

                nodes.Add(newNode);
                nodeDict.Add(id, newNode);

                return newNode;
            }
            /// <summary>
            /// Gets the id of the node.
            /// </summary>
            /// <param name="node">The node</param>
            /// <returns>The id</returns>
            public int GetNodeIdx(Node node)
            {
                if (node == null)
                {
                    return 0;
                }

                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] == node)
                    {
                        return i + 1;
                    }
                }

                return 0;
            }
            /// <summary>
            /// Return a node at a certain index. If index is out-of-bounds, return null.
            /// </summary>
            /// <param name="idx">Node index</param>
            /// <returns></returns>
            public Node GetNodeAtIdx(int idx)
            {
                if (idx <= 0 || idx > nodes.Count)
                {
                    return null;
                }

                return nodes[idx - 1];
            }
        }
        /// <summary>
        /// Query data
        /// </summary>
        class QueryData : IDisposable
        {
            public bool Status;
            public Node LastBestNode;
            public float LastBestNodeCost;
            public PathPoint Start;
            public PathPoint End;
            public FindPathOptions Options = new FindPathOptions();
            public float RaycastLimitSquared = 0f;
            public NavigationMeshQueryFilter Filter;

            /// <summary>
            /// Constructor
            /// </summary>
            public QueryData()
            {
                this.Filter = new NavigationMeshQueryFilter();
            }

            public void Dispose()
            {
                if (this.Filter != null)
                {
                    this.Filter = null;
                }
            }
        }
        /// <summary>
        /// Segment interval
        /// </summary>
        struct SegmentInterval
        {
            /// <summary>
            /// Polygon reference
            /// </summary>
            public PolyId Reference;
            /// <summary>
            /// Min
            /// </summary>
            public int TMin;
            /// <summary>
            /// Max
            /// </summary>
            public int TMax;
        }

        #endregion

        /// <summary>
        /// Tiled navigation mesh
        /// </summary>
        private TiledNavigationMesh navigationMesh = null;
        /// <summary>
        /// Area cost array
        /// </summary>
        private float[] areaCost;
        /// <summary>
        /// Node pool
        /// </summary>
        private NodePool nodePool;
        /// <summary>
        /// Priority queue
        /// </summary>
        private PriorityQueue<Node> openList;
        /// <summary>
        /// Query data
        /// </summary>
        private QueryData queryData;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationMeshQuery"/> class.
        /// </summary>
        /// <param name="nav">The navigation mesh to query.</param>
        /// <param name="maxNodes">The maximum number of nodes that can be queued in a query.</param>
        public NavigationMeshQuery(TiledNavigationMesh nav, int maxNodes)
        {
            this.navigationMesh = nav;

            this.areaCost = new float[byte.MaxValue + 1];
            for (int i = 0; i < this.areaCost.Length; i++)
            {
                this.areaCost[i] = 1.0f;
            }

            this.nodePool = new NodePool(maxNodes);
            this.openList = new PriorityQueue<Node>(maxNodes);
        }
        /// <summary>
        /// Resources dispose
        /// </summary>
        public void Dispose()
        {
            this.areaCost = null;

            if (this.nodePool != null)
            {
                this.nodePool.Clear();
                this.nodePool = null;
            }

            if (this.openList != null)
            {
                this.openList.Clear();
                this.openList = null;
            }

            Helper.Dispose(this.queryData);
        }

        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(Vector3 position, out Vector3? nearest)
        {
            var pt = this.FindNearestPoly(position, Vector3.Zero);

            if (pt.Polygon != PolyId.Null)
            {
                nearest = pt.Position;

                return pt.Position.X == position.X && pt.Position.Z == position.Z;
            }
            else
            {
                nearest = null;

                return false;
            }
        }

        /// <summary>
        /// Find a straight path
        /// </summary>
        /// <param name="from">Starting position</param>
        /// <param name="to">Ending position</param>
        /// <param name="resultPath">The straight path</param>
        /// <returns>True, if path found. False, if otherwise.</returns>
        public bool FindPath(Vector3 from, Vector3 to, out Vector3[] resultPath)
        {
            resultPath = null;

            var startPt = this.FindNearestPoly(from, Vector3.Zero);
            var endPt = this.FindNearestPoly(to, Vector3.Zero);

            if (startPt.Polygon == PathPoint.Null.Polygon)
            {
                startPt = this.FindNearestPoly(from, new Vector3(0, from.Y, 0));
            }
            if (endPt.Polygon == PathPoint.Null.Polygon)
            {
                endPt = this.FindNearestPoly(to, new Vector3(0, to.Y, 0));
            }

            PolygonPath path;
            if (this.FindPath(startPt, endPt, out path))
            {
                StraightPath stPath;
                if (FindStraightPath(from, to, path, PathBuildFlags.AllCrossingVertices, out stPath))
                {
                    resultPath = new Vector3[stPath.Count];
                    for (int i = 0; i < stPath.Count; i++)
                    {
                        resultPath[i] = stPath[i].Point.Position;
                    }

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Find a path from the start polygon to the end polygon.
        /// -If the end polygon can't be reached, the last polygon will be nearest the end polygon
        /// -If the path array is too small, it will be filled as far as possible 
        /// -start and end positions are used to calculate traversal costs
        /// </summary>
        /// <param name="startPt">The start point.</param>
        /// <param name="endPt">The end point.</param>
        /// <param name="resultPath">The path of polygon references</param>
        /// <returns>True, if path found. False, if otherwise.</returns>
        private bool FindPath(PathPoint startPt, PathPoint endPt, out PolygonPath resultPath)
        {
            resultPath = null;

            //validate input
            PolyId startRef = startPt.Polygon;
            PolyId endRef = endPt.Polygon;
            if (startRef == PolyId.Null || endRef == PolyId.Null)
            {
                return false;
            }
            if (!this.navigationMesh.IsValidPolyRef(startRef) || !this.navigationMesh.IsValidPolyRef(endRef))
            {
                return false;
            }

            //special case: both start and end are in the same polygon
            if (startRef == endRef)
            {
                resultPath = new PolygonPath();
                resultPath.Add(startRef);
                return true;
            }

            Vector3 startPos = startPt.Position;
            Vector3 endPos = endPt.Position;

            this.nodePool.Clear();
            this.openList.Clear();

            //initial node is located at the starting position
            Node startNode = this.nodePool.GetNode(startRef);
            startNode.Position = startPos;
            startNode.ParentIndex = 0;
            startNode.Cost = 0;
            startNode.TotalCost = (startPos - endPos).Length() * HeuristicScale;
            startNode.Id = startRef;
            startNode.Flags = NodeFlags.Open;

            this.openList.Push(startNode);

            Node lastBestNode = startNode;
            float lastBestTotalCost = startNode.TotalCost;

            while (this.openList.Count > 0)
            {
                //remove node from open list and put it in closed list
                Node bestNode = this.openList.Pop();
                bestNode.SetNodeFlagClosed();

                //reached the goal. stop searching
                if (bestNode.Id == endRef)
                {
                    lastBestNode = bestNode;
                    break;
                }

                //get current poly and tile
                PolyId bestRef = bestNode.Id;
                MeshTile bestTile;
                Poly bestPoly;
                this.navigationMesh.TryGetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                //get parent poly and tile
                PolyId parentRef = PolyId.Null;
                if (bestNode.ParentIndex != 0)
                {
                    parentRef = this.nodePool.GetNodeAtIdx(bestNode.ParentIndex).Id;
                }

                //examine neighbors
                foreach (Link link in bestPoly.Links)
                {
                    PolyId neighborRef = link.Reference;

                    //skip invalid ids and do not expand back to where we came from
                    if (neighborRef == PolyId.Null || neighborRef == parentRef)
                    {
                        continue;
                    }

                    //get neighbor poly and tile
                    MeshTile neighborTile;
                    Poly neighborPoly;
                    this.navigationMesh.TryGetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    Node neighborNode = this.nodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        continue;
                    }

                    //if node is visited the first time, calculate node position
                    if (neighborNode.Flags == NodeFlags.None)
                    {
                        this.GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, out neighborNode.Position);
                    }

                    //calculate cost and heuristic
                    float cost = 0;
                    float heuristic = 0;

                    //special case for last node
                    if (neighborRef == endRef)
                    {
                        //cost
                        float curCost = this.GetCost(bestNode.Position, neighborNode.Position, bestPoly);
                        float endCost = this.GetCost(neighborNode.Position, endPos, neighborPoly);

                        cost = bestNode.Cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        //cost
                        float curCost = this.GetCost(bestNode.Position, neighborNode.Position, bestPoly);

                        cost = bestNode.Cost + curCost;
                        heuristic = (neighborNode.Position - endPos).Length() * HeuristicScale;
                    }

                    float total = cost + heuristic;

                    //the node is already in open list and new result is worse, skip
                    if (neighborNode.IsInOpenList && total >= neighborNode.TotalCost)
                    {
                        continue;
                    }

                    //the node is already visited and processesd, and the new result is worse, skip
                    if (neighborNode.IsInClosedList && total >= neighborNode.TotalCost)
                    {
                        continue;
                    }

                    //add or update the node
                    neighborNode.ParentIndex = this.nodePool.GetNodeIdx(bestNode);
                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = neighborNode.RemoveNodeFlagClosed();
                    neighborNode.Cost = cost;
                    neighborNode.TotalCost = total;

                    if (neighborNode.IsInOpenList)
                    {
                        //already in open, update node location
                        this.openList.Modify(neighborNode);
                    }
                    else
                    {
                        //put the node in the open list
                        neighborNode.SetNodeFlagOpen();
                        this.openList.Push(neighborNode);
                    }

                    //update nearest node to target so far
                    if (heuristic < lastBestTotalCost)
                    {
                        lastBestTotalCost = heuristic;
                        lastBestNode = neighborNode;
                    }
                }
            }

            //save path
            resultPath = new PolygonPath();
            Node node = lastBestNode;
            do
            {
                resultPath.Add(node.Id);

                node = this.nodePool.GetNodeAtIdx(node.ParentIndex);
            }
            while (node != null);

            //reverse the path since it's backwards
            resultPath.Reverse();

            return true;
        }
        /// <summary>
        /// Add vertices and portals to a regular path computed from the method FindPath
        /// </summary>
        /// <param name="from">Starting position</param>
        /// <param name="to">Ending position</param>
        /// <param name="path">Path of polygon references</param>
        /// <param name="options">Options flag</param>
        /// <param name="straightPath">The straight path</param>
        /// <returns>True, if path found. False, if otherwise.</returns>
        public bool FindStraightPath(Vector3 from, Vector3 to, PolygonPath path, PathBuildFlags options, out StraightPath straightPath)
        {
            straightPath = null;

            if (path.Count == 0)
            {
                return false;
            }

            Vector3 closestStartPos;
            Vector3 closestEndPos;
            this.ClosestPointOnPolyBoundary(path[0], from, out closestStartPos);
            this.ClosestPointOnPolyBoundary(path[path.Count - 1], to, out closestEndPos);

            straightPath = new StraightPath();

            bool stat = straightPath.AppendVertex(new StraightPathVertex(new PathPoint(path[0], closestStartPos), StraightPathFlags.Start));
            if (!stat)
            {
                return true;
            }

            if (path.Count > 1)
            {
                Vector3 portalApex = closestStartPos;
                Vector3 portalLeft = portalApex;
                Vector3 portalRight = portalApex;
                int apexIndex = 0;
                int leftIndex = 0;
                int rightIndex = 0;

                PolyType leftPolyType = 0;
                PolyType rightPolyType = 0;

                PolyId leftPolyRef = path[0];
                PolyId rightPolyRef = path[0];

                for (int i = 0; i < path.Count; i++)
                {
                    Vector3 left = Vector3.Zero;
                    Vector3 right = Vector3.Zero;
                    PolyType fromType = PolyType.Ground;
                    PolyType toType = PolyType.Ground;

                    #region Get portal points between polygons

                    if (i + 1 < path.Count)
                    {
                        //Find next portal
                        PolyId fromId = path[i];
                        PolyId toId = path[i + 1];
                        if (!GetPortalPoints(fromId, toId, out left, out right, out fromType, out toType))
                        {
                            //failed to get portal points means toId is an invalid polygon

                            //try to clamp end point to fromId and return path so far
                            if (!this.ClosestPointOnPolyBoundary(fromId, to, out closestEndPos))
                            {
                                //failed to get closest point means first polygon is invalid
                                return false;
                            }

                            if ((options & (PathBuildFlags.AreaCrossingVertices | PathBuildFlags.AllCrossingVertices)) != 0)
                            {
                                //append portals
                                this.AppendPortals(apexIndex, i, closestEndPos, path, straightPath, options);
                            }

                            straightPath.AppendVertex(new StraightPathVertex(new PathPoint(fromId, closestEndPos), StraightPathFlags.None));

                            return true;
                        }

                        //if starting really close to the portal, advance
                        if (i == 0)
                        {
                            float t;
                            if (Intersection.PointToSegment2DSquared(portalApex, left, right, out t) < 0.001 * 0.001)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        //end of the path
                        left = closestEndPos;
                        right = closestEndPos;
                        fromType = PolyType.Ground;
                        toType = PolyType.Ground;
                    }

                    #endregion

                    //Test right vertex
                    float triArea2D;
                    Helper.Area2D(ref portalApex, ref portalRight, ref right, out triArea2D);
                    if (triArea2D <= 0.0)
                    {
                        Helper.Area2D(ref portalApex, ref portalLeft, ref right, out triArea2D);
                        if (portalApex == portalRight || triArea2D > 0.0)
                        {
                            portalRight = right;
                            rightPolyRef = (i + 1 < path.Count) ? path[i + 1] : PolyId.Null;
                            rightPolyType = toType;
                            rightIndex = i;
                        }
                        else
                        {
                            //append portals along current straight path segment
                            if ((options & (PathBuildFlags.AreaCrossingVertices | PathBuildFlags.AllCrossingVertices)) != 0)
                            {
                                stat = AppendPortals(apexIndex, leftIndex, portalLeft, path, straightPath, options);
                                if (!stat)
                                {
                                    return true;
                                }
                            }

                            portalApex = portalLeft;
                            apexIndex = leftIndex;

                            StraightPathFlags flags = 0;
                            if (leftPolyRef == PolyId.Null)
                            {
                                flags = StraightPathFlags.End;
                            }
                            else if (leftPolyType == PolyType.OffMeshConnection)
                            {
                                flags = StraightPathFlags.OffMeshConnection;
                            }

                            PolyId reference = leftPolyRef;

                            //append or update vertex
                            stat = straightPath.AppendVertex(new StraightPathVertex(new PathPoint(reference, portalApex), flags));
                            if (!stat)
                            {
                                return true;
                            }

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            //restart
                            i = apexIndex;

                            continue;
                        }
                    }

                    //left vertex
                    Helper.Area2D(ref portalApex, ref portalLeft, ref left, out triArea2D);
                    if (triArea2D >= 0.0)
                    {
                        Helper.Area2D(ref portalApex, ref portalRight, ref left, out triArea2D);
                        if (portalApex == portalLeft || triArea2D < 0.0f)
                        {
                            portalLeft = left;
                            leftPolyRef = (i + 1 < path.Count) ? path[i + 1] : PolyId.Null;
                            leftPolyType = toType;
                            leftIndex = i;
                        }
                        else
                        {
                            if ((options & (PathBuildFlags.AreaCrossingVertices | PathBuildFlags.AllCrossingVertices)) != 0)
                            {
                                stat = AppendPortals(apexIndex, rightIndex, portalRight, path, straightPath, options);
                                if (!stat)
                                {
                                    return true;
                                }
                            }

                            portalApex = portalRight;
                            apexIndex = rightIndex;

                            StraightPathFlags flags = 0;
                            if (rightPolyRef == PolyId.Null)
                            {
                                flags = StraightPathFlags.End;
                            }
                            else if (rightPolyType == PolyType.OffMeshConnection)
                            {
                                flags = StraightPathFlags.OffMeshConnection;
                            }

                            PolyId reference = rightPolyRef;

                            //append or update vertex
                            stat = straightPath.AppendVertex(new StraightPathVertex(new PathPoint(reference, portalApex), flags));
                            if (!stat)
                            {
                                return true;
                            }

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            //restart 
                            i = apexIndex;

                            continue;
                        }
                    }
                }

                //append portals along the current straight line segment
                if ((options & (PathBuildFlags.AreaCrossingVertices | PathBuildFlags.AllCrossingVertices)) != 0)
                {
                    stat = AppendPortals(apexIndex, path.Count - 1, closestEndPos, path, straightPath, options);
                    if (!stat)
                    {
                        return true;
                    }
                }
            }

            stat = straightPath.AppendVertex(new StraightPathVertex(new PathPoint(PolyId.Null, closestEndPos), StraightPathFlags.End));

            return true;
        }
        /// <summary>
        /// This method is optimized for small delta movement and a small number of polygons.
        /// If movement distance is too large, the result will form an incomplete path.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPos">End position</param>
        /// <param name="resultPos">Intermediate point</param>
        /// <param name="visited">Visited polygon references</param>
        /// <returns>True, if point found. False, if otherwise.</returns>
        public bool MoveAlongSurface(PathPoint startPoint, Vector3 endPos, out Vector3 resultPos, List<PolyId> visited)
        {
            resultPos = Vector3.Zero;

            if (this.navigationMesh == null)
            {
                return false;
            }

            //validate input
            if (startPoint.Polygon == PolyId.Null)
            {
                return false;
            }

            if (!this.navigationMesh.IsValidPolyRef(startPoint.Polygon))
            {
                return false;
            }

            visited.Clear();

            int MAX_STACK = 48;
            Queue<Node> nodeQueue = new Queue<Node>(MAX_STACK);

            NodePool tinyNodePool = new NodePool(64);

            Node startNode = tinyNodePool.GetNode(startPoint.Polygon);
            startNode.ParentIndex = 0;
            startNode.Cost = 0;
            startNode.TotalCost = 0;
            startNode.Id = startPoint.Polygon;
            startNode.Flags = NodeFlags.Closed;
            nodeQueue.Enqueue(startNode);

            Vector3 bestPos = startPoint.Position;
            float bestDist = float.MaxValue;
            Node bestNode = null;

            //search constraints
            Vector3 searchPos = Vector3.Lerp(startPoint.Position, endPos, 0.5f);
            float searchRad = (startPoint.Position - endPos).Length() / 2.0f + 0.001f;
            float searchRadSqr = searchRad * searchRad;

            while (nodeQueue.Count > 0)
            {
                //pop front
                Node curNode = nodeQueue.Dequeue();

                //get poly and tile
                PolyId curRef = curNode.Id;
                MeshTile curTile;
                Poly curPoly;
                this.navigationMesh.TryGetTileAndPolyByRefUnsafe(curRef, out curTile, out curPoly);

                //collect vertices
                Vector3[] verts = curTile.GetVertices(curPoly);

                //if target is inside poly, stop search
                if (Intersection.PointInPoly(endPos, verts))
                {
                    bestNode = curNode;
                    bestPos = endPos;
                    break;
                }

                //find wall edges and find nearest point inside walls
                for (int i = 0, j = curPoly.VertexCount - 1; i < curPoly.VertexCount; j = i++)
                {
                    //find links to neighbors
                    List<PolyId> neis = new List<PolyId>(8);

                    if ((curPoly.NeighborEdges[j] & Link.External) != 0)
                    {
                        //tile border
                        foreach (Link link in curPoly.Links)
                        {
                            if (link.Edge == j)
                            {
                                if (link.Reference != PolyId.Null)
                                {
                                    MeshTile neiTile;
                                    Poly neiPoly;
                                    this.navigationMesh.TryGetTileAndPolyByRefUnsafe(link.Reference, out neiTile, out neiPoly);

                                    if (neis.Count < neis.Capacity)
                                    {
                                        neis.Add(link.Reference);
                                    }
                                }
                            }
                        }
                    }
                    else if (curPoly.NeighborEdges[j] != 0)
                    {
                        int idx = curPoly.NeighborEdges[j] - 1;
                        PolyId reference = this.navigationMesh.GetTileRef(curTile);
                        this.navigationMesh.IdManager.SetPolyIndex(ref reference, idx, out reference);
                        neis.Add(reference); //internal edge, encode id
                    }

                    if (neis.Count == 0)
                    {
                        //wall edge, calculate distance
                        float tseg = 0;
                        float distSqr = Intersection.PointToSegment2DSquared(endPos, verts[j], verts[i], out tseg);
                        if (distSqr < bestDist)
                        {
                            //update nearest distance
                            bestPos = Vector3.Lerp(verts[j], verts[i], tseg);
                            bestDist = distSqr;
                            bestNode = curNode;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < neis.Count; k++)
                        {
                            //skip if no node can be allocated
                            Node neighborNode = tinyNodePool.GetNode(neis[k]);
                            if (neighborNode == null)
                            {
                                continue;
                            }

                            //skip if already visited
                            if ((neighborNode.Flags & NodeFlags.Closed) != 0)
                            {
                                continue;
                            }

                            //skip the link if too far from search constraint
                            float distSqr = Intersection.PointToSegment2DSquared(searchPos, verts[j], verts[i]);
                            if (distSqr > searchRadSqr)
                            {
                                continue;
                            }

                            //mark the node as visited and push to queue
                            if (nodeQueue.Count < MAX_STACK)
                            {
                                neighborNode.ParentIndex = tinyNodePool.GetNodeIdx(curNode);
                                neighborNode.Flags |= NodeFlags.Closed;
                                nodeQueue.Enqueue(neighborNode);
                            }
                        }
                    }
                }
            }

            if ((endPos - bestPos).Length() > 1f)
            {
                return false;
            }

            if (bestNode != null)
            {
                //save the path
                Node node = bestNode;
                do
                {
                    visited.Add(node.Id);
                    if (visited.Count >= visited.Capacity)
                    {
                        break;
                    }

                    node = tinyNodePool.GetNodeAtIdx(node.ParentIndex);
                }
                while (node != null);

                //reverse the path since it's backwards
                visited.Reverse();
            }

            resultPos = bestPos;

            return true;
        }

        /// <summary>
        /// The cost between two points may vary depending on the type of polygon.
        /// </summary>
        /// <param name="pa">Point A</param>
        /// <param name="pb">Point B</param>
        /// <param name="curPoly">Current polygon</param>
        /// <returns>Cost</returns>
        private float GetCost(Vector3 pa, Vector3 pb, Poly curPoly)
        {
            return (pa - pb).Length() * this.areaCost[(int)curPoly.Area.Id];
        }
        /// <summary>
        /// Update the vertices on the straight path
        /// </summary>
        /// <param name="startIdx">Original path's starting index</param>
        /// <param name="endIdx">Original path's end index</param>
        /// <param name="endPos">The end position</param>
        /// <param name="path">The original path of polygon references</param>
        /// <param name="straightPath">The straight path</param>
        /// <param name="options">Options flag</param>
        /// <returns>True, if vertices updated. False, if otherwise.</returns>
        private bool AppendPortals(int startIdx, int endIdx, Vector3 endPos, PolygonPath path, StraightPath straightPath, PathBuildFlags options)
        {
            Vector3 startPos = straightPath[straightPath.Count - 1].Point.Position;

            //append or update last vertex
            bool stat = false;
            for (int i = startIdx; i < endIdx; i++)
            {
                //calculate portal
                PolyId from = path[i];
                MeshTile fromTile;
                Poly fromPoly;
                if (this.navigationMesh.TryGetTileAndPolyByRef(from, out fromTile, out fromPoly) == false)
                {
                    return false;
                }

                PolyId to = path[i + 1];
                MeshTile toTile;
                Poly toPoly;
                if (this.navigationMesh.TryGetTileAndPolyByRef(to, out toTile, out toPoly) == false)
                {
                    return false;
                }

                Vector3 left;
                Vector3 right;
                if (GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out left, out right) == false)
                {
                    break;
                }

                if ((options & PathBuildFlags.AreaCrossingVertices) != 0)
                {
                    //skip intersection if only area crossings are requested
                    if (fromPoly.Area == toPoly.Area)
                    {
                        continue;
                    }
                }

                //append intersection
                float s, t;
                if (Intersection.SegmentSegment2D(ref startPos, ref endPos, ref left, ref right, out s, out t))
                {
                    Vector3 pt = Vector3.Lerp(left, right, t);

                    stat = straightPath.AppendVertex(new StraightPathVertex(new PathPoint(path[i + 1], pt), StraightPathFlags.None));
                    if (stat != true)
                    {
                        return true;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// Return false if the provided position is outside the xz-bounds.
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="pos">Current position</param>
        /// <param name="height">Resulting polygon height</param>
        /// <returns>True, if height found. False, if otherwise.</returns>
        public bool GetPolyHeight(PolyId reference, Vector3 pos, out float height)
        {
            height = 0;

            if (this.navigationMesh == null)
            {
                return false;
            }

            MeshTile tile;
            Poly poly;
            if (!this.navigationMesh.TryGetTileAndPolyByRef(reference, out tile, out poly))
            {
                return false;
            }

            //off-mesh connections don't have detail polygons
            if (poly.PolyType == PolyType.OffMeshConnection)
            {
                Vector3 closest;
                tile.ClosestPointOnPolyOffMeshConnection(poly, pos, out closest);
                height = closest.Y;
                return true;
            }
            else
            {
                int indexPoly = 0;
                for (int i = 0; i < tile.Polygons.Length; i++)
                {
                    if (tile.Polygons[i] == poly)
                    {
                        indexPoly = i;
                        break;
                    }
                }

                float h = 0;
                if (tile.ClosestHeight(indexPoly, pos, out h))
                {
                    height = h;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Get edge midpoint between two prolygons
        /// </summary>
        /// <param name="from">"From" polygon reference</param>
        /// <param name="fromPoly">"From" polygon data</param>
        /// <param name="fromTile">"From" mesh tile</param>
        /// <param name="to">"To" polygon reference</param>
        /// <param name="toPoly">"To" polygon data</param>
        /// <param name="toTile">"To" mesh tile</param>
        /// <param name="mid">Edge midpoint</param>
        /// <returns>True, if midpoint found. False, if otherwise.</returns>
        private bool GetEdgeMidPoint(PolyId from, Poly fromPoly, MeshTile fromTile, PolyId to, Poly toPoly, MeshTile toTile, out Vector3 mid)
        {
            mid = Vector3.Zero;

            Vector3 left;
            Vector3 right;
            if (!this.GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out left, out right))
            {
                return false;
            }

            mid = (left + right) * 0.5f;

            return true;
        }
        /// <summary>
        /// Find points on the left and right side.
        /// </summary>
        /// <param name="from">"From" polygon reference</param>
        /// <param name="to">"To" polygon reference</param>
        /// <param name="left">Point on the left side</param>
        /// <param name="right">Point on the right side</param>
        /// <param name="fromType">Polygon type of "From" polygon</param>
        /// <param name="toType">Polygon type of "To" polygon</param>
        /// <returns>True, if points found. False, if otherwise.</returns>
        private bool GetPortalPoints(PolyId from, PolyId to, out Vector3 left, out Vector3 right, out PolyType fromType, out PolyType toType)
        {
            left = Vector3.Zero;
            right = Vector3.Zero;

            fromType = PolyType.Ground;
            toType = PolyType.Ground;

            MeshTile fromTile;
            Poly fromPoly;
            if (this.navigationMesh.TryGetTileAndPolyByRef(from, out fromTile, out fromPoly) == false)
            {
                return false;
            }
            fromType = fromPoly.PolyType;

            MeshTile toTile;
            Poly toPoly;
            if (this.navigationMesh.TryGetTileAndPolyByRef(to, out toTile, out toPoly) == false)
            {
                return false;
            }
            toType = toPoly.PolyType;

            return this.GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out left, out right);
        }
        /// <summary>
        /// Find points on the left and right side.
        /// </summary>
        /// <param name="from">"From" polygon reference</param>
        /// <param name="fromPoly">"From" polygon data</param>
        /// <param name="fromTile">"From" mesh tile</param>
        /// <param name="to">"To" polygon reference</param>
        /// <param name="toPoly">"To" polygon data</param>
        /// <param name="toTile">"To" mesh tile</param>
        /// <param name="left">Resulting point on the left side</param>
        /// <param name="right">Resulting point on the right side</param>
        /// <returns>True, if points found. False, if otherwise.</returns>
        private bool GetPortalPoints(PolyId from, Poly fromPoly, MeshTile fromTile, PolyId to, Poly toPoly, MeshTile toTile, out Vector3 left, out Vector3 right)
        {
            left = Vector3.Zero;
            right = Vector3.Zero;

            //find the link that points to the 'to' polygon
            Link link = null;
            foreach (Link fromLink in fromPoly.Links)
            {
                if (fromLink.Reference == to)
                {
                    link = fromLink;
                    break;
                }
            }

            if (link == null)
            {
                return false;
            }

            //handle off-mesh connections
            if (fromPoly.PolyType == PolyType.OffMeshConnection)
            {
                //find link that points to first vertex
                foreach (Link fromLink in fromPoly.Links)
                {
                    if (fromLink.Reference == to)
                    {
                        int v = fromLink.Edge;
                        left = fromTile.Vertices[fromPoly.Vertices[v]];
                        right = fromTile.Vertices[fromPoly.Vertices[v]];
                        return true;
                    }
                }

                return false;
            }

            if (toPoly.PolyType == PolyType.OffMeshConnection)
            {
                //find link that points to first vertex
                foreach (Link toLink in toPoly.Links)
                {
                    if (toLink.Reference == from)
                    {
                        int v = toLink.Edge;
                        left = toTile.Vertices[toPoly.Vertices[v]];
                        right = toTile.Vertices[toPoly.Vertices[v]];
                        return true;
                    }
                }

                return false;
            }

            //find portal vertices
            int v0 = fromPoly.Vertices[link.Edge];
            int v1 = fromPoly.Vertices[(link.Edge + 1) % fromPoly.VertexCount];
            left = fromTile.Vertices[v0];
            right = fromTile.Vertices[v1];

            //if the link is at the tile boundary, clamp the vertices to tile width
            if (link.Side != BoundarySide.Internal)
            {
                //unpack portal limits
                if (link.BMin != 0 || link.BMax != 255)
                {
                    float s = 1.0f / 255.0f;
                    float tmin = link.BMin * s;
                    float tmax = link.BMax * s;
                    left = Vector3.Lerp(fromTile.Vertices[v0], fromTile.Vertices[v1], tmin);
                    right = Vector3.Lerp(fromTile.Vertices[v0], fromTile.Vertices[v1], tmax);
                }
            }

            return true;
        }

        /// <summary>
        /// Finds a random point in a NavMesh within a specified circle.
        /// </summary>
        /// <param name="center">The center point.</param>
        /// <param name="radius">The maximum distance away from the center that the random point can be. If 0, any connected point on the mesh can be returned.</param>
        /// <param name="randomPoint">A random point within the specified circle.</param>
        public bool FindRandomPointAroundCircle(PathPoint center, float radius, out PathPoint randomPoint)
        {
            randomPoint = PathPoint.Null;

            //validate input
            if (center.Polygon == PolyId.Null)
            {
                Console.Write("Null poly reference");

                return false;
            }

            if (!this.navigationMesh.IsValidPolyRef(center.Polygon))
            {
                Console.Write("Poly reference is not valid for this navmesh");

                return false;
            }

            MeshTile startTile;
            Poly startPoly;
            this.navigationMesh.TryGetTileAndPolyByRefUnsafe(center.Polygon, out startTile, out startPoly);

            nodePool.Clear();
            openList.Clear();

            Node startNode = nodePool.GetNode(center.Polygon);
            startNode.Position = center.Position;
            startNode.ParentIndex = 0;
            startNode.Cost = 0;
            startNode.TotalCost = 0;
            startNode.Id = center.Polygon;
            startNode.Flags = NodeFlags.Open;
            openList.Push(startNode);

            bool doRadiusCheck = radius != 0;

            float radiusSqr = radius * radius;
            float areaSum = 0.0f;

            PolyId randomPolyRef = PolyId.Null;

            while (openList.Count > 0)
            {
                Node bestNode = openList.Pop();
                bestNode.SetNodeFlagClosed();

                //get poly and tile
                PolyId bestRef = bestNode.Id;
                MeshTile bestTile;
                Poly bestPoly;
                this.navigationMesh.TryGetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                //place random locations on ground
                if (bestPoly.PolyType == PolyType.Ground)
                {
                    //calculate area of polygon
                    float polyArea = 0.0f;
                    float area;
                    for (int j = 2; j < bestPoly.VertexCount; j++)
                    {
                        Helper.Area2D(
                            ref bestTile.Vertices[bestPoly.Vertices[0]],
                            ref bestTile.Vertices[bestPoly.Vertices[j - 1]],
                            ref bestTile.Vertices[bestPoly.Vertices[j]],
                            out area);
                        polyArea += area;
                    }

                    //choose random polygon weighted by area using resevoir sampling
                    areaSum += polyArea;
                    float u = (float)Helper.RandomGenerator.NextDouble();
                    if (u * areaSum <= polyArea)
                    {
                        randomPolyRef = bestRef;
                    }
                }

                //get parent poly and tile
                PolyId parentRef = PolyId.Null;
                if (bestNode.ParentIndex != 0)
                {
                    parentRef = nodePool.GetNodeAtIdx(bestNode.ParentIndex).Id;
                }

                foreach (Link link in bestPoly.Links)
                {
                    PolyId neighborRef = link.Reference;

                    //skip invalid neighbors and do not follow back to parent
                    if (neighborRef == PolyId.Null || neighborRef == parentRef)
                    {
                        continue;
                    }

                    //expand to neighbor
                    MeshTile neighborTile;
                    Poly neighborPoly;
                    this.navigationMesh.TryGetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    //find edge and calculate distance to edge
                    Vector3 va;
                    Vector3 vb;
                    if (!GetPortalPoints(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, out va, out vb))
                    {
                        continue;
                    }

                    //if circle isn't touching next polygon, skip it
                    if (doRadiusCheck)
                    {
                        float tseg;
                        float distSqr = Intersection.PointToSegment2DSquared(center.Position, va, vb, out tseg);
                        if (distSqr > radiusSqr)
                        {
                            continue;
                        }
                    }

                    Node neighborNode = nodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        continue;
                    }

                    if (neighborNode.IsInClosedList)
                    {
                        continue;
                    }

                    //cost
                    if (neighborNode.Flags == 0)
                    {
                        neighborNode.Position = Vector3.Lerp(va, vb, 0.5f);
                    }

                    float total = bestNode.TotalCost + (bestNode.Position - neighborNode.Position).Length();

                    //node is already in open list and new result is worse, so skip
                    if (neighborNode.IsInOpenList && total >= neighborNode.TotalCost)
                    {
                        continue;
                    }

                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = neighborNode.RemoveNodeFlagClosed();
                    neighborNode.ParentIndex = nodePool.GetNodeIdx(bestNode);
                    neighborNode.TotalCost = total;

                    if (neighborNode.IsInOpenList)
                    {
                        openList.Modify(neighborNode);
                    }
                    else
                    {
                        neighborNode.Flags = NodeFlags.Open;
                        openList.Push(neighborNode);
                    }
                }
            }

            //TODO invalid state.
            if (randomPolyRef == PolyId.Null)
            {
                Console.Write("Poly null?");

                return false;
            }

            Vector3 randomPt;
            if (!FindRandomPointOnPoly(randomPolyRef, out randomPt))
            {
                return false;
            }

            randomPoint = new PathPoint(randomPolyRef, randomPt);

            return true;
        }
        /// <summary>
        /// Finds a random point on a polygon.
        /// </summary>
        /// <param name="polyId">Polygon to find a radom point on.</param>
        /// <param name="randomPt">Resulting random point.</param>
        public bool FindRandomPointOnPoly(PolyId polyId, out Vector3 randomPt)
        {
            randomPt = Vector3.Zero;

            MeshTile tile;
            Poly poly;
            if (!this.navigationMesh.TryGetTileAndPolyByRef(polyId, out tile, out poly))
            {
                Console.Write("Invalid polygon ID");

                return false;
            }

            Vector3[] verts = new Vector3[poly.VertexCount];
            for (int j = 0; j < poly.VertexCount; j++)
            {
                verts[j] = tile.Vertices[poly.Vertices[j]];
            }

            float s = (float)Helper.RandomGenerator.NextDouble();
            float t = (float)Helper.RandomGenerator.NextDouble();

            Polygon.RandomPointInConvexPoly(verts, s, t, out randomPt);

            float h;
            if (!this.GetPolyHeight(polyId, randomPt, out h))
            {
                Console.Write("Outside bounds?");

                return false;
            }

            randomPt.Y = h;

            return true;
        }
        /// <summary>
        /// Given a point on the polygon, find the closest point
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="pos">Current position</param>
        /// <param name="closest">Resulting closest position</param>
        /// <param name="posOverPoly">Determines whether the position can be found on the polygon</param>
        /// <returns>True, if the closest point is found. False, if otherwise.</returns>
        public bool ClosestPointOnPoly(PolyId reference, Vector3 pos, out Vector3 closest, out bool posOverPoly)
        {
            posOverPoly = false;
            closest = Vector3.Zero;

            MeshTile tile;
            Poly poly;
            if (!this.navigationMesh.TryGetTileAndPolyByRef(reference, out tile, out poly))
            {
                return false;
            }
            if (tile == null)
            {
                return false;
            }

            if (poly.PolyType == PolyType.OffMeshConnection)
            {
                Vector3 v0 = tile.Vertices[poly.Vertices[0]];
                Vector3 v1 = tile.Vertices[poly.Vertices[1]];
                float d0 = (pos - v0).Length();
                float d1 = (pos - v1).Length();
                float u = d0 / (d0 + d1);
                closest = Vector3.Lerp(v0, v1, u);
                return true;
            }

            closest = pos;

            //Clamp point to be inside the polygon
            Vector3[] verts = tile.GetVertices(poly);
            float[] edgeDistance;
            float[] edgeT;
            if (!Intersection.PointToPolygonEdgeSquared(pos, verts, out edgeDistance, out edgeT))
            {
                //Point is outside the polygon
                //Clamp to nearest edge
                float minDistance = float.MaxValue;
                int minIndex = -1;
                for (int i = 0; i < verts.Length; i++)
                {
                    if (edgeDistance[i] < minDistance)
                    {
                        minDistance = edgeDistance[i];
                        minIndex = i;
                    }
                }

                Vector3 va = verts[minIndex];
                Vector3 vb = verts[(minIndex + 1) % verts.Length];
                closest = Vector3.Lerp(va, vb, edgeT[minIndex]);
            }
            else
            {
                //Point is inside the polygon
                posOverPoly = true;
            }

            //find height at the location
            int indexPoly = Array.IndexOf(tile.Polygons, poly);
            var pd = tile.DetailMeshes[indexPoly];

            for (int j = 0; j < pd.TriangleCount; j++)
            {
                var t = tile.DetailTriangles[pd.TriangleIndex + j];

                Vector3 va;
                if (t.VertexHash0 < poly.VertexCount)
                {
                    va = tile.Vertices[poly.Vertices[t.VertexHash0]];
                }
                else
                {
                    va = tile.DetailVertices[pd.VertexIndex + (t.VertexHash0 - poly.VertexCount)];
                }

                Vector3 vb;
                if (t.VertexHash1 < poly.VertexCount)
                {
                    vb = tile.Vertices[poly.Vertices[t.VertexHash1]];
                }
                else
                {
                    vb = tile.DetailVertices[pd.VertexIndex + (t.VertexHash1 - poly.VertexCount)];
                }

                Vector3 vc;
                if (t.VertexHash2 < poly.VertexCount)
                {
                    vc = tile.Vertices[poly.Vertices[t.VertexHash2]];
                }
                else
                {
                    vc = tile.DetailVertices[pd.VertexIndex + (t.VertexHash2 - poly.VertexCount)];
                }

                float h;
                if (Intersection.PointToTriangle(pos, va, vb, vc, out h))
                {
                    closest.Y = h;
                    break;
                }
            }

            return true;
        }
        /// <summary>
        /// Given a point on a polygon, find the closest point which lies on the polygon boundary.
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="pos">Current position</param>
        /// <param name="closest">Resulting closest point</param>
        /// <returns>True, if the closest point is found. False, if otherwise.</returns>
        public bool ClosestPointOnPolyBoundary(PolyId reference, Vector3 pos, out Vector3 closest)
        {
            closest = Vector3.Zero;

            MeshTile tile;
            Poly poly;
            if (this.navigationMesh.TryGetTileAndPolyByRef(reference, out tile, out poly) == false)
            {
                return false;
            }

            tile.ClosestPointOnPolyBoundary(poly, pos, out closest);

            return true;
        }
        /// <summary>
        /// Find the nearest poly within a certain range.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="extents">Extents.</param>
        /// <returns>The neareast point.</returns>
        public PathPoint FindNearestPoly(Vector3 center, Vector3 extents)
        {
            PathPoint result;
            if (this.FindNearestPoly(center, extents, out result))
            {
                return result;
            }

            return PathPoint.Null;
        }
        /// <summary>
        /// Find the nearest poly within a certain range.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="extents">Extents.</param>
        /// <param name="nearestPt">The neareast point.</param>
        public bool FindNearestPoly(Vector3 center, Vector3 extents, out PathPoint nearestPt)
        {
            nearestPt = PathPoint.Null;

            // Get nearby polygons from proximity grid.
            PolyId[] polys;
            if (this.QueryPolygons(center, extents, out polys))
            {
                bool result = false;
                float nearestDistanceSqr = float.MaxValue;
                for (int i = 0; i < polys.Length; i++)
                {
                    PolyId reference = polys[i];
                    Vector3 closestPtPoly;
                    bool posOverPoly;
                    this.ClosestPointOnPoly(reference, center, out closestPtPoly, out posOverPoly);

                    // If a point is directly over a polygon and closer than climb height, favor that instead of straight line nearest point.
                    Vector3 diff = center - closestPtPoly;
                    float d = 0;
                    if (posOverPoly)
                    {
                        MeshTile tile;
                        Poly poly;
                        this.navigationMesh.TryGetTileAndPolyByRefUnsafe(polys[i], out tile, out poly);
                        d = Math.Abs(diff.Y) - tile.WalkableClimb;
                        d = d > 0 ? d * d : 0;
                    }
                    else
                    {
                        d = diff.LengthSquared();
                    }

                    if (d < nearestDistanceSqr)
                    {
                        nearestDistanceSqr = d;
                        nearestPt = new PathPoint(reference, closestPtPoly);
                        result = true;
                    }
                }

                return result;
            }

            return false;
        }
        /// <summary>
        /// Finds nearby polygons within a certain range.
        /// </summary>
        /// <param name="center">The starting point</param>
        /// <param name="extent">The range to search within</param>
        /// <param name="polys">A list of polygons</param>
        /// <returns>True, if successful. False, if otherwise.</returns>
        private bool QueryPolygons(Vector3 center, Vector3 extent, out PolyId[] polys)
        {
            List<PolyId> polyList = new List<PolyId>();

            Vector3 bmin = center - extent;
            Vector3 bmax = center + extent;

            int minx, miny, maxx, maxy;
            this.navigationMesh.CalcTileLoc(ref bmin, out minx, out miny);
            this.navigationMesh.CalcTileLoc(ref bmax, out maxx, out maxy);

            BoundingBox bounds = new BoundingBox(bmin, bmax);
            int n = 0;
            for (int y = miny; y <= maxy; y++)
            {
                for (int x = minx; x <= maxx; x++)
                {
                    var tiles = this.navigationMesh.GetTilesAt(x, y);

                    foreach (var neighborTile in tiles)
                    {
                        PolyId[] neighborPolys;
                        n += neighborTile.QueryPolygons(bounds, out neighborPolys);
                        polyList.AddRange(neighborPolys);
                    }
                }
            }

            polys = polyList.ToArray();

            return polyList.Count != 0;
        }

        /// <summary>
        /// Initialize a sliced path, which is used mostly for crowd pathfinding.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="filter">A filter for the navigation mesh.</param>
        /// <param name="options">Options for how the path should be found.</param>
        /// <returns>True if path initialized, false otherwise</returns>
        public bool InitSlicedFindPath(PathPoint startPoint, PathPoint endPoint, NavigationMeshQueryFilter filter, FindPathOptions options)
        {
            //validate input
            if (startPoint.Polygon == PolyId.Null || endPoint.Polygon == PolyId.Null)
            {
                return false;
            }

            if (!this.navigationMesh.IsValidPolyRef(startPoint.Polygon) || !this.navigationMesh.IsValidPolyRef(endPoint.Polygon))
            {
                return false;
            }

            //init path state
            queryData = new QueryData();
            queryData.Status = false;
            queryData.Start = startPoint;
            queryData.End = endPoint;

            if (startPoint.Polygon == endPoint.Polygon)
            {
                this.queryData.Status = true;
                return true;
            }

            nodePool.Clear();
            openList.Clear();

            Node startNode = nodePool.GetNode(startPoint.Polygon);
            startNode.Position = startPoint.Position;
            startNode.ParentIndex = 0;
            startNode.Cost = 0;
            startNode.TotalCost = (endPoint.Position - startPoint.Position).Length() * HeuristicScale;
            startNode.Id = startPoint.Polygon;
            startNode.Flags = NodeFlags.Open;
            openList.Push(startNode);

            queryData.Status = true;
            queryData.LastBestNode = startNode;
            queryData.LastBestNodeCost = startNode.TotalCost;

            return queryData.Status;
        }
        /// <summary>
        /// Update the sliced path as agents move across the path.
        /// </summary>
        /// <param name="maxIter">Maximum iterations</param>
        /// <param name="doneIters">Number of times iterated through</param>
        /// <returns>True if updated, false if not</returns>
        public bool UpdateSlicedFindPath(int maxIter, ref int doneIters)
        {
            if (queryData.Status != true)
                return queryData.Status;

            //make sure the request is still valid
            if (!this.navigationMesh.IsValidPolyRef(queryData.Start.Polygon) || !this.navigationMesh.IsValidPolyRef(queryData.End.Polygon))
            {
                queryData.Status = false;
                return false;
            }

            int iter = 0;
            while (iter < maxIter && !openList.Empty())
            {
                iter++;

                //remove node from open list and put it in closed list
                var bestNode = openList.Pop();
                bestNode.SetNodeFlagClosed();

                //reached the goal, stop searching
                if (bestNode.Id == queryData.End.Polygon)
                {
                    queryData.LastBestNode = bestNode;
                    queryData.Status = true;
                    doneIters = iter;
                    return queryData.Status;
                }

                //get current poly and tile
                PolyId bestRef = bestNode.Id;
                MeshTile bestTile;
                Poly bestPoly;
                if (!this.navigationMesh.TryGetTileAndPolyByRef(bestRef, out bestTile, out bestPoly))
                {
                    //the polygon has disappeared during the sliced query, fail
                    queryData.Status = false;
                    doneIters = iter;
                    return queryData.Status;
                }

                //get parent poly and tile
                PolyId parentRef = PolyId.Null;
                PolyId grandpaRef = PolyId.Null;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                Node parentNode = null;
                if (bestNode.ParentIndex != 0)
                {
                    parentNode = nodePool.GetNodeAtIdx(bestNode.ParentIndex);
                    parentRef = parentNode.Id;
                    if (parentNode.ParentIndex != 0)
                        grandpaRef = nodePool.GetNodeAtIdx(parentNode.ParentIndex).Id;
                }
                if (parentRef != PolyId.Null)
                {
                    bool invalidParent = !this.navigationMesh.TryGetTileAndPolyByRef(parentRef, out parentTile, out parentPoly);
                    if (invalidParent || (grandpaRef != PolyId.Null && !this.navigationMesh.IsValidPolyRef(grandpaRef)))
                    {
                        //the polygon has disappeared during the sliced query, fail
                        queryData.Status = false;
                        doneIters = iter;
                        return queryData.Status;
                    }
                }

                //decide whether to test raycast to previous nodes
                bool tryLOS = false;
                if ((queryData.Options & FindPathOptions.AnyAngle) != 0)
                {
                    if ((parentRef != PolyId.Null) && (parentNode.Position - bestNode.Position).LengthSquared() < queryData.RaycastLimitSquared)
                    {
                        tryLOS = true;
                    }
                }

                foreach (Link link in bestPoly.Links)
                {
                    PolyId neighborRef = link.Reference;

                    //skip invalid ids and do not expand back to where we came from
                    if (neighborRef == PolyId.Null || neighborRef == parentRef)
                    {
                        continue;
                    }

                    //get neighbor poly and tile
                    MeshTile neighborTile;
                    Poly neighborPoly;
                    this.navigationMesh.TryGetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    if (!queryData.Filter.PassFilter(neighborRef, neighborTile, neighborPoly))
                    {
                        continue;
                    }

                    Node neighborNode = nodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        continue;
                    }

                    if (neighborNode.ParentIndex != 0 && neighborNode.ParentIndex == bestNode.ParentIndex)
                    {
                        continue;
                    }

                    if (neighborNode.Flags == 0)
                    {
                        GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, out neighborNode.Position);
                    }

                    //calculate cost and heuristic
                    float cost = 0;
                    float heuristic = 0;

                    bool foundShortCut = false;
                    RaycastHit hit;
                    PolygonPath hitPath = null;
                    if (tryLOS)
                    {
                        PathPoint startPoint = new PathPoint(parentRef, parentNode.Position);
                        Raycast(startPoint, neighborNode.Position, grandpaRef, RaycastOptions.UseCosts, out hit, out hitPath);
                        foundShortCut = hit.HasContact;
                    }

                    if (foundShortCut)
                    {
                        cost = parentNode.Cost + hitPath.TotalCost;
                    }
                    else
                    {
                        float curCost = queryData.Filter.GetCost(bestNode.Position, neighborNode.Position,
                            parentRef, parentTile, parentPoly,
                            bestRef, bestTile, bestPoly,
                            neighborRef, neighborTile, neighborPoly);

                        cost = bestNode.Cost + curCost;
                    }

                    //special case for last node
                    if (neighborRef == queryData.End.Polygon)
                    {
                        //cost
                        float endCost = queryData.Filter.GetCost(bestNode.Position, neighborNode.Position,
                            bestRef, bestTile, bestPoly,
                            neighborRef, neighborTile, neighborPoly,
                            PolyId.Null, null, null);

                        cost = cost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        heuristic = (neighborNode.Position - queryData.End.Position).Length() * HeuristicScale;
                    }

                    float total = cost + heuristic;

                    //the node is already in open list and new result is worse, skip
                    if (neighborNode.IsInOpenList && total >= neighborNode.TotalCost)
                    {
                        continue;
                    }

                    //the node is already visited and processesd, and the new result is worse, skip
                    if (neighborNode.IsInClosedList && total >= neighborNode.TotalCost)
                    {
                        continue;
                    }

                    //add or update the node
                    neighborNode.ParentIndex = nodePool.GetNodeIdx(bestNode);
                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = neighborNode.RemoveNodeFlagClosed();
                    neighborNode.Cost = cost;
                    neighborNode.TotalCost = total;
                    if (foundShortCut)
                    {
                        neighborNode.Flags |= NodeFlags.ParentDetached;
                    }

                    if (neighborNode.IsInOpenList)
                    {
                        //already in open, update node location
                        openList.Modify(neighborNode);
                    }
                    else
                    {
                        //put the node in the open list
                        neighborNode.SetNodeFlagOpen();
                        openList.Push(neighborNode);
                    }

                    //update nearest node to target so far
                    if (heuristic < queryData.LastBestNodeCost)
                    {
                        queryData.LastBestNodeCost = heuristic;
                        queryData.LastBestNode = neighborNode;
                    }
                }
            }

            //exhausted all nodes, but could not find path
            if (openList.Empty())
            {
                queryData.Status = true;
            }

            doneIters = iter;

            return queryData.Status;
        }
        /// <summary>
        /// Save the sliced path 
        /// </summary>
        /// <param name="path">The path in terms of polygon references</param>
        /// <param name="pathCount">The path length</param>
        /// <param name="maxPath">The maximum path length allowed</param>
        /// <returns>True if the path is saved, false if not</returns>
        public bool FinalizeSlicedFindPath(PolygonPath path)
        {
            path.Clear();

            if (queryData.Status == false)
            {
                queryData = new QueryData();
                return false;
            }

            if (queryData.Start.Polygon == queryData.End.Polygon)
            {
                //special case: the search starts and ends at the same poly
                path.Add(queryData.Start.Polygon);
            }
            else
            {
                //reverse the path
                Node prev = null;
                Node node = queryData.LastBestNode;
                NodeFlags prevRay = 0;
                do
                {
                    Node next = nodePool.GetNodeAtIdx(node.ParentIndex);
                    node.ParentIndex = nodePool.GetNodeIdx(prev);
                    prev = node;
                    NodeFlags nextRay = node.Flags & NodeFlags.ParentDetached;
                    node.Flags = (node.Flags & ~NodeFlags.ParentDetached) | prevRay;
                    prevRay = nextRay;
                    node = next;
                }
                while (node != null);

                //store path
                node = prev;
                do
                {
                    Node next = nodePool.GetNodeAtIdx(node.ParentIndex);
                    if ((node.Flags & NodeFlags.ParentDetached) != 0)
                    {
                        RaycastHit hit;
                        PolygonPath m;
                        PathPoint startPoint = new PathPoint(node.Id, node.Position);
                        bool result = Raycast(startPoint, next.Position, RaycastOptions.None, out hit, out m);
                        path.AppendPath(m);

                        if (path[path.Count - 1] == next.Id)
                        {
                            path.RemoveAt(path.Count - 1);
                        }
                    }
                    else
                    {
                        path.Add(node.Id);
                    }

                    node = next;
                }
                while (node != null);
            }

            //reset query
            queryData = new QueryData();

            return true;
        }
        /// <summary>
        /// Save a partial path
        /// </summary>
        /// <param name="existing">Existing path</param>
        /// <param name="existingSize">Existing path's length</param>
        /// <param name="path">New path</param>
        /// <param name="pathCount">New path's length</param>
        /// <param name="maxPath">Maximum path length allowed</param>
        /// <returns>True if path saved, false if not</returns>
        public bool FinalizedSlicedPathPartial(PolygonPath existing, PolygonPath path)
        {
            path.Clear();

            if (existing.Count == 0)
            {
                return false;
            }

            if (queryData.Status == false)
            {
                queryData = new QueryData();
                return false;
            }

            if (queryData.Start.Polygon == queryData.End.Polygon)
            {
                //special case: the search starts and ends at the same poly
                path.Add(queryData.Start.Polygon);
            }
            else
            {
                //find furthest existing node that was visited
                Node prev = null;
                Node node = null;
                for (int i = existing.Count - 1; i >= 0; i--)
                {
                    node = nodePool.FindNode(existing[i]);
                    if (node != null)
                        break;
                }

                if (node == null)
                {
                    node = queryData.LastBestNode;
                }

                //reverse the path
                NodeFlags prevRay = 0;
                do
                {
                    Node next = nodePool.GetNodeAtIdx(node.ParentIndex);
                    node.ParentIndex = nodePool.GetNodeIdx(prev);
                    prev = node;
                    NodeFlags nextRay = node.Flags & NodeFlags.ParentDetached;
                    node.Flags = (node.Flags & ~NodeFlags.ParentDetached) | prevRay;
                    prevRay = nextRay;
                    node = next;
                }
                while (node != null);

                //store path
                node = prev;
                do
                {
                    Node next = nodePool.GetNodeAtIdx(node.ParentIndex);
                    if ((node.Flags & NodeFlags.ParentDetached) != 0)
                    {
                        RaycastHit hit;
                        PolygonPath m;
                        PathPoint startPoint = new PathPoint(node.Id, node.Position);
                        Raycast(startPoint, next.Position, RaycastOptions.None, out hit, out m);
                        path.AppendPath(m);

                        if (path[path.Count - 1] == next.Id)
                        {
                            path.RemoveAt(path.Count - 1);
                        }
                    }
                    else
                    {
                        path.Add(node.Id);
                    }

                    node = next;
                }
                while (node != null);
            }

            //reset query
            queryData = new QueryData();

            return true;
        }

        /// <summary>
        /// Store polygons that are within a certain range from the current polygon
        /// </summary>
        /// <param name="centerPoint">Starting position</param>
        /// <param name="radius">Range to search within</param>
        /// <param name="resultRef">All the polygons within range</param>
        /// <param name="resultParent">Polygon's parents</param>
        /// <param name="resultCount">Number of polygons stored</param>
        /// <param name="maxResult">Maximum number of polygons allowed</param>
        /// <returns>True, unless input is invalid</returns>
        public bool FindLocalNeighborhood(PathPoint centerPoint, float radius, int maxResult, out PolyId[] resultRef, out PolyId[] resultParent, out int resultCount)
        {
            resultRef = new PolyId[maxResult];
            resultParent = new PolyId[maxResult];
            resultCount = 0;

            //validate input
            if (centerPoint.Polygon == PolyId.Null || !this.navigationMesh.IsValidPolyRef(centerPoint.Polygon))
            {
                return false;
            }

            Node[] stack = new Node[MaximumStack];
            int nstack = 0;

            NodePool tinyNodePool = new NodePool(64);

            Node startNode = tinyNodePool.GetNode(centerPoint.Polygon);
            startNode.ParentIndex = 0;
            startNode.Id = centerPoint.Polygon;
            startNode.Flags = NodeFlags.Closed;
            stack[nstack++] = startNode;

            float radiusSqr = radius * radius;

            int n = 0;
            if (n < maxResult)
            {
                resultRef[n] = startNode.Id;
                resultParent[n] = PolyId.Null;
                ++n;
            }

            while (nstack > 0)
            {
                //pop front
                Node curNode = stack[0];
                for (int i = 0; i < nstack - 1; i++)
                {
                    stack[i] = stack[i + 1];
                }
                nstack--;

                //get poly and tile
                PolyId curRef = curNode.Id;
                MeshTile curTile;
                Poly curPoly;
                this.navigationMesh.TryGetTileAndPolyByRefUnsafe(curRef, out curTile, out curPoly);

                foreach (Link link in curPoly.Links)
                {
                    PolyId neighborRef = link.Reference;

                    //skip invalid neighbors
                    if (neighborRef == PolyId.Null)
                    {
                        continue;
                    }

                    //skip if cannot allocate more nodes
                    Node neighborNode = tinyNodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        continue;
                    }

                    //skip visited
                    if ((neighborNode.Flags & NodeFlags.Closed) != 0)
                    {
                        continue;
                    }

                    //expand to neighbor
                    MeshTile neighborTile;
                    Poly neighborPoly;
                    this.navigationMesh.TryGetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    //skip off-mesh connections
                    if (neighborPoly.PolyType == PolyType.OffMeshConnection)
                    {
                        continue;
                    }

                    //find edge and calculate distance to edge
                    Vector3 va;
                    Vector3 vb;
                    if (!this.GetPortalPoints(curRef, curPoly, curTile, neighborRef, neighborPoly, neighborTile, out va, out vb))
                    {
                        continue;
                    }

                    //if the circle is not touching the next polygon, skip it
                    float tseg;
                    float distSqr = Intersection.PointToSegment2DSquared(centerPoint.Position, va, vb, out tseg);
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    //mark node visited
                    neighborNode.Flags |= NodeFlags.Closed;
                    neighborNode.ParentIndex = tinyNodePool.GetNodeIdx(curNode);

                    //check that the polygon doesn't collide with existing polygons

                    //collect vertices of the neighbor poly
                    Vector3[] pa = neighborTile.GetVertices(neighborPoly);

                    bool overlap = false;
                    for (int j = 0; j < n; j++)
                    {
                        PolyId pastRef = resultRef[j];

                        //connected polys do not overlap
                        bool connected = false;
                        foreach (Link link2 in curPoly.Links)
                        {
                            if (link2.Reference == pastRef)
                            {
                                connected = true;
                                break;
                            }
                        }

                        if (!connected)
                        {
                            //potentially overlapping
                            MeshTile pastTile;
                            Poly pastPoly;
                            this.navigationMesh.TryGetTileAndPolyByRefUnsafe(pastRef, out pastTile, out pastPoly);

                            //get vertices and test overlap
                            Vector3[] pb = pastTile.GetVertices(pastPoly);

                            if (Intersection.PolygonIntersectsPolygon2D(pa, pb))
                            {
                                overlap = true;
                                break;
                            }
                        }
                    }

                    if (!overlap)
                    {
                        //store poly
                        if (n < maxResult)
                        {
                            resultRef[n] = neighborRef;
                            resultParent[n] = curRef;
                            n++;
                        }

                        if (nstack < MaximumStack)
                        {
                            stack[nstack++] = neighborNode;
                        }
                    }
                }
            }

            resultCount = n;

            return true;
        }
        /// <summary>
        /// Collect all the edges from a polygon.
        /// </summary>
        /// <param name="reference">The polygon reference</param>
        /// <param name="segmentVertices">Segment vertices</param>
        /// <param name="segmentReferences">The polygon reference containing the segment</param>
        /// <param name="segmentCount">The number of segments stored</param>
        /// <param name="maxSegments">The maximum number of segments allowed</param>
        /// <returns>True, unless the polygon reference is invalid</returns>
        public bool GetPolyWallSegments(PolyId reference, int maxSegments, out Segment[] segmentVertices, out PolyId[] segmentReferences, out int segmentCount)
        {
            segmentVertices = new Segment[maxSegments];
            segmentReferences = new PolyId[maxSegments];
            segmentCount = 0;

            MeshTile tile;
            Poly poly;
            if (this.navigationMesh.TryGetTileAndPolyByRef(reference, out tile, out poly) == false)
            {
                return false;
            }

            int n = 0;
            SegmentInterval[] ints = new SegmentInterval[MaximumInterval];

            bool storePortals = segmentReferences.Length != 0;

            for (int i = 0, j = poly.VertexCount - 1; i < poly.VertexCount; j = i++)
            {
                //skip non-solid edges
                int nints = 0;
                if ((poly.NeighborEdges[j] & Link.External) != 0)
                {
                    //tile border
                    foreach (Link link in poly.Links)
                    {
                        if (link.Edge == j)
                        {
                            if (link.Reference != PolyId.Null)
                            {
                                MeshTile neiTile;
                                Poly neiPoly;
                                this.navigationMesh.TryGetTileAndPolyByRefUnsafe(link.Reference, out neiTile, out neiPoly);
                                InsertInterval(ints, ref nints, MaximumInterval, link.BMin, link.BMax, link.Reference);
                            }
                        }
                    }
                }
                else
                {
                    //internal edge
                    PolyId neiRef = PolyId.Null;
                    if (poly.NeighborEdges[j] != 0)
                    {
                        int idx = poly.NeighborEdges[j] - 1;
                        PolyId id = this.navigationMesh.GetTileRef(tile);
                        this.navigationMesh.IdManager.SetPolyIndex(ref id, idx, out neiRef);
                    }

                    //if the edge leads to another polygon and portals are not stored, skip
                    if (neiRef != PolyId.Null && !storePortals)
                    {
                        continue;
                    }

                    if (n < maxSegments)
                    {
                        Vector3 vj = tile.Vertices[poly.Vertices[j]];
                        Vector3 vi = tile.Vertices[poly.Vertices[i]];
                        segmentVertices[n].Start = vj;
                        segmentVertices[n].End = vi;
                        segmentReferences[n] = neiRef;
                        n++; //could be n += 2, since segments have 2 vertices
                    }

                    continue;
                }

                //add sentinels
                InsertInterval(ints, ref nints, MaximumInterval, -1, 0, PolyId.Null);
                InsertInterval(ints, ref nints, MaximumInterval, 255, 256, PolyId.Null);

                //store segments
                Vector3 vj2 = tile.Vertices[poly.Vertices[j]];
                Vector3 vi2 = tile.Vertices[poly.Vertices[i]];
                for (int k = 1; k < nints; k++)
                {
                    //portal segment
                    if (storePortals && ints[k].Reference != PolyId.Null)
                    {
                        float tmin = ints[k].TMin / 255.0f;
                        float tmax = ints[k].TMax / 255.0f;
                        if (n < maxSegments)
                        {
                            Vector3.Lerp(ref vj2, ref vi2, tmin, out segmentVertices[n].Start);
                            Vector3.Lerp(ref vj2, ref vi2, tmax, out segmentVertices[n].End);
                            segmentReferences[n] = ints[k].Reference;
                            n++;
                        }
                    }

                    //wall segment
                    int imin = ints[k - 1].TMax;
                    int imax = ints[k].TMin;
                    if (imin != imax)
                    {
                        float tmin = imin / 255.0f;
                        float tmax = imax / 255.0f;
                        if (n < maxSegments)
                        {
                            Vector3.Lerp(ref vj2, ref vi2, tmin, out segmentVertices[n].Start);
                            Vector3.Lerp(ref vj2, ref vi2, tmax, out segmentVertices[n].End);
                            segmentReferences[n] = PolyId.Null;
                            n++;
                        }
                    }
                }
            }

            segmentCount = n;

            return true;
        }

        /// <summary>
        /// Retrieve the endpoints of the offmesh connection at the specified polygon
        /// </summary>
        /// <param name="prevRef">The previous polygon reference</param>
        /// <param name="polyRef">The current polygon reference</param>
        /// <param name="startPos">The starting position</param>
        /// <param name="endPos">The ending position</param>
        /// <returns>True if endpoints found, false if not</returns>
        public bool GetOffMeshConnectionPolyEndPoints(PolyId prevRef, PolyId polyRef, out Vector3 startPos, out Vector3 endPos)
        {
            return this.navigationMesh.GetOffMeshConnectionPolyEndPoints(prevRef, polyRef, out startPos, out endPos);
        }

        /// <summary>
        /// Insert a segment into the array
        /// </summary>
        /// <param name="intervals">The array of segments</param>
        /// <param name="nints">The number of segments</param>
        /// <param name="maxInts">The maximium number of segments allowed</param>
        /// <param name="tmin">Parameter t minimum</param>
        /// <param name="tmax">Parameter t maximum</param>
        /// <param name="reference">Polygon reference</param>
        private void InsertInterval(SegmentInterval[] intervals, ref int nints, int maxInts, int tmin, int tmax, PolyId reference)
        {
            if (nints + 1 > maxInts)
            {
                return;
            }

            //find insertion point
            int idx = 0;
            while (idx < nints)
            {
                if (tmax <= intervals[idx].TMin)
                {
                    break;
                }

                idx++;
            }

            //move current results
            if (nints - idx > 0)
            {
                for (int i = 0; i < nints - idx; i++)
                {
                    intervals[idx + 1 + i] = intervals[idx + i];
                }
            }

            //store
            intervals[idx].Reference = reference;
            intervals[idx].TMin = tmin;
            intervals[idx].TMax = tmax;
            nints++;
        }
        /// <summary>
        /// Ray casting between positions to find a path of polygons
        /// </summary>
        /// <param name="startPoint">Start point</param>
        /// <param name="endPosition">Position to reach</param>
        /// <param name="options">Ray castin options</param>
        /// <param name="hit">Returns the hit result</param>
        /// <param name="resultPath">Returns the polygon path between positions</param>
        /// <returns>Returns true if the ray casting operation was performed correctly</returns>
        public bool Raycast(PathPoint startPoint, Vector3 endPosition, RaycastOptions options, out RaycastHit hit, out PolygonPath resultPath)
        {
            return Raycast(startPoint, endPosition, PolyId.Null, options, out hit, out resultPath);
        }
        /// <summary>
        /// Ray casting between positions to find a path of polygons
        /// </summary>
        /// <param name="startPoint">Start point</param>
        /// <param name="endPosition">Position to reach</param>
        /// <param name="prevReference">Previous reference</param>
        /// <param name="options">Returns the hit result</param>
        /// <param name="hit">Returns the hit result</param>
        /// <param name="resultPath"></param>
        /// <returns>Returns true if the ray casting operation was performed correctly</returns>
        public bool Raycast(PathPoint startPoint, Vector3 endPosition, PolyId prevReference, RaycastOptions options, out RaycastHit hit, out PolygonPath resultPath)
        {
            hit = new RaycastHit();
            resultPath = new PolygonPath();

            //validate input
            if (startPoint.Polygon == PolyId.Null || !this.navigationMesh.IsValidPolyRef(startPoint.Polygon))
            {
                return false;
            }

            if (prevReference != PolyId.Null && !this.navigationMesh.IsValidPolyRef(prevReference))
            {
                return false;
            }

            MeshTile curTile;
            Poly curPoly;
            this.navigationMesh.TryGetTileAndPolyByRefUnsafe(
                startPoint.Polygon,
                out curTile, out curPoly);

            MeshTile prevTile = curTile;
            Poly prevPoly = curPoly;

            if (prevReference != PolyId.Null)
            {
                this.navigationMesh.TryGetTileAndPolyByRefUnsafe(
                    prevReference,
                    out prevTile, out prevPoly);
            }

            MeshTile nextTile = curTile;
            Poly nextPoly = curPoly;

            PolyId curRef = startPoint.Polygon;
            while (curRef != PolyId.Null)
            {
                //collect vertices
                var vertices = curTile.GetVertices(curPoly);

                float tmin;
                float tmax;
                int segMin;
                int segMax;
                bool intersects = Intersection.SegmentPolygon2D(
                    startPoint.Position, endPosition, vertices,
                    out tmin, out tmax, out segMin, out segMax);
                if (!intersects)
                {
                    //could not hit the polygon, keep the old t and report hit
                    return true;
                }

                hit.EdgeIndex = segMax;

                //keep track of furthest t so far
                if (tmax > hit.T)
                {
                    hit.T = tmax;
                }

                //store visited polygons
                resultPath.Add(curRef);

                //ray end is completely inside the polygon
                if (segMax == -1)
                {
                    hit.T = float.MaxValue;

                    return true;
                }

                //follow neighbors
                PolyId nextRef = PolyId.Null;

                foreach (Link link in curPoly.Links)
                {
                    //find link which contains the edge
                    if (link.Edge != segMax)
                    {
                        continue;
                    }

                    //get pointer to the next polygon
                    this.navigationMesh.TryGetTileAndPolyByRefUnsafe(
                        link.Reference,
                        out nextTile, out nextPoly);

                    //skip off-mesh connection
                    if (nextPoly.PolyType != PolyType.OffMeshConnection)
                    {
                        //TODO QueryFilter

                        //if the link is internal, just return the reference
                        if (link.Side == BoundarySide.Internal)
                        {
                            nextRef = link.Reference;
                            break;
                        }

                        //check if the link spans the whole edge and accept
                        if (link.BMin == 0 && link.BMax == 255)
                        {
                            nextRef = link.Reference;
                            break;
                        }

                        //check for partial edge links
                        var vIndices = curPoly.GetEdgeIndices(link);
                        Vector3 left = curTile.Vertices[vIndices[0]];
                        Vector3 right = curTile.Vertices[vIndices[1]];

                        //check that the intersection lies inside the link portal
                        if (link.CheckBoundaries(startPoint.Position, endPosition, left, right, tmax))
                        {
                            nextRef = link.Reference;
                            break;
                        }
                    }
                }

                if ((options & RaycastOptions.UseCosts) != 0)
                {
                    //TODO add cost
                }

                if (nextRef == PolyId.Null)
                {
                    //no neighbor, we hit a wall

                    //calculate hit normal
                    int a = segMax;
                    int b = (segMax + 1) < vertices.Length ? segMax + 1 : 0;
                    Vector3 va = vertices[a];
                    Vector3 vb = vertices[b];
                    float dx = vb.X - va.X;
                    float dz = vb.Z - va.Z;
                    hit.Normal = new Vector3(dz, 0, dx);
                    hit.Normal.Normalize();
                    return true;
                }

                //no hit, advance to neighbor polygon
                prevReference = curRef;
                curRef = nextRef;
                prevTile = curTile;
                curTile = nextTile;
                prevPoly = curPoly;
                curPoly = nextPoly;
            }

            return true;
        }
        /// <summary>
        /// Gets if the polygon reference is valid
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <returns>Returns true if the polygon reference is valid</returns>
        public bool IsValidPolyRef(PolyId reference)
        {
            MeshTile meshTile;
            Poly polygon;
            return this.navigationMesh.TryGetTileAndPolyByRef(reference, out meshTile, out polygon);
        }
    }
}
