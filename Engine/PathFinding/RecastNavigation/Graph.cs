using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Navigation graph
    /// </summary>
    public class Graph : IGraph, IDisposable
    {
        public const int MAX_POLYS = 256;
        public const int MAX_SMOOTH = 2048;

        /// <summary>
        /// Graph item
        /// </summary>
        [Serializable]
        class GraphItem
        {
            /// <summary>
            /// Id counter
            /// </summary>
            private static int ID = 0;

            /// <summary>
            /// Graph item id
            /// </summary>
            public readonly int Id;
            /// <summary>
            /// Index list per agent
            /// </summary>
            public Tuple<Agent, int>[] Indices { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public GraphItem()
            {
                this.Id = ++ID;
            }
        }

        /// <summary>
        /// Calcs a path
        /// </summary>
        /// <param name="navQuery">Navigation query</param>
        /// <param name="filter">Filter</param>
        /// <param name="polyPickExt">Extensions</param>
        /// <param name="mode">Path mode</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="resultPath">Result path</param>
        /// <returns>Returns the status of the path calculation</returns>
        private static Status CalcPath(
            NavMeshQuery navQuery, QueryFilter filter, Vector3 polyPickExt,
            PathFindingMode mode,
            Vector3 startPos, Vector3 endPos, out Vector3[] resultPath)
        {
            resultPath = null;

            navQuery.FindNearestPoly(startPos, polyPickExt, filter, out int startRef, out Vector3 nsp);

            navQuery.FindNearestPoly(endPos, polyPickExt, filter, out int endRef, out Vector3 nep);

            Status pathFindStatus = Status.DT_FAILURE;

            if (mode == PathFindingMode.TOOLMODE_PATHFIND_FOLLOW)
            {
                List<Vector3> smoothPath = new List<Vector3>();

                if (startRef != 0 && endRef != 0)
                {
                    navQuery.FindPath(
                        startRef, endRef, startPos, endPos, filter,
                        out int[] polys, out int npolys, MAX_POLYS);

                    if (npolys != 0)
                    {
                        // Iterate over the path to find smooth path on the detail mesh surface.
                        int[] iterPolys = new int[MAX_POLYS];
                        Array.Copy(polys, iterPolys, npolys);
                        int nIterPolys = npolys;

                        navQuery.ClosestPointOnPoly(startRef, startPos, out Vector3 iterPos, out bool iOver);
                        navQuery.ClosestPointOnPoly(iterPolys[nIterPolys - 1], endPos, out Vector3 targetPos, out bool eOver);

                        float STEP_SIZE = 0.5f;
                        float SLOP = 0.01f;

                        smoothPath.Add(iterPos);

                        // Move towards target a small advancement at a time until target reached or
                        // when ran out of memory to store the path.
                        while (nIterPolys != 0 && smoothPath.Count < MAX_SMOOTH)
                        {
                            // Find location to steer towards.
                            if (!GetSteerTarget(
                                navQuery, iterPos, targetPos, SLOP,
                                iterPolys, nIterPolys, out Vector3 steerPos, out StraightPathFlags steerPosFlag, out int steerPosRef,
                                out Vector3[] points, out int nPoints))
                            {
                                break;
                            }

                            bool endOfPath = (steerPosFlag & StraightPathFlags.DT_STRAIGHTPATH_END) != 0 ? true : false;
                            bool offMeshConnection = (steerPosFlag & StraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ? true : false;

                            // Find movement delta.
                            Vector3 delta = Vector3.Subtract(steerPos, iterPos);
                            float len = delta.Length();
                            // If the steer target is end of path or off-mesh link, do not move past the location.
                            if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
                            {
                                len = 1;
                            }
                            else
                            {
                                len = STEP_SIZE / len;
                            }
                            Vector3 moveTgt = Vector3.Add(iterPos, delta * len);

                            // Move
                            navQuery.MoveAlongSurface(
                                iterPolys[0], iterPos, moveTgt, filter,
                                out Vector3 result, out int[] visited, out int nvisited, 16);

                            nIterPolys = FixupCorridor(iterPolys, nIterPolys, MAX_POLYS, visited, nvisited);
                            nIterPolys = FixupShortcuts(iterPolys, nIterPolys, navQuery);

                            navQuery.GetPolyHeight(iterPolys[0], result, out float h);
                            result.Y = h;
                            iterPos = result;

                            // Handle end of path and off-mesh links when close enough.
                            if (endOfPath && InRange(iterPos, steerPos, SLOP, 1.0f))
                            {
                                // Reached end of path.
                                iterPos = targetPos;
                                if (smoothPath.Count < MAX_SMOOTH)
                                {
                                    smoothPath.Add(iterPos);
                                }
                                break;
                            }
                            else if (offMeshConnection && InRange(iterPos, steerPos, SLOP, 1.0f))
                            {
                                // Reached off-mesh connection.

                                // Advance the path up to and over the off-mesh connection.
                                int prevRef = 0;
                                int polyRef = iterPolys[0];
                                int npos = 0;
                                while (npos < nIterPolys && polyRef != steerPosRef)
                                {
                                    prevRef = polyRef;
                                    polyRef = iterPolys[npos];
                                    npos++;
                                }
                                for (int i = npos; i < nIterPolys; ++i)
                                {
                                    iterPolys[i - npos] = iterPolys[i];
                                }
                                nIterPolys -= npos;

                                // Handle the connection.
                                if (navQuery.GetAttachedNavMesh().GetOffMeshConnectionPolyEndPoints(
                                    prevRef, polyRef, out Vector3 sPos, out Vector3 ePos))
                                {
                                    if (smoothPath.Count < MAX_SMOOTH)
                                    {
                                        smoothPath.Add(sPos);
                                        // Hack to make the dotted path not visible during off-mesh connection.
                                        if ((smoothPath.Count & 1) != 0)
                                        {
                                            smoothPath.Add(sPos);
                                        }
                                    }
                                    // Move position at the other side of the off-mesh link.
                                    iterPos = ePos;
                                    navQuery.GetPolyHeight(iterPolys[0], iterPos, out float eh);
                                    iterPos.Y = eh;
                                }
                            }

                            // Store results.
                            if (smoothPath.Count < MAX_SMOOTH)
                            {
                                smoothPath.Add(iterPos);
                            }
                        }
                    }
                }

                if (smoothPath.Count > 0)
                {
                    resultPath = smoothPath.ToArray();

                    return pathFindStatus;
                }
            }
            else if (mode == PathFindingMode.TOOLMODE_PATHFIND_STRAIGHT)
            {
                Vector3[] straightPath = null;
                StraightPathFlags[] straightPathFlags = null;
                int[] straightPathPolys = null;
                int nstraightPath = 0;

                StraightPathOptions m_straightPathOptions = StraightPathOptions.DT_STRAIGHTPATH_ALL_CROSSINGS;

                if (startRef != 0 && endRef != 0)
                {
                    navQuery.FindPath(
                        startRef, endRef, startPos, endPos, filter,
                        out int[] polys, out int npolys, MAX_POLYS);

                    nstraightPath = 0;
                    if (npolys != 0)
                    {
                        // In case of partial path, make sure the end point is clamped to the last polygon.
                        Vector3 epos = endPos;
                        if (polys[npolys - 1] != endRef)
                        {
                            navQuery.ClosestPointOnPoly(polys[npolys - 1], endPos, out epos, out bool eOver);
                        }

                        navQuery.FindStraightPath(
                            startPos, epos, polys, npolys,
                            out straightPath, out straightPathFlags,
                            out straightPathPolys, out nstraightPath, MAX_POLYS, m_straightPathOptions);
                    }
                }

                if (nstraightPath > 0)
                {
                    resultPath = new Vector3[nstraightPath];
                    Array.Copy(straightPath, resultPath, nstraightPath);

                    return pathFindStatus;
                }
            }
            else if (mode == PathFindingMode.TOOLMODE_PATHFIND_SLICED)
            {
                if (startRef != 0 && endRef != 0)
                {
                    pathFindStatus = navQuery.InitSlicedFindPath(
                        startRef, endRef, startPos, endPos, filter,
                        FindPathOptions.DT_FINDPATH_ANY_ANGLE);

                    return pathFindStatus;
                }
            }

            return Status.DT_FAILURE;
        }
        /// <summary>
        /// Gets a steer target
        /// </summary>
        /// <param name="navQuery">Navigation query</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="minTargetDist">Miminum tangent distance</param>
        /// <param name="path">Current path</param>
        /// <param name="pathSize">Current path size</param>
        /// <param name="steerPos">Returns the steer position</param>
        /// <param name="steerPosFlag">Returns the steer position flag</param>
        /// <param name="steerPosRef">Returns the steer position reference</param>
        /// <param name="outPoints">Returns the point list</param>
        /// <param name="outPointCount">Returns the point count</param>
        /// <returns></returns>
        private static bool GetSteerTarget(
            NavMeshQuery navQuery, Vector3 startPos, Vector3 endPos,
            float minTargetDist,
            int[] path, int pathSize,
            out Vector3 steerPos, out StraightPathFlags steerPosFlag, out int steerPosRef,
            out Vector3[] outPoints, out int outPointCount)
        {
            steerPos = Vector3.Zero;
            steerPosFlag = 0;
            steerPosRef = 0;
            outPoints = null;
            outPointCount = 0;

            // Find steer target.
            int MAX_STEER_POINTS = 3;
            navQuery.FindStraightPath(
                startPos, endPos, path, pathSize,
                out Vector3[] steerPath, out StraightPathFlags[] steerPathFlags, out int[] steerPathPolys, out int nsteerPath,
                MAX_STEER_POINTS, StraightPathOptions.DT_STRAIGHTPATH_NONE);

            if (nsteerPath == 0)
            {
                return false;
            }

            outPointCount = nsteerPath;
            outPoints = steerPath;

            // Find vertex far enough to steer to.
            int ns = 0;
            while (ns < nsteerPath)
            {
                // Stop at Off-Mesh link or when point is further than slop away.
                if ((steerPathFlags[ns] & StraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
                    !InRange(steerPath[ns], startPos, minTargetDist, 1000.0f))
                {
                    break;
                }
                ns++;
            }
            // Failed to find good point to steer to.
            if (ns >= nsteerPath)
            {
                return false;
            }

            steerPos = steerPath[ns];
            steerPos.Y = startPos.Y;
            steerPosFlag = steerPathFlags[ns];
            steerPosRef = steerPathPolys[ns];

            return true;
        }
        /// <summary>
        /// Fix ups corridor
        /// </summary>
        /// <param name="path">Current path</param>
        /// <param name="npath">Current path size</param>
        /// <param name="maxPath">Maximum path size</param>
        /// <param name="visited">Visted references</param>
        /// <param name="nvisited">Number of visited references</param>
        /// <returns>Returns the new size of the path</returns>
        private static int FixupCorridor(int[] path, int npath, int maxPath, int[] visited, int nvisited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = npath - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = nvisited - 1; j >= 0; --j)
                {
                    if (path[i] == visited[j])
                    {
                        furthestPath = i;
                        furthestVisited = j;
                        found = true;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // If no intersection found just return current path. 
            if (furthestPath == -1 || furthestVisited == -1)
            {
                return npath;
            }

            // Concatenate paths.	

            // Adjust beginning of the buffer to include the visited.
            int req = nvisited - furthestVisited;
            int orig = Math.Min(furthestPath + 1, npath);
            int size = Math.Max(0, npath - orig);
            if (req + size > maxPath)
            {
                size = maxPath - req;
            }
            if (size != 0)
            {
                Array.Copy(path, orig, path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                path[i] = visited[(nvisited - 1) - i];
            }

            return req + size;
        }
        /// <summary>
        /// This function checks if the path has a small U-turn, that is,
        /// a polygon further in the path is adjacent to the first polygon
        /// in the path. If that happens, a shortcut is taken.
        /// This can happen if the target (T) location is at tile boundary,
        /// and we're (S) approaching it parallel to the tile edge.
        /// The choice at the vertex can be arbitrary, 
        ///  +---+---+
        ///  |:::|:::|
        ///  +-S-+-T-+
        ///  |:::|   | -- the step can end up in here, resulting U-turn path.
        ///  +---+---+
        /// </summary>
        /// <param name="path">Current path</param>
        /// <param name="npath">Current path size</param>
        /// <param name="navQuery">Navigation query</param>
        /// <returns>Returns the new size of the path</returns>
        private static int FixupShortcuts(int[] path, int npath, NavMeshQuery navQuery)
        {
            if (npath < 3)
            {
                return npath;
            }

            // Get connected polygons
            int maxNeis = 16;
            List<int> neis = new List<int>();

            if (navQuery.GetAttachedNavMesh().GetTileAndPolyByRef(path[0], out MeshTile tile, out Poly poly))
            {
                return npath;
            }

            for (int k = poly.FirstLink; k != Detour.DT_NULL_LINK; k = tile.links[k].next)
            {
                var link = tile.links[k];
                if (link.nref != 0)
                {
                    if (neis.Count < maxNeis)
                    {
                        neis.Add(link.nref);
                    }
                }
            }

            // If any of the neighbour polygons is within the next few polygons
            // in the path, short cut to that polygon directly.
            int maxLookAhead = 6;
            int cut = 0;
            for (int i = Math.Min(maxLookAhead, npath) - 1; i > 1 && cut == 0; i--)
            {
                for (int j = 0; j < neis.Count; j++)
                {
                    if (path[i] == neis[j])
                    {
                        cut = i;
                        break;
                    }
                }
            }
            if (cut > 1)
            {
                int offset = cut - 1;
                npath -= offset;
                for (int i = 1; i < npath; i++)
                {
                    path[i] = path[i + offset];
                }
            }

            return npath;
        }

        private static bool InRange(Vector3 v1, Vector3 v2, float radius, float height)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;

            return (dx * dx + dz * dz) < (radius * radius) && Math.Abs(dy) < height;
        }

        /// <summary>
        /// On graph updating event
        /// </summary>
        public event EventHandler Updating;
        /// <summary>
        /// On graph updated event
        /// </summary>
        public event EventHandler Updated;
        /// <summary>
        /// Updated flag
        /// </summary>
        private bool updated = true;

        /// <summary>
        /// Item indices
        /// </summary>
        private List<GraphItem> itemIndices = new List<GraphItem>();

        /// <summary>
        /// Input geometry
        /// </summary>
        public InputGeometry Input { get; set; }
        /// <summary>
        /// Build settings
        /// </summary>
        public BuildSettings Settings { get; set; }
        /// <summary>
        /// Navigation mesh query dictionary (by agent type)
        /// </summary>
        public Dictionary<Agent, NavMeshQuery> MeshQueryDictionary = new Dictionary<Agent, NavMeshQuery>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Graph()
        {

        }
        /// <summary>
        /// Instance dispose
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(MeshQueryDictionary);
        }

        /// <summary>
        /// Builds the tile in the specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void BuildTile(Vector3 position)
        {
            foreach (var agent in MeshQueryDictionary.Keys)
            {
                MeshQueryDictionary[agent].GetAttachedNavMesh().BuildTile(position, Input, Settings, agent);
            }
        }
        /// <summary>
        /// Removes the tile in the specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void RemoveTile(Vector3 position)
        {
            foreach (var agent in MeshQueryDictionary.Keys)
            {
                MeshQueryDictionary[agent].GetAttachedNavMesh().RemoveTile(position, Input, Settings);
            }
        }
        /// <summary>
        /// Updates the graph at specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void UpdateAt(Vector3 position)
        {
            this.BuildTile(position);
        }

        /// <summary>
        /// Gets the node collection of the graph for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection for the agent type</returns>
        public IGraphNode[] GetNodes(AgentType agent)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            nodes.AddRange(GraphNode.Build(MeshQueryDictionary[(Agent)agent].GetAttachedNavMesh()));

            return nodes.ToArray();
        }
        /// <summary>
        /// Find path from point to point for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public Vector3[] FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            var filter = new QueryFilter()
            {
                m_includeFlags = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK,
            };

            CalcPath(
                MeshQueryDictionary[(Agent)agent],
                filter, new Vector3(2, 4, 2), PathFindingMode.TOOLMODE_PATHFIND_FOLLOW,
                from, to, out Vector3[] result);

            return result;
        }
        /// <summary>
        /// Gets wether the specified position is walkable for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest)
        {
            nearest = null;

            //Find agent query
            var query = MeshQueryDictionary[(Agent)agent];
            if (query != null)
            {
                var filter = new QueryFilter()
                {
                    m_includeFlags = SamplePolyFlags.SAMPLE_POLYFLAGS_WALK,
                };

                //Set extents based upon agent height
                var agentExtents = new Vector3(agent.Height, agent.Height * 2, agent.Height);

                var status = query.FindNearestPoly(
                    position, agentExtents, filter,
                    out int nRef, out Vector3 nPoint);

                if (!status.HasFlag(Status.DT_FAILURE))
                {
                    if (nRef != 0)
                    {
                        nearest = nPoint;

                        return nPoint.X == position.X && nPoint.Z == position.Z;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a cylinder obstacle
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 position, float radius, float height)
        {
            this.updated = false;

            List<Tuple<Agent, int>> obstacles = new List<Tuple<Agent, int>>();

            foreach (var agent in MeshQueryDictionary.Keys)
            {
                var cache = MeshQueryDictionary[agent].GetAttachedNavMesh().TileCache;
                if (cache != null)
                {
                    cache.AddObstacle(position, radius, height, out int res);

                    obstacles.Add(new Tuple<Agent, int>(agent, res));
                }
            }

            if (obstacles.Count > 0)
            {
                var o = new GraphItem()
                {
                    Indices = obstacles.ToArray()
                };

                itemIndices.Add(o);

                return o.Id;
            }

            return -1;
        }
        /// <summary>
        /// Adds a oriented bounding box obstacle
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="halfExtents">half extent vectors</param>
        /// <param name="yRotation">Rotation in the y axis</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 position, Vector3 halfExtents, float yRotation)
        {
            this.updated = false;

            List<Tuple<Agent, int>> obstacles = new List<Tuple<Agent, int>>();

            foreach (var agent in MeshQueryDictionary.Keys)
            {
                var cache = MeshQueryDictionary[agent].GetAttachedNavMesh().TileCache;
                if (cache != null)
                {
                    cache.AddBoxObstacle(position, halfExtents, yRotation, out int res);

                    obstacles.Add(new Tuple<Agent, int>(agent, res));
                }
            }

            if (obstacles.Count > 0)
            {
                var o = new GraphItem()
                {
                    Indices = obstacles.ToArray()
                };

                itemIndices.Add(o);

                return o.Id;
            }

            return -1;
        }
        /// <summary>
        /// Adds a bounding box obstacle
        /// </summary>
        /// <param name="minimum">Minimum corner</param>
        /// <param name="maximum">Maximum corner</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 minimum, Vector3 maximum)
        {
            this.updated = false;

            List<Tuple<Agent, int>> obstacles = new List<Tuple<Agent, int>>();

            foreach (var agent in MeshQueryDictionary.Keys)
            {
                var cache = MeshQueryDictionary[agent].GetAttachedNavMesh().TileCache;
                if (cache != null)
                {
                    cache.AddBoxObstacle(minimum, maximum, out int res);

                    obstacles.Add(new Tuple<Agent, int>(agent, res));
                }
            }

            if (obstacles.Count > 0)
            {
                var o = new GraphItem()
                {
                    Indices = obstacles.ToArray()
                };

                itemIndices.Add(o);

                return o.Id;
            }

            return -1;
        }
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacleId">Obstacle id</param>
        public void RemoveObstacle(int obstacleId)
        {
            this.updated = false;

            var obstacle = itemIndices.Find(o => o.Id == obstacleId);
            if (obstacle != null)
            {
                foreach (var item in obstacle.Indices)
                {
                    var cache = MeshQueryDictionary[item.Item1].GetAttachedNavMesh().TileCache;
                    if (cache != null)
                    {
                        cache.RemoveObstacle(item.Item2);
                    }
                }

                itemIndices.Remove(obstacle);
            }
        }

        /// <summary>
        /// Adds a new off-mesh connection
        /// </summary>
        /// <param name="from">From point</param>
        /// <param name="to">To point</param>
        /// <returns>Returns the off-mesh connection id</returns>
        public int AddConnection(Vector3 from, Vector3 to)
        {
            this.updated = false;

            List<Tuple<Agent, int>> offmeshConnections = new List<Tuple<Agent, int>>();

            foreach (var agent in MeshQueryDictionary.Keys)
            {
                var cache = MeshQueryDictionary[agent].GetAttachedNavMesh().TileCache;
                if (cache != null)
                {
                    cache.AddOffmeshConnection(from, to, out int res);

                    offmeshConnections.Add(new Tuple<Agent, int>(agent, res));
                }
            }

            if (offmeshConnections.Count > 0)
            {
                var o = new GraphItem()
                {
                    Indices = offmeshConnections.ToArray()
                };

                itemIndices.Add(o);

                return o.Id;
            }

            return -1;
        }
        /// <summary>
        /// Removes an off-mesh connection by off-mesh connection id
        /// </summary>
        /// <param name="offmeshId">Off-mesh connection id</param>
        public void RemoveConnection(int offmeshId)
        {
            this.updated = false;

            var offmeshConnection = itemIndices.Find(o => o.Id == offmeshId);
            if (offmeshConnection != null)
            {
                foreach (var item in offmeshConnection.Indices)
                {
                    var cache = MeshQueryDictionary[item.Item1].GetAttachedNavMesh().TileCache;
                    if (cache != null)
                    {
                        cache.RemoveOffmeshConnection(item.Item2);
                    }
                }

                itemIndices.Remove(offmeshConnection);
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            foreach (var agent in MeshQueryDictionary.Keys)
            {
                var nm = MeshQueryDictionary[agent].GetAttachedNavMesh();
                if (nm.TileCache != null)
                {
                    var status = nm.TileCache.Update(gameTime.TotalMilliseconds, nm, out bool upToDate);
                    if (status.HasFlag(Status.DT_SUCCESS))
                    {
                        if (updated != upToDate)
                        {
                            updated = upToDate;

                            if (updated)
                            {
                                this.Updated?.Invoke(this, new EventArgs());
                            }
                            else
                            {
                                this.Updating?.Invoke(this, new EventArgs());
                            }
                        }
                    }
                }
            }
        }
    }
}
