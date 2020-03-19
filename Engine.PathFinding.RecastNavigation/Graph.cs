using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using Engine.PathFinding.RecastNavigation.Detour.Crowds;

    /// <summary>
    /// Navigation graph
    /// </summary>
    public class Graph : IGraph
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
            /// Gets the next Id
            /// </summary>
            /// <returns>Returns the next Id</returns>
            private static int GetNextId()
            {
                return ++ID;
            }

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
                this.Id = GetNextId();
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
            Vector3 startPos, Vector3 endPos,
            out Vector3[] resultPath)
        {
            resultPath = null;

            navQuery.FindNearestPoly(startPos, polyPickExt, filter, out int startRef, out Vector3 nsp);
            navQuery.FindNearestPoly(endPos, polyPickExt, filter, out int endRef, out Vector3 nep);

            var endPointsDefined = (startRef != 0 && endRef != 0);
            if (!endPointsDefined)
            {
                return Status.DT_FAILURE;
            }

            if (mode == PathFindingMode.TOOLMODE_PATHFIND_FOLLOW)
            {
                if (CalcPathFollow(navQuery, filter, startPos, endPos, startRef, endRef, out var path))
                {
                    resultPath = path;

                    return Status.DT_SUCCESS;
                }
            }
            else if (mode == PathFindingMode.TOOLMODE_PATHFIND_STRAIGHT)
            {
                if (CalcPathStraigh(navQuery, filter, startPos, endPos, startRef, endRef, out var path))
                {
                    resultPath = path;

                    return Status.DT_SUCCESS;
                }
            }
            else if (mode == PathFindingMode.TOOLMODE_PATHFIND_SLICED)
            {
                return navQuery.InitSlicedFindPath(
                    startRef, endRef, startPos, endPos, filter,
                    FindPathOptions.DT_FINDPATH_ANY_ANGLE);
            }

            return Status.DT_FAILURE;
        }

        private static bool CalcPathFollow(
            NavMeshQuery navQuery, QueryFilter filter,
            Vector3 startPos, Vector3 endPos,
            int startRef, int endRef,
            out Vector3[] resultPath)
        {
            resultPath = null;

            navQuery.FindPath(
                startRef, endRef, startPos, endPos, filter,
                MAX_POLYS,
                out var polys);

            if (polys.Count != 0)
            {
                // Iterate over the path to find smooth path on the detail mesh surface.
                var iterPath = polys.Copy();

                navQuery.ClosestPointOnPoly(startRef, startPos, out Vector3 iterPos, out bool iOver);
                navQuery.ClosestPointOnPoly(iterPath.Path[iterPath.Count - 1], endPos, out Vector3 targetPos, out bool eOver);

                float STEP_SIZE = 0.5f;
                float SLOP = 0.01f;

                List<Vector3> smoothPath = new List<Vector3>
                {
                    iterPos
                };

                // Move towards target a small advancement at a time until target reached or
                // when ran out of memory to store the path.
                while (iterPath.Count != 0 && smoothPath.Count < MAX_SMOOTH)
                {
                    // Find location to steer towards.
                    if (!GetSteerTarget(
                        navQuery, iterPos, targetPos, SLOP,
                        iterPath, out var target))
                    {
                        break;
                    }

                    bool endOfPath = (target.Flag & StraightPathFlagTypes.DT_STRAIGHTPATH_END) != 0;
                    bool offMeshConnection = (target.Flag & StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0;

                    // Find movement delta.
                    Vector3 delta = Vector3.Subtract(target.Position, iterPos);
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
                        iterPath.Path[0], iterPos, moveTgt, filter, 16,
                        out var result, out var visited, out var nvisited);

                    iterPath.Count = FixupCorridor(iterPath, MAX_POLYS, visited, nvisited);
                    iterPath.Count = FixupShortcuts(iterPath, navQuery);

                    navQuery.GetPolyHeight(iterPath.Path[0], result, out float h);
                    result.Y = h;
                    iterPos = result;

                    // Handle end of path and off-mesh links when close enough.
                    if (endOfPath && InRange(iterPos, target.Position, SLOP, 1.0f))
                    {
                        // Reached end of path.
                        iterPos = targetPos;
                        if (smoothPath.Count < MAX_SMOOTH)
                        {
                            smoothPath.Add(iterPos);
                        }
                        break;
                    }
                    else if (offMeshConnection && InRange(iterPos, target.Position, SLOP, 1.0f))
                    {
                        // Reached off-mesh connection.

                        // Advance the path up to and over the off-mesh connection.
                        int prevRef = 0;
                        int polyRef = iterPath.Path[0];
                        int npos = 0;
                        while (npos < iterPath.Count && polyRef != target.Ref)
                        {
                            prevRef = polyRef;
                            polyRef = iterPath.Path[npos];
                            npos++;
                        }
                        iterPath.Prune(npos);

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
                            navQuery.GetPolyHeight(iterPath.Path[0], iterPos, out float eh);
                            iterPos.Y = eh;
                        }
                    }

                    // Store results.
                    if (smoothPath.Count < MAX_SMOOTH)
                    {
                        smoothPath.Add(iterPos);
                    }
                }

                if (smoothPath.Count > 0)
                {
                    resultPath = smoothPath.ToArray();

                    return true;
                }
            }

            return false;
        }

        private static bool CalcPathStraigh(
            NavMeshQuery navQuery, QueryFilter filter,
            Vector3 startPos, Vector3 endPos,
            int startRef, int endRef,
            out Vector3[] resultPath)
        {
            resultPath = null;

            navQuery.FindPath(
                startRef, endRef, startPos, endPos, filter, MAX_POLYS,
                out SimplePath polys);

            if (polys.Count != 0)
            {
                // In case of partial path, make sure the end point is clamped to the last polygon.
                Vector3 epos = endPos;
                if (polys.Path[polys.Count - 1] != endRef)
                {
                    navQuery.ClosestPointOnPoly(polys.Path[polys.Count - 1], endPos, out epos, out bool eOver);
                }

                navQuery.FindStraightPath(
                    startPos, epos, polys,
                    MAX_POLYS, StraightPathOptions.DT_STRAIGHTPATH_ALL_CROSSINGS,
                    out var straightPath);

                if (straightPath.Count > 0)
                {
                    resultPath = straightPath.Path;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a steer target
        /// </summary>
        /// <param name="navQuery">Navigation query</param>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End position</param>
        /// <param name="minTargetDist">Miminum tangent distance</param>
        /// <param name="path">Current path</param>
        /// <param name="target">Out target</param>
        /// <returns></returns>
        private static bool GetSteerTarget(
            NavMeshQuery navQuery,
            Vector3 startPos, Vector3 endPos,
            float minTargetDist,
            SimplePath path,
            out SteerTarget target)
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
            navQuery.FindStraightPath(
                startPos, endPos, path,
                MAX_STEER_POINTS, StraightPathOptions.DT_STRAIGHTPATH_NONE,
                out var steerPath);

            if (steerPath.Count == 0)
            {
                return false;
            }

            target.PointCount = steerPath.Count;
            target.Points = steerPath.Path;

            // Find vertex far enough to steer to.
            int ns = 0;
            while (ns < steerPath.Count)
            {
                // Stop at Off-Mesh link or when point is further than slop away.
                if ((steerPath.Flags[ns] & StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
                    !InRange(steerPath.Path[ns], startPos, minTargetDist, 1000.0f))
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

            var pos = steerPath.Path[ns];
            pos.Y = startPos.Y;

            target.Position = pos;
            target.Flag = steerPath.Flags[ns];
            target.Ref = steerPath.Refs[ns];

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
        private static int FixupCorridor(SimplePath path, int maxPath, int[] visited, int nvisited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            // Find furthest common polygon.
            for (int i = path.Count - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = nvisited - 1; j >= 0; --j)
                {
                    if (path.Path[i] == visited[j])
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
                return path.Count;
            }

            // Concatenate paths.	

            // Adjust beginning of the buffer to include the visited.
            int req = nvisited - furthestVisited;
            int orig = Math.Min(furthestPath + 1, path.Count);
            int size = Math.Max(0, path.Count - orig);
            if (req + size > maxPath)
            {
                size = maxPath - req;
            }
            if (size != 0)
            {
                Array.Copy(path.Path, orig, path.Path, req, size);
            }

            // Store visited
            for (int i = 0; i < req; ++i)
            {
                path.Path[i] = visited[(nvisited - 1) - i];
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
        private static int FixupShortcuts(SimplePath path, NavMeshQuery navQuery)
        {
            if (path.Count < 3)
            {
                return path.Count;
            }

            // Get connected polygons
            int maxNeis = 16;
            List<int> neis = new List<int>();

            if (navQuery.GetAttachedNavMesh().GetTileAndPolyByRef(path.Path[0], out MeshTile tile, out Poly poly))
            {
                return path.Count;
            }

            for (int k = poly.FirstLink; k != DetourUtils.DT_NULL_LINK; k = tile.Links[k].Next)
            {
                var link = tile.Links[k];
                if (link.NRef != 0 && neis.Count < maxNeis)
                {
                    neis.Add(link.NRef);
                }
            }

            // If any of the neighbour polygons is within the next few polygons
            // in the path, short cut to that polygon directly.
            int maxLookAhead = 6;
            int cut = 0;
            for (int i = Math.Min(maxLookAhead, path.Count) - 1; i > 1 && cut == 0; i--)
            {
                for (int j = 0; j < neis.Count; j++)
                {
                    if (path.Path[i] == neis[j])
                    {
                        cut = i;
                        break;
                    }
                }
            }
            if (cut > 1)
            {
                int offset = cut - 1;
                path.Count -= offset;
                for (int i = 1; i < path.Count; i++)
                {
                    path.Path[i] = path.Path[i + offset];
                }
            }

            return path.Count;
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
        private readonly List<GraphItem> itemIndices = new List<GraphItem>();

        /// <summary>
        /// Gets whether the graph is initialized
        /// </summary>
        public bool Initialized { get; set; }
        /// <summary>
        /// Input geometry
        /// </summary>
        public InputGeometry Input { get; set; }
        /// <summary>
        /// Build settings
        /// </summary>
        public BuildSettings Settings { get; set; }
        /// <summary>
        /// Agent query list
        /// </summary>
        public List<GraphAgentQuery> AgentQueries { get; set; } = new List<GraphAgentQuery>();
        /// <summary>
        /// Crowd list
        /// </summary>
        public List<Crowd> Crowds { get; set; } = new List<Crowd>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Graph()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Graph()
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
                foreach (var item in this.AgentQueries)
                {
                    item?.Dispose();
                }

                this.AgentQueries.Clear();
                this.AgentQueries = null;
            }
        }

        /// <summary>
        /// Builds the tile in the specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void BuildTile(Vector3 position)
        {
            this.Updating?.Invoke(this, new EventArgs());

            foreach (var agentQ in AgentQueries)
            {
                var navmesh = agentQ.NavMesh;

                navmesh.BuildTileAtPosition(position, Input, Settings, agentQ.Agent);
            }

            this.Updated?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Removes the tile in the specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void RemoveTile(Vector3 position)
        {
            this.Updating?.Invoke(this, new EventArgs());

            foreach (var agentQ in AgentQueries)
            {
                var navmesh = agentQ.NavMesh;

                navmesh.RemoveTileAtPosition(position, Input, Settings);
            }

            this.Updated?.Invoke(this, new EventArgs());
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

            var graphQuery = AgentQueries.Find(a => agent.Equals(a.Agent));
            if (graphQuery == null)
            {
                return new IGraphNode[] { };
            }

            nodes.AddRange(GraphNode.Build(graphQuery.NavMesh));

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
                IncludeFlags = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK,
            };

            var graphQuery = AgentQueries.Find(a => agent.Equals(a.Agent));
            if (graphQuery == null)
            {
                return new Vector3[] { };
            }

            var status = CalcPath(
                graphQuery.CreateQuery(),
                filter, new Vector3(2, 4, 2), PathFindingMode.TOOLMODE_PATHFIND_FOLLOW,
                from, to, out Vector3[] result);

            if (status.HasFlag(Status.DT_SUCCESS))
            {
                return result;
            }
            else
            {
                return new Vector3[] { };
            }
        }
        /// <summary>
        /// Find path from point to point for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public async Task<Vector3[]> FindPathAsync(AgentType agent, Vector3 from, Vector3 to)
        {
            Vector3[] result = new Vector3[] { };

            await Task.Run(() =>
            {
                var filter = new QueryFilter()
                {
                    IncludeFlags = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK,
                };

                var graphQuery = AgentQueries.Find(a => agent.Equals(a.Agent));
                if (graphQuery == null)
                {
                    return;
                }

                var status = CalcPath(
                    graphQuery.CreateQuery(),
                    filter, new Vector3(2, 4, 2), PathFindingMode.TOOLMODE_PATHFIND_FOLLOW,
                    from, to, out Vector3[] res);

                if (status.HasFlag(Status.DT_SUCCESS))
                {
                    result = res;
                }
            });

            return result;
        }

        /// <summary>
        /// Gets wether the specified position is walkable for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(AgentType agent, Vector3 position)
        {
            return IsWalkable(agent, position, out var nearest);
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
            var query = AgentQueries.Find(a => agent.Equals(a.Agent))?.CreateQuery();
            if (query != null)
            {
                var filter = new QueryFilter()
                {
                    IncludeFlags = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK,
                };

                //Set extents based upon agent height
                var agentExtents = new Vector3(agent.Height, agent.Height * 2, agent.Height);

                var status = query.FindNearestPoly(
                    position, agentExtents, filter,
                    out int nRef, out Vector3 nPoint);

                if (nRef != 0 && !status.HasFlag(Status.DT_FAILURE))
                {
                    nearest = nPoint;

                    return nPoint.X == position.X && nPoint.Z == position.Z;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a cylinder obstacle
        /// </summary>
        /// <param name="cylinder">Bounding Cylinder</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(BoundingCylinder cylinder)
        {
            this.updated = false;

            List<Tuple<Agent, int>> obstacles = new List<Tuple<Agent, int>>();

            foreach (var agentQ in AgentQueries)
            {
                var cache = agentQ.NavMesh.TileCache;
                if (cache != null)
                {
                    cache.AddObstacle(cylinder.Position, cylinder.Radius, cylinder.Height, out int res);

                    obstacles.Add(new Tuple<Agent, int>(agentQ.Agent, res));
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
        /// <param name="bbox">Bounding Box</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(BoundingBox bbox)
        {
            this.updated = false;

            List<Tuple<Agent, int>> obstacles = new List<Tuple<Agent, int>>();

            foreach (var agentQ in AgentQueries)
            {
                var cache = agentQ.NavMesh.TileCache;
                if (cache != null)
                {
                    cache.AddBoxObstacle(bbox.Minimum, bbox.Maximum, out int res);

                    obstacles.Add(new Tuple<Agent, int>(agentQ.Agent, res));
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
        /// <param name="obb">Oriented Bounding Box</param>
        /// <returns>Returns the obstacle id</returns>
        /// <remarks>Only applies rotation if the obb's transform has rotation in the Y axis</remarks>
        public int AddObstacle(OrientedBoundingBox obb)
        {
            this.updated = false;

            List<Tuple<Agent, int>> obstacles = new List<Tuple<Agent, int>>();

            var position = obb.Center;
            var halfExtents = obb.Extents;
            var yRotation = GetYRotation(obb.Transformation);

            foreach (var agentQ in AgentQueries)
            {
                var cache = agentQ.NavMesh.TileCache;
                if (cache == null)
                {
                    continue;
                }

                cache.AddBoxObstacle(position, halfExtents, yRotation, out int res);

                obstacles.Add(new Tuple<Agent, int>(agentQ.Agent, res));
            }

            if (obstacles.Count == 0)
            {
                return -1;
            }

            var o = new GraphItem()
            {
                Indices = obstacles.ToArray()
            };

            itemIndices.Add(o);

            return o.Id;
        }
        /// <summary>
        /// Gets the Y axis rotation from a transform matrix
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        /// <returns>Returns the Y axis angle, only if the rotation is in the Y axis</returns>
        private static float GetYRotation(Matrix transform)
        {
            if (transform.Decompose(out var scale, out var rotation, out var translation))
            {
                return GetYRotation(rotation);
            }
            else
            {
                throw new ArgumentException("Bad transform. Cannot decompose.", nameof(transform));
            }
        }
        /// <summary>
        /// Gets the Y axis rotation from a rotation quaternion
        /// </summary>
        /// <param name="rotation">Rotation Quaternion</param>
        /// <returns>Returns the Y axis angle, only if the rotation is in the Y axis</returns>
        private static float GetYRotation(Quaternion rotation)
        {
            var yRotation = 0f;

            // Validates the angle and axis
            if (rotation.Angle != 0)
            {
                Vector3 epsilon = Vector3.Up * 0.0001f;

                if (Vector3.NearEqual(rotation.Axis, Vector3.Up, epsilon))
                {
                    yRotation = rotation.Angle;
                }

                if (Vector3.NearEqual(rotation.Axis, Vector3.Down, epsilon))
                {
                    yRotation = -rotation.Angle;
                }
            }

            return yRotation;
        }
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacleId">Obstacle id</param>
        public void RemoveObstacle(int obstacleId)
        {
            this.updated = false;

            var instance = itemIndices.Find(o => o.Id == obstacleId);
            if (instance != null)
            {
                foreach (var item in instance.Indices)
                {
                    var cache = AgentQueries.Find(a => a.Agent.Equals(item.Item1))?.NavMesh.TileCache;
                    if (cache != null)
                    {
                        cache.RemoveObstacle(item.Item2);
                    }
                }

                itemIndices.Remove(instance);
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            foreach (var agentQ in AgentQueries)
            {
                var nm = agentQ.NavMesh;
                if (nm.TileCache != null)
                {
                    var status = nm.TileCache.Update(nm, out bool upToDate);
                    if (status.HasFlag(Status.DT_SUCCESS) && updated != upToDate)
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

            foreach (var crowd in Crowds)
            {
                crowd.Update(gameTime.ElapsedSeconds, null);
            }
        }


        public Crowd AddCrowd(int maxAgents, Agent agent)
        {
            var navMesh = AgentQueries.Find(a => agent.Equals(a.Agent))?.NavMesh;

            Crowd cr = new Crowd();
            cr.Init(maxAgents, agent.Radius, navMesh);

            this.Crowds.Add(cr);

            return cr;
        }
        public void RequestMoveCrowd(Crowd crowd, AgentType agent, Vector3 p)
        {
            //Find agent query
            var query = AgentQueries.Find(a => agent.Equals(a.Agent))?.CreateQuery();
            if (query != null)
            {
                Status status = query.FindNearestPoly(p, crowd.GetQueryExtents(), crowd.GetFilter(0), out int poly, out Vector3 nP);
                if (status == Status.DT_FAILURE)
                {
                    return;
                }

                for (int i = 0; i < crowd.GetAgentCount(); i++)
                {
                    crowd.RequestMoveTarget(i, poly, nP);
                }
            }
        }
        public void RequestMoveAgent(Crowd crowd, int crowdAgent, AgentType agent, Vector3 p)
        {
            //Find agent query
            var query = AgentQueries.Find(a => agent.Equals(a.Agent))?.CreateQuery();
            if (query != null)
            {
                Status status = query.FindNearestPoly(p, crowd.GetQueryExtents(), crowd.GetFilter(0), out int poly, out Vector3 nP);
                if (status == Status.DT_FAILURE)
                {
                    return;
                }

                crowd.RequestMoveTarget(crowdAgent, poly, nP);
            }
        }

    }
}
