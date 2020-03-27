using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Data to update tiles
        /// </summary>
        class UpdateTileData
        {
            /// <summary>
            /// X tile position
            /// </summary>
            public int X { get; set; }
            /// <summary>
            /// Y tile position
            /// </summary>
            public int Y { get; set; }
            /// <summary>
            /// Bounding box
            /// </summary>
            public BoundingBox BoundingBox { get; set; }
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
            out IEnumerable<Vector3> resultPath)
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

            if (polys.Count <= 0)
            {
                return false;
            }

            // Iterate over the path to find smooth path on the detail mesh surface.
            var iterPath = polys.Copy();

            navQuery.ClosestPointOnPoly(startRef, startPos, out Vector3 iterPos, out bool iOver);
            navQuery.ClosestPointOnPoly(iterPath.End, endPos, out Vector3 targetPos, out bool eOver);

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
                if (!GetSteerTarget(navQuery, iterPos, targetPos, SLOP, iterPath, out var target))
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
                    iterPath.Start, iterPos, moveTgt, filter, 16,
                    out var result, out var visited);

                SimplePath.FixupCorridor(iterPath, visited);
                SimplePath.FixupShortcuts(iterPath, navQuery);

                navQuery.GetPolyHeight(iterPath.Start, result, out float h);
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
                    int polyRef = iterPath.Start;
                    int npos = 0;
                    int[] iterNodes = iterPath.GetPath();
                    while (npos < iterPath.Count && polyRef != target.Ref)
                    {
                        prevRef = polyRef;
                        polyRef = iterNodes[npos];
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
                        navQuery.GetPolyHeight(iterPath.Start, iterPos, out float eh);
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

            return false;
        }

        private static bool CalcPathStraigh(
            NavMeshQuery navQuery, QueryFilter filter,
            Vector3 startPos, Vector3 endPos,
            int startRef, int endRef,
            out IEnumerable<Vector3> resultPath)
        {
            resultPath = null;

            navQuery.FindPath(
                startRef, endRef, startPos, endPos, filter, MAX_POLYS,
                out SimplePath polys);

            if (polys.Count != 0)
            {
                // In case of partial path, make sure the end point is clamped to the last polygon.
                Vector3 epos = endPos;
                if (polys.End != endRef)
                {
                    navQuery.ClosestPointOnPoly(polys.End, endPos, out epos, out bool eOver);
                }

                navQuery.FindStraightPath(
                    startPos, epos, polys,
                    MAX_POLYS, StraightPathOptions.DT_STRAIGHTPATH_ALL_CROSSINGS,
                    out var straightPath);

                if (straightPath.Count > 0)
                {
                    resultPath = straightPath.GetPaths();

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
            target.Points = steerPath.GetPaths().ToArray();

            // Find vertex far enough to steer to.
            int ns = 0;
            while (ns < steerPath.Count)
            {
                // Stop at Off-Mesh link or when point is further than slop away.
                if ((steerPath.GetFlag(ns) & StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0 ||
                    !InRange(steerPath.GetPath(ns), startPos, minTargetDist, 1000.0f))
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

            var pos = steerPath.GetPath(ns);
            pos.Y = startPos.Y;

            target.Position = pos;
            target.Flag = steerPath.GetFlag(ns);
            target.Ref = steerPath.GetRef(ns);

            return true;
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
        /// Debug info
        /// </summary>
        private readonly Dictionary<Crowd, List<CrowdAgentDebugInfo>> debugInfo = new Dictionary<Crowd, List<CrowdAgentDebugInfo>>();

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
        public void BuildTile(IEnumerable<Vector3> positions)
        {
            this.Updating?.Invoke(this, new EventArgs());

            foreach (var agentQ in AgentQueries)
            {
                var navmesh = agentQ.NavMesh;

                List<UpdateTileData> tiles = new List<UpdateTileData>();

                foreach (var position in positions)
                {
                    NavMesh.GetTileAtPosition(position, Input, Settings, out var tx, out var ty, out var bbox);

                    if (!tiles.Any(t => t.X == tx && t.Y == ty))
                    {
                        UpdateTileData v = new UpdateTileData()
                        {
                            X = tx,
                            Y = ty,
                            BoundingBox = bbox,
                        };

                        tiles.Add(v);
                    }
                }

                foreach (var tile in tiles)
                {
                    navmesh.BuildTileAtPosition(tile.X, tile.Y, tile.BoundingBox, Input, Settings, agentQ.Agent);
                }
            }

            this.Updated?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Removes the tile in the specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        public void RemoveTile(IEnumerable<Vector3> positions)
        {
            this.Updating?.Invoke(this, new EventArgs());

            foreach (var agentQ in AgentQueries)
            {
                var navmesh = agentQ.NavMesh;

                List<UpdateTileData> tiles = new List<UpdateTileData>();

                foreach (var position in positions)
                {
                    NavMesh.GetTileAtPosition(position, Input, Settings, out var tx, out var ty, out var bbox);

                    if (!tiles.Any(t => t.X == tx && t.Y == ty))
                    {
                        UpdateTileData v = new UpdateTileData()
                        {
                            X = tx,
                            Y = ty,
                            BoundingBox = bbox,
                        };

                        tiles.Add(v);
                    }
                }

                foreach (var tile in tiles)
                {
                    navmesh.RemoveTileAtPosition(tile.X, tile.Y, Settings);
                }
            }

            this.Updated?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Updates the graph at specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void UpdateAt(Vector3 position)
        {
            this.BuildTile(new[] { position });
        }
        /// <summary>
        /// Updates the graph at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        public void UpdateAt(IEnumerable<Vector3> positions)
        {
            this.BuildTile(positions);
        }
        /// <summary>
        /// Removes the graph node at specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void RemoveAt(Vector3 position)
        {
            RemoveTile(new[] { position });
        }
        /// <summary>
        /// Removes the graph node at specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        public void RemoveAt(IEnumerable<Vector3> positions)
        {
            RemoveTile(positions);
        }

        /// <summary>
        /// Gets the node collection of the graph for the specified agent type
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection for the agent type</returns>
        public IEnumerable<IGraphNode> GetNodes(AgentType agent)
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
        public IEnumerable<Vector3> FindPath(AgentType agent, Vector3 from, Vector3 to)
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
                from, to, out var result);

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
        public async Task<IEnumerable<Vector3>> FindPathAsync(AgentType agent, Vector3 from, Vector3 to)
        {
            IEnumerable<Vector3> result = new Vector3[] { };

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
                    from, to, out var res);

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
                debugInfo.TryGetValue(crowd, out var debug);

                crowd.Update(gameTime.ElapsedSeconds, debug);
            }
        }

        /// <summary>
        /// Adds a new crowd
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Returns the new crowd</returns>
        public Crowd AddCrowd(CrowdParameters settings)
        {
            var navMesh = AgentQueries.Find(a => settings.Agent.Equals(a.Agent))?.NavMesh;

            Crowd cr = new Crowd(navMesh, settings);

            this.Crowds.Add(cr);

            return cr;
        }
        /// <summary>
        /// Request move all agents in the crowd
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="agent">Agent type</param>
        /// <param name="p">Destination position</param>
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

                foreach (var ag in crowd.GetAgents())
                {
                    crowd.RequestMoveTarget(ag, poly, nP);
                }
            }
        }
        /// <summary>
        /// Request move a single crowd agent
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="crowdAgent">Agent</param>
        /// <param name="agent">Agent type</param>
        /// <param name="p">Destination position</param>
        public void RequestMoveAgent(Crowd crowd, CrowdAgent crowdAgent, AgentType agent, Vector3 p)
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

        /// <summary>
        /// Enables debug info for crowd and agent
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="crowdAgent">Agent</param>
        public void EnableDebugInfo(Crowd crowd, CrowdAgent crowdAgent)
        {
            if (!debugInfo.ContainsKey(crowd))
            {
                debugInfo.Add(crowd, new List<CrowdAgentDebugInfo>());
            }

            if (!debugInfo[crowd].Any(l => l.Agent == crowdAgent))
            {
                debugInfo[crowd].Add(new CrowdAgentDebugInfo { Agent = crowdAgent });
            }
        }
        /// <summary>
        /// Disables debug info for crowd and agent
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="crowdAgent">Agent</param>
        public void DisableDebugInfo(Crowd crowd, CrowdAgent crowdAgent)
        {
            if (!debugInfo.ContainsKey(crowd))
            {
                return;
            }

            debugInfo[crowd].RemoveAll(l => l.Agent == crowdAgent);
        }
    }
}
