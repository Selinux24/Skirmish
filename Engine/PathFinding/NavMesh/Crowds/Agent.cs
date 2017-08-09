using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// A crowd agent is a unit that moves across the navigation mesh
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// The maximum number of corners a crowd agent will look ahead in the path
        /// </summary>
        private const int AgentMaxCorners = 4;
        /// <summary>
        /// Collision resolve factor
        /// </summary>
        private const float CollisionResolveFactor = 0.7f;

        /// <summary>
        /// Find the crowd agent's distance to its goal
        /// </summary>
        /// <param name="agent">Thw crowd agent</param>
        /// <param name="range">The maximum range</param>
        /// <returns>Distance to goal</returns>
        private static float GetDistanceToGoal(Agent agent, float range)
        {
            if (agent.Corners.Count == 0)
            {
                return range;
            }

            bool endOfPath = ((agent.Corners[agent.Corners.Count - 1].Flags & StraightPathFlags.End) != 0) ? true : false;
            if (endOfPath)
            {
                return Math.Min(Helper.Distance2D(agent.Position, agent.Corners[agent.Corners.Count - 1].Point.Position), range);
            }

            return range;
        }
        /// <summary>
        /// Calculate a vector based off of the map
        /// </summary>
        /// <param name="agent">The agent</param>
        /// <param name="dir">The resulting steer direction</param>
        private static void CalcSmoothSteerDirection(Agent agent, ref Vector3 dir)
        {
            if (agent.Corners.Count == 0)
            {
                dir = Vector3.Zero;
                return;
            }

            int ip0 = 0;
            int ip1 = Math.Min(1, agent.Corners.Count - 1);
            Vector3 p0 = agent.Corners[ip0].Point.Position;
            Vector3 p1 = agent.Corners[ip1].Point.Position;

            Vector3 dir0 = p0 - agent.Position;
            Vector3 dir1 = p1 - agent.Position;
            dir0.Y = 0;
            dir1.Y = 0;

            float len0 = dir0.Length();
            float len1 = dir1.Length();
            if (len1 > 0.001f)
            {
                dir1 = dir1 * 1.0f / len1;
            }

            dir.X = dir0.X - dir1.X * len0 * 0.5f;
            dir.Y = 0;
            dir.Z = dir0.Z - dir1.Z * len0 * 0.5f;

            dir.Normalize();
        }
        /// <summary>
        /// Calculate a straight vector to the destination
        /// </summary>
        /// <param name="agent">The agent</param>
        /// <param name="dir">The resulting steer direction</param>
        private static void CalcStraightSteerDirection(Agent agent, ref Vector3 dir)
        {
            if (agent.Corners.Count == 0)
            {
                dir = Vector3.Zero;
                return;
            }

            dir = agent.Corners[0].Point.Position - agent.Position;
            dir.Y = 0;
            dir.Normalize();
        }
        /// <summary>
        /// Get the crowd agent's neighbors.
        /// </summary>
        /// <param name="skip">The current crowd agent</param>
        /// <param name="neighborAgents">Candidate neighbor agents</param>
        /// <param name="neighborAgentsCount">Candidate count</param>
        /// <returns>The neighbors array</returns>
        private static CrowdNeighbor[] GetNeighbors(Agent skip, Agent[] neighborAgents, int neighborAgentsCount)
        {
            List<CrowdNeighbor> result = new List<CrowdNeighbor>();

            Vector3 position = skip.Position;
            float height = skip.Parameters.Height;

            for (int i = 0; i < neighborAgentsCount; i++)
            {
                var agent = neighborAgents[i];
                if (agent == skip)
                {
                    continue;
                }

                //check for overlap
                Vector3 diff = position - agent.Position;
                if (Math.Abs(diff.Y) >= (height + agent.Parameters.Height) / 2.0f)
                {
                    continue;
                }

                diff.Y = 0;

                float distSqr = diff.LengthSquared();

                result.Add(new CrowdNeighbor()
                {
                    Neighbor = agent,
                    Distance = distSqr,
                });
            }

            if (result.Count > 1)
            {
                result.Sort((a1, a2) =>
                {
                    return a1.Distance.CompareTo(a2.Distance);
                });
            }

            return result.ToArray();
        }

        public float TopologyOptTime { get; set; }
        public float DesiredSpeed { get; set; }

        public Vector3 Disp { get; set; }
        public Vector3 DesiredVelocity { get; set; }
        public Vector3 NVel { get; set; }
        public Vector3 Velocity { get; set; }

        public AgentParams Parameters { get; private set; }
        public StraightPath Corners { get; set; }
        public PolyId TargetReference { get; set; }
        public int TargetPathQueryIndex { get; set; }
        public bool TargetReplan { get; set; }
        public float TargetReplanTime { get; set; }

        public bool IsActive { get; set; }
        public bool IsPartial { get; set; }
        public AgentState State { get; set; }
        public Vector3 Position { get; set; }
        internal LocalBoundary Boundary { get; private set; }
        internal PathCorridor Corridor { get; private set; }
        public CrowdNeighbor[] Neighbors { get; private set; }
        public TargetState TargetState { get; set; }
        public Vector3 TargetPosition { get; set; }
        public AgentAnimation Animation { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters">Parameters</param>
        public Agent(AgentParams parameters)
        {
            this.IsActive = false;
            this.Corridor = new PathCorridor();
            this.Boundary = new LocalBoundary();
            this.Neighbors = new CrowdNeighbor[] { };
            this.Corners = new StraightPath();
            this.Parameters = parameters;
            this.Animation = new AgentAnimation();
        }

        /// <summary>
        /// Update the position after a certain time 'dt'
        /// </summary>
        /// <param name="dt">Time that passed</param>
        public void Integrate(float dt)
        {
            //fake dyanmic constraint
            float maxDelta = this.Parameters.MaxAcceleration * dt;
            Vector3 dv = this.NVel - this.Velocity;
            float ds = dv.Length();
            if (ds > maxDelta)
            {
                dv = dv * (maxDelta / ds);
            }
            this.Velocity += dv;

            //integrate
            if (this.Velocity.Length() > 0.0001f)
            {
                this.Position += (this.Velocity * dt);
            }
            else
            {
                this.Velocity = Vector3.Zero;
            }
        }

        public void Reset(PolyId reference, Vector3 nearest)
        {
            this.Corridor.Reset(reference, nearest);
            this.Boundary.Reset();
            this.IsPartial = false;

            this.TopologyOptTime = 0;
            this.TargetReplanTime = 0;

            this.DesiredVelocity = Vector3.Zero;
            this.NVel = Vector3.Zero;
            this.Velocity = Vector3.Zero;
            this.Position = nearest;

            this.DesiredSpeed = 0;

            if (reference != PolyId.Null)
            {
                this.State = AgentState.Walking;
            }
            else
            {
                this.State = AgentState.Invalid;
            }

            this.TargetState = TargetState.None;
        }
        /// <summary>
        /// Change the move target
        /// </summary>
        public void RequestMoveTargetReplan()
        {
            //initialize request
            this.TargetPathQueryIndex = PathQueue.Invalid;
            this.TargetReplan = true;
            if (this.TargetReference != PolyId.Null)
            {
                this.TargetState = TargetState.Requesting;
            }
            else
            {
                this.TargetState = TargetState.Failed;
            }
        }
        /// <summary>
        /// Request a new move target
        /// </summary>
        /// <param name="reference">The polygon reference</param>
        /// <param name="pos">The target's coordinates</param>
        /// <returns>True if request met, false if not</returns>
        public bool RequestMoveTarget(PolyId reference, Vector3 pos)
        {
            if (reference == PolyId.Null)
            {
                return false;
            }

            //initialize request
            this.TargetReference = reference;
            this.TargetPosition = pos;
            this.TargetPathQueryIndex = PathQueue.Invalid;
            this.TargetReplan = false;
            if (this.TargetReference != PolyId.Null)
            {
                this.TargetState = TargetState.Requesting;
            }
            else
            {
                this.TargetState = TargetState.Failed;
            }

            return true;
        }
        /// <summary>
        /// Sets the agent to invalid state
        /// </summary>
        /// <param name="position">Position</param>
        public void SetInvalidState(Vector3 position)
        {
            this.Corridor.Reset(PolyId.Null, position);
            this.IsPartial = false;
            this.Boundary.Reset();
            this.State = AgentState.Invalid;
        }
        /// <summary>
        /// Set agent target
        /// </summary>
        /// <param name="reference">Polygon reference</param>
        /// <param name="position">Position</param>
        public void SetTarget(PolyId reference, Vector3 position)
        {
            this.TargetReference = reference;
            this.TargetPosition = position;
        }

        public void ResetTarget(PolyId reference, Vector3 position)
        {
            this.Corridor.Reset(reference, position);
            this.IsPartial = false;
            this.TargetState = TargetState.None;
        }
        /// <summary>
        /// Sets new safe position for agent
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="position"></param>
        public void Reposition(PolyId reference, Vector3 position)
        {
            this.Corridor.FixPathStart(reference, position);
            this.Boundary.Reset();
            this.Position = position;
        }

        internal void MovePosition(NavigationMeshQuery navQuery)
        {
            //move along navmesh
            this.Corridor.MovePosition(this.Position, navQuery);

            //get valid constrained position back
            this.Position = this.Corridor.Pos;

            //if not using path, truncate the corridor to just one poly
            if (this.TargetState == TargetState.None ||
                this.TargetState == TargetState.Velocity)
            {
                this.Corridor.Reset(this.Corridor.GetFirstPoly(), this.Position);
                this.IsPartial = false;
            }
        }

        internal void UpdateCollision(NavigationMeshQuery navQuery)
        {
            if (Helper.Distance2D(this.Position, this.Boundary.Center) > this.Parameters.UpdateThreshold || !this.Boundary.IsValid(navQuery))
            {
                this.Boundary.Update(this.Corridor.GetFirstPoly(), this.Position, this.Parameters.CollisionQueryRange, navQuery);
            }
        }

        public void SetNeighbors(Agent[] neighborAgents, int count)
        {
            this.Neighbors = GetNeighbors(this, neighborAgents, count);
        }

        public void TriggerOffmeshConnection()
        {
            this.State = AgentState.Offmesh;
            this.Corners.Clear();
        }

        internal void Steer1(NavigationMeshQuery navQuery)
        {
            //find corners for steering
            this.Corners = this.Corridor.FindCorners(navQuery);

            //check to see if the corner after the next corner is directly visible 
            if ((this.Parameters.UpdateFlags & UpdateFlags.OptimizeVis) != 0 && this.Corners.Count > 0)
            {
                Vector3 target = this.Corners[Math.Min(1, this.Corners.Count - 1)].Point.Position;
                this.Corridor.OptimizePathVisibility(target, this.Parameters.PathOptimizationRange, navQuery);
            }
        }

        public void Steer2(List<Agent> agents)
        {
            Vector3 dvel = Vector3.Zero;

            if (this.TargetState == TargetState.Velocity)
            {
                dvel = this.TargetPosition;
                this.DesiredSpeed = this.TargetPosition.Length();
            }
            else
            {
                //calculate steering direction
                if ((this.Parameters.UpdateFlags & UpdateFlags.AnticipateTurns) != 0)
                {
                    CalcSmoothSteerDirection(this, ref dvel);
                }
                else
                {
                    CalcStraightSteerDirection(this, ref dvel);
                }

                //calculate speed scale, which tells the agent to slowdown at the end of the path
                float slowDownRadius = this.Parameters.Radius * 2;
                float speedScale = GetDistanceToGoal(this, slowDownRadius) / slowDownRadius;

                this.DesiredSpeed = this.Parameters.MaxSpeed;
                dvel = dvel * (this.DesiredSpeed * speedScale);
            }

            //separation
            if ((this.Parameters.UpdateFlags & UpdateFlags.Separation) != 0)
            {
                float separationDist = this.Parameters.CollisionQueryRange;
                float invSeparationDist = 1.0f / separationDist;
                float separationWeight = this.Parameters.SeparationWeight;

                float w = 0;
                Vector3 disp = Vector3.Zero;

                for (int j = 0; j < this.Neighbors.Length; j++)
                {
                    var n = this.Neighbors[j].Neighbor;

                    Vector3 diff = this.Position - n.Position;
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
                    float weight = separationWeight * (1.0f - (dist * invSeparationDist) * (dist * invSeparationDist));

                    disp = disp + diff * (weight / dist);
                    w += 1.0f;
                }

                if (w > 0.0001f)
                {
                    //adjust desired veloctiy
                    dvel = dvel + disp * (1.0f / w);

                    //clamp desired velocity to desired speed
                    float speedSqr = dvel.LengthSquared();
                    float desiredSqr = this.DesiredSpeed * this.DesiredSpeed;
                    if (speedSqr > desiredSqr)
                    {
                        dvel = dvel * (desiredSqr / speedSqr);
                    }
                }
            }

            //set the desired velocity
            this.DesiredVelocity = dvel;
        }

        public void VelocityPlanning(ObstacleAvoidanceQuery obstacleQuery)
        {
            if ((this.Parameters.UpdateFlags & UpdateFlags.ObstacleAvoidance) != 0)
            {
                obstacleQuery.Reset();

                //add neighhbors as obstacles
                for (int j = 0; j < this.Neighbors.Length; j++)
                {
                    var n = this.Neighbors[j].Neighbor;
                    obstacleQuery.AddCircle(n.Position, n.Parameters.Radius, n.Velocity, n.DesiredVelocity);
                }

                //append neighbor segments as obstacles
                for (int j = 0; j < this.Boundary.SegCount; j++)
                {
                    Segment s = this.Boundary.Segs[j];
                    if (Helper.Area2D(this.Position, s.Start, s.End) < 0.0f)
                    {
                        continue;
                    }
                    obstacleQuery.AddSegment(s.Start, s.End);
                }

                //sample new safe velocity
                bool adaptive = true;
                int ns = 0;
                var parameters = obstacleQuery.GetParams(this.Parameters.ObstacleAvoidanceType);
                Vector3 nVel;
                if (adaptive)
                {
                    ns = obstacleQuery.SampleVelocityAdaptive(this.Position, this.Parameters.Radius, this.DesiredSpeed, this.Velocity, this.DesiredVelocity, parameters, out nVel);
                }
                else
                {
                    ns = obstacleQuery.SampleVelocityGrid(this.Position, this.Parameters.Radius, this.DesiredSpeed, this.Velocity, this.DesiredVelocity, parameters, out nVel);
                }
                this.NVel = nVel;
            }
            else
            {
                //if not using velocity planning, new velocity is directly the desired velocity
                this.NVel = this.DesiredVelocity;
            }
        }

        public void HandleCollisions(List<Agent> agents)
        {
            int idx0 = agents.IndexOf(this);

            this.Disp = Vector3.Zero;

            float w = 0;

            for (int i = 0; i < this.Neighbors.Length; i++)
            {
                Agent neighbor = this.Neighbors[i].Neighbor;
                int idx1 = agents.IndexOf(neighbor);

                Vector3 diff = this.Position - neighbor.Position;
                diff.Y = 0;

                float dist = diff.LengthSquared();
                if (dist > (this.Parameters.Radius + neighbor.Parameters.Radius) * (this.Parameters.Radius + neighbor.Parameters.Radius))
                {
                    continue;
                }

                dist = (float)Math.Sqrt(dist);
                float pen = (this.Parameters.Radius + neighbor.Parameters.Radius) - dist;
                if (dist < 0.0001f)
                {
                    //agents on top of each other, try to choose diverging separation directions
                    if (idx0 > idx1)
                    {
                        diff = new Vector3(-this.DesiredVelocity.Z, 0, this.DesiredVelocity.X);
                    }
                    else
                    {
                        diff = new Vector3(this.DesiredVelocity.Z, 0, -this.DesiredVelocity.X);
                    }
                    pen = 0.01f;
                }
                else
                {
                    pen = (1.0f / dist) * (pen * 0.5f) * CollisionResolveFactor;
                }

                this.Disp = this.Disp + diff * pen;

                w += 1.0f;
            }

            if (w > 0.0001f)
            {
                float iw = 1.0f / w;
                this.Disp = this.Disp * iw;
            }
        }
    }
}
