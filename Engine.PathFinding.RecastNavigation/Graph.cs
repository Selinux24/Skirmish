using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using Engine.PathFinding.RecastNavigation.Detour.Crowds;
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;

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

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"X:{X}; Y:{Y};";
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
            out IEnumerable<Vector3> resultPath)
        {
            resultPath = null;

            navQuery.FindNearestPoly(startPos, polyPickExt, filter, out int startRef, out _);
            navQuery.FindNearestPoly(endPos, polyPickExt, filter, out int endRef, out _);

            var endPointsDefined = (startRef != 0 && endRef != 0);
            if (!endPointsDefined)
            {
                return Status.DT_FAILURE;
            }

            if (mode == PathFindingMode.Follow)
            {
                if (CalcPathFollow(navQuery, filter, startPos, endPos, startRef, endRef, out var path))
                {
                    resultPath = path;

                    return Status.DT_SUCCESS;
                }
            }
            else if (mode == PathFindingMode.Straight)
            {
                if (CalcPathStraigh(navQuery, filter, startPos, endPos, startRef, endRef, out var path))
                {
                    resultPath = path;

                    return Status.DT_SUCCESS;
                }
            }
            else if (mode == PathFindingMode.Sliced)
            {
                var status = navQuery.InitSlicedFindPath(startRef, endRef, startPos, endPos, filter);
                if (status != Status.DT_SUCCESS)
                {
                    return status;
                }
                return navQuery.UpdateSlicedFindPath(20, out _);
            }

            return Status.DT_FAILURE;
        }

        private static bool CalcPathFollow(
            NavMeshQuery navQuery, QueryFilter filter,
            Vector3 startPos, Vector3 endPos,
            int startRef, int endRef,
            out IEnumerable<Vector3> resultPath)
        {
            resultPath = null;

            navQuery.FindPath(
                startRef, endRef, startPos, endPos, filter,
                MAX_POLYS,
                out var iterPath);

            if (iterPath.Count <= 0)
            {
                return false;
            }

            // Iterate over the path to find smooth path on the detail mesh surface.
            navQuery.ClosestPointOnPoly(startRef, startPos, out Vector3 iterPos, out _);
            navQuery.ClosestPointOnPoly(iterPath.End, endPos, out Vector3 targetPos, out _);

            List<Vector3> smoothPath = new List<Vector3>
            {
                iterPos
            };

            // Move towards target a small advancement at a time until target reached or
            // when ran out of memory to store the path.
            while (iterPath.Count != 0 && smoothPath.Count < MAX_SMOOTH)
            {
                if (IterPathFollow(navQuery, filter, targetPos, smoothPath, iterPath, ref iterPos))
                {
                    //End reached
                    break;
                }
            }

            resultPath = smoothPath.ToArray();

            return smoothPath.Count > 0;
        }
        private static bool IterPathFollow(
            NavMeshQuery navQuery, QueryFilter filter,
            Vector3 targetPos,
            List<Vector3> smoothPath,
            SimplePath iterPath,
            ref Vector3 iterPos)
        {
            float SLOP = 0.01f;

            // Find location to steer towards.
            if (!GetSteerTarget(navQuery, iterPos, targetPos, SLOP, iterPath, out var target))
            {
                return true;
            }

            bool endOfPath = (target.Flag & StraightPathFlagTypes.DT_STRAIGHTPATH_END) != 0;
            bool offMeshConnection = (target.Flag & StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0;

            // Find movement delta.
            Vector3 moveTgt = FindMovementDelta(iterPos, target.Position, endOfPath || offMeshConnection);

            // Move
            navQuery.MoveAlongSurface(
                iterPath.Start, iterPos, moveTgt, filter, 16,
                out var result, out var visited);

            SimplePath.FixupCorridor(iterPath, visited);
            SimplePath.FixupShortcuts(iterPath, navQuery);

            navQuery.GetPolyHeight(iterPath.Start, result, out float h);
            result.Y = h;
            iterPos = result;

            bool inRange = InRange(iterPos, target.Position, SLOP, 1.0f);
            if (!inRange)
            {
                // Store results.
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return false;
            }

            // Handle end of path and off-mesh links when close enough.
            if (endOfPath)
            {
                // Reached end of path.
                iterPos = targetPos;

                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return true;
            }

            if (offMeshConnection)
            {
                // Reached off-mesh connection.
                HandleOffMeshConnection(navQuery, target, smoothPath, iterPath, ref iterPos);

                // Store results.
                if (smoothPath.Count < MAX_SMOOTH)
                {
                    smoothPath.Add(iterPos);
                }

                return false;
            }

            // Store results.
            if (smoothPath.Count < MAX_SMOOTH)
            {
                smoothPath.Add(iterPos);
            }

            return false;
        }
        private static Vector3 FindMovementDelta(
            Vector3 position,
            Vector3 targetPos,
            bool overMesh)
        {
            float STEP_SIZE = 0.5f;

            // Find movement delta.
            Vector3 delta = Vector3.Subtract(targetPos, position);
            float len = delta.Length();

            // If the steer target is end of path or off-mesh link, do not move past the location.
            if (overMesh && len < STEP_SIZE)
            {
                len = 1;
            }
            else
            {
                len = STEP_SIZE / len;
            }

            return Vector3.Add(position, delta * len);
        }
        private static void HandleOffMeshConnection(
            NavMeshQuery navQuery,
            SteerTarget target,
            List<Vector3> smoothPath,
            SimplePath iterPath, ref Vector3 iterPos)
        {
            // Advance the path up to and over the off-mesh connection.
            int prevRef = 0;
            int polyRef = iterPath.Start;
            int npos = 0;
            var iterNodes = iterPath.GetPath();
            while (npos < iterPath.Count && polyRef != target.Ref)
            {
                prevRef = polyRef;
                polyRef = iterNodes.ElementAt(npos);
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
                }

                // Move position at the other side of the off-mesh link.
                iterPos = ePos;
                navQuery.GetPolyHeight(iterPath.Start, iterPos, out float eh);
                iterPos.Y = eh;
            }
        }

        private static bool CalcPathStraigh(
            NavMeshQuery navQuery, QueryFilter filter,
            Vector3 startPos, Vector3 endPos,
            int startRef, int endRef,
            out IEnumerable<Vector3> resultPath)
        {
            navQuery.FindPath(
                startRef, endRef, startPos, endPos, filter, MAX_POLYS,
                out var polys);

            if (polys.Count < 0)
            {
                resultPath = new Vector3[] { };

                return false;
            }

            // In case of partial path, make sure the end point is clamped to the last polygon.
            Vector3 epos = endPos;
            if (polys.End != endRef)
            {
                navQuery.ClosestPointOnPoly(polys.End, endPos, out epos, out _);
            }

            navQuery.FindStraightPath(
                startPos, epos, polys,
                MAX_POLYS, StraightPathOptions.AllCrossings,
                out var straightPath);

            resultPath = straightPath.GetPaths();

            return straightPath.Count > 0;
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
                MAX_STEER_POINTS, StraightPathOptions.None,
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

        /// <inheritdoc/>
        public event EventHandler Updating;
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
                foreach (var item in AgentQueries)
                {
                    item?.Dispose();
                }

                AgentQueries.Clear();
                AgentQueries = null;
            }
        }

        /// <summary>
        /// Gets a query for the specified agent
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns a new navigation mesh query</returns>
        private GraphAgentQuery GetAgentQuery(AgentType agent)
        {
            return AgentQueries.Find(a => agent.Equals(a.Agent));
        }

        /// <summary>
        /// Look up tiles in a bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns a list of tiles</returns>
        private IEnumerable<UpdateTileData> LookupTiles(BoundingBox bbox)
        {
            var points = bbox.GetCorners();

            var cornerTiles = LookupTiles(points).ToList();

            int xMin = cornerTiles.Min(c => c.X);
            int xMax = cornerTiles.Max(c => c.X);

            int yMin = cornerTiles.Min(c => c.Y);
            int yMax = cornerTiles.Max(c => c.Y);

            cornerTiles.Clear();

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    BoundingBox tileBounds = NavMesh.GetTileBounds(x, y, Input, Settings);

                    cornerTiles.Add(new UpdateTileData
                    {
                        X = x,
                        Y = y,
                        BoundingBox = tileBounds,
                    });
                }
            }

            return cornerTiles;
        }
        /// <summary>
        /// Look up for tiles in a position list
        /// </summary>
        /// <param name="positions">Position list</param>
        /// <returns>Returns a list of tiles</returns>
        private IEnumerable<UpdateTileData> LookupTiles(IEnumerable<Vector3> positions)
        {
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

            return tiles;
        }
        /// <summary>
        /// Builds the tiles in the specified position
        /// </summary>
        /// <param name="position">Position</param>
        private void BuildTiles(IEnumerable<Vector3> positions, bool update)
        {
            var tiles = LookupTiles(positions);

            foreach (var agentQ in AgentQueries)
            {
                BuildTiles(agentQ, tiles, update);
            }
        }
        /// <summary>
        /// Builds the tiles into the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        private void BuildTiles(BoundingBox bbox, bool update)
        {
            var tiles = LookupTiles(bbox);

            foreach (var agentQ in AgentQueries)
            {
                BuildTiles(agentQ, tiles, update);
            }
        }
        /// <summary>
        /// Builds the tiles into the list
        /// </summary>
        /// <param name="agentQ">Agent query</param>
        /// <param name="tiles">Tile list</param>
        private void BuildTiles(GraphAgentQuery agentQ, IEnumerable<UpdateTileData> tiles, bool update)
        {
            var bbox = Settings.Bounds ?? Input.BoundingBox;

            foreach (var tile in tiles)
            {
                if (update && agentQ.NavMesh.HasTilesAt(tile.X, tile.Y))
                {
                    continue;
                }

                var tileCfg = Settings.GetTiledConfig(agentQ.Agent, tile.BoundingBox);
                var tileCacheCfg = Settings.GetTileCacheConfig(agentQ.Agent, bbox);

                agentQ.NavMesh.BuildTileAtPosition(tile.X, tile.Y, Input, tileCfg, tileCacheCfg);
            }
        }
        /// <summary>
        /// Removes the tiles in the specified position list
        /// </summary>
        /// <param name="positions">Position list</param>
        private void RemoveTiles(IEnumerable<Vector3> positions)
        {
            var tiles = LookupTiles(positions);

            foreach (var agentQ in AgentQueries)
            {
                RemoveTiles(agentQ, tiles);
            }
        }
        /// <summary>
        /// Removes the tiles into the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        private void RemoveTiles(BoundingBox bbox)
        {
            var tiles = LookupTiles(bbox);

            foreach (var agentQ in AgentQueries)
            {
                RemoveTiles(agentQ, tiles);
            }
        }
        /// <summary>
        /// Removes the tiles in the list
        /// </summary>
        /// <param name="agentQ">Agent query</param>
        /// <param name="tiles">Tile list</param>
        private void RemoveTiles(GraphAgentQuery agentQ, IEnumerable<UpdateTileData> tiles)
        {
            foreach (var tile in tiles)
            {
                agentQ.NavMesh.RemoveTilesAtPosition(tile.X, tile.Y);
            }
        }

        /// <inheritdoc/>
        public bool CreateAt(Vector3 position)
        {
            Updating?.Invoke(this, new EventArgs());

            BuildTiles(new[] { position }, false);

            Updated?.Invoke(this, new EventArgs());

            return true;
        }
        /// <inheritdoc/>
        public bool CreateAt(BoundingBox bbox)
        {
            if (LookupTiles(bbox).Any())
            {
                return false;
            }

            Updating?.Invoke(this, new EventArgs());

            BuildTiles(bbox, false);

            Updated?.Invoke(this, new EventArgs());

            return true;
        }
        /// <inheritdoc/>
        public bool CreateAt(IEnumerable<Vector3> positions)
        {
            if (LookupTiles(positions).Any())
            {
                return false;
            }

            Updating?.Invoke(this, new EventArgs());

            BuildTiles(positions, false);

            Updated?.Invoke(this, new EventArgs());

            return true;
        }
        /// <inheritdoc/>
        public bool UpdateAt(Vector3 position)
        {
            if (!LookupTiles(new[] { position }).Any())
            {
                return false;
            }

            Updating?.Invoke(this, new EventArgs());

            BuildTiles(new[] { position }, true);

            Updated?.Invoke(this, new EventArgs());

            return true;
        }
        /// <inheritdoc/>
        public bool UpdateAt(BoundingBox bbox)
        {
            if (!LookupTiles(bbox).Any())
            {
                return false;
            }

            Updating?.Invoke(this, new EventArgs());

            BuildTiles(bbox, true);

            Updated?.Invoke(this, new EventArgs());

            return true;
        }
        /// <inheritdoc/>
        public bool UpdateAt(IEnumerable<Vector3> positions)
        {
            if (!LookupTiles(positions).Any())
            {
                return false;
            }

            Updating?.Invoke(this, new EventArgs());

            BuildTiles(positions, true);

            Updated?.Invoke(this, new EventArgs());

            return true;
        }
        /// <inheritdoc/>
        public bool RemoveAt(Vector3 position)
        {
            if (!LookupTiles(new[] { position }).Any())
            {
                return false;
            }

            Updating?.Invoke(this, new EventArgs());

            RemoveTiles(new[] { position });

            Updated?.Invoke(this, new EventArgs());

            return true;
        }
        /// <inheritdoc/>
        public bool RemoveAt(BoundingBox bbox)
        {
            if (!LookupTiles(bbox).Any())
            {
                return false;
            }

            Updating?.Invoke(this, new EventArgs());

            RemoveTiles(bbox);

            Updated?.Invoke(this, new EventArgs());

            return true;
        }
        /// <inheritdoc/>
        public bool RemoveAt(IEnumerable<Vector3> positions)
        {
            if (!LookupTiles(positions).Any())
            {
                return false;
            }

            Updating?.Invoke(this, new EventArgs());

            RemoveTiles(positions);

            Updated?.Invoke(this, new EventArgs());

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<IGraphNode> GetNodes(AgentType agent)
        {
            List<GraphNode> nodes = new List<GraphNode>();

            var graphQuery = GetAgentQuery(agent);
            if (graphQuery == null)
            {
                return new IGraphNode[] { };
            }

            nodes.AddRange(GraphNode.Build(graphQuery.NavMesh));

            return nodes.ToArray();
        }
        /// <inheritdoc/>
        public IEnumerable<Vector3> FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            var graphQuery = GetAgentQuery(agent);
            if (graphQuery == null)
            {
                return new Vector3[] { };
            }

            var status = CalcPath(
                graphQuery.CreateQuery(),
                new QueryFilter(), new Vector3(2, 4, 2), PathFindingMode.Follow,
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
        /// <inheritdoc/>
        public async Task<IEnumerable<Vector3>> FindPathAsync(AgentType agent, Vector3 from, Vector3 to)
        {
            IEnumerable<Vector3> result = new Vector3[] { };

            await Task.Run(() =>
            {
                var graphQuery = GetAgentQuery(agent);
                if (graphQuery == null)
                {
                    return;
                }

                var status = CalcPath(
                    graphQuery.CreateQuery(),
                    new QueryFilter(), new Vector3(2, 4, 2), PathFindingMode.Follow,
                    from, to, out var res);

                if (status.HasFlag(Status.DT_SUCCESS))
                {
                    result = res;
                }
            });

            return result;
        }

        /// <inheritdoc/>
        public bool IsWalkable(AgentType agent, Vector3 position)
        {
            return IsWalkable(agent, position, out _);
        }
        /// <inheritdoc/>
        public bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest)
        {
            nearest = null;

            //Find agent query
            var query = GetAgentQuery(agent)?.CreateQuery();
            if (query != null)
            {
                //Set extents based upon agent height
                var agentExtents = new Vector3(agent.Height, agent.Height * 2, agent.Height);

                var status = query.FindNearestPoly(
                    position, agentExtents, new QueryFilter(),
                    out int nRef, out Vector3 nPoint);

                if (nRef != 0 && !status.HasFlag(Status.DT_FAILURE))
                {
                    nearest = nPoint;

                    return nPoint.X == position.X && nPoint.Z == position.Z;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public int AddObstacle(IObstacle obstacle)
        {
            updated = false;

            List<Tuple<Agent, int>> obstacles = new List<Tuple<Agent, int>>();

            foreach (var agentQ in AgentQueries)
            {
                var cache = agentQ.NavMesh.TileCache;
                if (cache != null)
                {
                    cache.AddObstacle(obstacle, out int res);

                    obstacles.Add(new Tuple<Agent, int>(agentQ.Agent, res));
                }
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
        /// <inheritdoc/>
        public int AddObstacle(BoundingCylinder cylinder)
        {
            return AddObstacle(new ObstacleCylinder(cylinder));
        }
        /// <inheritdoc/>
        public int AddObstacle(BoundingBox bbox)
        {
            return AddObstacle(new ObstacleBox(bbox));
        }
        /// <inheritdoc/>
        public int AddObstacle(OrientedBoundingBox obb)
        {
            return AddObstacle(new ObstacleOrientedBox(obb));
        }
        /// <inheritdoc/>
        public void RemoveObstacle(int obstacleId)
        {
            updated = false;

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

        /// <inheritdoc/>
        public Vector3? FindRandomPoint(AgentType agent)
        {
            var query = GetAgentQuery(agent)?.CreateQuery();
            if (query != null)
            {
                var status = query.FindRandomPoint(new QueryFilter(), out _, out var pt);
                if (status == Status.DT_SUCCESS)
                {
                    return pt;
                }
            }

            return null;
        }
        /// <inheritdoc/>
        public Vector3? FindRandomPoint(AgentType agent, Vector3 position, float radius)
        {
            var query = GetAgentQuery(agent)?.CreateQuery();
            if (query != null)
            {
                QueryFilter filter = new QueryFilter();

                var fStatus = query.FindNearestPoly(position, new Vector3(2, 4, 2), filter, out int startRef, out var nearestPt);
                if (fStatus != Status.DT_SUCCESS)
                {
                    return null;
                }

                var pStatus = query.FindRandomPointAroundCircle(startRef, nearestPt, radius, filter, out _, out var pt);
                if (pStatus != Status.DT_SUCCESS)
                {
                    return null;
                }

                return pt;
            }

            return null;
        }

        /// <inheritdoc/>
        public void Update(GameTime gameTime)
        {
            var agentNms = AgentQueries
                .Select(agentQ => agentQ.NavMesh)
                .Where(nm => nm.TileCache != null)
                .ToArray();

            foreach (var nm in agentNms)
            {
                var status = nm.TileCache.Update(out bool upToDate);
                if (status.HasFlag(Status.DT_SUCCESS) && updated != upToDate)
                {
                    updated = upToDate;

                    if (updated)
                    {
                        Updated?.Invoke(this, new EventArgs());
                    }
                    else
                    {
                        Updating?.Invoke(this, new EventArgs());
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

            Crowds.Add(cr);

            return cr;
        }
        /// <summary>
        /// Adds a croud agent
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="pos">Position</param>
        /// <param name="param">Agent parameters</param>
        /// <returns>Returns the agent</returns>
        public CrowdAgent AddCrowdAgent(Crowd crowd, Vector3 pos, CrowdAgentParameters param)
        {
            return crowd.AddAgent(pos, param);
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
