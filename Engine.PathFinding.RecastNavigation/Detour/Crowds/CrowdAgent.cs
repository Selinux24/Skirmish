using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Represents an agent managed by a Crowd object.
    /// </summary>
    public class CrowdAgent
    {
        /// The maximum number of corners a crowd agent will look ahead in the path.
        /// This value is used for sizing the crowd agent corner buffers.
        /// Due to the behavior of the crowd manager, the actual number of useful
        /// corners will be one less than this number.
        /// @ingroup crowd
        const int DT_CROWDAGENT_MAX_CORNERS = 4;

        /// <summary>
        /// Neightbour list
        /// </summary>
        private readonly List<CrowdNeighbour> neighbours = [];

        /// <summary>
        /// True if the agent is active, false if the agent is in an unused slot in the agent pool.
        /// </summary>
        public bool Active { get; set; } = false;
        /// <summary>
        /// The type of mesh polygon the agent is traversing.
        /// </summary>
        public CrowdAgentState State { get; set; } = CrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
        /// <summary>
        /// True if the agent has valid path (targetState == DT_CROWDAGENT_TARGET_VALID) and the path does not lead to the requested position, else false.
        /// </summary>
        public bool Partial { get; set; }
        /// <summary>
        /// The path corridor the agent is using.
        /// </summary>
        public PathCorridor Corridor { get; private set; } = new();
        /// <summary>
        /// The local boundary data for the agent.
        /// </summary>
        public LocalBoundary Boundary { get; private set; } = new();
        /// <summary>
        /// Time since the agent's path corridor was optimized.
        /// </summary>
        public float TopologyOptTime { get; set; }
        /// <summary>
        /// The desired speed.
        /// </summary>
        public float DesiredSpeed { get; set; }

        /// <summary>
        /// The current agent position. [(x, y, z)]
        /// </summary>
        public Vector3 NPos { get; set; }
        /// <summary>
        /// A temporary value used to accumulate agent displacement during iterative collision resolution. [(x, y, z)]
        /// </summary>
        public Vector3 Disp { get; set; }
        /// <summary>
        /// The desired velocity of the agent. Based on the current path, calculated from scratch each frame. [(x, y, z)]
        /// </summary>
        public Vector3 DVel { get; set; }
        /// <summary>
        /// The desired velocity adjusted by obstacle avoidance, calculated from scratch each frame. [(x, y, z)]
        /// </summary>
        public Vector3 NVel { get; set; }
        /// <summary>
        /// The actual velocity of the agent. The change from nvel -> vel is constrained by max acceleration. [(x, y, z)]
        /// </summary>
        public Vector3 Vel { get; set; }

        /// <summary>
        /// The agent's configuration parameters.
        /// </summary>
        public CrowdAgentParameters Params { get; set; }

        /// <summary>
        /// The local path corridor corners for the agent.
        /// </summary>
        public StraightPath Corners { get; private set; } = new(DT_CROWDAGENT_MAX_CORNERS);

        /// <summary>
        /// State of the movement request.
        /// </summary>
        public MoveRequestState TargetState { get; set; } = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;
        /// <summary>
        /// Target polyref of the movement request.
        /// </summary>
        public int TargetRef { get; set; }
        /// <summary>
        /// Target position of the movement request (or velocity in case of DT_CROWDAGENT_TARGET_VELOCITY).
        /// </summary>
        public Vector3 TargetPos { get; set; }
        /// <summary>
        /// Path finder ref.
        /// </summary>
        public int TargetPathqRef { get; set; }
        /// <summary>
        /// Flag indicating that the current path is being replanned.
        /// </summary>
        public bool TargetReplan { get; set; }
        /// <summary>
        /// Time since the agent's target was replanned.
        /// </summary>
        public float TargetReplanTime { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters">Agent parameters</param>
        public CrowdAgent(CrowdAgentParameters parameters)
        {
            Params = parameters;
            Corridor.Init(256);
        }

        /// <summary>
        /// Initializes the agent position
        /// </summary>
        /// <param name="poly">Initial polygon</param>
        /// <param name="position">Initial position</param>
        /// <param name="maxPathResult">Maximum path result</param>
        public void Initialize(int poly, Vector3 position, int maxPathResult)
        {
            Corridor.Init(maxPathResult);
            Corridor.Reset(poly, position);
            Boundary.Reset();
            ClearNeighbours();
        }

        /// <summary>
        /// Updates agent settings
        /// </summary>
        /// <param name="settings">Settings</param>
        public void UpdateSettings(CrowdAgentSettings settings)
        {
            Params.UpdateSettings(settings);
        }

        /// <summary>
        /// Submits a new move request for the specified agent.
        /// </summary>
        /// <param name="poly">The position's polygon reference.</param>
        /// <param name="position">The position within the polygon.</param>
        /// <returns>True if the request was successfully submitted.</returns>
        public bool RequestMoveTarget(int poly, Vector3 position)
        {
            if (poly == 0)
            {
                return false;
            }

            // Initialize request.
            TargetRef = poly;
            TargetPos = position;
            TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
            TargetReplan = false;
            TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;

            return true;
        }
        /// <summary>
        /// Submits a new move request for the specified agent.
        /// </summary>
        /// <param name="velocity">The movement velocity. [(x, y, z)]</param>
        /// <returns>True if the request was successfully submitted.</returns>
        public bool RequestMoveVelocity(Vector3 velocity)
        {
            // Initialize request.
            TargetRef = 0;
            TargetPos = velocity;
            TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
            TargetReplan = false;
            TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY;

            return true;
        }
        /// <summary>
        /// Request move target replan
        /// </summary>
        /// <param name="targetPoly">Target reference</param>
        /// <param name="targetPosition">Target position</param>
        public bool RequestMoveTargetReplan(int targetPoly, Vector3 targetPosition)
        {
            // Initialize request.
            TargetRef = targetPoly;
            TargetPos = targetPosition;
            TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
            TargetReplan = true;
            TargetState = TargetRef > 0 ? MoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING : MoveRequestState.DT_CROWDAGENT_TARGET_FAILED;

            return true;
        }
        /// <summary>
        /// Resets any request for the specified agent.
        /// </summary>
        /// <returns>True if the request was successfully reseted.</returns>
        public bool ResetMoveTarget()
        {
            // Initialize request.
            TargetRef = 0;
            TargetPos = Vector3.Zero;
            DVel = Vector3.Zero;
            TargetPathqRef = PathQueue.DT_PATHQ_INVALID;
            TargetReplan = false;
            TargetState = MoveRequestState.DT_CROWDAGENT_TARGET_NONE;

            return true;
        }

        /// <summary>
        /// The known neighbors of the agent.
        /// </summary>
        public IEnumerable<CrowdNeighbour> GetNeighbours()
        {
            return [.. neighbours];
        }
        /// <summary>
        /// Adds new neighbour to list, based on distance (nearest first)
        /// </summary>
        /// <param name="ag">Agent</param>
        /// <param name="dist">Distance</param>
        private void AddNeighbour(CrowdAgent ag, float dist)
        {
            neighbours.Add(new()
            {
                Agent = ag,
                Dist = dist,
            });

            if (neighbours.Count > 1)
            {
                neighbours.Sort((n1, n2) => n1.Dist.CompareTo(n2.Dist));
            }
        }
        /// <summary>
        /// Clears the neighbour list
        /// </summary>
        private void ClearNeighbours()
        {
            neighbours.Clear();
        }
        /// <summary>
        /// Updates the agent's neighbor list
        /// </summary>
        /// <param name="grid">Proximity grid</param>
        public void UpdateNeighbours(ProximityGrid<CrowdAgent> grid)
        {
            var pos = NPos;
            float height = Params.Height;
            float range = Params.CollisionQueryRange;

            ClearNeighbours();

            var currAgent = this;
            var queryAgents = grid.QueryItems(pos, range).Where(a => a != currAgent);

            foreach (var ag in queryAgents)
            {
                // Check for overlap.
                var diff = pos - ag.NPos;
                if (MathF.Abs(diff.Y) >= (height + ag.Params.Height) / 2.0f)
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
                AddNeighbour(ag, distSqr);
            }
        }

        /// <summary>
        /// Integrates the crowd agent in time
        /// </summary>
        /// <param name="dt">Time</param>
        public void Integrate(float dt)
        {
            // Fake dynamic constraint.
            float maxDelta = Params.MaxAcceleration * dt;
            Vector3 dv = NVel - Vel;
            float ds = dv.Length();
            if (ds > maxDelta)
            {
                dv *= maxDelta / ds;
            }
            Vel += dv;

            // Integrate
            if (Vel.Length() > 0.0001f)
            {
                NPos += Vel * dt;
            }
            else
            {
                Vel = Vector3.Zero;
            }
        }

        /// <summary>
        /// Gets whether the crowd agent is over the off-mesh connection or not
        /// </summary>
        /// <param name="radius">Search radius</param>
        public bool OverOffmeshConnection(float radius)
        {
            if (Corners.Count <= 0)
            {
                return false;
            }

            bool offMeshConnection = Corners.EndFlags.HasFlag(StraightPathFlagTypes.DT_STRAIGHTPATH_OFFMESH_CONNECTION);
            if (offMeshConnection)
            {
                float distSq = Utils.DistanceSqr2D(NPos, Corners.EndPath);
                if (distSq < radius * radius)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Find next corner
        /// </summary>
        /// <param name="query">Agent query</param>
        /// <param name="filter">Query filter</param>
        /// <param name="d">Debug info</param>
        public void FindNextCorner(NavMeshQuery query, IGraphQueryFilter filter, CrowdAgentDebugInfo d)
        {
            // Find corners for steering
            var straightPath = Corridor.FindCorners(query, DT_CROWDAGENT_MAX_CORNERS);

            Corners = straightPath.Copy();

            // Check to see if the corner after the next corner is directly visible, and short cut to there.
            if (Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_OPTIMIZE_VIS) && Corners.Count > 0)
            {
                var target = Corners.GetPathPosition(Math.Min(1, Corners.Count - 1));
                Corridor.OptimizePathVisibility(query.GetAttachedNavMesh(), filter, target, Params.PathOptimizationRange);

                // Copy data for debug purposes.
                if (d != null)
                {
                    d.OptStart = Corridor.GetPos();
                    d.OptEnd = target;
                }
            }
            else
            {
                // Copy data for debug purposes.
                if (d != null)
                {
                    d.OptStart = Vector3.Zero;
                    d.OptEnd = Vector3.Zero;
                }
            }
        }
        /// <summary>
        /// Sets the agent state over off-mesh connection
        /// </summary>
        public void SetOverOffmeshConnection()
        {
            State = CrowdAgentState.DT_CROWDAGENT_STATE_OFFMESH;
            Corners.Clear();
            ClearNeighbours();
        }

        /// <summary>
        /// Calculate agent steering
        /// </summary>
        public void CalculateSteering()
        {
            var (dspeed, dvel) = CalculateDesiredVelocity();

            DesiredSpeed = dspeed;

            if (!Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_SEPARATION))
            {
                // Set the desired velocity.
                DVel = dvel;
            }

            // Separation
            var (w, disp) = CalcDisplacement();
            if (w > 0.0001f)
            {
                // Adjust desired velocity.
                dvel += disp * (1.0f / w);

                // Clamp desired velocity to desired speed.
                float speedSqr = dvel.LengthSquared();
                float desiredSqr = DesiredSpeed * DesiredSpeed;
                if (speedSqr > desiredSqr)
                {
                    dvel *= desiredSqr / speedSqr;
                }
            }

            // Set the desired velocity.
            DVel = dvel;
        }
        /// <summary>
        /// Calculates the desired velocity
        /// </summary>
        /// <returns>Returns the desired speed magnitude and velocity vector</returns>
        private (float dspeed, Vector3 dvel) CalculateDesiredVelocity()
        {
            float dspeed;
            Vector3 dvel;
            if (TargetState == MoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
            {
                dspeed = TargetPos.Length();
                dvel = TargetPos;
            }
            else
            {
                // Calculate steering direction.
                if (Params.UpdateFlags.HasFlag(UpdateFlagTypes.DT_CROWD_ANTICIPATE_TURNS))
                {
                    dvel = CalcSmoothSteerDirection(Corners, NPos);
                }
                else
                {
                    dvel = CalcStraightSteerDirection(Corners, NPos);
                }

                // Calculate speed scale, which tells the agent to slowdown at the end of the path.
                float speedScale = GetDistanceToGoal(Corners, NPos, Params);

                dspeed = Params.MaxSpeed;
                dvel *= dspeed * speedScale;
            }

            return (dspeed, dvel);
        }
        /// <summary>
        /// Calculates the smooth steer direction
        /// </summary>
        /// <param name="corners">Local path corridor corners</param>
        /// <param name="pos">Current position</param>
        private static Vector3 CalcSmoothSteerDirection(StraightPath corners, Vector3 pos)
        {
            if (corners.Count <= 0)
            {
                return Vector3.Zero;
            }

            int ip0 = 0;
            int ip1 = Math.Min(1, corners.Count - 1);
            Vector3 p0 = corners.GetPathPosition(ip0);
            Vector3 p1 = corners.GetPathPosition(ip1);

            Vector3 dir0 = p0 - pos;
            Vector3 dir1 = p1 - pos;
            dir0.Y = 0;
            dir1.Y = 0;

            float len0 = dir0.Length();
            float len1 = dir1.Length();
            if (len1 > 0.001f)
            {
                dir1 *= 1.0f / len1;
            }

            float x = dir0.X - dir1.X * len0 * 0.5f;
            float y = 0;
            float z = dir0.Z - dir1.Z * len0 * 0.5f;

            return Vector3.Normalize(new(x, y, z));
        }
        /// <summary>
        /// Calculates the straigh steer direction
        /// </summary>
        /// <param name="corners">Local path corridor corners</param>
        /// <param name="pos">Current position</param>
        private static Vector3 CalcStraightSteerDirection(StraightPath corners, Vector3 pos)
        {
            if (corners.Count <= 0)
            {
                return Vector3.Zero;
            }

            var dir = corners.StartPath - pos;
            dir.Y = 0;

            return Vector3.Normalize(dir);
        }
        /// <summary>
        /// Returns the distance to goal
        /// </summary>
        /// <param name="corners">Local path corridor corners</param>
        /// <param name="pos">Current position</param>
        /// <param name="parameters">Agent parameters</param>
        private static float GetDistanceToGoal(StraightPath corners, Vector3 pos, CrowdAgentParameters parameters)
        {
            float slowDownRadius = parameters.Radius * parameters.SlowDownRadiusFactor;
            if (slowDownRadius <= 0)
            {
                //Speed scale is 1
                return 1;
            }

            if (corners.Count <= 0)
            {
                return slowDownRadius;
            }

            bool endOfPath = corners.EndFlags.HasFlag(StraightPathFlagTypes.DT_STRAIGHTPATH_END);
            if (endOfPath)
            {
                return MathF.Min(Utils.Distance2D(pos, corners.EndPath), slowDownRadius);
            }

            return slowDownRadius;
        }
        /// <summary>
        /// Calculates the displacement to separate current agent from neighbours
        /// </summary>
        /// <returns>Returns the displacemente weight and the displacement vector</returns>
        private (float w, Vector3 disp) CalcDisplacement()
        {
            float separationDist = Params.CollisionQueryRange;
            float separationDistSqr = separationDist * separationDist;
            float invSeparationDist = 1.0f / separationDist;
            float separationWeight = Params.SeparationWeight;

            float w = 0;
            Vector3 disp = Vector3.Zero;

            var crowdNeiAgents = neighbours
                .Select(crowdNei => crowdNei.Agent)
                .ToArray();

            foreach (var nei in crowdNeiAgents)
            {
                var diff = NPos - nei.NPos;
                diff.Y = 0;

                float distSqr = diff.LengthSquared();
                if (distSqr < 0.00001f)
                {
                    continue;
                }
                if (distSqr > separationDistSqr)
                {
                    continue;
                }
                float dist = MathF.Sqrt(distSqr);
                float dDiv = dist * invSeparationDist;
                float weight = separationWeight * (1.0f - (dDiv * dDiv));

                disp += diff * (weight / dist);
                w += 1.0f;
            }

            return (w, disp);
        }
    }
}
