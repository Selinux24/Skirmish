using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Provides local steering behaviors for a group of agents. 
    /// </summary>
    public class Crowd
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
        /// Navigation query
        /// </summary>
        private readonly NavMeshQuery m_navquery = null;
        /// <summary>
        /// Agent list
        /// </summary>
        private readonly List<CrowdAgent> m_agents = [];
        /// <summary>
        /// Agent animation dictionary
        /// </summary>
        private readonly Dictionary<CrowdAgent, CrowdAgentAnimation> m_agentAnims = [];
        /// <summary>
        /// Filter list
        /// </summary>
        private readonly List<IGraphQueryFilter> m_filters = [];
        /// <summary>
        /// Obstacle query list
        /// </summary>
        private readonly List<ObstacleAvoidanceParams> m_obstacleQueryParams = [];
        /// <summary>
        /// Path queue
        /// </summary>
        private readonly PathQueue m_pathq = null;
        /// <summary>
        /// Obstacle avoidance query
        /// </summary>
        private readonly ObstacleAvoidanceQuery m_obstacleQuery = null;
        /// <summary>
        /// Proximity grid
        /// </summary>
        private readonly ProximityGrid<CrowdAgent> m_grid = null;
        /// <summary>
        /// Maximum path result count
        /// </summary>
        private readonly int m_maxPathResult;
        /// <summary>
        /// Sample velocity adaptative
        /// </summary>
        private readonly bool m_sampleVelocityAdaptative;
        /// <summary>
        /// Collision resolve iteration count
        /// </summary>
        private readonly int m_collisionResolveIterations;
        /// <summary>
        /// Collision resolve factor
        /// </summary>
        private readonly float m_collisionResolveFactor;
        /// <summary>
        /// Agent placement extents
        /// </summary>
        private readonly Vector3 m_agentPlacementHalfExtents;
        /// <summary>
        /// Velocity sample count
        /// </summary>
        private int m_velocitySampleCount = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nav">The navigation mesh to use for planning.</param>
        /// <param name="settings">Settings</param>
        public Crowd(NavMesh nav, CrowdParameters settings)
        {
            m_maxPathResult = settings.MaxPathResult;
            m_sampleVelocityAdaptative = settings.SampleVelocityAdaptative;
            m_collisionResolveIterations = settings.CollisionResolveIterations;
            m_collisionResolveFactor = settings.CollisionResolveFactor;

            // Larger than agent radius because it is also used for agent recovery.
            m_agentPlacementHalfExtents = new Vector3(settings.MaxAgentRadius * 2.0f, settings.MaxAgentRadius * 1.5f, settings.MaxAgentRadius * 2.0f);

            m_grid = new(1000, settings.MaxAgentRadius * 3);

            m_obstacleQuery = new(6, 8);

            // Init filters
            for (int i = 0; i < DT_CROWD_MAX_QUERY_FILTER_TYPE; i++)
            {
                m_filters.Add(settings.Agent.PathFilter);
            }

            // Init obstacle query params.
            for (int i = 0; i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS; i++)
            {
                m_obstacleQueryParams.Add(new()
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
            m_pathq = new(nav, m_maxPathResult, MAX_PATHQUEUE_NODES);

            // The navquery is mostly used for local searches, no need for large node pool.
            m_navquery = new(nav, MAX_COMMON_NODES);
        }

        /// <summary>
        /// Calculates the steering of the specified agent list
        /// </summary>
        /// <param name="walkingAgents">Walking agents</param>
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
        /// Integrates the specified agent list
        /// </summary>
        /// <param name="walkingAgents">Walking agents</param>
        /// <param name="dt">Time</param>
        private static void IntegrateAgents(CrowdAgent[] walkingAgents, float dt)
        {
            foreach (var ag in walkingAgents)
            {
                ag.Integrate(dt);
            }
        }

        /// <summary>
        /// Adds a new agent to the crowd.
        /// </summary>
        /// <param name="pos">The requested position of the agent.</param>
        /// <param name="param">The configutation of the agent.</param>
        /// <returns>The new agent.</returns>
        public CrowdAgent AddAgent(Vector3 pos, CrowdAgentParameters param)
        {
            // Find nearest position on navmesh and place the agent there.
            Status status = m_navquery.FindNearestPoly(
                pos, m_agentPlacementHalfExtents, m_filters[param.QueryFilterTypeIndex],
                out int r, out Vector3 nearest);
            if (status != Status.DT_SUCCESS)
            {
                nearest = pos;
                r = 0;
            }

            CrowdAgentState state;
            if (r > 0)
            {
                state = CrowdAgentState.DT_CROWDAGENT_STATE_WALKING;
            }
            else
            {
                state = CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
            }

            var ag = new CrowdAgent()
            {
                Params = param,
                Partial = false,
                TopologyOptTime = 0,
                TargetReplanTime = 0,
                DVel = Vector3.Zero,
                NVel = Vector3.Zero,
                Vel = Vector3.Zero,
                NPos = nearest,
                DesiredSpeed = 0,
                State = state,
                TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE,
                Active = true,
            };

            ag.Corridor.Init(m_maxPathResult);
            ag.Corridor.Reset(r, nearest);
            ag.Boundary.Reset();
            ag.ClearNeighbours();

            m_agents.Add(ag);

            return ag;
        }
        /// <summary>
        /// Removes the agent from the crowd.
        /// </summary>
        /// <param name="ag">Agent to remove</param>
        public void RemoveAgent(CrowdAgent ag)
        {
            if (ag == null)
            {
                return;
            }

            m_agents.Remove(ag);
        }
        /// <summary>
        /// Gets the agents int the agent pool.
        /// </summary>
        /// <returns>The collection of agents.</returns>
        public CrowdAgent[] GetAgents()
        {
            return [.. m_agents];
        }
        /// <summary>
        /// Gets the active agents int the agent pool.
        /// </summary>
        /// <returns>The collection of active agents.</returns>
        public CrowdAgent[] GetActiveAgents()
        {
            return m_agents.Where(a => a.Active).ToArray();
        }

        /// <summary>
        /// Updates the steering and positions of all agents.
        /// </summary>
        /// <param name="dt">The time, in seconds, to update the simulation. [Limit: > 0]</param>
        /// <param name="debug">A debug object to load with debug information. [Opt]</param>
        public void Update(float dt, IEnumerable<CrowdAgentDebugInfo> debug)
        {
            m_velocitySampleCount = 0;

            var activeAgents = GetActiveAgents();
            if (activeAgents.Length == 0)
            {
                return;
            }

            var walkingAgents = activeAgents
                .Where(a => a.State == CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                .ToArray();

            // Check that all agents still have valid paths.
            CheckPathValidity(walkingAgents, dt);

            // Update async move request and path finder.
            UpdateMoveRequest();

            // Optimize path topology.
            UpdateTopologyOptimization(walkingAgents, dt);

            // Register agents to proximity grid.
            GridRegisterAgents(activeAgents);

            // Get nearby navmesh segments and agents to collide with.
            FindColliders(walkingAgents);

            // Find next corner to steer to.
            FindNextCorner(walkingAgents, debug);

            // Trigger off-mesh connections (depends on corners).
            TriggerOffMeshConnections(walkingAgents);

            // Calculate steering.
            CalculateSteering(walkingAgents);

            // Velocity planning.	
            VelocityPlanning(walkingAgents, debug);

            // Integrate.
            IntegrateAgents(walkingAgents, dt);

            // Handle collisions.
            for (int iter = 0; iter < m_collisionResolveIterations; iter++)
            {
                HandleCollisions(walkingAgents);
            }

            MoveAgents(walkingAgents);

            // Update agents using off-mesh connection.
            AnimateAgentsOverOffMeshConnection(dt);
        }

        private void CheckPathValidity(CrowdAgent[] walkingAgents, float dt)
        {
            foreach (var ag in walkingAgents)
            {
                bool replan = CheckPathValidity(ag, dt);

                // Try to replan path to goal.
                if (replan && ag.TargetState != MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                {
                    bool requested = ag.RequestMoveTargetReplan(ag.TargetRef, ag.TargetPos);
                    if (!requested)
                    {
                        Logger.WriteError(this, $"RequestMoveTargetReplan error: {m_agents.IndexOf(ag)} {ag.TargetRef} {ag.TargetPos}");
                    }
                }
            }
        }
        private bool CheckPathValidity(CrowdAgent ag, float dt)
        {
            int CHECK_LOOKAHEAD = 10;
            float TARGET_REPLAN_DELAY = 1; // seconds

            ag.TargetReplanTime += dt;

            bool replan = false;

            // First check that the current location is valid.
            Vector3 agentPos = ag.NPos;
            int agentRef = ag.Corridor.GetFirstPoly();
            if (!m_navquery.IsValidPolyRef(agentRef, m_filters[ag.Params.QueryFilterTypeIndex]))
            {
                // Current location is not valid, try to reposition.
                // FIX: this can snap agents, how to handle that?
                m_navquery.FindNearestPoly(
                    ag.NPos, m_agentPlacementHalfExtents, m_filters[ag.Params.QueryFilterTypeIndex],
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
                if (!m_navquery.IsValidPolyRef(ag.TargetRef, m_filters[ag.Params.QueryFilterTypeIndex]))
                {
                    // Current target is not valid, try to reposition.
                    m_navquery.FindNearestPoly(
                        ag.TargetPos, m_agentPlacementHalfExtents, m_filters[ag.Params.QueryFilterTypeIndex],
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
            if (!ag.Corridor.IsValid(m_navquery, m_filters[ag.Params.QueryFilterTypeIndex], CHECK_LOOKAHEAD))
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
        private void UpdateMoveRequest()
        {
            // Fire off new requests.
            var queue = FireNewRequests();

            foreach (var ag in queue)
            {
                ag.TargetPathqRef = m_pathq.Request(
                    ag.Corridor.GetLastPoly(),
                    ag.TargetRef,
                    ag.Corridor.GetTarget(),
                    ag.TargetPos,
                    m_filters[ag.Params.QueryFilterTypeIndex]);

                if (ag.TargetPathqRef != PathQueue.DT_PATHQ_INVALID)
                {
                    ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH;
                }
            }

            // Update requests.
            m_pathq.Update(MAX_ITERS_PER_UPDATE);

            // Process path results.
            ProcessPathResults();
        }
        private CrowdAgent[] FireNewRequests()
        {
            var queue = new List<CrowdAgent>();

            foreach (var ag in m_agents)
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
                    FireNewRequest(ag);
                }

                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                {
                    queue.Add(ag);
                }
            }

            if (queue.Count > 1)
            {
                // Sort neighbours based on greatest time.
                queue.Sort((a1, a2) => -a1.TargetReplanTime.CompareTo(a2.TargetReplanTime));
            }

            return [.. queue];
        }
        private void FireNewRequest(CrowdAgent ag)
        {
            var path = ag.Corridor.GetPath();
            if (path.Length == 0)
            {
                Logger.WriteWarning(this, $"Crowd.UpdateMoveRequest {ag} no path assigned;");
            }

            const int MAX_RES = 32;
            Vector3 reqPos = Vector3.Zero;
            SimplePath reqPath;

            // Quick search towards the goal.
            PathPoint start = new() { Ref = path[0], Pos = ag.NPos };
            PathPoint end = new() { Ref = ag.TargetRef, Pos = ag.TargetPos };
            m_navquery.InitSlicedFindPath(m_filters[ag.Params.QueryFilterTypeIndex], start, end);
            const int MAX_ITER = 20;
            m_navquery.UpdateSlicedFindPath(MAX_ITER, out _);

            Status qStatus;
            if (ag.TargetReplan) // && npath > 10)
            {
                // Try to use existing steady path during replan if possible.
                qStatus = m_navquery.FinalizeSlicedFindPathPartial(MAX_RES, path, out reqPath);
            }
            else
            {
                // Try to move towards target when goal changes.
                qStatus = m_navquery.FinalizeSlicedFindPath(MAX_RES, out reqPath);
            }

            if (qStatus != Status.DT_FAILURE && reqPath.Count > 0)
            {
                // In progress or succeed.
                if (reqPath.End != ag.TargetRef)
                {
                    // Partial path, constrain target position inside the last polygon.
                    Status cStatus = m_navquery.GetAttachedNavMesh().ClosestPointOnPoly(reqPath.End, ag.TargetPos, out reqPos, out _);
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

            if (reqPath?.Count <= 0)
            {
                // Could not find path, start the request from current location.
                reqPos = ag.NPos;
                reqPath.StartPath(path[0]);
            }

            ag.Corridor.SetCorridor(reqPos, reqPath);
            ag.Boundary.Reset();
            ag.Partial = false;

            if (reqPath?.End == ag.TargetRef)
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
        private void ProcessPathResults()
        {
            // Process path results.
            foreach (var ag in m_agents)
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

                Status rStatus = m_pathq.GetRequestStatus(ag.TargetPathqRef);
                if (rStatus != Status.DT_SUCCESS)
                {
                    // Path find failed, retry if the target location is still valid.
                    ag.TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
                    ag.TargetState = ag.TargetRef > 0 ? MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING : MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                    ag.TargetReplanTime = 0;
                }
                else
                {
                    ProcessPathResults(ag);
                }
            }
        }
        private void ProcessPathResults(CrowdAgent ag)
        {
            var path = ag.Corridor.GetPath();
            if (path.Length == 0)
            {
                Logger.WriteWarning(this, $"Crowd.UpdateMoveRequest {ag} no path assigned;");
            }

            // Apply results.
            var targetPos = ag.TargetPos;

            Status prStatus = m_pathq.GetPathResult(ag.TargetPathqRef, m_maxPathResult, out SimplePath res);
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
                Status cStatus = m_navquery.GetAttachedNavMesh().ClosestPointOnPoly(res.End, targetPos, out Vector3 nearest, out _);
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
        private void UpdateTopologyOptimization(CrowdAgent[] walkingAgents, float dt)
        {
            const float OPT_TIME_THR = 0.5f; // seconds

            var queue = new List<CrowdAgent>();

            foreach (var ag in walkingAgents)
            {
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }
                if (!ag.Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_OPTIMIZE_TOPO))
                {
                    continue;
                }

                ag.TopologyOptTime += dt;
                if (ag.TopologyOptTime >= OPT_TIME_THR)
                {
                    queue.Add(ag);
                }
            }

            if (queue.Count > 1)
            {
                queue.Sort((a1, a2) => -a1.TopologyOptTime.CompareTo(a2.TopologyOptTime));
            }

            foreach (var ag in queue)
            {
                ag.Corridor.OptimizePathTopology(m_navquery, m_filters[ag.Params.QueryFilterTypeIndex]);
                ag.TopologyOptTime = 0;
            }
        }
        private void GridRegisterAgents(CrowdAgent[] agents)
        {
            m_grid.Clear();

            foreach (var ag in agents)
            {
                m_grid.AddItem(ag, ag.NPos, ag.Params.Radius);
            }
        }
        private void FindColliders(CrowdAgent[] walkingAgents)
        {
            foreach (var ag in walkingAgents)
            {
                // Update the collision boundary after certain distance has been passed or
                // if it has become invalid.
                float updateThr = ag.Params.CollisionQueryRange * 0.25f;
                float distSqr = Utils.DistanceSqr2D(ag.NPos, ag.Boundary.GetCenter());
                if (distSqr > updateThr * updateThr || !m_navquery.IsValid(ag.Boundary, m_filters[ag.Params.QueryFilterTypeIndex]))
                {
                    ag.Boundary.Update(
                        ag.Corridor.GetFirstPoly(),
                        ag.NPos,
                        ag.Params.CollisionQueryRange,
                        m_navquery,
                        m_filters[ag.Params.QueryFilterTypeIndex]);
                }

                // Query neighbour agents
                ag.UpdateNeighbours(m_grid);
            }
        }
        private void FindNextCorner(CrowdAgent[] walkingAgents, IEnumerable<CrowdAgentDebugInfo> debug)
        {
            foreach (var ag in walkingAgents)
            {
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                var filter = m_filters[ag.Params.QueryFilterTypeIndex];

                ag.FindNextCorner(m_navquery, filter, debug?.FirstOrDefault(a => a.Agent == ag));
            }
        }
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
                float triggerRadius = ag.Params.Radius * 2.25f;
                if (!ag.OverOffmeshConnection(triggerRadius))
                {
                    continue;
                }

                // Prepare to off-mesh connection.
                var anim = m_agentAnims[ag];

                // Adjust the path over the off-mesh connection.
                int[] refs = new int[2];
                if (ag.Corridor.MoveOverOffmeshConnection(
                    m_navquery,
                    ag.Corners.EndRef,
                    refs,
                    out var startPos,
                    out var endPos))
                {
                    anim.InitPos = ag.NPos;
                    anim.PolyRef = refs[1];
                    anim.Active = true;
                    anim.T = 0.0f;
                    anim.TMax = Utils.Distance2D(anim.StartPos, anim.EndPos) / ag.Params.MaxSpeed * 0.5f;
                    anim.StartPos = startPos;
                    anim.EndPos = endPos;

                    ag.State = CrowdAgentState.DT_CROWDAGENT_STATE_OFFMESH;
                    ag.Corners.Clear();
                    ag.ClearNeighbours();
                }
                else
                {
                    // Path validity check will ensure that bad/blocked connections will be replanned.
                }
            }
        }
        private void VelocityPlanning(CrowdAgent[] walkingAgents, IEnumerable<CrowdAgentDebugInfo> debug)
        {
            foreach (var ag in walkingAgents)
            {
                if (!ag.Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_OBSTACLE_AVOIDANCE))
                {
                    // If not using velocity planning, new velocity is directly the desired velocity.
                    ag.NVel = ag.DVel;

                    continue;
                }

                ObstacleAvoidance(ag, debug?.FirstOrDefault(a => a.Agent == ag));
            }
        }
        private void ObstacleAvoidance(CrowdAgent ag, CrowdAgentDebugInfo d)
        {
            m_obstacleQuery.Reset();

            // Add neighbours as obstacles.
            var crowdNeiAgents = ag
                .GetNeighbours()
                .Select(crowdNei => crowdNei.Agent)
                .ToArray();

            foreach (var nei in crowdNeiAgents)
            {
                m_obstacleQuery.AddCircle(nei.NPos, nei.Params.Radius, nei.Vel, nei.DVel);
            }

            // Append neighbour segments as obstacles.
            foreach (var s in ag.Boundary.GetSegments())
            {
                if (Utils.TriArea2D(ag.NPos, s.S1, s.S2) < 0.0f)
                {
                    continue;
                }
                m_obstacleQuery.AddSegment(s.S1, s.S2);
            }

            // Sample new safe velocity.
            int ns;

            var req = new ObstacleAvoidanceSampleRequest
            {
                Pos = ag.NPos,
                Rad = ag.Params.Radius,
                VMax = ag.DesiredSpeed,
                Vel = ag.Vel,
                DVel = ag.DVel,
                Param = m_obstacleQueryParams[ag.Params.ObstacleAvoidanceType],
                Debug = d?.Vod,
            };

            if (m_sampleVelocityAdaptative)
            {
                ns = m_obstacleQuery.SampleVelocityAdaptive(req, out Vector3 nvel);

                ag.NVel = nvel;
            }
            else
            {
                ns = m_obstacleQuery.SampleVelocityGrid(req, out Vector3 nvel);

                ag.NVel = nvel;
            }

            m_velocitySampleCount += ns;
        }
        private void HandleCollisions(CrowdAgent[] walkingAgents)
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
        private void HandleCollisions(CrowdAgent ag)
        {
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
                float diffRad = ag.Params.Radius + nei.Params.Radius;
                if (dist > diffRad * diffRad)
                {
                    continue;
                }

                dist = MathF.Sqrt(dist);
                float pen = diffRad - dist;
                if (dist < 0.0001f)
                {
                    // Agents on top of each other, try to choose diverging separation directions.
                    if (m_agents.IndexOf(ag) > m_agents.IndexOf(nei))
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
                    pen = (1.0f / dist) * (pen * 0.5f) * m_collisionResolveFactor;
                }

                ag.Disp += diff * pen;

                w += 1.0f;
            }

            if (w > 0.0001f)
            {
                float iw = 1.0f / w;
                ag.Disp *= iw;
            }
        }
        private void MoveAgents(CrowdAgent[] walkingAgents)
        {
            foreach (var ag in walkingAgents)
            {
                // Move along navmesh.
                ag.Corridor.MovePosition(m_navquery, m_filters[ag.Params.QueryFilterTypeIndex], ag.NPos);
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
        private void AnimateAgentsOverOffMeshConnection(float dt)
        {
            var activeAnims = m_agentAnims.Where(a => a.Value.Active);

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

        /// <summary>
        /// Gets the filter used by the crowd.
        /// </summary>
        /// <param name="i">Filter index</param>
        /// <returns>The filter used by the crowd.</returns>
        public IGraphQueryFilter GetFilter(int i)
        {
            return (i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE) ? m_filters[i] : null;
        }
        /// <summary>
        /// Sets the filter for the specified index.
        /// </summary>
        /// <param name="i">The index</param>
        /// <param name="filter">The new filter</param>
        public void SetFilter(int i, IGraphQueryFilter filter)
        {
            if (i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE)
            {
                m_filters[i] = filter;
            }
        }
        /// <summary>
        /// Gets the shared avoidance configuration for the specified index.
        /// </summary>
        /// <param name="i">The index of the configuration to retreive. [Limits:  0 <= value < #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]</param>
        /// <returns>The requested configuration.</returns>
        public ObstacleAvoidanceParams GetObstacleAvoidanceParams(int i)
        {
            if (i >= 0 && i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                return m_obstacleQueryParams[i];
            }

            return null;
        }
        /// <summary>
        /// Sets the shared avoidance configuration for the specified index.
        /// </summary>
        /// <param name="i">The index. [Limits: 0 <= value < #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]</param>
        /// <param name="param">The new configuration.</param>
        public void SetObstacleAvoidanceParams(int i, ObstacleAvoidanceParams param)
        {
            if (i >= 0 && i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                m_obstacleQueryParams[i] = param;
            }
        }
        /// <summary>
        /// Gets the search halfExtents [(x, y, z)] used by the crowd for query operations. 
        /// </summary>
        /// <returns>The search halfExtents used by the crowd. [(x, y, z)]</returns>
        public Vector3 GetQueryHalfExtents()
        {
            return m_agentPlacementHalfExtents;
        }
        /// <summary>
        /// Same as getQueryHalfExtents. Left to maintain backwards compatibility.
        /// </summary>
        /// <returns>The search halfExtents used by the crowd. [(x, y, z)]</returns>
        public Vector3 GetQueryExtents()
        {
            return m_agentPlacementHalfExtents;
        }
        /// <summary>
        /// Gets the velocity sample count.
        /// </summary>
        /// <returns>The velocity sample count.</returns>
        public int GetVelocitySampleCount()
        {
            return m_velocitySampleCount;
        }
        /// <summary>
        /// Gets the crowd's proximity grid.
        /// </summary>
        /// <returns>The crowd's proximity grid.</returns>
        public ProximityGrid<CrowdAgent> GetGrid()
        {
            return m_grid;
        }
        /// <summary>
        /// Gets the crowd's path request queue.
        /// </summary>
        /// <returns>The crowd's path request queue.</returns>
        public PathQueue GetPathQueue()
        {
            return m_pathq;
        }
        /// <summary>
        /// Gets the query object used by the crowd.
        /// </summary>
        /// <returns></returns>
        public NavMeshQuery GetNavMeshQuery()
        {
            return m_navquery;
        }
    }
}
