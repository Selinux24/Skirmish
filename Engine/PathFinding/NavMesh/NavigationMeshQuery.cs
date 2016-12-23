using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Collections;
    using Engine.Common;

    /// <summary>
    /// Do pathfinding calculations on the TiledNavMesh
    /// </summary>
    public class NavigationMeshQuery
    {
        /// <summary>
        /// Maximum number of vertices
        /// </summary>
        public const int VertsPerPolygon = 6;
        /// <summary>
        /// Heuristic scale
        /// </summary>
        private const float HeuristicScale = 0.999f;

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
            int[] path;
            if (this.FindPath(ref startPt, ref endPt, out path))
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
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(Vector3 position, out Vector3? nearest)
        {
            var pt = this.FindNearestPoly(position, Vector3.Zero);

            if (pt.Polygon != 0)
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
        /// Find a path from the start polygon to the end polygon.
        /// -If the end polygon can't be reached, the last polygon will be nearest the end polygon
        /// -If the path array is too small, it will be filled as far as possible 
        /// -start and end positions are used to calculate traversal costs
        /// </summary>
        /// <param name="startPt">The start point.</param>
        /// <param name="endPt">The end point.</param>
        /// <param name="resultPath">The path of polygon references</param>
        /// <returns>True, if path found. False, if otherwise.</returns>
        private bool FindPath(ref PathPoint startPt, ref PathPoint endPt, out int[] resultPath)
        {
            resultPath = null;

            //validate input
            int startRef = startPt.Polygon;
            int endRef = endPt.Polygon;
            if (startRef == 0 || endRef == 0)
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
                resultPath = new int[] { startRef };
                return true;
            }

            Vector3 startPos = startPt.Position;
            Vector3 endPos = endPt.Position;

            this.nodePool.Clear();
            this.openList.Clear();

            //initial node is located at the starting position
            Node startNode = this.nodePool.GetNode(startRef);
            startNode.Position = startPos;
            startNode.ParentIdx = 0;
            startNode.cost = 0;
            startNode.total = (startPos - endPos).Length() * HeuristicScale;
            startNode.Id = startRef;
            startNode.Flags = NodeFlags.Open;

            this.openList.Push(startNode);

            Node lastBestNode = startNode;
            float lastBestTotalCost = startNode.total;

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
                int bestRef = bestNode.Id;
                MeshTile bestTile;
                Poly bestPoly;
                this.navigationMesh.TryGetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                //get parent poly and tile
                int parentRef = 0;
                if (bestNode.ParentIdx != 0)
                {
                    parentRef = this.nodePool.GetNodeAtIdx(bestNode.ParentIdx).Id;
                }

                //examine neighbors
                foreach (Link link in bestPoly.Links)
                {
                    int neighborRef = link.Reference;

                    //skip invalid ids and do not expand back to where we came from
                    if (neighborRef == 0 || neighborRef == parentRef)
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
                        this.GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, ref neighborNode.Position);
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

                        cost = bestNode.cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        //cost
                        float curCost = this.GetCost(bestNode.Position, neighborNode.Position, bestPoly);

                        cost = bestNode.cost + curCost;
                        heuristic = (neighborNode.Position - endPos).Length() * HeuristicScale;
                    }

                    float total = cost + heuristic;

                    //the node is already in open list and new result is worse, skip
                    if (neighborNode.IsInOpenList && total >= neighborNode.total)
                    {
                        continue;
                    }

                    //the node is already visited and processesd, and the new result is worse, skip
                    if (neighborNode.IsInClosedList && total >= neighborNode.total)
                    {
                        continue;
                    }

                    //add or update the node
                    neighborNode.ParentIdx = this.nodePool.GetNodeIdx(bestNode);
                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = neighborNode.RemoveNodeFlagClosed();
                    neighborNode.cost = cost;
                    neighborNode.total = total;

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
            List<int> path = new List<int>();
            Node node = lastBestNode;
            do
            {
                path.Add(node.Id);

                node = this.nodePool.GetNodeAtIdx(node.ParentIdx);
            }
            while (node != null);

            //reverse the path since it's backwards
            path.Reverse();

            resultPath = path.ToArray();
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
        private bool FindStraightPath(Vector3 from, Vector3 to, int[] path, PathBuildFlags options, out StraightPath straightPath)
        {
            straightPath = null;

            if (path.Length == 0)
            {
                return false;
            }

            Vector3 closestStartPos;
            Vector3 closestEndPos;
            this.ClosestPointOnPolyBoundary(path[0], from, out closestStartPos);
            this.ClosestPointOnPolyBoundary(path[path.Length - 1], to, out closestEndPos);

            straightPath = new StraightPath();

            bool stat = straightPath.AppendVertex(new StraightPathVertex(new PathPoint(path[0], closestStartPos), StraightPathFlags.Start));
            if (!stat)
            {
                return true;
            }

            if (path.Length > 1)
            {
                Vector3 portalApex = closestStartPos;
                Vector3 portalLeft = portalApex;
                Vector3 portalRight = portalApex;
                int apexIndex = 0;
                int leftIndex = 0;
                int rightIndex = 0;

                PolyType leftPolyType = 0;
                PolyType rightPolyType = 0;

                int leftPolyRef = path[0];
                int rightPolyRef = path[0];

                for (int i = 0; i < path.Length; i++)
                {
                    Vector3 left = new Vector3();
                    Vector3 right = new Vector3();
                    PolyType fromType = PolyType.Ground;
                    PolyType toType = PolyType.Ground;

                    #region Get portal points between polygons

                    if (i + 1 < path.Length)
                    {
                        //Find next portal
                        int fromId = path[i];
                        int toId = path[i + 1];
                        if (!GetPortalPoints(fromId, toId, ref left, ref right, ref fromType, ref toType))
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
                            if (Intersection.PointToSegment2DSquared(ref portalApex, ref left, ref right, out t) < 0.001 * 0.001)
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
                            rightPolyRef = (i + 1 < path.Length) ? path[i + 1] : 0;
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
                            if (leftPolyRef == 0)
                            {
                                flags = StraightPathFlags.End;
                            }
                            else if (leftPolyType == PolyType.OffMeshConnection)
                            {
                                flags = StraightPathFlags.OffMeshConnection;
                            }

                            int reference = leftPolyRef;

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
                            leftPolyRef = (i + 1 < path.Length) ? path[i + 1] : 0;
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
                            if (rightPolyRef == 0)
                            {
                                flags = StraightPathFlags.End;
                            }
                            else if (rightPolyType == PolyType.OffMeshConnection)
                            {
                                flags = StraightPathFlags.OffMeshConnection;
                            }

                            int reference = rightPolyRef;

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
                    stat = AppendPortals(apexIndex, path.Length - 1, closestEndPos, path, straightPath, options);
                    if (!stat)
                    {
                        return true;
                    }
                }
            }

            stat = straightPath.AppendVertex(new StraightPathVertex(new PathPoint(0, closestEndPos), StraightPathFlags.End));

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
        private bool AppendPortals(int startIdx, int endIdx, Vector3 endPos, int[] path, StraightPath straightPath, PathBuildFlags options)
        {
            Vector3 startPos = straightPath[straightPath.Count - 1].Point.Position;

            //append or update last vertex
            bool stat = false;
            for (int i = startIdx; i < endIdx; i++)
            {
                //calculate portal
                int from = path[i];
                MeshTile fromTile;
                Poly fromPoly;
                if (this.navigationMesh.TryGetTileAndPolyByRef(from, out fromTile, out fromPoly) == false)
                {
                    return false;
                }

                int to = path[i + 1];
                MeshTile toTile;
                Poly toPoly;
                if (this.navigationMesh.TryGetTileAndPolyByRef(to, out toTile, out toPoly) == false)
                {
                    return false;
                }

                Vector3 left = new Vector3();
                Vector3 right = new Vector3();
                if (GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right) == false)
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
        private bool GetEdgeMidPoint(int from, Poly fromPoly, MeshTile fromTile, int to, Poly toPoly, MeshTile toTile, ref Vector3 mid)
        {
            Vector3 left = new Vector3();
            Vector3 right = new Vector3();
            if (!this.GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right))
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
        private bool GetPortalPoints(int from, int to, ref Vector3 left, ref Vector3 right, ref PolyType fromType, ref PolyType toType)
        {
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

            return this.GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right);
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
        private bool GetPortalPoints(int from, Poly fromPoly, MeshTile fromTile, int to, Poly toPoly, MeshTile toTile, ref Vector3 left, ref Vector3 right)
        {
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
                        left = fromTile.Verts[fromPoly.Vertices[v]];
                        right = fromTile.Verts[fromPoly.Vertices[v]];
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
                        left = toTile.Verts[toPoly.Vertices[v]];
                        right = toTile.Verts[toPoly.Vertices[v]];
                        return true;
                    }
                }

                return false;
            }

            //find portal vertices
            int v0 = fromPoly.Vertices[link.Edge];
            int v1 = fromPoly.Vertices[(link.Edge + 1) % fromPoly.VertexCount];
            left = fromTile.Verts[v0];
            right = fromTile.Verts[v1];

            //if the link is at the tile boundary, clamp the vertices to tile width
            if (link.Side != BoundarySide.Internal)
            {
                //unpack portal limits
                if (link.BMin != 0 || link.BMax != 255)
                {
                    float s = 1.0f / 255.0f;
                    float tmin = link.BMin * s;
                    float tmax = link.BMax * s;
                    left = Vector3.Lerp(fromTile.Verts[v0], fromTile.Verts[v1], tmin);
                    right = Vector3.Lerp(fromTile.Verts[v0], fromTile.Verts[v1], tmax);
                }
            }

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
        private bool ClosestPointOnPoly(int reference, Vector3 pos, out Vector3 closest, out bool posOverPoly)
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
                Vector3 v0 = tile.Verts[poly.Vertices[0]];
                Vector3 v1 = tile.Verts[poly.Vertices[1]];
                float d0 = (pos - v0).Length();
                float d1 = (pos - v1).Length();
                float u = d0 / (d0 + d1);
                closest = Vector3.Lerp(v0, v1, u);
                return true;
            }

            //Clamp point to be inside the polygon
            Vector3[] verts = new Vector3[NavigationMeshQuery.VertsPerPolygon];
            float[] edgeDistance = new float[NavigationMeshQuery.VertsPerPolygon];
            float[] edgeT = new float[NavigationMeshQuery.VertsPerPolygon];
            int numPolyVerts = poly.VertexCount;
            for (int i = 0; i < numPolyVerts; i++)
            {
                verts[i] = tile.Verts[poly.Vertices[i]];
            }

            closest = pos;
            if (!Intersection.PointToPolygonEdgeSquared(pos, verts, numPolyVerts, edgeDistance, edgeT))
            {
                //Point is outside the polygon
                //Clamp to nearest edge
                float minDistance = float.MaxValue;
                int minIndex = -1;
                for (int i = 0; i < numPolyVerts; i++)
                {
                    if (edgeDistance[i] < minDistance)
                    {
                        minDistance = edgeDistance[i];
                        minIndex = i;
                    }
                }

                Vector3 va = verts[minIndex];
                Vector3 vb = verts[(minIndex + 1) % numPolyVerts];
                closest = Vector3.Lerp(va, vb, edgeT[minIndex]);
            }
            else
            {
                //Point is inside the polygon
                posOverPoly = true;
            }

            //find height at the location
            int indexPoly = Array.IndexOf(tile.Polys, poly);
            var pd = tile.DetailMeshes[indexPoly];

            for (int j = 0; j < pd.TriangleCount; j++)
            {
                var t = tile.DetailTris[pd.TriangleIndex + j];

                Vector3 va;
                if (t.VertexHash0 < poly.VertexCount)
                {
                    va = tile.Verts[poly.Vertices[t.VertexHash0]];
                }
                else
                {
                    va = tile.DetailVerts[pd.VertexIndex + (t.VertexHash0 - poly.VertexCount)];
                }

                Vector3 vb;
                if (t.VertexHash1 < poly.VertexCount)
                {
                    vb = tile.Verts[poly.Vertices[t.VertexHash1]];
                }
                else
                {
                    vb = tile.DetailVerts[pd.VertexIndex + (t.VertexHash1 - poly.VertexCount)];
                }

                Vector3 vc;
                if (t.VertexHash2 < poly.VertexCount)
                {
                    vc = tile.Verts[poly.Vertices[t.VertexHash2]];
                }
                else
                {
                    vc = tile.DetailVerts[pd.VertexIndex + (t.VertexHash2 - poly.VertexCount)];
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
        private bool ClosestPointOnPolyBoundary(int reference, Vector3 pos, out Vector3 closest)
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
        private PathPoint FindNearestPoly(Vector3 center, Vector3 extents)
        {
            PathPoint result;
            this.FindNearestPoly(ref center, ref extents, out result);
            return result;
        }
        /// <summary>
        /// Find the nearest poly within a certain range.
        /// </summary>
        /// <param name="center">Center.</param>
        /// <param name="extents">Extents.</param>
        /// <param name="nearestPt">The neareast point.</param>
        private void FindNearestPoly(ref Vector3 center, ref Vector3 extents, out PathPoint nearestPt)
        {
            nearestPt = PathPoint.Null;

            // Get nearby polygons from proximity grid.
            List<int> polys = new List<int>();
            if (this.QueryPolygons(ref center, ref extents, polys))
            {
                float nearestDistanceSqr = float.MaxValue;
                for (int i = 0; i < polys.Count; i++)
                {
                    int reference = polys[i];
                    Vector3 closestPtPoly;
                    bool posOverPoly;
                    this.ClosestPointOnPoly(reference, center, out closestPtPoly, out posOverPoly);

                    // If a point is directly over a polygon and closer than
                    // climb height, favor that instead of straight line nearest point.
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
                    }
                }
            }
        }
        /// <summary>
        /// Finds nearby polygons within a certain range.
        /// </summary>
        /// <param name="center">The starting point</param>
        /// <param name="extent">The range to search within</param>
        /// <param name="polys">A list of polygons</param>
        /// <returns>True, if successful. False, if otherwise.</returns>
        private bool QueryPolygons(ref Vector3 center, ref Vector3 extent, List<int> polys)
        {
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
                        n += neighborTile.QueryPolygons(bounds, polys);
                    }
                }
            }

            return polys.Count != 0;
        }

        #region Helper classes

        /// <summary>
        /// Determine which list the node is in.
        /// </summary>
        [Flags]
        private enum NodeFlags
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
            Closed = 0x02
        }
        /// <summary>
        /// Every polygon becomes a Node, which contains a position and cost.
        /// </summary>
        private class Node : IValueWithCost
        {
            public Vector3 Position;
            public float cost;
            public float total;
            public int ParentIdx = 30; //index to parent node
            public NodeFlags Flags = 0; //node flags 0/open/closed
            public int Id; //polygon ref the node corresponds to

            public float Cost
            {
                get
                {
                    return total;
                }
            }
            public bool IsInOpenList
            {
                get
                {
                    return (this.Flags & NodeFlags.Open) != 0;
                }
            }
            public bool IsInClosedList
            {
                get
                {
                    return (this.Flags & NodeFlags.Closed) != 0;
                }
            }

            public void SetNodeFlagOpen()
            {
                this.Flags |= NodeFlags.Open;
            }
            public void SetNodeFlagClosed()
            {
                this.Flags &= ~NodeFlags.Open;
                this.Flags |= NodeFlags.Closed;
            }
            public NodeFlags RemoveNodeFlagClosed()
            {
                return this.Flags & ~NodeFlags.Closed;
            }
        }
        /// <summary>
        /// Link all nodes together. Store indices in hash map.
        /// </summary>
        private class NodePool
        {
            private List<Node> nodes;
            private Dictionary<int, Node> nodeDict;
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
                nodeDict = new Dictionary<int, Node>();
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
            public Node FindNode(int id)
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
            public Node GetNode(int id)
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
                newNode.ParentIdx = 0;
                newNode.cost = 0;
                newNode.total = 0;
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
        /// 
        /// </summary>
        [Flags]
        private enum StraightPathFlags
        {
            /// <summary>
            /// None
            /// </summary>
            None = 0,
            /// <summary>
            /// vertex is in start position of path
            /// </summary>
            Start = 0x01,
            /// <summary>
            /// vertex is in end position of path
            /// </summary>
            End = 0x02,
            /// <summary>
            /// vertex is at start of an off-mesh connection
            /// </summary>
            OffMeshConnection = 0x04,
        }
        /// <summary>
        /// Straight path vertex
        /// </summary>
        private struct StraightPathVertex
        {
            /// <summary>
            /// Path point
            /// </summary>
            public PathPoint Point;
            /// <summary>
            /// Flags
            /// </summary>
            public StraightPathFlags Flags;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="point">Point</param>
            /// <param name="flags">Flags</param>
            public StraightPathVertex(PathPoint point, StraightPathFlags flags)
            {
                Point = point;
                Flags = flags;
            }

            /// <summary>
            /// Gets the string representation of the instance
            /// </summary>
            public override string ToString()
            {
                return string.Format("Point: {0}; Flags: {1}", this.Point, this.Flags);
            }
        }
        /// <summary>
        /// Straight path
        /// </summary>
        private class StraightPath
        {
            /// <summary>
            /// Vertex list
            /// </summary>
            private List<StraightPathVertex> verts = new List<StraightPathVertex>();

            /// <summary>
            /// Vertex count
            /// </summary>
            public int Count { get { return verts.Count; } }
            /// <summary>
            /// Gets or set the vertex at index
            /// </summary>
            /// <param name="i">Index</param>
            /// <returns>Returns the vertex at index</returns>
            public StraightPathVertex this[int i]
            {
                get { return verts[i]; }
                set { verts[i] = value; }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public StraightPath()
            {

            }

            /// <summary>
            /// Clear path
            /// </summary>
            public void Clear()
            {
                this.verts.Clear();
            }
            /// <summary>
            /// Append vertex to path
            /// </summary>
            /// <param name="vert">Vertex</param>
            /// <returns></returns>
            public bool AppendVertex(StraightPathVertex vert)
            {
                bool equalToLast = false;
                if (this.Count > 0)
                {
                    //can only be done if at least one vertex in path
                    Vector3 lastStraightPath = this.verts[Count - 1].Point.Position;
                    Vector3 pos = vert.Point.Position;
                    equalToLast = Helper.NearEqual(lastStraightPath, pos);
                }

                if (equalToLast)
                {
                    //the vertices are equal, update flags and polys
                    this.verts[this.Count - 1] = vert;
                }
                else
                {
                    //append new vertex
                    this.verts.Add(vert);

                    if (vert.Flags == StraightPathFlags.End)
                    {
                        return false;
                    }
                }

                return true;
            }
            /// <summary>
            /// Remove vertex at index
            /// </summary>
            /// <param name="index">Index</param>
            public void RemoveAt(int index)
            {
                this.verts.RemoveAt(index);
            }
            /// <summary>
            /// Remove vertex range
            /// </summary>
            /// <param name="index">Index from</param>
            /// <param name="count">Count from index</param>
            public void RemoveRange(int index, int count)
            {
                this.verts.RemoveRange(index, count);
            }
        }

        #endregion
    }
}
