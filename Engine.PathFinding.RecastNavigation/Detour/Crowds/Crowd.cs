using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Provides local steering behaviors for a group of agents. 
    /// </summary>
    public class Crowd : IGroup<CrowdAgentSettings>
    {
        /// The maximum number of crowd avoidance configurations supported by the
        /// crowd manager.
        /// @ingroup crowd
        /// @see dtObstacleAvoidanceParams, dtCrowd::setObstacleAvoidanceParams(), dtCrowd::getObstacleAvoidanceParams(),
        ///		 dtCrowdAgentParams::obstacleAvoidanceType
        const int DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS = 8;
        /// The maximum number of query filter types supported by the crowd manager.
        /// @ingroup crowd
        /// @see dtQueryFilter, dtCrowd::getFilter() dtCrowd::getEditableFilter(),
        ///		dtCrowdAgentParams::queryFilterType
        const int DT_CROWD_MAX_QUERY_FILTER_TYPE = 16;
        /// <summary>
        /// The maximum number of iterations per update
        /// </summary>
        const int MAX_ITERS_PER_UPDATE = 100;
        /// <summary>
        /// The maximum number of path queue nodes
        /// </summary>
        const int MAX_PATHQUEUE_NODES = 4096;
        /// <summary>
        /// The maximum number of navigation mesh query nodes
        /// </summary>
        const int MAX_COMMON_NODES = 512;

        /// <summary>
        /// Agent info structure
        /// </summary>
        struct AgentInfo
        {
            /// <summary>
            /// Crowd agent id
            /// </summary>
            public int Id;
            /// <summary>
            /// Crowd agent
            /// </summary>
            public CrowdAgent Agent;
            /// <summary>
            /// Crowd agent debug information
            /// </summary>
            public CrowdAgentDebugInfo DebugInfo;
        }

        /// <summary>
        /// Agent id
        /// </summary>
        private int agentId = 0;
        /// <summary>
        /// Agent settings
        /// </summary>
        private readonly CrowdAgentSettings agentSettings;
        /// <summary>
        /// Maximum number of agents
        /// </summary>
        private readonly int maxCrowdAgents;
        /// <summary>
        /// Agent list
        /// </summary>
        private readonly List<AgentInfo> crowdAgents;
        /// <summary>
        /// Movement request queue
        /// </summary>
        private readonly List<CrowdAgent> movQueue;
        /// <summary>
        /// Topology optimization queue
        /// </summary>
        private readonly List<CrowdAgent> topoQueue;
        /// <summary>
        /// Agent animation dictionary
        /// </summary>
        private readonly Dictionary<CrowdAgent, CrowdAgentAnimation> crowdAgentAnims = [];
        /// <summary>
        /// Filter list
        /// </summary>
        private readonly List<IGraphQueryFilter> filters = [];
        /// <summary>
        /// Obstacle query list
        /// </summary>
        private readonly List<ObstacleAvoidanceParams> obstacleQueryParams = [];
        /// <summary>
        /// Obstacle avoidance query
        /// </summary>
        private readonly ObstacleAvoidanceQuery obstacleQuery;
        /// <summary>
        /// Proximity grid
        /// </summary>
        private readonly ProximityGrid<CrowdAgent> grid;
        /// <summary>
        /// Maximum path result count
        /// </summary>
        private readonly int maxPathResult;
        /// <summary>
        /// Sample velocity adaptative
        /// </summary>
        private readonly bool sampleVelocityAdaptative;
        /// <summary>
        /// Collision resolve iteration count
        /// </summary>
        private readonly int collisionResolveIterations;
        /// <summary>
        /// Collision resolve factor
        /// </summary>
        private readonly float collisionResolveFactor;
        /// <summary>
        /// Agent placement extents
        /// </summary>
        private readonly Vector3 agentPlacementHalfExtents;
        /// <summary>
        /// Target position
        /// </summary>
        private Vector3? target;
        /// <summary>
        /// Navigation query
        /// </summary>
        private readonly NavMeshQuery navquery;
        /// <summary>
        /// Path queue
        /// </summary>
        private readonly PathQueue pathQueue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graph">Navigation graph of the crowd</param>
        /// <param name="settings">Settings</param>
        public Crowd(Graph graph, CrowdParameters settings)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(settings.Agent);
            var navMesh = graph.CreateAgentQuery(settings.Agent)?.GetAttachedNavMesh();

            ArgumentNullException.ThrowIfNull(navMesh);
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(settings.MaxAgents);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(settings.MaxPathResult);

            agentSettings = CrowdAgentSettings.FromAgent(settings.Agent);
            maxCrowdAgents = settings.MaxAgents;
            crowdAgents = new(settings.MaxAgents);
            movQueue = new(settings.MaxAgents);
            topoQueue = new(settings.MaxAgents);
            maxPathResult = settings.MaxPathResult;
            sampleVelocityAdaptative = settings.SampleVelocityAdaptative;
            collisionResolveIterations = settings.CollisionResolveIterations;
            collisionResolveFactor = settings.CollisionResolveFactor;

            // Larger than agent radius because it is also used for agent recovery.
            agentPlacementHalfExtents = new Vector3(settings.MaxAgentRadius * 2.0f, settings.MaxAgentRadius * 1.5f, settings.MaxAgentRadius * 2.0f);

            grid = new(1000, settings.MaxAgentRadius * 3);

            obstacleQuery = new(6, 8);

            // Init filters
            for (int i = 0; i < DT_CROWD_MAX_QUERY_FILTER_TYPE; i++)
            {
                filters.Add(settings.Agent.PathFilter);
            }

            // Init obstacle query params.
            for (int i = 0; i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS; i++)
            {
                obstacleQueryParams.Add(new()
                {
                    VelBias = 0.4f,
                    WeightDesVel = 2.0f,
                    WeightCurVel = 0.75f,
                    WeightSide = 0.75f,
                    WeightToi = 2.5f,
                    HorizTime = 2.5f,
                    GridSize = 33,
                    AdaptiveDivs = 7,
                    AdaptiveRings = 2,
                    AdaptiveDepth = 5
                });
            }

            // Allocate temp buffer for merging paths.
            pathQueue = new(navMesh, maxPathResult, MAX_PATHQUEUE_NODES);

            // The navquery is mostly used for local searches, no need for large node pool.
            navquery = new(navMesh, MAX_COMMON_NODES);
        }

        /// <summary>
        /// Gets the next agent id for the crowd
        /// </summary>
        private int NextId()
        {
            return ++agentId;
        }

        /// <inheritdoc/>
        public int AddAgent(Vector3 pos)
        {
            if (crowdAgents.Count >= maxCrowdAgents)
            {
                return -1;
            }

            // Find nearest position on navmesh and place the agent there.
            var (poly, nP) = FindNearestPoly(pos, agentSettings.QueryFilterTypeIndex);

            var state = poly > 0 ?
                CrowdAgentState.DT_CROWDAGENT_STATE_WALKING :
                CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;

            CrowdAgent cag = new(agentSettings)
            {
                Partial = false,
                TopologyOptTime = 0,
                TargetReplanTime = 0,
                DVel = Vector3.Zero,
                NVel = Vector3.Zero,
                Vel = Vector3.Zero,
                NPos = nP,
                DesiredSpeed = 0,
                State = state,
                Active = true,
                TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE,
            };

            cag.Initialize(poly, nP, maxPathResult);

            int id = NextId();
            crowdAgents.Add(new() { Id = id, Agent = cag, DebugInfo = new(8) });
            return id;
        }
        /// <inheritdoc/>
        public void RemoveAgent(int id)
        {
            crowdAgents.RemoveAll(a => a.Id == id);
        }

        /// <inheritdoc/>
        public int Count()
        {
            return crowdAgents.Count;
        }

        /// <inheritdoc/>
        public Vector3 GetTarget()
        {
            return target ?? new(float.MaxValue);
        }
        /// <inheritdoc/>
        public (int Id, Vector3 Position)[] GetPositions()
        {
            return crowdAgents
                .Select(a => (a.Id, a.Agent.NPos))
                .ToArray();
        }
        /// <inheritdoc/>
        public Vector3 GetPosition(int id)
        {
            return crowdAgents
                .Find(a => a.Id == id).Agent?.NPos ?? Vector3.Zero;
        }

        /// <summary>
        /// Gets the crowd's proximity grid.
        /// </summary>
        /// <returns>The crowd's proximity grid.</returns>
        public ProximityGrid<CrowdAgent> GetGrid()
        {
            return grid;
        }

        /// <inheritdoc/>
        public void UpdateSettings(CrowdAgentSettings settings)
        {
            foreach (var ag in crowdAgents)
            {
                ag.Agent.UpdateSettings(settings);
            }
        }
        /// <inheritdoc/>
        public void UpdateSettings(int id, CrowdAgentSettings settings)
        {
            crowdAgents.Find(a => a.Id == id).Agent?.UpdateSettings(settings);
        }

        /// <inheritdoc/>
        public IGraphQueryFilter GetFilter(int i)
        {
            return (i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE) ? filters[i] : null;
        }
        /// <inheritdoc/>
        public void SetFilter(int i, IGraphQueryFilter filter)
        {
            if (i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE)
            {
                filters[i] = filter;
            }
        }

        /// <inheritdoc/>
        public void RequestMove(Vector3 pos)
        {
            var (poly, nP) = FindNearestPoly(pos, 0);

            target = nP;

            foreach (var ag in crowdAgents)
            {
                ag.Agent.RequestMoveTarget(poly, nP);
            }
        }
        /// <inheritdoc/>
        public void RequestMove(int id, Vector3 pos)
        {
            var ag = crowdAgents.Find(a => a.Id == id).Agent;
            if (ag == null)
            {
                return;
            }

            var (poly, nP) = FindNearestPoly(pos, ag.Settings.QueryFilterTypeIndex);

            ag.RequestMoveTarget(poly, nP);
        }
        /// <summary>
        /// Finds the nearest polygon, and position in the polygon, to the specified position
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="filterIndex">Filter index</param>
        private (int poly, Vector3 position) FindNearestPoly(Vector3 pos, int filterIndex)
        {
            //Find nearest polygon
            Status status = navquery.FindNearestPoly(pos, agentPlacementHalfExtents, GetFilter(filterIndex), out int poly, out var nP);
            if (status == Status.DT_FAILURE)
            {
                return (0, pos);
            }

            return (poly, nP);
        }

        /// <inheritdoc/>
        public void Update(IGameTime gameTime)
        {
            var activeAgents = crowdAgents.Where(a => a.Agent.Active);
            if (!activeAgents.Any())
            {
                return;
            }

            var allAgents = crowdAgents
                .Select(a => a.Agent)
                .ToArray();

            var toProcAgents = activeAgents
                .Where(a => a.Agent.State == CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                .ToArray();

            var walkingAgents = toProcAgents
                .Select(a => a.Agent)
                .ToArray();

            float dt = gameTime.ElapsedSeconds;

            // Check that all agents still have valid paths.
            CheckPathValidity(walkingAgents, dt);

            // Update async move request and path finder.
            UpdateMoveRequest(allAgents);

            // Process path results.
            ProcessPathResults(allAgents);

            // Optimize path topology.
            UpdateTopologyOptimization(walkingAgents, dt);

            // Register agents to proximity grid.
            GridRegisterAgents(allAgents);

            // Get nearby navmesh segments and agents to collide with.
            FindColliders(walkingAgents);

            // Find next corner to steer to.
            FindNextCorner(toProcAgents);

            // Trigger off-mesh connections (depends on corners).
            TriggerOffMeshConnections(walkingAgents);

            // Calculate steering.
            CalculateSteering(walkingAgents);

            // Velocity planning.	
            VelocityPlanning(toProcAgents);

            // Integrate.
            IntegrateAgents(walkingAgents, dt);

            // Handle collisions.
            HandleCollisions(walkingAgents);

            // Moves the agents over the navigation mesh
            MoveAgents(walkingAgents);

            // Update agents using off-mesh connection.
            AnimateAgentsOverOffMeshConnection(dt);
        }
        /// <summary>
        /// Checks the validity of the path for the specified agents
        /// </summary>
        /// <param name="agents">Agent list</param>
        /// <param name="dt">Elapsed seconds</param>
        private void CheckPathValidity(CrowdAgent[] agents, float dt)
        {
            foreach (var ag in agents)
            {
                bool replan = CheckPathValidity(ag, dt);

                if (!replan || ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                {
                    continue;
                }

                // Try to replan path to goal.
                if (!ag.RequestMoveTargetReplan(ag.TargetRef, ag.TargetPos))
                {
                    int idx = crowdAgents.FindIndex(a => a.Agent == ag);

                    Logger.WriteError(this, $"RequestMoveTargetReplan error: Id=>{idx} from {ag.TargetRef} to {ag.TargetPos}");
                }
            }
        }
        /// <summary>
        /// Checks the validity of the path for the specified agent
        /// </summary>
        /// <param name="ag">Agent</param>
        /// <param name="dt">Elapsed seconds</param>
        private bool CheckPathValidity(CrowdAgent ag, float dt)
        {
            int CHECK_LOOKAHEAD = 10;
            float TARGET_REPLAN_DELAY = 1; // seconds

            ag.TargetReplanTime += dt;

            bool replan = false;

            // First check that the current location is valid.
            Vector3 agentPos = ag.NPos;
            int agentRef = ag.Corridor.GetFirstPoly();
            if (!navquery.IsValidPolyRef(agentRef, filters[ag.Settings.QueryFilterTypeIndex]))
            {
                // Current location is not valid, try to reposition.
                // FIX: this can snap agents, how to handle that?
                navquery.FindNearestPoly(
                    ag.NPos, agentPlacementHalfExtents, filters[ag.Settings.QueryFilterTypeIndex],
                    out agentRef, out Vector3 nearest);

                agentPos = nearest;

                if (agentRef <= 0)
                {
                    // Could not find location in navmesh, set state to invalid.
                    ag.Corridor.Reset(0, agentPos);
                    ag.Partial = false;
                    ag.Boundary.Reset();
                    ag.State = CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
                    return false;
                }

                // Make sure the first polygon is valid, but leave other valid
                // polygons in the path so that replanner can adjust the path better.
                ag.Corridor.FixPathStart(agentRef, agentPos);
                ag.Boundary.Reset();
                ag.NPos = agentPos;

                replan = true;
            }

            // If the agent does not have move target or is controlled by velocity, no need to recover the target nor replan.
            if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
            {
                return false;
            }

            // Try to recover move request position.
            if (ag.TargetState != MoveRequestState.DT_CROWDAGENT_TARGET_NONE &&
                ag.TargetState != MoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
            {
                if (!navquery.IsValidPolyRef(ag.TargetRef, filters[ag.Settings.QueryFilterTypeIndex]))
                {
                    // Current target is not valid, try to reposition.
                    navquery.FindNearestPoly(
                        ag.TargetPos, agentPlacementHalfExtents, filters[ag.Settings.QueryFilterTypeIndex],
                        out int r, out Vector3 nearest);

                    ag.TargetRef = r;
                    ag.TargetPos = nearest;

                    replan = true;
                }
                if (ag.TargetRef <= 0)
                {
                    // Failed to reposition target, fail moverequest.
                    ag.Corridor.Reset(agentRef, agentPos);
                    ag.Partial = false;
                    ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;
                }
            }

            // If nearby corridor is not valid, replan.
            if (!ag.Corridor.IsValid(navquery, filters[ag.Settings.QueryFilterTypeIndex], CHECK_LOOKAHEAD))
            {
                replan = true;
            }

            // If the end of the path is near and it is not the requested location, replan.
            if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VALID &&
                ag.TargetReplanTime > TARGET_REPLAN_DELAY &&
                ag.Corridor.GetPathCount() < CHECK_LOOKAHEAD &&
                ag.Corridor.GetLastPoly() != ag.TargetRef)
            {
                replan = true;
            }

            return replan;
        }
        /// <summary>
        /// Updates the movement request
        /// </summary>
        /// <param name="allAgents">Agent list</param>
        private void UpdateMoveRequest(CrowdAgent[] allAgents)
        {
            // Fire off new requests.
            FireNewRequests(allAgents);

            foreach (var ag in movQueue)
            {
                ag.TargetPathqRef = pathQueue.Request(
                    ag.Corridor.GetLastPoly(),
                    ag.TargetRef,
                    ag.Corridor.GetTarget(),
                    ag.TargetPos,
                    filters[ag.Settings.QueryFilterTypeIndex]);

                if (ag.TargetPathqRef != PathQueue.DT_PATHQ_INVALID)
                {
                    ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH;
                }
            }

            // Update requests.
            pathQueue.Update(MAX_ITERS_PER_UPDATE);
        }
        /// <summary>
        /// Fires new requests
        /// </summary>
        /// <param name="allAgents">Agent list</param>
        private void FireNewRequests(CrowdAgent[] allAgents)
        {
            movQueue.Clear();

            foreach (var ag in allAgents)
            {
                if (!ag.Active)
                {
                    continue;
                }
                if (ag.State == CrowdAgentState.DT_CROWDAGENT_STATE_INVALID)
                {
                    continue;
                }
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING)
                {
                    UpdateAgent(ag);
                }

                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                {
                    movQueue.Add(ag);
                }
            }

            if (movQueue.Count > 1)
            {
                // Sort neighbours based on greatest time.
                movQueue.Sort((a1, a2) => -a1.TargetReplanTime.CompareTo(a2.TargetReplanTime));
            }
        }
        /// <summary>
        /// Updates the specified agent
        /// </summary>
        /// <param name="ag">Agent</param>
        private void UpdateAgent(CrowdAgent ag)
        {
            const int MAX_ITER = 20;
            const int MAX_RES = 32;

            var path = ag.Corridor.GetPath();
            if (path.Length == 0)
            {
                Logger.WriteWarning(this, $"Crowd.UpdateMoveRequest {ag} no path assigned;");
            }

            // Quick search towards the goal.
            PathPoint start = new() { Ref = path[0], Pos = ag.NPos };
            PathPoint end = new() { Ref = ag.TargetRef, Pos = ag.TargetPos };
            navquery.InitSlicedFindPath(filters[ag.Settings.QueryFilterTypeIndex], start, end);
            navquery.UpdateSlicedFindPath(MAX_ITER, out _);

            Status qStatus;
            SimplePath reqPath;
            if (ag.TargetReplan)
            {
                // Try to use existing steady path during replan if possible.
                qStatus = navquery.FinalizeSlicedFindPathPartial(MAX_RES, path, out reqPath);
            }
            else
            {
                // Try to move towards target when goal changes.
                qStatus = navquery.FinalizeSlicedFindPath(MAX_RES, out reqPath);
            }

            Vector3 reqPos = Vector3.Zero;
            if (qStatus != Status.DT_FAILURE && reqPath.Count > 0)
            {
                // In progress or succeed.
                if (reqPath.End != ag.TargetRef)
                {
                    // Partial path, constrain target position inside the last polygon.
                    Status cStatus = navquery.GetAttachedNavMesh().ClosestPointOnPoly(reqPath.End, ag.TargetPos, out reqPos, out _);
                    if (cStatus != Status.DT_SUCCESS)
                    {
                        reqPath.Clear();
                    }
                }
                else
                {
                    reqPos = ag.TargetPos;
                }
            }

            if (reqPath.Count <= 0)
            {
                // Could not find path, start the request from current location.
                reqPos = ag.NPos;
                reqPath.StartPath(path[0]);
            }

            ag.Corridor.SetCorridor(reqPos, reqPath);
            ag.Boundary.Reset();
            ag.Partial = false;

            if (reqPath.End == ag.TargetRef)
            {
                ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_VALID;
                ag.TargetReplanTime = 0;
            }
            else
            {
                // The path is longer or potentially unreachable, full plan.
                ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE;
            }
        }
        /// <summary>
        /// Processes the path results over all agents
        /// </summary>
        /// <param name="allAgents">Agent list</param>
        private void ProcessPathResults(CrowdAgent[] allAgents)
        {
            // Process path results.
            foreach (var ag in allAgents)
            {
                if (!ag.Active ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if (ag.TargetState != MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                {
                    continue;
                }

                Status rStatus = pathQueue.GetRequestStatus(ag.TargetPathqRef);
                if (rStatus != Status.DT_SUCCESS)
                {
                    // Path find failed, retry if the target location is still valid.
                    ag.TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
                    ag.TargetState = ag.TargetRef > 0 ? MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING : MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                    ag.TargetReplanTime = 0;

                    continue;
                }

                ProcessPathResults(ag);
            }
        }
        /// <summary>
        /// Processes the path results for the specified agent
        /// </summary>
        /// <param name="ag">Agent</param>
        private void ProcessPathResults(CrowdAgent ag)
        {
            var path = ag.Corridor.GetPath();
            if (path.Length == 0)
            {
                Logger.WriteWarning(this, $"Crowd.UpdateMoveRequest {ag} no path assigned;");
            }

            // Apply results.
            var targetPos = ag.TargetPos;

            Status prStatus = pathQueue.GetPathResult(ag.TargetPathqRef, maxPathResult, out SimplePath res);
            if (prStatus != Status.DT_SUCCESS || res.Count <= 0)
            {
                ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                ag.TargetReplanTime = 0;

                return;
            }

            ag.Partial = prStatus.HasFlag(Status.DT_PARTIAL_RESULT);

            // Merge result and existing path.
            // The agent might have moved whilst the request is
            // being processed, so the path may have changed.
            // We assume that the end of the path is at the same location
            // where the request was issued.

            // The last ref in the old path should be the same as
            // the location where the request was issued..
            if (path[^1] != res.Start)
            {
                ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                ag.TargetReplanTime = 0;

                return;
            }

            // Put the old path infront of the old path.
            if (path.Length > 1)
            {
                res.Merge(path, path.Length);
            }

            // Check for partial path.
            if (res.End != ag.TargetRef)
            {
                // Partial path, constrain target position inside the last polygon.
                Status cStatus = navquery.GetAttachedNavMesh().ClosestPointOnPoly(res.End, targetPos, out Vector3 nearest, out _);
                if (cStatus == Status.DT_SUCCESS)
                {
                    targetPos = nearest;
                }
                else
                {
                    ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                    ag.TargetReplanTime = 0;

                    return;
                }
            }

            // Set current corridor.
            ag.Corridor.SetCorridor(targetPos, res);

            // Force to update boundary.
            ag.Boundary.Reset();
            ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_VALID;
            ag.TargetReplanTime = 0;
        }
        /// <summary>
        /// Updates topology optimization for the specified agents
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        /// <param name="dt">Elapsed seconds</param>
        private void UpdateTopologyOptimization(CrowdAgent[] walkingAgents, float dt)
        {
            const float OPT_TIME_THR = 0.5f; // seconds

            topoQueue.Clear();

            foreach (var ag in walkingAgents)
            {
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if (!ag.Settings.OptimizeTopology)
                {
                    continue;
                }

                ag.TopologyOptTime += dt;
                if (ag.TopologyOptTime >= OPT_TIME_THR)
                {
                    topoQueue.Add(ag);
                }
            }

            if (topoQueue.Count > 1)
            {
                topoQueue.Sort((a1, a2) => -a1.TopologyOptTime.CompareTo(a2.TopologyOptTime));
            }

            foreach (var ag in topoQueue)
            {
                ag.Corridor.OptimizePathTopology(navquery, filters[ag.Settings.QueryFilterTypeIndex]);
                ag.TopologyOptTime = 0;
            }
        }
        /// <summary>
        /// Registers the agents to the proximity grid
        /// </summary>
        /// <param name="agents">Agent list</param>
        private void GridRegisterAgents(CrowdAgent[] agents)
        {
            grid.Clear();

            foreach (var ag in agents)
            {
                grid.AddItem(ag, ag.NPos, ag.Settings.Radius);
            }
        }
        /// <summary>
        /// Finds the colliders for the specified agents
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        /// <remarks>
        /// Updates each agent's collision boundary and neighbour agents, using the proximity grid data.
        /// </remarks>
        private void FindColliders(CrowdAgent[] walkingAgents)
        {
            foreach (var ag in walkingAgents)
            {
                // Update the collision boundary after certain distance has been passed or
                // if it has become invalid.
                float updateThr = ag.Settings.CollisionQueryRange * 0.25f;
                float distSqr = Utils.DistanceSqr2D(ag.NPos, ag.Boundary.GetCenter());
                if (distSqr > updateThr * updateThr || !navquery.IsValid(ag.Boundary, filters[ag.Settings.QueryFilterTypeIndex]))
                {
                    ag.Boundary.Update(
                        ag.Corridor.GetFirstPoly(),
                        ag.NPos,
                        ag.Settings.CollisionQueryRange,
                        navquery,
                        filters[ag.Settings.QueryFilterTypeIndex]);
                }

                // Query neighbour agents
                ag.UpdateNeighbours(grid);
            }
        }
        /// <summary>
        /// Finds the next path corner for the specified agents
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        /// <param name="debug">Crowd debug information</param>
        private void FindNextCorner(AgentInfo[] walkingAgents)
        {
            foreach (var (ag, debug) in walkingAgents.Select(a => (a.Agent, a.DebugInfo)))
            {
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                var filter = filters[ag.Settings.QueryFilterTypeIndex];

                ag.FindNextCorner(navquery, filter, debug);
            }
        }
        /// <summary>
        /// Triggers the off-mesh connections for the specified agents
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        private void TriggerOffMeshConnections(CrowdAgent[] walkingAgents)
        {
            foreach (var ag in walkingAgents)
            {
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Check 
                float triggerRadius = ag.Settings.Radius * 2.25f;
                if (!ag.OverOffmeshConnection(triggerRadius))
                {
                    continue;
                }

                // Prepare to off-mesh connection.
                var anim = crowdAgentAnims[ag];

                // Adjust the path over the off-mesh connection.
                int[] refs = new int[2];
                if (ag.Corridor.MoveOverOffmeshConnection(
                    navquery,
                    ag.Corners.EndRef,
                    refs,
                    out var startPos,
                    out var endPos))
                {
                    anim.InitPos = ag.NPos;
                    anim.PolyRef = refs[1];
                    anim.Active = true;
                    anim.T = 0.0f;
                    anim.TMax = Utils.Distance2D(anim.StartPos, anim.EndPos) / ag.Settings.MaxSpeed * 0.5f;
                    anim.StartPos = startPos;
                    anim.EndPos = endPos;

                    ag.SetOverOffmeshConnection();
                }

                // Path validity check will ensure that bad/blocked connections will be replanned.
            }
        }
        /// <summary>
        /// Calculates the steering of the specified agent list
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        private static void CalculateSteering(CrowdAgent[] walkingAgents)
        {
            foreach (var ag in walkingAgents)
            {
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                {
                    continue;
                }

                ag.CalculateSteering();
            }
        }
        /// <summary>
        /// Process the obstacles avoidance for the specified agent list
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        /// <param name="debug">Crowd debug information</param>
        private void VelocityPlanning(AgentInfo[] walkingAgents)
        {
            foreach (var (ag, debug) in walkingAgents.Select(a => (a.Agent, a.DebugInfo)))
            {
                if (!ag.Settings.ObstacleAvoidance)
                {
                    // If not using velocity planning, new velocity is directly the desired velocity.
                    ag.NVel = ag.DVel;

                    continue;
                }

                ObstacleAvoidance(ag, debug);
            }
        }
        /// <summary>
        /// Process the obstacle avoidance for the specified agent
        /// </summary>
        /// <param name="ag">Agent</param>
        /// <param name="d">Agent debug information</param>
        private void ObstacleAvoidance(CrowdAgent ag, CrowdAgentDebugInfo d)
        {
            obstacleQuery.Reset();

            // Add neighbours as obstacles.
            var crowdNeiAgents = ag
                .GetNeighbours()
                .Select(crowdNei => crowdNei.Agent)
                .ToArray();

            foreach (var nei in crowdNeiAgents)
            {
                obstacleQuery.AddCircle(nei.NPos, nei.Settings.Radius, nei.Vel, nei.DVel);
            }

            // Append neighbour segments as obstacles.
            foreach (var s in ag.Boundary.GetSegments())
            {
                if (Utils.TriArea2D(ag.NPos, s.S1, s.S2) < 0.0f)
                {
                    continue;
                }
                obstacleQuery.AddSegment(s.S1, s.S2);
            }

            // Sample new safe velocity.
            var req = new ObstacleAvoidanceSampleRequest
            {
                Pos = ag.NPos,
                Rad = ag.Settings.Radius,
                MaxSpeed = ag.DesiredSpeed,
                Vel = ag.Vel,
                DVel = ag.DVel,
                Param = obstacleQueryParams[ag.Settings.AvoidanceQuality],
                Debug = d.Vod,
            };

            Vector3 nvel;
            if (sampleVelocityAdaptative)
            {
                obstacleQuery.SampleVelocityAdaptive(req, out nvel);
            }
            else
            {
                obstacleQuery.SampleVelocityGrid(req, out nvel);
            }
            ag.NVel = nvel;
        }
        /// <summary>
        /// Integrates the specified agent list
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        /// <param name="dt">Elapsed seconds</param>
        private static void IntegrateAgents(CrowdAgent[] walkingAgents, float dt)
        {
            foreach (var ag in walkingAgents)
            {
                ag.Integrate(dt);
            }
        }
        /// <summary>
        /// Handles the collisions for the specified agent list
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        private void HandleCollisions(CrowdAgent[] walkingAgents)
        {
            for (int iter = 0; iter < collisionResolveIterations; iter++)
            {
                foreach (var ag in walkingAgents)
                {
                    HandleCollisions(ag);
                }

                foreach (var ag in walkingAgents)
                {
                    ag.NPos += ag.Disp;
                }
            }
        }
        /// <summary>
        /// Handles the collisions for the specified agent
        /// </summary>
        /// <param name="ag">Agent</param>
        private void HandleCollisions(CrowdAgent ag)
        {
            const float THRESHOLD = 0.0001f;

            ag.Disp = Vector3.Zero;

            float w = 0;

            var crowdNeiAgents = ag
                .GetNeighbours()
                .Select(crowdNei => crowdNei.Agent)
                .ToArray();

            foreach (var nei in crowdNeiAgents)
            {
                var diff = ag.NPos - nei.NPos;
                diff.Y = 0;

                float dist = diff.LengthSquared();
                float diffRad = ag.Settings.Radius + nei.Settings.Radius;
                if (dist > diffRad * diffRad)
                {
                    continue;
                }

                dist = MathF.Sqrt(dist);
                float pen = diffRad - dist;
                if (dist < THRESHOLD)
                {
                    // Agents on top of each other, try to choose diverging separation directions.
                    int agIdx = crowdAgents.FindIndex(a => a.Agent == ag);
                    int niIdx = crowdAgents.FindIndex(a => a.Agent == nei);

                    if (agIdx > niIdx)
                    {
                        diff = new Vector3(-ag.DVel.Z, 0, ag.DVel.X);
                    }
                    else
                    {
                        diff = new Vector3(ag.DVel.Z, 0, -ag.DVel.X);
                    }
                    pen = 0.01f;
                }
                else
                {
                    pen = (1.0f / dist) * (pen * 0.5f) * collisionResolveFactor;
                }

                ag.Disp += diff * pen;

                w += 1.0f;
            }

            if (w > THRESHOLD)
            {
                float iw = 1.0f / w;
                ag.Disp *= iw;
            }
        }
        /// <summary>
        /// Moves the specified agent list over the navigation mesh
        /// </summary>
        /// <param name="walkingAgents">Agent list</param>
        private void MoveAgents(CrowdAgent[] walkingAgents)
        {
            foreach (var ag in walkingAgents)
            {
                // Move along navmesh.
                ag.Corridor.MovePosition(navquery, filters[ag.Settings.QueryFilterTypeIndex], ag.NPos);
                // Get valid constrained position back.
                ag.NPos = ag.Corridor.GetPos();

                // If not using path, truncate the corridor to just one poly.
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    ag.Corridor.Reset(ag.Corridor.GetFirstPoly(), ag.NPos);
                    ag.Partial = false;
                }
            }
        }
        /// <summary>
        /// Animates the agents over the off-mesh connection
        /// </summary>
        /// <param name="dt">Elapsed seconds</param>
        private void AnimateAgentsOverOffMeshConnection(float dt)
        {
            var activeAnims = crowdAgentAnims.Where(a => a.Value.Active);

            foreach (var agentAnim in activeAnims)
            {
                var anim = agentAnim.Value;
                if (!anim.Active)
                {
                    continue;
                }

                var ag = agentAnim.Key;

                anim.T += dt;
                if (anim.T > anim.TMax)
                {
                    // Reset animation
                    anim.Active = false;

                    // Prepare agent for walking.
                    ag.State = CrowdAgentState.DT_CROWDAGENT_STATE_WALKING;

                    continue;
                }

                // Update position
                float ta = anim.TMax * 0.15f;
                float tb = anim.TMax;
                if (anim.T < ta)
                {
                    float u = Utils.Tween(anim.T, 0f, ta);
                    ag.NPos = Vector3.Lerp(anim.InitPos, anim.StartPos, u);
                }
                else
                {
                    float u = Utils.Tween(anim.T, ta, tb);
                    ag.NPos = Vector3.Lerp(anim.StartPos, anim.EndPos, u);
                }

                // Update velocity.
                ag.Vel = Vector3.Zero;
                ag.DVel = Vector3.Zero;
            }
        }
    }
}
