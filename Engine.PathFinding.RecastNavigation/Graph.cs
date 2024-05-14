using Engine.PathFinding.RecastNavigation.Detour;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using Engine.PathFinding.RecastNavigation.Detour.Tiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Navigation graph
    /// </summary>
    public class Graph : IGraph
    {
        /// <summary>
        /// Updated flag
        /// </summary>
        private bool updated = true;

        /// <summary>
        /// Item indices
        /// </summary>
        private readonly List<GraphItem> itemIndices = [];
        /// <summary>
        /// Debug info
        /// </summary>
        private readonly Dictionary<Crowd, List<CrowdAgentDebugInfo>> debugInfo = [];
        /// <summary>
        /// Agent query list
        /// </summary>
        private readonly List<GraphAgentQueryFactory> agentQuerieFactories = [];
        /// <summary>
        /// Crowd list
        /// </summary>
        private readonly List<Crowd> crowds = [];

        /// <inheritdoc/>
        public bool Initialized { get; internal set; }
        /// <inheritdoc/>
        public bool EnableDebug { get; internal set; }
        /// <summary>
        /// Input geometry
        /// </summary>
        public InputGeometry Input { get; set; }
        /// <summary>
        /// Build settings
        /// </summary>
        public BuildSettings Settings { get; set; }
        /// <summary>
        /// Graph bounds
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                return Input.BoundingBox;
            }
        }

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
        public void AddAgent(GraphAgentType agent, NavMesh navMesh)
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
        public IEnumerable<(GraphAgentType Agent, NavMesh NavMesh)> GetAgents()
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
        /// Creates a new agent query
        /// </summary>
        /// <param name="agent">Agent</param>
        public NavMeshQuery CreateAgentQuery(AgentType agent)
        {
            return GetAgentQueryFactory(agent)?.CreateQuery();
        }

        /// <summary>
        /// Look up tiles in a bounding box
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns a list of tiles</returns>
        private List<(int x, int y)> LookupTiles(BoundingBox bbox)
        {
            var points = bbox.GetVertices();

            var cornerTiles = LookupTiles(points);

            int txMin = cornerTiles.Min(c => c.x);
            int txMax = cornerTiles.Max(c => c.x);

            int tyMin = cornerTiles.Min(c => c.y);
            int tyMax = cornerTiles.Max(c => c.y);

            List<(int x, int y)> tiles = [];

            for (int ty = tyMin; ty <= tyMax; ty++)
            {
                for (int tx = txMin; tx <= txMax; tx++)
                {
                    tiles.Add((tx, ty));
                }
            }

            return tiles;
        }
        /// <summary>
        /// Look up for tiles in a position list
        /// </summary>
        /// <param name="positions">Position list</param>
        /// <returns>Returns a list of tiles</returns>
        private List<(int x, int y)> LookupTiles(IEnumerable<Vector3> positions)
        {
            List<(int x, int y)> tiles = [];

            var tileCellSize = Settings.TileCellSize;
            var bounds = Bounds;

            foreach (var position in positions)
            {
                NavMesh.GetTileAtPosition(position, tileCellSize, bounds, out var tx, out var ty);

                if (tiles.Exists(t => t.x == tx && t.y == ty))
                {
                    continue;
                }

                tiles.Add((tx, ty));
            }

            return tiles;
        }
        /// <summary>
        /// Builds the tiles in the list
        /// </summary>
        /// <param name="tiles">Tile list</param>
        private void BuildTiles(List<(int x, int y)> tiles, bool update)
        {
            foreach (var agentQ in agentQuerieFactories)
            {
                BuildTiles(agentQ, tiles, update);
            }
        }
        /// <summary>
        /// Builds the tiles into the list
        /// </summary>
        /// <param name="agentQ">Agent query</param>
        /// <param name="updateTiles">Tile list</param>
        private void BuildTiles(GraphAgentQueryFactory agentQ, IEnumerable<(int x, int y)> updateTiles, bool update)
        {
            foreach (var (x, y) in updateTiles)
            {
                if (update && agentQ.NavMesh.HasTilesAt(x, y))
                {
                    continue;
                }

                agentQ.NavMesh.BuildTileAt(x, y, Settings, Input, agentQ.Agent);
            }
        }
        /// <summary>
        /// Removes the tile list
        /// </summary>
        /// <param name="tiles">Tile list</param>
        private void RemoveTiles(List<(int x, int y)> tiles)
        {
            foreach (var agentQ in agentQuerieFactories)
            {
                agentQ.NavMesh.RemoveTilesAt(tiles);
            }
        }

        /// <inheritdoc/>
        public Vector3? FindRandomPoint(AgentType agent)
        {
            var query = CreateAgentQuery(agent);
            if (query == null)
            {
                return null;
            }

            var agentFilter = agent.PathFilter;

            var status = query.FindRandomPoint(agentFilter, out _, out var pt);
            if (status == Status.DT_SUCCESS)
            {
                return pt;
            }

            return null;
        }
        /// <inheritdoc/>
        public Vector3? FindRandomPoint(AgentType agent, Vector3 position, float radius)
        {
            var query = CreateAgentQuery(agent);
            if (query == null)
            {
                return null;
            }

            var agentExtents = new Vector3(agent.Height);
            var agentFilter = agent.PathFilter;

            var fStatus = query.FindNearestPoly(position, agentExtents, agentFilter, out int startRef, out var nearestPt);
            if (fStatus != Status.DT_SUCCESS)
            {
                return null;
            }

            var pStatus = query.FindRandomPointAroundCircle(startRef, nearestPt, radius, agentFilter, out _, out var pt);
            if (pStatus != Status.DT_SUCCESS)
            {
                return null;
            }

            return pt;
        }
        /// <inheritdoc/>
        public IEnumerable<IGraphNode> GetNodes(AgentType agent)
        {
            var graphQuery = GetAgentQueryFactory(agent);
            if (graphQuery == null)
            {
                return [];
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
            var query = CreateAgentQuery(agent);
            if (query == null)
            {
                return [];
            }

            var agentExtents = new Vector3(agent.Height);
            var agentFilter = agent.PathFilter;

            var status = query.CalcPath(agentFilter, agentExtents, PathFindingMode.Straight, from, to, out var result);
            if (!status.HasFlag(Status.DT_SUCCESS))
            {
                return [];
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
            var query = CreateAgentQuery(agent);
            if (query == null)
            {
                return false;
            }

            //Set extents based upon agent height
            var agentExtents = new Vector3(agent.Height);
            var agentFilter = agent.PathFilter;

            var status = query.FindNearestPoly(position, agentExtents, agentFilter, out int nRef, out var nPoint);
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
        public void CreateAt(Vector3 position, Action<GraphUpdateStates> callback = null)
        {
            CreateAt(LookupTiles([position]), callback);
        }
        /// <inheritdoc/>
        public void CreateAt(BoundingBox bbox, Action<GraphUpdateStates> callback = null)
        {
            CreateAt(LookupTiles(bbox), callback);
        }
        /// <inheritdoc/>
        public void CreateAt(IEnumerable<Vector3> positions, Action<GraphUpdateStates> callback = null)
        {
            CreateAt(LookupTiles(positions), callback);
        }
        /// <summary>
        /// Creates the tile list
        /// </summary>
        /// <param name="tiles">Tile list</param>
        private void CreateAt(List<(int x, int y)> tiles, Action<GraphUpdateStates> callback)
        {
            if (tiles.Count == 0)
            {
                return;
            }

            Task.Run(() =>
            {
                callback?.Invoke(GraphUpdateStates.Updating);

                BuildTiles(tiles, false);

                callback?.Invoke(GraphUpdateStates.Updated);
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void UpdateAt(Vector3 position, Action<GraphUpdateStates> callback = null)
        {
            UpdateAt(LookupTiles([position]), callback);
        }
        /// <inheritdoc/>
        public void UpdateAt(BoundingBox bbox, Action<GraphUpdateStates> callback = null)
        {
            UpdateAt(LookupTiles(bbox), callback);
        }
        /// <inheritdoc/>
        public void UpdateAt(IEnumerable<Vector3> positions, Action<GraphUpdateStates> callback = null)
        {
            UpdateAt(LookupTiles(positions), callback);
        }
        /// <summary>
        /// Updates the tile list
        /// </summary>
        /// <param name="tiles">Tile list</param>
        private void UpdateAt(List<(int x, int y)> tiles, Action<GraphUpdateStates> callback)
        {
            if (tiles.Count == 0)
            {
                return;
            }

            Task.Run(() =>
            {
                callback?.Invoke(GraphUpdateStates.Updating);

                BuildTiles(tiles, true);

                callback?.Invoke(GraphUpdateStates.Updated);

            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void RemoveAt(Vector3 position, Action<GraphUpdateStates> callback = null)
        {
            RemoveAt(LookupTiles([position]), callback);
        }
        /// <inheritdoc/>
        public void RemoveAt(BoundingBox bbox, Action<GraphUpdateStates> callback = null)
        {
            RemoveAt(LookupTiles(bbox), callback);
        }
        /// <inheritdoc/>
        public void RemoveAt(IEnumerable<Vector3> positions, Action<GraphUpdateStates> callback = null)
        {
            RemoveAt(LookupTiles(positions), callback);
        }
        /// <summary>
        /// Removes the tile list
        /// </summary>
        /// <param name="tiles">Tile list</param>
        private void RemoveAt(List<(int x, int y)> tiles, Action<GraphUpdateStates> callback)
        {
            if (tiles.Count == 0)
            {
                return;
            }

            Task.Run(() =>
            {
                callback?.Invoke(GraphUpdateStates.Updating);

                RemoveTiles(tiles);

                callback?.Invoke(GraphUpdateStates.Updated);
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public int AddObstacle(IObstacle obstacle)
        {
            updated = false;

            var obstacles = new List<Tuple<GraphAgentType, int>>();

            foreach (var agentQ in agentQuerieFactories)
            {
                var cache = agentQ.NavMesh.TileCache;
                if (cache != null)
                {
                    cache.AddObstacle(obstacle, out int res);

                    obstacles.Add(new Tuple<GraphAgentType, int>(agentQ.Agent, res));
                }
            }

            if (obstacles.Count == 0)
            {
                return -1;
            }

            var o = new GraphItem()
            {
                Indices = [.. obstacles]
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
        public IGraphDebug GetDebugInfo(AgentType agent)
        {
            return new GraphDebug(this, agent);
        }

        /// <inheritdoc/>
        public void Update(IGameTime gameTime, Action<GraphUpdateStates> callback = null)
        {
            var tcs = agentQuerieFactories
                .Where(a => a.NavMesh?.TileCache != null)
                .Select(a => (a.Agent, a.NavMesh.TileCache));

            foreach (var (agent, tc) in tcs)
            {
                if (tc.Updating())
                {
                    callback?.Invoke(GraphUpdateStates.Updating);
                }

                var status = tc.Update(agent, out bool upToDate, out bool cacheUpdated);
                if (updated == upToDate || !status.HasFlag(Status.DT_SUCCESS))
                {
                    continue;
                }

                updated = upToDate;

                if (cacheUpdated)
                {
                    callback?.Invoke(GraphUpdateStates.Updated);
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
            var query = CreateAgentQuery(agent);
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
            var query = CreateAgentQuery(agent);
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
            if (!debugInfo.TryGetValue(crowd, out var debugData))
            {
                debugData = [];
                debugInfo.Add(crowd, debugData);
            }

            if (!debugData.Exists(l => l.Agent == crowdAgent))
            {
                debugData.Add(new() { Agent = crowdAgent });
            }
        }
        /// <summary>
        /// Disables debug info for crowd and agent
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="crowdAgent">Agent</param>
        public void DisableDebugInfo(Crowd crowd, CrowdAgent crowdAgent)
        {
            if (!debugInfo.TryGetValue(crowd, out var debugData))
            {
                return;
            }

            debugData.RemoveAll(l => l.Agent == crowdAgent);
        }
    }
}
