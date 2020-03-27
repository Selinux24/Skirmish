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
        /// The maximum number of corners a crowd agent will look ahead in the path.
        /// This value is used for sizing the crowd agent corner buffers.
        /// Due to the behavior of the crowd manager, the actual number of useful
        /// corners will be one less than this number.
        /// @ingroup crowd
        public const int DT_CROWDAGENT_MAX_CORNERS = 4;
        /// The maximum number of crowd avoidance configurations supported by the
        /// crowd manager.
        /// @ingroup crowd
        /// @see dtObstacleAvoidanceParams, dtCrowd::setObstacleAvoidanceParams(), dtCrowd::getObstacleAvoidanceParams(),
        ///		 dtCrowdAgentParams::obstacleAvoidanceType
        public const int DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS = 8;
        /// The maximum number of query filter types supported by the crowd manager.
        /// @ingroup crowd
        /// @see dtQueryFilter, dtCrowd::getFilter() dtCrowd::getEditableFilter(),
        ///		dtCrowdAgentParams::queryFilterType
        public const int DT_CROWD_MAX_QUERY_FILTER_TYPE = 16;
        /// <summary>
        /// The maximum number of iterations per update
        /// </summary>
        public const int MAX_ITERS_PER_UPDATE = 100;
        /// <summary>
        /// The maximum number of path queue nodes
        /// </summary>
        public const int MAX_PATHQUEUE_NODES = 4096;
        /// <summary>
        /// The maximum number of navigation mesh query nodes
        /// </summary>
        public const int MAX_COMMON_NODES = 512;


        public static float Tween(float t, float t0, float t1)
        {
            return MathUtil.Clamp((t - t0) / (t1 - t0), 0.0f, 1.0f);
        }

        public static void Integrate(CrowdAgent ag, float dt)
        {
            // Fake dynamic constraint.
            float maxDelta = ag.Params.MaxAcceleration * dt;
            Vector3 dv = ag.NVel - ag.Vel;
            float ds = dv.Length();
            if (ds > maxDelta)
            {
                dv *= maxDelta / ds;
            }
            ag.Vel += dv;

            // Integrate
            if (ag.Vel.Length() > 0.0001f)
            {
                ag.NPos += ag.Vel * dt;
            }
            else
            {
                ag.Vel = Vector3.Zero;
            }
        }

        public static bool OverOffmeshConnection(CrowdAgent ag, float radius)
        {
            if (ag.Corners.Count <= 0)
            {
                return false;
            }

            bool offMeshConnection = ag.Corners.EndFlags.HasFlag(StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION);
            if (offMeshConnection)
            {
                float distSq = Vector2.DistanceSquared(ag.NPos.XZ(), ag.Corners.EndPath.XZ());
                if (distSq < radius * radius)
                {
                    return true;
                }
            }

            return false;
        }

        public static float GetDistanceToGoal(CrowdAgent ag, float range)
        {
            if (ag.Corners.Count <= 0)
            {
                return range;
            }

            bool endOfPath = ag.Corners.EndFlags.HasFlag(StraightPathFlagTypes.DT_STRAIGHTPATH_END);
            if (endOfPath)
            {
                return Math.Min(Vector2.Distance(ag.NPos.XZ(), ag.Corners.EndPath.XZ()), range);
            }

            return range;
        }

        public static void CalcSmoothSteerDirection(CrowdAgent ag, out Vector3 dir)
        {
            dir = Vector3.Zero;

            if (ag.Corners.Count <= 0)
            {
                return;
            }

            int ip0 = 0;
            int ip1 = Math.Min(1, ag.Corners.Count - 1);
            Vector3 p0 = ag.Corners.GetPath(ip0);
            Vector3 p1 = ag.Corners.GetPath(ip1);

            Vector3 dir0 = p0 - ag.NPos;
            Vector3 dir1 = p1 - ag.NPos;
            dir0.Y = 0;
            dir1.Y = 0;

            float len0 = dir0.Length();
            float len1 = dir1.Length();
            if (len1 > 0.001f)
            {
                dir1 *= 1.0f / len1;
            }

            dir.X = dir0.X - dir1.X * len0 * 0.5f;
            dir.Y = 0;
            dir.Z = dir0.Z - dir1.Z * len0 * 0.5f;

            dir.Normalize();
        }

        public static void CalcStraightSteerDirection(CrowdAgent ag, out Vector3 dir)
        {
            dir = Vector3.Zero;

            if (ag.Corners.Count <= 0)
            {
                return;
            }

            dir = ag.Corners.StartPath - ag.NPos;
            dir.Y = 0;

            dir.Normalize();
        }

        public static void GetNeighbours(CrowdAgent agent, ProximityGrid<CrowdAgent> grid)
        {
            Vector3 pos = agent.NPos;
            float height = agent.Params.Height;
            float range = agent.Params.CollisionQueryRange;

            agent.ClearNeighbours();

            var queryAgents = grid.QueryItems(pos, range);

            foreach (var ag in queryAgents)
            {
                if (ag == agent)
                {
                    continue;
                }

                // Check for overlap.
                Vector3 diff = pos - ag.NPos;
                if (Math.Abs(diff.Y) >= (height + ag.Params.Height) / 2.0f)
                {
                    continue;
                }
                diff.Y = 0;
                float distSqr = diff.LengthSquared();
                if (distSqr > range * range)
                {
                    continue;
                }

                // Insert neighbour based on the distance.
                agent.AddNeighbour(ag, distSqr);
            }
        }

        /// <summary>
        /// Navigation query
        /// </summary>
        private readonly NavMeshQuery m_navquery = null;
        /// <summary>
        /// Agent list
        /// </summary>
        private readonly List<CrowdAgent> m_agents = new List<CrowdAgent>();
        /// <summary>
        /// Agent animation dictionary
        /// </summary>
        private readonly Dictionary<CrowdAgent, CrowdAgentAnimation> m_agentAnims = new Dictionary<CrowdAgent, CrowdAgentAnimation>();
        /// <summary>
        /// Filter list
        /// </summary>
        private readonly List<QueryFilter> m_filters = new List<QueryFilter>();
        /// <summary>
        /// Obstacle query list
        /// </summary>
        private readonly List<ObstacleAvoidanceParams> m_obstacleQueryParams = new List<ObstacleAvoidanceParams>();
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

            m_grid = new ProximityGrid<CrowdAgent>(1000, settings.MaxAgentRadius * 3);

            m_obstacleQuery = new ObstacleAvoidanceQuery();
            if (!m_obstacleQuery.Init(6, 8))
            {
                throw new EngineException($"Error initializing the ObstacleAvoidanceQuery");
            }

            // Init filters
            for (int i = 0; i < DT_CROWD_MAX_QUERY_FILTER_TYPE; i++)
            {
                QueryFilter filter = new QueryFilter()
                {
                    IncludeFlags = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK,
                };

                m_filters.Add(filter);
            }

            // Init obstacle query params.
            for (int i = 0; i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS; i++)
            {
                ObstacleAvoidanceParams param = new ObstacleAvoidanceParams
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
                };

                m_obstacleQueryParams.Add(param);
            }

            // Allocate temp buffer for merging paths.
            m_pathq = new PathQueue(nav, m_maxPathResult, MAX_PATHQUEUE_NODES);

            // The navquery is mostly used for local searches, no need for large node pool.
            m_navquery = new NavMeshQuery();
            if (m_navquery.Init(nav, MAX_COMMON_NODES) != Status.DT_SUCCESS)
            {
                throw new EngineException($"Error initializing the NavMeshQuery");
            }
        }

        /// <summary>
        /// Adds a new agent to the crowd.
        /// </summary>
        /// <param name="pos">The requested position of the agent.</param>
        /// <param name="param">The configutation of the agent.</param>
        /// <returns>The new agent.</returns>
        public CrowdAgent AddAgent(Vector3 pos, CrowdAgentParams param)
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

            CrowdAgent ag = new CrowdAgent()
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

            if (m_agents.Contains(ag))
            {
                m_agents.Remove(ag);
            }
        }
        /// <summary>
        /// Submits a new move request for the specified agent.
        /// </summary>
        /// <param name="ag">The agent.</param>
        /// <param name="r">The position's polygon reference.</param>
        /// <param name="pos">The position within the polygon.</param>
        /// <returns>True if the request was successfully submitted.</returns>
        public bool RequestMoveTarget(CrowdAgent ag, int r, Vector3 pos)
        {
            if (ag == null)
            {
                return false;
            }

            if (r == 0)
            {
                return false;
            }

            // Initialize request.
            ag.TargetRef = r;
            ag.TargetPos = pos;
            ag.TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
            ag.TargetReplan = false;
            if (ag.TargetRef > 0)
            {
                ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;
            }
            else
            {
                ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
            }

            return true;
        }
        /// <summary>
        /// Submits a new move request for the specified agent.
        /// </summary>
        /// <param name="ag">The agent.</param>
        /// <param name="vel">The movement velocity. [(x, y, z)]</param>
        /// <returns>True if the request was successfully submitted.</returns>
        public bool RequestMoveVelocity(CrowdAgent ag, Vector3 vel)
        {
            if (ag == null)
            {
                return false;
            }

            // Initialize request.
            ag.TargetRef = 0;
            ag.TargetPos = vel;
            ag.TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
            ag.TargetReplan = false;
            ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY;

            return true;
        }
        /// <summary>
        /// Resets any request for the specified agent.
        /// </summary>
        /// <param name="ag">The agent.</param>
        /// <returns>True if the request was successfully reseted.</returns>
        public bool ResetMoveTarget(CrowdAgent ag)
        {
            if (ag == null)
            {
                return false;
            }

            // Initialize request.
            ag.TargetRef = 0;
            ag.TargetPos = Vector3.Zero;
            ag.DVel = Vector3.Zero;
            ag.TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
            ag.TargetReplan = false;
            ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;

            return true;
        }
        /// <summary>
        /// Gets the agents int the agent pool.
        /// </summary>
        /// <returns>The collection of agents.</returns>
        public IEnumerable<CrowdAgent> GetAgents()
        {
            return m_agents.ToArray();
        }
        /// <summary>
        /// Gets the active agents int the agent pool.
        /// </summary>
        /// <returns>The collection of active agents.</returns>
        public IEnumerable<CrowdAgent> GetActiveAgents()
        {
            var agents = m_agents.Where(a => a.Active);

            return agents.ToArray();
        }

        /// <summary>
        /// Updates the steering and positions of all agents.
        /// </summary>
        /// <param name="dt">The time, in seconds, to update the simulation. [Limit: > 0]</param>
        /// <param name="debug">A debug object to load with debug information. [Opt]</param>
        public void Update(float dt, IEnumerable<CrowdAgentDebugInfo> debug)
        {
            m_velocitySampleCount = 0;

            var agents = GetActiveAgents();
            if (!agents.Any())
            {
                return;
            }

            // Check that all agents still have valid paths.
            CheckPathValidity(agents, dt);

            // Update async move request and path finder.
            UpdateMoveRequest();

            // Optimize path topology.
            UpdateTopologyOptimization(agents, dt);

            // Register agents to proximity grid.
            GridRegisterAgents(agents);

            // Get nearby navmesh segments and agents to collide with.
            FindColliders(agents);

            // Find next corner to steer to.
            FindNextCorner(agents, debug);

            // Trigger off-mesh connections (depends on corners).
            TriggerOffMeshConnections(agents);

            // Calculate steering.
            CalculateSteering(agents);

            // Velocity planning.	
            VelocityPlanning(agents, debug);

            // Integrate.
            IntegrateAgents(agents, dt);

            // Handle collisions.
            for (int iter = 0; iter < m_collisionResolveIterations; iter++)
            {
                HandleCollisions(agents);
            }

            MoveAgents(agents);

            // Update agents using off-mesh connection.
            AnimateAgentsOverOffMeshConnection(dt);
        }

        private void CheckPathValidity(IEnumerable<CrowdAgent> agents, float dt)
        {
            int CHECK_LOOKAHEAD = 10;
            float TARGET_REPLAN_DELAY = 1; // seconds

            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                ag.TargetReplanTime += dt;

                bool replan = false;

                // First check that the current location is valid.
                Vector3 agentPos = ag.NPos;
                int agentRef = ag.Corridor.GetFirstPoly();
                if (!m_navquery.IsValidPolyRef(agentRef, m_filters[ag.Params.QueryFilterTypeIndex]))
                {
                    // Current location is not valid, try to reposition.
                    // TODO: this can snap agents, how to handle that?
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
                        continue;
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
                    continue;
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
                if (!ag.Corridor.IsValid(CHECK_LOOKAHEAD, m_navquery, m_filters[ag.Params.QueryFilterTypeIndex]))
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

                // Try to replan path to goal.
                if (replan && ag.TargetState != MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                {
                    bool requested = RequestMoveTargetReplan(ag, ag.TargetRef, ag.TargetPos);
                    if (!requested)
                    {
                        Console.WriteLine($"RequestMoveTargetReplan error: {m_agents.IndexOf(ag)} {ag.TargetRef} {ag.TargetPos}");
                    }
                }
            }
        }
        private bool RequestMoveTargetReplan(CrowdAgent ag, int r, Vector3 pos)
        {
            if (ag == null)
            {
                return false;
            }

            // Initialize request.
            ag.TargetRef = r;
            ag.TargetPos = pos;
            ag.TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
            ag.TargetReplan = true;
            if (ag.TargetRef > 0)
            {
                ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;
            }
            else
            {
                ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
            }

            return true;
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
        private IEnumerable<CrowdAgent> FireNewRequests()
        {
            List<CrowdAgent> queue = new List<CrowdAgent>();

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

            return queue;
        }
        private void FireNewRequest(CrowdAgent ag)
        {
            var path = ag.Corridor.GetPath();
            if (!path.Any())
            {
                Console.WriteLine($"Crowd.UpdateMoveRequest {ag} no path assigned;");
            }

            const int MAX_RES = 32;
            Vector3 reqPos = Vector3.Zero;
            SimplePath reqPath;

            // Quick search towards the goal.
            const int MAX_ITER = 20;
            m_navquery.InitSlicedFindPath(path.First(), ag.TargetRef, ag.NPos, ag.TargetPos, m_filters[ag.Params.QueryFilterTypeIndex], FindPathOptions.DT_FINDPATH_ANY_ANGLE);
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
                    Status cStatus = m_navquery.ClosestPointOnPoly(reqPath.End, ag.TargetPos, out reqPos);
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
                reqPath.StartPath(path.First());
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
                if (!ag.Active)
                {
                    continue;
                }

                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                {
                    // Poll path queue.
                    Status rStatus = m_pathq.GetRequestStatus(ag.TargetPathqRef);
                    if (rStatus != Status.DT_SUCCESS)
                    {
                        // Path find failed, retry if the target location is still valid.
                        ag.TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
                        if (ag.TargetRef > 0)
                        {
                            ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;
                        }
                        else
                        {
                            ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                        }

                        ag.TargetReplanTime = 0;
                    }
                    else if (rStatus == Status.DT_SUCCESS)
                    {
                        var path = ag.Corridor.GetPath();
                        if (!path.Any())
                        {
                            Console.WriteLine($"Crowd.UpdateMoveRequest {ag} no path assigned;");
                        }

                        // Apply results.
                        Vector3 targetPos = ag.TargetPos;

                        bool valid = true;
                        Status prStatus = m_pathq.GetPathResult(ag.TargetPathqRef, m_maxPathResult, out SimplePath res);
                        if (prStatus != Status.DT_SUCCESS || res.Count <= 0)
                        {
                            valid = false;
                        }

                        ag.Partial = prStatus.HasFlag(Status.DT_PARTIAL_RESULT);

                        // Merge result and existing path.
                        // The agent might have moved whilst the request is
                        // being processed, so the path may have changed.
                        // We assume that the end of the path is at the same location
                        // where the request was issued.

                        // The last ref in the old path should be the same as
                        // the location where the request was issued..
                        if (valid && path.Last() != res.Start)
                        {
                            valid = false;
                        }

                        if (valid)
                        {
                            // Put the old path infront of the old path.
                            if (path.Count() > 1)
                            {
                                res.Merge(path, path.Count());
                            }

                            // Check for partial path.
                            if (res.End != ag.TargetRef)
                            {
                                // Partial path, constrain target position inside the last polygon.
                                Status cStatus = m_navquery.ClosestPointOnPoly(res.End, targetPos, out Vector3 nearest);
                                if (cStatus == Status.DT_SUCCESS)
                                {
                                    targetPos = nearest;
                                }
                                else
                                {
                                    valid = false;
                                }
                            }
                        }

                        if (valid)
                        {
                            // Set current corridor.
                            ag.Corridor.SetCorridor(targetPos, res);
                            // Force to update boundary.
                            ag.Boundary.Reset();
                            ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_VALID;
                        }
                        else
                        {
                            // Something went wrong.
                            ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                        }

                        ag.TargetReplanTime = 0;
                    }
                }
            }
        }

        private void UpdateTopologyOptimization(IEnumerable<CrowdAgent> agents, float dt)
        {
            if (!agents.Any())
            {
                return;
            }

            float OPT_TIME_THR = 0.5f; // seconds
            List<CrowdAgent> queue = new List<CrowdAgent>();

            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }
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

        private void GridRegisterAgents(IEnumerable<CrowdAgent> agents)
        {
            m_grid.Clear();

            foreach (var ag in agents)
            {
                m_grid.AddItem(ag, ag.NPos, ag.Params.Radius);
            }
        }

        private void FindColliders(IEnumerable<CrowdAgent> agents)
        {
            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Update the collision boundary after certain distance has been passed or
                // if it has become invalid.
                float updateThr = ag.Params.CollisionQueryRange * 0.25f;
                float distSqr = Vector2.DistanceSquared(ag.NPos.XZ(), ag.Boundary.GetCenter().XZ());
                if (distSqr > updateThr * updateThr ||
                    !ag.Boundary.IsValid(m_navquery, m_filters[ag.Params.QueryFilterTypeIndex]))
                {
                    ag.Boundary.Update(
                        ag.Corridor.GetFirstPoly(),
                        ag.NPos,
                        ag.Params.CollisionQueryRange,
                        m_navquery,
                        m_filters[ag.Params.QueryFilterTypeIndex]);
                }

                // Query neighbour agents
                GetNeighbours(ag, m_grid);
            }
        }

        private void FindNextCorner(IEnumerable<CrowdAgent> agents, IEnumerable<CrowdAgentDebugInfo> debug)
        {
            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Find corners for steering
                ag.Corridor.FindCorners(
                    m_navquery,
                    DT_CROWDAGENT_MAX_CORNERS,
                    out StraightPath straightPath);

                ag.Corners = straightPath.Copy();

                // Check to see if the corner after the next corner is directly visible, and short cut to there.
                if (ag.Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_OPTIMIZE_VIS) && ag.Corners.Count > 0)
                {
                    Vector3 target = ag.Corners.GetPath(Math.Min(1, ag.Corners.Count - 1));
                    ag.Corridor.OptimizePathVisibility(
                        target,
                        ag.Params.PathOptimizationRange,
                        m_navquery,
                        m_filters[ag.Params.QueryFilterTypeIndex]);

                    // Copy data for debug purposes.
                    var d = debug?.FirstOrDefault(a => a.Agent == ag);
                    if (d != null)
                    {
                        d.OptStart = ag.Corridor.GetPos();
                        d.OptEnd = target;
                    }
                }
                else
                {
                    // Copy data for debug purposes.
                    var d = debug?.FirstOrDefault(a => a.Agent == ag);
                    if (d != null)
                    {
                        d.OptStart = Vector3.Zero;
                        d.OptEnd = Vector3.Zero;
                    }
                }
            }
        }

        private void TriggerOffMeshConnections(IEnumerable<CrowdAgent> agents)
        {
            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE ||
                    ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Check 
                float triggerRadius = ag.Params.Radius * 2.25f;
                if (OverOffmeshConnection(ag, triggerRadius))
                {
                    // Prepare to off-mesh connection.
                    CrowdAgentAnimation anim = m_agentAnims[ag];

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
                        anim.TMax = (Vector2.Distance(anim.StartPos.XZ(), anim.EndPos.XZ()) / ag.Params.MaxSpeed) * 0.5f;
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
        }

        private void CalculateSteering(IEnumerable<CrowdAgent> agents)
        {
            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                {
                    continue;
                }

                Vector3 dvel;
                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    dvel = ag.TargetPos;
                    ag.DesiredSpeed = ag.TargetPos.Length();
                }
                else
                {
                    // Calculate steering direction.
                    if (ag.Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_ANTICIPATE_TURNS))
                    {
                        CalcSmoothSteerDirection(ag, out dvel);
                    }
                    else
                    {
                        CalcStraightSteerDirection(ag, out dvel);
                    }

                    // Calculate speed scale, which tells the agent to slowdown at the end of the path.
                    float slowDownRadius = ag.Params.Radius * 2; // TODO: make less hacky.
                    float speedScale = GetDistanceToGoal(ag, slowDownRadius) / slowDownRadius;

                    ag.DesiredSpeed = ag.Params.MaxSpeed;
                    dvel *= ag.DesiredSpeed * speedScale;
                }

                // Separation
                if (ag.Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_SEPARATION))
                {
                    float separationDist = ag.Params.CollisionQueryRange;
                    float invSeparationDist = 1.0f / separationDist;
                    float separationWeight = ag.Params.SeparationWeight;

                    float w = 0;
                    Vector3 disp = Vector3.Zero;

                    foreach (var crowdNei in ag.GetNeighbours())
                    {
                        var nei = crowdNei.Agent;

                        Vector3 diff = ag.NPos - nei.NPos;
                        diff.Y = 0;

                        float distSqr = diff.LengthSquared();
                        if (distSqr < 0.00001f)
                        {
                            continue;
                        }
                        if (distSqr > separationDist * separationDist)
                        {
                            continue;
                        }
                        float dist = (float)Math.Sqrt(distSqr);
                        float dDiv = dist * invSeparationDist;
                        float weight = separationWeight * (1.0f - (dDiv * dDiv));

                        disp += diff * (weight / dist);
                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        // Adjust desired velocity.
                        dvel += disp * (1.0f / w);
                        // Clamp desired velocity to desired speed.
                        float speedSqr = dvel.LengthSquared();
                        float desiredSqr = ag.DesiredSpeed * ag.DesiredSpeed;
                        if (speedSqr > desiredSqr)
                        {
                            dvel *= desiredSqr / speedSqr;
                        }
                    }
                }

                // Set the desired velocity.
                ag.DVel = dvel;
            }
        }

        private void VelocityPlanning(IEnumerable<CrowdAgent> agents, IEnumerable<CrowdAgentDebugInfo> debug)
        {
            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_OBSTACLE_AVOIDANCE))
                {
                    m_obstacleQuery.Reset();

                    // Add neighbours as obstacles.
                    foreach (var crowdNei in ag.GetNeighbours())
                    {
                        var nei = crowdNei.Agent;
                        m_obstacleQuery.AddCircle(nei.NPos, nei.Params.Radius, nei.Vel, nei.DVel);
                    }

                    // Append neighbour segments as obstacles.
                    for (int j = 0; j < ag.Boundary.GetSegmentCount(); ++j)
                    {
                        Vector3[] s = ag.Boundary.GetSegment(j);
                        if (DetourUtils.TriArea2D(ag.NPos, s[0], s[1]) < 0.0f)
                        {
                            continue;
                        }
                        m_obstacleQuery.AddSegment(s[0], s[1]);
                    }

                    // Sample new safe velocity.
                    int ns;

                    var param = m_obstacleQueryParams[ag.Params.ObstacleAvoidanceType];
                    var vod = debug?.FirstOrDefault(a => a.Agent == ag).Vod;

                    if (m_sampleVelocityAdaptative)
                    {
                        ns = m_obstacleQuery.SampleVelocityAdaptive(
                            ag.NPos,
                            ag.Params.Radius,
                            ag.DesiredSpeed,
                            ag.Vel,
                            ag.DVel,
                            out Vector3 nvel,
                            param,
                            vod);

                        ag.NVel = nvel;
                    }
                    else
                    {
                        ns = m_obstacleQuery.SampleVelocityGrid(
                            ag.NPos,
                            ag.Params.Radius,
                            ag.DesiredSpeed,
                            ag.Vel,
                            ag.DVel,
                            out Vector3 nvel,
                            param,
                            vod);

                        ag.NVel = nvel;
                    }

                    m_velocitySampleCount += ns;
                }

                else
                {
                    // If not using velocity planning, new velocity is directly the desired velocity.
                    ag.NVel = ag.DVel;
                }
            }
        }

        private void IntegrateAgents(IEnumerable<CrowdAgent> agents, float dt)
        {
            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                Integrate(ag, dt);
            }
        }

        private void HandleCollisions(IEnumerable<CrowdAgent> agents)
        {
            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                ag.Disp = Vector3.Zero;

                float w = 0;

                foreach (var crowdNei in ag.GetNeighbours())
                {
                    var nei = crowdNei.Agent;

                    Vector3 diff = ag.NPos - nei.NPos;
                    diff.Y = 0;

                    float dist = diff.LengthSquared();
                    float diffRad = ag.Params.Radius + nei.Params.Radius;
                    if (dist > diffRad * diffRad)
                    {
                        continue;
                    }

                    dist = (float)Math.Sqrt(dist);
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

                    ag.Disp = (ag.Disp + diff * pen);

                    w += 1.0f;
                }

                if (w > 0.0001f)
                {
                    float iw = 1.0f / w;
                    ag.Disp *= iw;
                }
            }

            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                ag.NPos += ag.Disp;
            }
        }

        private void MoveAgents(IEnumerable<CrowdAgent> agents)
        {
            foreach (var ag in agents)
            {
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Move along navmesh.
                ag.Corridor.MovePosition(ag.NPos, m_navquery, m_filters[ag.Params.QueryFilterTypeIndex]);
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
            foreach (var agentAnim in m_agentAnims)
            {
                CrowdAgentAnimation anim = agentAnim.Value;

                if (!anim.Active)
                {
                    continue;
                }

                CrowdAgent ag = agentAnim.Key;

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
                    float u = Tween(anim.T, 0f, ta);
                    ag.NPos = Vector3.Lerp(anim.InitPos, anim.StartPos, u);
                }
                else
                {
                    float u = Tween(anim.T, ta, tb);
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
        public QueryFilter GetFilter(int i)
        {
            return (i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE) ? m_filters[i] : null;
        }
        /// <summary>
        /// Sets the filter for the specified index.
        /// </summary>
        /// <param name="i">The index</param>
        /// <param name="filter">The new filter</param>
        public void SetFilter(int i, QueryFilter filter)
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
