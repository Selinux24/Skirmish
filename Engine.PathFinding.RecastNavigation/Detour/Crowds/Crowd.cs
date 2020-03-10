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
        /// The maximum number of neighbors that a crowd agent can take into account
        /// for steering decisions.
        /// @ingroup crowd
        public const int DT_CROWDAGENT_MAX_NEIGHBOURS = 6;
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

        public const int MAX_ITERS_PER_UPDATE = 100;

        public const int MAX_PATHQUEUE_NODES = 4096;

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
            ag.Vel += ag.Vel * dv;

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
            if (ag.NCorners <= 0)
            {
                return false;
            }

            bool offMeshConnection = ag.CornerFlags[ag.NCorners - 1].HasFlag(StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION);
            if (offMeshConnection)
            {
                float distSq = Vector2.DistanceSquared(ag.NPos.XZ(), ag.CornerVerts[ag.NCorners - 1].XZ());
                if (distSq < radius * radius)
                {
                    return true;
                }
            }

            return false;
        }

        public static float GetDistanceToGoal(CrowdAgent ag, float range)
        {
            if (ag.NCorners <= 0)
            {
                return range;
            }

            bool endOfPath = ag.CornerFlags[ag.NCorners - 1].HasFlag(StraightPathFlagTypes.DT_STRAIGHTPATH_END);
            if (endOfPath)
            {
                return Math.Min(Vector2.Distance(ag.NPos.XZ(), ag.CornerVerts[ag.NCorners - 1].XZ()), range);
            }

            return range;
        }

        public static void CalcSmoothSteerDirection(CrowdAgent ag, out Vector3 dir)
        {
            dir = Vector3.Zero;

            if (ag.NCorners <= 0)
            {
                return;
            }

            int ip0 = 0;
            int ip1 = Math.Min(1, ag.NCorners - 1);
            Vector3 p0 = ag.CornerVerts[ip0];
            Vector3 p1 = ag.CornerVerts[ip1];

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

            if (ag.NCorners <= 0)
            {
                return;
            }

            dir = ag.CornerVerts[0] - ag.NPos;
            dir.Y = 0;

            dir.Normalize();
        }

        public static int AddNeighbour(int idx, float dist, CrowdNeighbour[] neis, int nneis, int maxNeis)
        {
            // Insert neighbour based on the distance.
            CrowdNeighbour nei;
            if (nneis <= 0)
            {
                nei = new CrowdNeighbour();
                neis[nneis] = nei;
            }
            else if (dist >= neis[nneis - 1].Dist)
            {
                if (nneis >= maxNeis)
                {
                    return nneis;
                }

                nei = new CrowdNeighbour();
                neis[nneis] = nei;
            }
            else
            {
                int i;
                for (i = 0; i < nneis; ++i)
                {
                    if (dist <= neis[i].Dist)
                    {
                        break;
                    }
                }

                int tgt = i + 1;
                int n = Math.Min(nneis - i, maxNeis - tgt);

                if (n > 0)
                {
                    Array.ConstrainedCopy(neis, tgt, neis, i, n);
                }

                nei = neis[i];
            }

            nei.Idx = idx;
            nei.Dist = dist;

            return Math.Min(nneis + 1, maxNeis);
        }

        public static int GetNeighbours(Vector3 pos, float height, float range, CrowdAgent skip, CrowdNeighbour[] result, int maxResult, CrowdAgent[] agents, ProximityGrid grid)
        {
            int n = 0;

            int MAX_NEIS = 32;
            int nids = grid.QueryItems(
                pos.X - range,
                pos.Z - range,
                pos.X + range,
                pos.Z + range,
                MAX_NEIS,
                out int[] ids);

            for (int i = 0; i < nids; ++i)
            {
                CrowdAgent ag = agents[ids[i]];

                if (ag == skip)
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

                n = AddNeighbour(ids[i], distSqr, result, n, maxResult);
            }

            return n;
        }

        public static int AddToOptQueue(CrowdAgent newag, CrowdAgent[] agents, int nagents, int maxAgents)
        {
            // Insert neighbour based on greatest time.
            int slot = 0;
            if (nagents <= 0)
            {
                slot = nagents;
            }
            else if (newag.TopologyOptTime <= agents[nagents - 1].TopologyOptTime)
            {
                if (nagents >= maxAgents)
                {
                    return nagents;
                }

                slot = nagents;
            }
            else
            {
                int i;
                for (i = 0; i < nagents; ++i)
                {
                    if (newag.TopologyOptTime >= agents[i].TopologyOptTime)
                    {
                        break;
                    }
                }

                int tgt = i + 1;
                int n = Math.Min(nagents - i, maxAgents - tgt);

                if (n > 0)
                {
                    Array.ConstrainedCopy(agents, tgt, agents, i, n);
                }

                slot = i;
            }

            agents[slot] = newag;

            return Math.Min(nagents + 1, maxAgents);
        }

        public static int AddToPathQueue(CrowdAgent newag, CrowdAgent[] agents, int nagents, int maxAgents)
        {
            // Insert neighbour based on greatest time.
            int slot = 0;
            if (nagents <= 0)
            {
                slot = nagents;
            }
            else if (newag.TargetReplanTime <= agents[nagents - 1].TargetReplanTime)
            {
                if (nagents >= maxAgents)
                {
                    return nagents;
                }

                slot = nagents;
            }
            else
            {
                int i;
                for (i = 0; i < nagents; ++i)
                {
                    if (newag.TargetReplanTime >= agents[i].TargetReplanTime)
                    {
                        break;
                    }
                }

                int tgt = i + 1;
                int n = Math.Min(nagents - i, maxAgents - tgt);

                if (n > 0)
                {
                    Array.ConstrainedCopy(agents, tgt, agents, i, n);
                }

                slot = i;
            }

            agents[slot] = newag;

            return Math.Min(nagents + 1, maxAgents);
        }


        private int m_maxAgents = 0;
        private readonly List<CrowdAgent> m_agents = new List<CrowdAgent>();
        private readonly List<CrowdAgent> m_activeAgents = new List<CrowdAgent>();
        private readonly List<CrowdAgentAnimation> m_agentAnims = new List<CrowdAgentAnimation>();

        private PathQueue m_pathq = null;

        private readonly ObstacleAvoidanceParams[] m_obstacleQueryParams = new ObstacleAvoidanceParams[DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS];
        private ObstacleAvoidanceQuery m_obstacleQuery = null;

        private ProximityGrid m_grid = null;

        private int m_maxPathResult = 0;

        private Vector3 m_agentPlacementHalfExtents;

        private QueryFilter[] m_filters = null;

        private int m_velocitySampleCount = 0;

        private NavMeshQuery m_navquery = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public Crowd()
        {

        }


        private void UpdateTopologyOptimization(IEnumerable<CrowdAgent> agents, float dt)
        {
            if (!agents.Any())
            {
                return;
            }

            float OPT_TIME_THR = 0.5f; // seconds
            int OPT_MAX_AGENTS = 1;
            CrowdAgent[] queue = new CrowdAgent[OPT_MAX_AGENTS];
            int nqueue = 0;

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
                    nqueue = AddToOptQueue(ag, queue, nqueue, OPT_MAX_AGENTS);
                }
            }

            for (int i = 0; i < nqueue; ++i)
            {
                CrowdAgent ag = queue[i];
                ag.Corridor.OptimizePathTopology(m_navquery, m_filters[ag.Params.QueryFilterType]);
                ag.TopologyOptTime = 0;
            }
        }
        private void UpdateMoveRequest()
        {
            int PATH_MAX_AGENTS = 8;
            CrowdAgent[] queue = new CrowdAgent[PATH_MAX_AGENTS];
            int nqueue = 0;

            // Fire off new requests.
            for (int i = 0; i < m_maxAgents; ++i)
            {
                CrowdAgent ag = m_agents[i];
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
                    int[] path = ag.Corridor.GetPath();
                    int npath = ag.Corridor.GetPathCount();
                    if (npath <= 0)
                    {
                        Console.WriteLine($"Crowd.UpdateMoveRequest {ag} no path assigned;");
                    }

                    const int MAX_RES = 32;
                    Vector3 reqPos = Vector3.Zero;
                    SimplePath reqPath;

                    // Quick search towards the goal.
                    const int MAX_ITER = 20;
                    m_navquery.InitSlicedFindPath(path[0], ag.TargetRef, ag.NPos, ag.TargetPos, m_filters[ag.Params.QueryFilterType], FindPathOptions.DT_FINDPATH_ANY_ANGLE);
                    m_navquery.UpdateSlicedFindPath(MAX_ITER, out var doneIters);

                    Status qStatus;
                    if (ag.TargetReplan) // && npath > 10)
                    {
                        // Try to use existing steady path during replan if possible.
                        qStatus = m_navquery.FinalizeSlicedFindPathPartial(MAX_RES, path, npath, out reqPath);
                    }
                    else
                    {
                        // Try to move towards target when goal changes.
                        qStatus = m_navquery.FinalizeSlicedFindPath(MAX_RES, out reqPath);
                    }

                    if (qStatus != Status.DT_FAILURE && reqPath.Count > 0)
                    {
                        // In progress or succeed.
                        if (reqPath.Path[reqPath.Count - 1] != ag.TargetRef)
                        {
                            // Partial path, constrain target position inside the last polygon.
                            Status cStatus = m_navquery.ClosestPointOnPoly(reqPath.Path[reqPath.Count - 1], ag.TargetPos, out reqPos, out bool posOverPoly);
                            if (cStatus != Status.DT_SUCCESS)
                            {
                                reqPath.Count = 0;
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
                        reqPath.Path[0] = path[0];
                        reqPath.Count = 1;
                    }

                    ag.Corridor.SetCorridor(reqPos, reqPath);
                    ag.Boundary.Reset();
                    ag.Partial = false;

                    if (reqPath?.Path[reqPath.Count - 1] == ag.TargetRef)
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

                if (ag.TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                {
                    nqueue = AddToPathQueue(ag, queue, nqueue, PATH_MAX_AGENTS);
                }
            }

            for (int i = 0; i < nqueue; ++i)
            {
                CrowdAgent ag = queue[i];
                ag.TargetPathqRef = m_pathq.Request(
                    ag.Corridor.GetLastPoly(),
                    ag.TargetRef,
                    ag.Corridor.GetTarget(),
                    ag.TargetPos,
                    m_filters[ag.Params.QueryFilterType]);

                if (ag.TargetPathqRef != PathQueue.DT_PATHQ_INVALID)
                {
                    ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH;
                }
            }

            // Update requests.
            m_pathq.Update(MAX_ITERS_PER_UPDATE);

            Status status;

            // Process path results.
            for (int i = 0; i < m_maxAgents; ++i)
            {
                CrowdAgent ag = m_agents[i];

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
                    status = m_pathq.GetRequestStatus(ag.TargetPathqRef);
                    if (status != Status.DT_SUCCESS)
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
                    else if (status == Status.DT_SUCCESS)
                    {
                        int[] path = ag.Corridor.GetPath();
                        int npath = ag.Corridor.GetPathCount();
                        if (npath <= 0)
                        {
                            Console.WriteLine($"Crowd.UpdateMoveRequest {ag} no path assigned;");
                        }

                        // Apply results.
                        Vector3 targetPos = ag.TargetPos;

                        bool valid = true;
                        status = m_pathq.GetPathResult(ag.TargetPathqRef, m_maxPathResult, out SimplePath res);
                        if (status != Status.DT_SUCCESS || res.Count <= 0)
                        {
                            valid = false;
                        }

                        if (status.HasFlag(Status.DT_PARTIAL_RESULT))
                        {
                            ag.Partial = true;
                        }
                        else
                        {
                            ag.Partial = false;
                        }

                        // Merge result and existing path.
                        // The agent might have moved whilst the request is
                        // being processed, so the path may have changed.
                        // We assume that the end of the path is at the same location
                        // where the request was issued.

                        // The last ref in the old path should be the same as
                        // the location where the request was issued..
                        if (valid && path[npath - 1] != res.Path[0])
                        {
                            valid = false;
                        }

                        if (valid)
                        {
                            // Put the old path infront of the old path.
                            if (npath > 1)
                            {
                                // Make space for the old path.
                                if ((npath - 1) + res.Count > m_maxPathResult)
                                {
                                    res.Count = m_maxPathResult - (npath - 1);
                                }

                                // Copy old path in the beginning.
                                List<int> tmp = new List<int>(res.Path);
                                tmp.InsertRange(0, path);
                                res.Path = tmp.ToArray();
                                res.Count += npath - 1;

                                // Remove trackbacks
                                for (int j = 0; j < res.Count; ++j)
                                {
                                    if (j - 1 >= 0 && j + 1 < res.Count)
                                    {
                                        bool samePoly = res.Path[j - 1] == res.Path[j + 1];
                                        if (samePoly)
                                        {
                                            Array.ConstrainedCopy(res.Path, j - 1, res.Path, j + 1, res.Count - (j + 1));
                                            res.Count -= 2;
                                            j -= 2;
                                        }
                                    }
                                }
                            }

                            // Check for partial path.
                            if (res.Path[res.Count - 1] != ag.TargetRef)
                            {
                                // Partial path, constrain target position inside the last polygon.
                                Vector3 nearest;
                                Status cStatus = m_navquery.ClosestPointOnPoly(res.Path[res.Count - 1], targetPos, out nearest, out bool posOverPoly);
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
                int idx = GetAgentIndex(ag);
                Vector3 agentPos = ag.NPos;
                int agentRef = ag.Corridor.GetFirstPoly();
                if (!m_navquery.IsValidPolyRef(agentRef, m_filters[ag.Params.QueryFilterType]))
                {
                    // Current location is not valid, try to reposition.
                    // TODO: this can snap agents, how to handle that?
                    m_navquery.FindNearestPoly(
                        ag.NPos, m_agentPlacementHalfExtents, m_filters[ag.Params.QueryFilterType],
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
                    if (!m_navquery.IsValidPolyRef(ag.TargetRef, m_filters[ag.Params.QueryFilterType]))
                    {
                        // Current target is not valid, try to reposition.
                        m_navquery.FindNearestPoly(
                            ag.TargetPos, m_agentPlacementHalfExtents, m_filters[ag.Params.QueryFilterType],
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
                if (!ag.Corridor.IsValid(CHECK_LOOKAHEAD, m_navquery, m_filters[ag.Params.QueryFilterType]))
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
                if (replan &&
                    ag.TargetState != MoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                {
                    bool requested = RequestMoveTargetReplan(idx, ag.TargetRef, ag.TargetPos);
                    if (!requested)
                    {
                        Console.WriteLine($"RequestMoveTargetReplan error: {idx} {ag.TargetRef} {ag.TargetPos}");
                    }
                }
            }
        }
        private int GetAgentIndex(CrowdAgent agent)
        {
            return m_agents.IndexOf(agent);
        }
        private bool RequestMoveTargetReplan(int idx, int r, Vector3 pos)
        {
            if (idx < 0 || idx >= m_maxAgents)
            {
                return false;
            }

            CrowdAgent ag = m_agents[idx];

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
        private void Purge()
        {
            m_maxAgents = 0;
            m_agents.Clear();
            m_activeAgents.Clear();
            m_agentAnims.Clear();

            m_grid = null;

            m_obstacleQuery = null;

            m_navquery = null;
        }

        /// <summary>
        /// Initializes the crowd.  
        /// </summary>
        /// <param name="maxAgents">The maximum number of agents the crowd can manage. [Limit: >= 1]</param>
        /// <param name="maxAgentRadius">The maximum radius of any agent that will be added to the crowd. [Limit: > 0]</param>
        /// <param name="nav">The navigation mesh to use for planning.</param>
        /// <returns>True if the initialization succeeded.</returns>
        public bool Init(int maxAgents, float maxAgentRadius, NavMesh nav)
        {
            Purge();

            m_maxAgents = maxAgents;

            // Larger than agent radius because it is also used for agent recovery.
            m_agentPlacementHalfExtents = new Vector3(maxAgentRadius * 2.0f, maxAgentRadius * 1.5f, maxAgentRadius * 2.0f);

            m_grid = new ProximityGrid();
            if (!m_grid.Init(m_maxAgents * 4, maxAgentRadius * 3))
            {
                return false;
            }

            m_obstacleQuery = new ObstacleAvoidanceQuery();
            if (!m_obstacleQuery.Init(6, 8))
            {
                return false;
            }

            // Init filters
            m_filters = Helper.CreateArray(DT_CROWD_MAX_QUERY_FILTER_TYPE, () => new QueryFilter()
            {
                IncludeFlags = SamplePolyFlagTypes.SAMPLE_POLYFLAGS_WALK,
            });

            // Init obstacle query params.
            for (int i = 0; i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS; ++i)
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

                m_obstacleQueryParams[i] = param;
            }

            // Allocate temp buffer for merging paths.
            m_maxPathResult = 256;
            m_pathq = new PathQueue();
            if (!m_pathq.Init(m_maxPathResult, MAX_PATHQUEUE_NODES, nav))
            {
                return false;
            }

            for (int i = 0; i < m_maxAgents; ++i)
            {
                CrowdAgent agent = new CrowdAgent
                {
                    Active = false
                };

                if (!agent.Corridor.Init(m_maxPathResult))
                {
                    return false;
                }

                m_agents.Add(agent);
            }

            for (int i = 0; i < m_maxAgents; ++i)
            {
                CrowdAgentAnimation anim = new CrowdAgentAnimation
                {
                    Active = false,
                };

                m_agentAnims.Add(anim);
            }

            // The navquery is mostly used for local searches, no need for large node pool.
            m_navquery = new NavMeshQuery();
            if (m_navquery.Init(nav, MAX_COMMON_NODES) != Status.DT_SUCCESS)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Sets the shared avoidance configuration for the specified index.
        /// </summary>
        /// <param name="idx">The index. [Limits: 0 <= value < #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]</param>
        /// <param name="param">The new configuration.</param>
        public void SetObstacleAvoidanceParams(int idx, ObstacleAvoidanceParams param)
        {
            if (idx >= 0 && idx < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                m_obstacleQueryParams[idx] = param;
            }
        }
        /// <summary>
        /// Gets the shared avoidance configuration for the specified index.
        /// </summary>
        /// <param name="idx">The index of the configuration to retreive. [Limits:  0 <= value < #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]</param>
        /// <returns>The requested configuration.</returns>
        public ObstacleAvoidanceParams GetObstacleAvoidanceParams(int idx)
        {
            if (idx >= 0 && idx < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                return m_obstacleQueryParams[idx];
            }

            return null;
        }
        /// <summary>
        /// Gets the specified agent from the pool.
        /// </summary>
        /// <param name="idx">The agent index. [Limits: 0 <= value < #getAgentCount()]</param>
        /// <returns>The requested agent.</returns>
        public CrowdAgent GetAgent(int idx)
        {
            if (idx < 0 || idx >= m_maxAgents)
            {
                return null;
            }

            return m_agents[idx];
        }
        /// <summary>
        /// Gets the specified agent from the pool.
        /// </summary>
        /// <param name="idx">The agent index. [Limits: 0 <= value < #getAgentCount()]</param>
        /// <returns>The requested agent.</returns>
        public CrowdAgent GetEditableAgent(int idx)
        {
            if (idx < 0 || idx >= m_maxAgents)
            {
                return null;
            }

            var res = m_agents[idx];
            if (res.Active)
            {
                return res;
            }

            return null;
        }
        /// <summary>
        /// The maximum number of agents that can be managed by the object.
        /// </summary>
        /// <returns>The maximum number of agents.</returns>
        public int GetAgentCount()
        {
            return m_maxAgents;
        }
        /// <summary>
        /// Adds a new agent to the crowd.
        /// </summary>
        /// <param name="pos">The requested position of the agent.</param>
        /// <param name="param">The configutation of the agent.</param>
        /// <returns>The index of the agent in the agent pool. Or -1 if the agent could not be added.</returns>
        public int AddAgent(Vector3 pos, CrowdAgentParams param)
        {
            // Find empty slot.
            int idx = -1;
            for (int i = 0; i < m_maxAgents; ++i)
            {
                if (!m_agents[i].Active)
                {
                    idx = i;
                    break;
                }
            }
            if (idx == -1)
            {
                return -1;
            }

            CrowdAgent ag = m_agents[idx];

            UpdateAgentParameters(idx, param);

            // Find nearest position on navmesh and place the agent there.
            Status status = m_navquery.FindNearestPoly(
                pos, m_agentPlacementHalfExtents, m_filters[ag.Params.QueryFilterType],
                out int r, out Vector3 nearest);
            if (status != Status.DT_SUCCESS)
            {
                nearest = pos;
                r = 0;
            }

            ag.Corridor.Reset(r, nearest);
            ag.Boundary.Reset();
            ag.Partial = false;

            ag.TopologyOptTime = 0;
            ag.TargetReplanTime = 0;
            ag.NNeis = 0;

            ag.DVel = Vector3.Zero;
            ag.NVel = Vector3.Zero;
            ag.Vel = Vector3.Zero;
            ag.NPos = nearest;

            ag.DesiredSpeed = 0;

            if (r > 0)
            {
                ag.State = CrowdAgentState.DT_CROWDAGENT_STATE_WALKING;
            }
            else
            {
                ag.State = CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
            }

            ag.TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;

            ag.Active = true;

            return idx;
        }
        /// <summary>
        /// Updates the specified agent's configuration.
        /// </summary>
        /// <param name="idx">The agent index. [Limits: 0 <= value < #getAgentCount()]</param>
        /// <param name="param">The new agent configuration.</param>
        public void UpdateAgentParameters(int idx, CrowdAgentParams param)
        {
            if (idx < 0 || idx >= m_maxAgents)
            {
                return;
            }

            m_agents[idx].Params = param;
        }
        /// <summary>
        /// Removes the agent from the crowd.
        /// </summary>
        /// <param name="idx">The agent index. [Limits: 0 <= value < #getAgentCount()]</param>
        public void RemoveAgent(int idx)
        {
            if (idx >= 0 && idx < m_maxAgents)
            {
                m_agents[idx].Active = false;
            }
        }
        /// <summary>
        /// Submits a new move request for the specified agent.
        /// </summary>
        /// <param name="idx">The agent index. [Limits: 0 <= value < #getAgentCount()]</param>
        /// <param name="r">The position's polygon reference.</param>
        /// <param name="pos">The position within the polygon.</param>
        /// <returns>True if the request was successfully submitted.</returns>
        public bool RequestMoveTarget(int idx, int r, Vector3 pos)
        {
            if (idx < 0 || idx >= m_maxAgents)
            {
                return false;
            }

            if (r == 0)
            {
                return false;
            }

            CrowdAgent ag = m_agents[idx];

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
        /// <param name="idx">The agent index. [Limits: 0 <= value < #getAgentCount()]</param>
        /// <param name="vel">The movement velocity. [(x, y, z)]</param>
        /// <returns>True if the request was successfully submitted.</returns>
        public bool RequestMoveVelocity(int idx, Vector3 vel)
        {
            if (idx < 0 || idx >= m_maxAgents)
            {
                return false;
            }

            CrowdAgent ag = m_agents[idx];

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
        /// <param name="idx">The agent index. [Limits: 0 <= value < #getAgentCount()]</param>
        /// <returns>True if the request was successfully reseted.</returns>
        public bool ResetMoveTarget(int idx)
        {
            if (idx < 0 || idx >= m_maxAgents)
            {
                return false;
            }

            CrowdAgent ag = m_agents[idx];

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
        /// Gets the active agents int the agent pool.
        /// </summary>
        /// <param name="maxAgents">An array of agent pointers. [(#dtCrowdAgent *) * maxAgents]</param>
        /// <returns>The number of agents returned in @p agents.</returns>
        public IEnumerable<CrowdAgent> GetActiveAgents(int maxAgents)
        {
            var agents = m_agents.Where(a => a.Active);

            if (agents.Count() > maxAgents)
            {
                agents = agents.Take(maxAgents);
            }

            return agents.ToArray();
        }
        /// <summary>
        /// Updates the steering and positions of all agents.
        /// </summary>
        /// <param name="dt">The time, in seconds, to update the simulation. [Limit: > 0]</param>
        /// <param name="debug">A debug object to load with debug information. [Opt]</param>
        public void Update(float dt, CrowdAgentDebugInfo debug)
        {
            m_velocitySampleCount = 0;

            int debugIdx = debug?.Idx ?? -1;

            CrowdAgent[] agents = GetActiveAgents(m_maxAgents).ToArray();
            int nagents = agents.Count();

            // Check that all agents still have valid paths.
            CheckPathValidity(agents, dt);

            // Update async move request and path finder.
            UpdateMoveRequest();

            // Optimize path topology.
            UpdateTopologyOptimization(agents, dt);

            // Register agents to proximity grid.
            m_grid.Clear();
            for (int i = 0; i < nagents; ++i)
            {
                CrowdAgent ag = agents[i];
                Vector3 p = ag.NPos;
                float r = ag.Params.Radius;
                m_grid.AddItem(i, p.X - r, p.Z - r, p.X + r, p.Z + r);
            }

            // Get nearby navmesh segments and agents to collide with.
            for (int i = 0; i < nagents; ++i)
            {
                var ag = agents[i];
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Update the collision boundary after certain distance has been passed or
                // if it has become invalid.
                float updateThr = ag.Params.CollisionQueryRange * 0.25f;
                float distSqr = Vector2.DistanceSquared(ag.NPos.XZ(), ag.Boundary.GetCenter().XZ());
                if (distSqr > updateThr * updateThr ||
                    !ag.Boundary.IsValid(m_navquery, m_filters[ag.Params.QueryFilterType]))
                {
                    ag.Boundary.Update(
                        ag.Corridor.GetFirstPoly(),
                        ag.NPos,
                        ag.Params.CollisionQueryRange,
                        m_navquery,
                        m_filters[ag.Params.QueryFilterType]);
                }

                // Query neighbour agents
                ag.NNeis = GetNeighbours(
                    ag.NPos,
                    ag.Params.Height,
                    ag.Params.CollisionQueryRange,
                    ag,
                    ag.Neis,
                    DT_CROWDAGENT_MAX_NEIGHBOURS,
                    agents,
                    m_grid);

                for (int j = 0; j < ag.NNeis; j++)
                {
                    ag.Neis[j].Idx = GetAgentIndex(agents[ag.Neis[j].Idx]);
                }
            }

            // Find next corner to steer to.
            for (int i = 0; i < nagents; ++i)
            {
                CrowdAgent ag = agents[i];

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
                ag.NCorners = ag.Corridor.FindCorners(
                    m_navquery,
                    m_filters[ag.Params.QueryFilterType],
                    DT_CROWDAGENT_MAX_CORNERS,
                    out StraightPath straightPath);

                ag.CornerVerts = straightPath.Path;
                ag.CornerFlags = straightPath.Flags;
                ag.CornerPolys = straightPath.Refs;

                // Check to see if the corner after the next corner is directly visible, and short cut to there.
                if (ag.Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_OPTIMIZE_VIS) && ag.NCorners > 0)
                {
                    Vector3 target = ag.CornerVerts[Math.Min(1, ag.NCorners - 1)];
                    ag.Corridor.OptimizePathVisibility(
                        target,
                        ag.Params.PathOptimizationRange,
                        m_navquery,
                        m_filters[ag.Params.QueryFilterType]);

                    // Copy data for debug purposes.
                    if (debugIdx == i)
                    {
                        debug.OptStart = ag.Corridor.GetPos();
                        debug.OptEnd = target;
                    }
                }
                else
                {
                    // Copy data for debug purposes.
                    if (debugIdx == i)
                    {
                        debug.OptStart = Vector3.Zero;
                        debug.OptEnd = Vector3.Zero;
                    }
                }
            }

            // Trigger off-mesh connections (depends on corners).
            for (int i = 0; i < nagents; ++i)
            {
                CrowdAgent ag = agents[i];

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
                    int idx = m_agents.IndexOf(ag);
                    CrowdAgentAnimation anim = m_agentAnims[idx];

                    // Adjust the path over the off-mesh connection.
                    int[] refs = new int[2];
                    if (ag.Corridor.MoveOverOffmeshConnection(
                        m_navquery,
                        ag.CornerPolys[ag.NCorners - 1],
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
                        ag.NCorners = 0;
                        ag.NNeis = 0;
                    }
                    else
                    {
                        // Path validity check will ensure that bad/blocked connections will be replanned.
                    }
                }
            }

            // Calculate steering.
            for (int i = 0; i < nagents; ++i)
            {
                CrowdAgent ag = agents[i];

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

                    for (int j = 0; j < ag.NNeis; ++j)
                    {
                        CrowdAgent nei = m_agents[ag.Neis[j].Idx];

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

            // Velocity planning.	
            for (int i = 0; i < nagents; ++i)
            {
                CrowdAgent ag = agents[i];

                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_OBSTACLE_AVOIDANCE))
                {
                    m_obstacleQuery.Reset();

                    // Add neighbours as obstacles.
                    for (int j = 0; j < ag.NNeis; ++j)
                    {
                        CrowdAgent nei = m_agents[ag.Neis[j].Idx];
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

                    ObstacleAvoidanceDebugData vod = null;
                    if (debugIdx == i)
                    {
                        vod = debug.Vod;
                    }

                    // Sample new safe velocity.
                    bool adaptive = true;
                    int ns;

                    ObstacleAvoidanceParams param = m_obstacleQueryParams[ag.Params.ObstacleAvoidanceType];

                    if (adaptive)
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

            // Integrate.
            for (int i = 0; i < nagents; ++i)
            {
                CrowdAgent ag = agents[i];
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                Integrate(ag, dt);
            }

            // Handle collisions.
            float COLLISION_RESOLVE_FACTOR = 0.7f;

            for (int iter = 0; iter < 4; ++iter)
            {
                for (int i = 0; i < nagents; ++i)
                {
                    CrowdAgent ag = agents[i];
                    int idx0 = GetAgentIndex(ag);

                    if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                    {
                        continue;
                    }

                    ag.Disp = Vector3.Zero;

                    float w = 0;

                    for (int j = 0; j < ag.NNeis; ++j)
                    {
                        CrowdAgent nei = m_agents[ag.Neis[j].Idx];
                        int idx1 = GetAgentIndex(nei);

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
                            if (idx0 > idx1)
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
                            pen = (1.0f / dist) * (pen * 0.5f) * COLLISION_RESOLVE_FACTOR;
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

                for (int i = 0; i < nagents; ++i)
                {
                    CrowdAgent ag = agents[i];
                    if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                    {
                        continue;
                    }

                    ag.NPos += ag.Disp;
                }
            }

            for (int i = 0; i < nagents; ++i)
            {
                CrowdAgent ag = agents[i];
                if (ag.State != CrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Move along navmesh.
                ag.Corridor.MovePosition(ag.NPos, m_navquery, m_filters[ag.Params.QueryFilterType]);
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

            // Update agents using off-mesh connection.
            for (int i = 0; i < m_maxAgents; ++i)
            {
                CrowdAgentAnimation anim = m_agentAnims[i];
                if (!anim.Active)
                {
                    continue;
                }
                CrowdAgent ag = agents[i];

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
        /// <param name="i"></param>
        /// <returns>The filter used by the crowd.</returns>
        public QueryFilter GetFilter(int i)
        {
            return (i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE) ? m_filters[i] : null;
        }
        /// <summary>
        /// Gets the filter used by the crowd.
        /// </summary>
        /// <param name="i"></param>
        /// <returns>The filter used by the crowd.</returns>
        public QueryFilter GetEditableFilter(int i)
        {
            return (i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE) ? m_filters[i] : null;
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
        public ProximityGrid GetGrid()
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
