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
        private readonly List<GraphItem> itemIndices = new();
        /// <summary>
        /// Debug info
        /// </summary>
        private readonly Dictionary<Crowd, List<CrowdAgentDebugInfo>> debugInfo = new();
        /// <summary>
        /// Agent query list
        /// </summary>
        private readonly List<GraphAgentQueryFactory> agentQuerieFactories = new();
        /// <summary>
        /// Crowd list
        /// </summary>
        private readonly List<Crowd> crowds = new();

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
                foreach (var item in agentQuerieFactories)
                {
                    item?.Dispose();
                }

                agentQuerieFactories.Clear();
            }
        }

        /// <summary>
        /// Adds an agent to the graph
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="navMesh">Navigation mesh</param>
        public void AddAgent(Agent agent, NavMesh navMesh)
        {
            agentQuerieFactories.Add(new GraphAgentQueryFactory
            {
                Agent = agent,
                NavMesh = navMesh,
                MaxNodes = Settings.MaxNodes,
            });
        }
        /// <summary>
        /// Gets the agent list
        /// </summary>
        public IEnumerable<(Agent Agent, NavMesh NavMesh)> GetAgents()
        {
            return agentQuerieFactories.Select(agentQ => (agentQ.Agent, agentQ.NavMesh)).ToArray();
        }
        /// <summary>
        /// Gets a query for the specified agent
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns a new navigation mesh query</returns>
        private GraphAgentQueryFactory GetAgentQueryFactory(AgentType agent)
        {
            return agentQuerieFactories.Find(a => agent.Equals(a.Agent));
        }

        /// <summary>
        /// Look up tiles in a bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns a list of tiles</returns>
        private IEnumerable<UpdateTileData> LookupTiles(BoundingBox bbox)
        {
            var points = bbox.GetVertices();

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
            var tiles = new List<UpdateTileData>();

            foreach (var position in positions)
            {
                NavMesh.GetTileAtPosition(position, Input, Settings, out var tx, out var ty, out var bbox);

                if (!tiles.Exists(t => t.X == tx && t.Y == ty))
                {
                    var v = new UpdateTileData()
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

            foreach (var agentQ in agentQuerieFactories)
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

            foreach (var agentQ in agentQuerieFactories)
            {
                BuildTiles(agentQ, tiles, update);
            }
        }
        /// <summary>
        /// Builds the tiles into the list
        /// </summary>
        /// <param name="agentQ">Agent query</param>
        /// <param name="tiles">Tile list</param>
        private void BuildTiles(GraphAgentQueryFactory agentQ, IEnumerable<UpdateTileData> tiles, bool update)
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

            foreach (var agentQ in agentQuerieFactories)
            {
                agentQ.RemoveTiles(tiles);
            }
        }
        /// <summary>
        /// Removes the tiles into the bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        private void RemoveTiles(BoundingBox bbox)
        {
            var tiles = LookupTiles(bbox);

            foreach (var agentQ in agentQuerieFactories)
            {
                agentQ.RemoveTiles(tiles);
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
            var graphQuery = GetAgentQueryFactory(agent);
            if (graphQuery == null)
            {
                return Enumerable.Empty<IGraphNode>();
            }

            return GraphNode.FindAll(graphQuery.NavMesh);
        }
        /// <inheritdoc/>
        public IGraphNode FindNode(AgentType agent, Vector3 point)
        {
            var graphQuery = GetAgentQueryFactory(agent);
            if (graphQuery == null)
            {
                return null;
            }

            return GraphNode.FindNode(graphQuery.NavMesh, point);
        }
        /// <inheritdoc/>
        public IEnumerable<Vector3> FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            var graphQuery = GetAgentQueryFactory(agent)?.CreateQuery();
            if (graphQuery == null)
            {
                return Enumerable.Empty<Vector3>();
            }

            var status = graphQuery.CalcPath(
                new QueryFilter(), new Vector3(2, 4, 2), PathFindingMode.Follow,
                from, to, out var result);

            if (!status.HasFlag(Status.DT_SUCCESS))
            {
                return Enumerable.Empty<Vector3>();
            }

            return result;
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<Vector3>> FindPathAsync(AgentType agent, Vector3 from, Vector3 to)
        {
            return await Task.Run(() => FindPath(agent, from, to));
        }

        /// <inheritdoc/>
        public bool IsWalkable(AgentType agent, Vector3 position, float distanceThreshold)
        {
            return IsWalkable(agent, position, distanceThreshold, out _);
        }
        /// <inheritdoc/>
        public bool IsWalkable(AgentType agent, Vector3 position, float distanceThreshold, out Vector3? nearest)
        {
            nearest = null;

            //Find agent query
            var query = GetAgentQueryFactory(agent)?.CreateQuery();
            if (query == null)
            {
                return false;
            }

            //Set extents based upon agent height
            var agentExtents = new Vector3(agent.Height);

            var status = query.FindNearestPoly(
                position, agentExtents, new QueryFilter(),
                out int nRef, out Vector3 nPoint);

            if (nRef == 0 || status.HasFlag(Status.DT_FAILURE))
            {
                return false;
            }

            nearest = nPoint;

            return
                nPoint.XZ() == position.XZ() &&
                Vector3.Distance(nPoint, position) <= distanceThreshold;
        }

        /// <inheritdoc/>
        public int AddObstacle(IObstacle obstacle)
        {
            updated = false;

            var obstacles = new List<Tuple<Agent, int>>();

            foreach (var agentQ in agentQuerieFactories)
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
            if (instance == null)
            {
                return;
            }

            foreach (var item in instance.Indices)
            {
                var tileCache = GetAgentQueryFactory(item.Item1)?.NavMesh.TileCache;

                tileCache?.RemoveObstacle(item.Item2);
            }

            itemIndices.Remove(instance);
        }

        /// <inheritdoc/>
        public Vector3? FindRandomPoint(AgentType agent)
        {
            var query = GetAgentQueryFactory(agent)?.CreateQuery();
            if (query == null)
            {
                return null;
            }

            var status = query.FindRandomPoint(new QueryFilter(), out _, out var pt);
            if (status == Status.DT_SUCCESS)
            {
                return pt;
            }

            return null;
        }
        /// <inheritdoc/>
        public Vector3? FindRandomPoint(AgentType agent, Vector3 position, float radius)
        {
            var query = GetAgentQueryFactory(agent)?.CreateQuery();
            if (query == null)
            {
                return null;
            }

            var filter = new QueryFilter();

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

        /// <inheritdoc/>
        public void Update(GameTime gameTime)
        {
            var agentNms = agentQuerieFactories
                .Select(agentQ => agentQ.NavMesh)
                .Where(nm => nm.TileCache != null);

            foreach (var nm in agentNms)
            {
                var status = nm.TileCache.Update(out bool upToDate);
                if (updated == upToDate || !status.HasFlag(Status.DT_SUCCESS))
                {
                    continue;
                }

                updated = upToDate;

                if (updated)
                {
                    Updated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Updating?.Invoke(this, EventArgs.Empty);
                }
            }

            foreach (var crowd in crowds)
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
            var navMesh = (GetAgentQueryFactory(settings.Agent)?.NavMesh) ?? throw new ArgumentException($"No navigation mesh found for the specified {nameof(settings.Agent)}.", nameof(settings));

            var cr = new Crowd(navMesh, settings);
            crowds.Add(cr);
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
            var query = GetAgentQueryFactory(agent)?.CreateQuery();
            if (query == null)
            {
                return;
            }

            Status status = query.FindNearestPoly(p, crowd.GetQueryExtents(), crowd.GetFilter(0), out int poly, out Vector3 nP);
            if (status == Status.DT_FAILURE)
            {
                return;
            }

            foreach (var ag in crowd.GetAgents())
            {
                ag.RequestMoveTarget(poly, nP);
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
            var query = GetAgentQueryFactory(agent)?.CreateQuery();
            if (query == null)
            {
                return;
            }

            Status status = query.FindNearestPoly(p, crowd.GetQueryExtents(), crowd.GetFilter(0), out int poly, out Vector3 nP);
            if (status == Status.DT_FAILURE)
            {
                return;
            }

            crowdAgent.RequestMoveTarget(poly, nP);
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

            if (!debugInfo[crowd].Exists(l => l.Agent == crowdAgent))
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
