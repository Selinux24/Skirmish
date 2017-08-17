using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// A crowd agent is a unit that moves across the navigation mesh
    /// </summary>
    class Agent
    {
        /// <summary>
        /// The maximum number of corners a crowd agent will look ahead in the path
        /// </summary>
        private const int AgentMaxCorners = 4;
        /// <summary>
        /// Collision resolve factor
        /// </summary>
        private const float CollisionResolveFactor = 0.7f;

        private const int CheckLookAhead = 10;
        /// <summary>
        /// Target replan delay in seconds
        /// </summary>
        private const float TargetReplanDelay = 1.0f;

        private const int MaximumIterations = 20;
        /// <summary>
        /// Topology optimization threshold in seconds
        /// </summary>
        private const float OptimizationTimeThresshold = 0.5f;

        /// <summary>
        /// Find the crowd agent's distance to its goal
        /// </summary>
        /// <param name="agent">Thw crowd agent</param>
        /// <param name="range">The maximum range</param>
        /// <returns>Distance to goal</returns>
        private static float GetDistanceToGoal(Agent agent, float range)
        {
            if (agent.corners.Count == 0)
            {
                return range;
            }

            bool endOfPath = ((agent.corners[agent.corners.Count - 1].Flags & StraightPathFlags.End) != 0) ? true : false;
            if (endOfPath)
            {
                return Math.Min(Helper.Distance2D(agent.Position, agent.corners[agent.corners.Count - 1].Point.Position), range);
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
            if (agent.corners.Count == 0)
            {
                dir = Vector3.Zero;
                return;
            }

            int ip0 = 0;
            int ip1 = Math.Min(1, agent.corners.Count - 1);
            Vector3 p0 = agent.corners[ip0].Point.Position;
            Vector3 p1 = agent.corners[ip1].Point.Position;

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
            if (agent.corners.Count == 0)
            {
                dir = Vector3.Zero;
                return;
            }

            dir = agent.corners[0].Point.Position - agent.Position;
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
        /// <summary>
        /// Gets if the specified agent is over an Offmesh connection
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns true if the agent is over an Offmesh connection</returns>
        private static bool OverOffmeshConnection(Agent agent)
        {
            if (agent.corners.Count == 0)
            {
                return false;
            }

            bool offmeshConnection = (agent.corners[agent.corners.Count - 1].Flags & StraightPathFlags.OffMeshConnection) != 0;
            if (offmeshConnection)
            {
                float dist = Helper.Distance2D(agent.Position, agent.corners[agent.corners.Count - 1].Point.Position);
                float radius = agent.Parameters.TriggerRadius;
                if (dist * dist < radius * radius)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Add the CrowdAgent to the path queue
        /// </summary>
        /// <param name="agent">The new CrowdAgent</param>
        /// <param name="agents">The current CrowdAgent array</param>
        /// <param name="numAgents">The number of CrowdAgents</param>
        /// <param name="maxAgents">The maximum number of agents allowed</param>
        /// <returns>An updated agent count</returns>
        private static int AddToPathQueue(Agent agent, Agent[] agents, int numAgents, int maxAgents)
        {
            //insert neighbor based on greatest time
            int slot = 0;
            if (numAgents == 0)
            {
                slot = numAgents;
            }
            else if (agent.targetReplanTime <= agents[numAgents - 1].targetReplanTime)
            {
                if (numAgents >= maxAgents)
                {
                    return numAgents;
                }
                slot = numAgents;
            }
            else
            {
                int i;
                for (i = 0; i < numAgents; i++)
                {
                    if (agent.targetReplanTime >= agents[i].targetReplanTime)
                    {
                        break;
                    }
                }

                int tgt = i + 1;
                int n = Math.Min(numAgents - i, maxAgents - tgt);

                if (n > 0)
                {
                    for (int j = 0; j < n; j++)
                    {
                        agents[tgt + j] = agents[i + j];
                    }
                }

                slot = i;
            }

            agents[slot] = agent;

            return Math.Min(numAgents + 1, maxAgents);
        }
        /// <summary>
        /// Add the CrowdAgent to the optimization queue
        /// </summary>
        /// <param name="agent">The new CrowdAgent</param>
        /// <param name="agents">The current CrowdAgent array</param>
        /// <param name="numAgents">The number of CrowdAgents</param>
        /// <param name="maxAgents">The maximum number of agents allowed</param>
        /// <returns>An updated agent count</returns>
        private static int AddToOptQueue(Agent agent, Agent[] agents, int numAgents, int maxAgents)
        {
            //insert neighbor based on greatest time
            int slot = 0;
            if (numAgents == 0)
            {
                slot = numAgents;
            }
            else if (agent.topologyOptTime <= agents[numAgents - 1].topologyOptTime)
            {
                if (numAgents >= maxAgents)
                {
                    return numAgents;
                }
                slot = numAgents;
            }
            else
            {
                int i;
                for (i = 0; i < numAgents; i++)
                {
                    if (agent.topologyOptTime >= agents[i].topologyOptTime)
                    {
                        break;
                    }
                }

                int tgt = i + 1;
                int n = Math.Min(numAgents - i, maxAgents - tgt);

                if (n > 0)
                {
                    for (int j = 0; j < n; j++)
                    {
                        agents[tgt + j] = agents[i + j];
                    }
                }

                slot = i;
            }

            agents[slot] = agent;

            return Math.Min(numAgents + 1, maxAgents);
        }

        private LocalBoundary boundary = new LocalBoundary();
        private PathCorridor corridor = new PathCorridor();
        private StraightPath corners = new StraightPath();
        private CrowdNeighbor[] neighbors = new CrowdNeighbor[] { };
        private AgentAnimation animation = new AgentAnimation();
        private float topologyOptTime;

        private PolyId targetReference;
        private int targetPathQueryIndex;
        private bool targetReplan;
        private float targetReplanTime;

        private float desiredSpeed;
        private Vector3 disp;
        private Vector3 desiredVelocity;
        private Vector3 nVelocity;
        private Vector3 velocity;

        /// <summary>
        /// Base crowd
        /// </summary>
        protected Crowd Crowd = null;

        /// <summary>
        /// Gets the agent parameters
        /// </summary>
        public AgentParams Parameters { get; private set; }
        /// <summary>
        /// Gets if the agent is active
        /// </summary>
        public bool IsActive { get; private set; }
        /// <summary>
        /// Gets if the current path is partial
        /// </summary>
        public bool IsPartial { get; private set; }
        /// <summary>
        /// Gets the agent state
        /// </summary>
        public AgentState State { get; private set; }
        /// <summary>
        /// Gets or sets the agent current position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Gets the target state
        /// </summary>
        public TargetState TargetState { get; private set; }
        /// <summary>
        /// Gets the target position
        /// </summary>
        public Vector3 TargetPosition { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="parameters">Parameters</param>
        public Agent(Crowd crowd, AgentParams parameters)
        {
            this.Crowd = crowd;
            this.Parameters = parameters;

            this.IsActive = false;
        }

        /// <summary>
        /// Update the position after a certain time 'timeDelta'
        /// </summary>
        /// <param name="timeDelta">Time delta</param>
        public void Integrate(float timeDelta)
        {
            //fake dyanmic constraint
            float maxDelta = this.Parameters.MaxAcceleration * timeDelta;
            Vector3 dv = this.nVelocity - this.velocity;
            float ds = dv.Length();
            if (ds > maxDelta)
            {
                dv = dv * (maxDelta / ds);
            }
            this.velocity += dv;

            //integrate
            if (this.velocity.Length() > 0.0001f)
            {
                this.Position += (this.velocity * timeDelta);
            }
            else
            {
                this.velocity = Vector3.Zero;
            }
        }


        public void ResetToPosition(PolyId reference, Vector3 nearest)
        {
            this.corridor.Reset(reference, nearest);
            this.boundary.Reset();
            this.IsPartial = false;
            this.IsActive = true;
            this.Position = nearest;

            this.topologyOptTime = 0;
            this.targetReplanTime = 0;

            this.desiredVelocity = Vector3.Zero;
            this.nVelocity = Vector3.Zero;
            this.velocity = Vector3.Zero;
            this.desiredSpeed = 0;

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
        /// Checks plan validity
        /// </summary>
        /// <param name="timeDelta">Delta time</param>
        public void CheckPlan(float timeDelta)
        {
            this.targetReplanTime += timeDelta;

            bool replan = false;

            //first check that the current location is valid
            var agentRef = this.corridor.FirstPoly;
            var agentPos = this.Position;
            if (!this.Crowd.NavQuery.IsValidPolyRef(agentRef))
            {
                //current location is not valid, try to reposition
                Vector3 nearest = agentPos;
                agentRef = PolyId.Null;
                PathPoint nearestPt;
                if (this.Crowd.NavQuery.FindNearestPoly(this.Position, this.Crowd.Extents, out nearestPt))
                {
                    nearest = nearestPt.Position;
                    agentRef = nearestPt.Polygon;
                    agentPos = nearestPt.Position;

                    if (agentRef == PolyId.Null)
                    {
                        //could not find location in navmesh, set state to invalid
                        this.corridor.Reset(PolyId.Null, agentPos);
                        this.boundary.Reset();
                        this.IsPartial = false;
                        this.State = AgentState.Invalid;

                        return;
                    }

                    //make sure the first polygon is valid
                    this.corridor.FixPathStart(agentRef, agentPos);
                    this.boundary.Reset();
                    this.Position = agentPos;
                    replan = true;
                }
            }

            //try to recover move request position
            if (this.TargetState != TargetState.None &&
                this.TargetState != TargetState.Failed)
            {
                if (!this.Crowd.NavQuery.IsValidPolyRef(this.targetReference))
                {
                    //current target is not valid, try to reposition
                    Vector3 nearest = this.TargetPosition;
                    this.targetReference = PolyId.Null;
                    PathPoint nearestPt;
                    if (this.Crowd.NavQuery.FindNearestPoly(this.TargetPosition, this.Crowd.Extents, out nearestPt))
                    {
                        nearest = nearestPt.Position;

                        this.targetReference = nearestPt.Polygon;
                        this.TargetPosition = nearestPt.Position;

                        replan = true;
                    }
                }

                if (this.targetReference == PolyId.Null)
                {
                    //failed to reposition target
                    this.corridor.Reset(agentRef, agentPos);
                    this.IsPartial = false;
                    this.TargetState = TargetState.None;
                }
            }

            //if nearby corridor is not valid, replan
            if (!this.corridor.IsValid(CheckLookAhead, this.Crowd.NavQuery))
            {
                replan = true;
            }

            //if the end of the path is near and it is not the request location, replan
            if (this.TargetState == TargetState.Valid)
            {
                if (this.targetReplanTime > TargetReplanDelay &&
                    this.corridor.NavigationPath.Count < CheckLookAhead &&
                    this.corridor.LastPoly != this.targetReference)
                {
                    replan = true;
                }
            }

            //try to replan path to goal
            if (replan && this.TargetState != TargetState.None)
            {
                //initialize request
                this.targetPathQueryIndex = PathQueue.Invalid;
                this.targetReplan = true;
                if (this.targetReference != PolyId.Null)
                {
                    this.TargetState = TargetState.Requesting;
                }
                else
                {
                    this.TargetState = TargetState.Failed;
                }
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
            this.targetReference = reference;
            this.TargetPosition = pos;
            this.targetPathQueryIndex = PathQueue.Invalid;
            this.targetReplan = false;
            if (this.targetReference != PolyId.Null)
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
        /// Move agent to position
        /// </summary>
        public void MovePosition()
        {
            //move along navmesh
            this.corridor.MovePosition(this.Position, this.Crowd.NavQuery);

            //get valid constrained position back
            this.Position = this.corridor.Position;

            //if not using path, truncate the corridor to just one poly
            if (this.TargetState == TargetState.None ||
                this.TargetState == TargetState.Velocity)
            {
                this.corridor.Reset(this.corridor.FirstPoly, this.Position);
                this.IsPartial = false;
            }
        }


        public void UpdateCollision()
        {
            if (Helper.Distance2D(this.Position, this.boundary.Center) > this.Parameters.UpdateThreshold || !this.boundary.IsValid(this.Crowd.NavQuery))
            {
                this.boundary.Update(this.corridor.FirstPoly, this.Position, this.Parameters.CollisionQueryRange, this.Crowd.NavQuery);
            }
        }

        public void SetNeighbors(Agent[] neighborAgents, int count)
        {
            this.neighbors = GetNeighbors(this, neighborAgents, count);
        }

        public bool TriggerOffmeshConnection()
        {
            if (OverOffmeshConnection(this))
            {
                //adjust the path over the off-mesh connection
                PolyId[] refs = new PolyId[2];
                var agentAnim = this.animation;
                if (this.corridor.MoveOverOffmeshConnection(this.corners[this.corners.Count - 1].Point.Polygon, refs, this.Crowd.NavQuery, out agentAnim.StartPos, out agentAnim.EndPos))
                {
                    agentAnim.InitPos = this.Position;
                    agentAnim.PolyRef = refs[1];
                    agentAnim.Active = true;
                    agentAnim.T = 0.0f;
                    agentAnim.TMax = (Helper.Distance2D(agentAnim.StartPos, agentAnim.EndPos) / this.Parameters.MaxSpeed) * 0.5f;

                    this.State = AgentState.Offmesh;
                    this.corners.Clear();

                    return true;
                }
            }

            return false;
        }

        public void Steer1()
        {
            //find corners for steering
            this.corners = this.corridor.FindCorners(this.Crowd.NavQuery);

            //check to see if the corner after the next corner is directly visible 
            if ((this.Parameters.UpdateFlags & UpdateFlags.OptimizeVis) != 0 && this.corners.Count > 0)
            {
                Vector3 target = this.corners[Math.Min(1, this.corners.Count - 1)].Point.Position;
                this.corridor.OptimizePathVisibility(target, this.Parameters.PathOptimizationRange, this.Crowd.NavQuery);
            }
        }

        public void Steer2(List<Agent> agents)
        {
            Vector3 dvel = Vector3.Zero;

            if (this.TargetState == TargetState.Velocity)
            {
                dvel = this.TargetPosition;
                this.desiredSpeed = this.TargetPosition.Length();
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

                this.desiredSpeed = this.Parameters.MaxSpeed;
                dvel = dvel * (this.desiredSpeed * speedScale);
            }

            //separation
            if ((this.Parameters.UpdateFlags & UpdateFlags.Separation) != 0)
            {
                float separationDist = this.Parameters.CollisionQueryRange;
                float invSeparationDist = 1.0f / separationDist;
                float separationWeight = this.Parameters.SeparationWeight;

                float w = 0;
                Vector3 disp = Vector3.Zero;

                for (int j = 0; j < this.neighbors.Length; j++)
                {
                    var n = this.neighbors[j].Neighbor;

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
                    float desiredSqr = this.desiredSpeed * this.desiredSpeed;
                    if (speedSqr > desiredSqr)
                    {
                        dvel = dvel * (desiredSqr / speedSqr);
                    }
                }
            }

            //set the desired velocity
            this.desiredVelocity = dvel;
        }

        public void VelocityPlanning()
        {
            if ((this.Parameters.UpdateFlags & UpdateFlags.ObstacleAvoidance) != 0)
            {
                this.Crowd.ObstacleQuery.Reset();

                //add neighhbors as obstacles
                for (int j = 0; j < this.neighbors.Length; j++)
                {
                    var n = this.neighbors[j].Neighbor;
                    this.Crowd.ObstacleQuery.AddCircle(n.Position, n.Parameters.Radius, n.velocity, n.desiredVelocity);
                }

                //append neighbor segments as obstacles
                for (int j = 0; j < this.boundary.SegmentCount; j++)
                {
                    Segment s = this.boundary.Segments[j];
                    if (Helper.Area2D(this.Position, s.Start, s.End) < 0.0f)
                    {
                        continue;
                    }
                    this.Crowd.ObstacleQuery.AddSegment(s.Start, s.End);
                }

                //sample new safe velocity
                bool adaptive = true;
                int ns = 0;
                var parameters = this.Crowd.ObstacleQuery.GetParams(this.Parameters.ObstacleAvoidanceType);
                Vector3 nVel;
                if (adaptive)
                {
                    ns = this.Crowd.ObstacleQuery.SampleVelocityAdaptive(this.Position, this.Parameters.Radius, this.desiredSpeed, this.velocity, this.desiredVelocity, parameters, out nVel);
                }
                else
                {
                    ns = this.Crowd.ObstacleQuery.SampleVelocityGrid(this.Position, this.Parameters.Radius, this.desiredSpeed, this.velocity, this.desiredVelocity, parameters, out nVel);
                }
                this.nVelocity = nVel;
            }
            else
            {
                //if not using velocity planning, new velocity is directly the desired velocity
                this.nVelocity = this.desiredVelocity;
            }
        }

        public void HandleCollisions(List<Agent> agents)
        {
            int idx0 = agents.IndexOf(this);

            this.disp = Vector3.Zero;

            float w = 0;

            for (int i = 0; i < this.neighbors.Length; i++)
            {
                Agent neighbor = this.neighbors[i].Neighbor;
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
                        diff = new Vector3(-this.desiredVelocity.Z, 0, this.desiredVelocity.X);
                    }
                    else
                    {
                        diff = new Vector3(this.desiredVelocity.Z, 0, -this.desiredVelocity.X);
                    }
                    pen = 0.01f;
                }
                else
                {
                    pen = (1.0f / dist) * (pen * 0.5f) * CollisionResolveFactor;
                }

                this.disp = this.disp + diff * pen;

                w += 1.0f;
            }

            if (w > 0.0001f)
            {
                float iw = 1.0f / w;
                this.disp = this.disp * iw;
            }
        }

        public void UpdateOffmeshConnections(float timeDelta)
        {
            var anim = this.animation;

            if (anim.Active)
            {
                anim.T += timeDelta;
                if (anim.T > anim.TMax)
                {
                    //reset animation
                    anim.Active = false;

                    //prepare agent for walking
                    this.State = AgentState.Walking;
                }
                else
                {
                    //update position
                    float ta = anim.TMax * 0.15f;
                    float tb = anim.TMax;
                    if (anim.T < ta)
                    {
                        float u = Helper.Normalize(anim.T, 0.0f, ta);
                        this.Position = Vector3.Lerp(anim.InitPos, anim.StartPos, u);
                    }
                    else
                    {
                        float u = Helper.Normalize(anim.T, ta, tb);
                        this.Position = Vector3.Lerp(anim.StartPos, anim.EndPos, u);
                    }

                    this.velocity = Vector3.Zero;
                    this.desiredVelocity = Vector3.Zero;
                }
            }
        }

        public void ResolveRequesting(Agent[] queue, ref int numQueue)
        {
            if (this.TargetState == TargetState.Requesting)
            {
                var path = this.corridor.NavigationPath;

                Vector3 reqPos = new Vector3();
                PolygonPath reqPath = new PolygonPath();

                //quick search towards the goal
                PathPoint startPoint = new PathPoint(path[0], this.Position);
                PathPoint endPoint = new PathPoint(this.targetReference, this.TargetPosition);
                this.Crowd.NavQuery.InitSlicedFindPath(startPoint, endPoint, this.Crowd.NavQueryFilter, FindPathOptions.None);
                int tempInt = 0;
                this.Crowd.NavQuery.UpdateSlicedFindPath(MaximumIterations, ref tempInt);
                var status = Status.Failure;
                if (this.targetReplan)
                {
                    //try to use an existing steady path during replan if possible
                    status = this.Crowd.NavQuery.FinalizedSlicedPathPartial(path, reqPath).ToStatus();
                }
                else
                {
                    //try to move towards the target when the goal changes
                    status = this.Crowd.NavQuery.FinalizeSlicedFindPath(reqPath).ToStatus();
                }

                if (status != Status.Failure && reqPath.Count > 0)
                {
                    //in progress or succeed
                    if (reqPath[reqPath.Count - 1] != this.targetReference)
                    {
                        //partial path, constrain target position in last polygon
                        bool tempBool;
                        status = this.Crowd.NavQuery.ClosestPointOnPoly(reqPath[reqPath.Count - 1], this.TargetPosition, out reqPos, out tempBool).ToStatus();
                        if (status == Status.Failure)
                        {
                            reqPath.Clear();
                        }
                    }
                    else
                    {
                        reqPos = this.TargetPosition;
                    }
                }
                else
                {
                    reqPath.Clear();
                }

                if (reqPath.Count == 0)
                {
                    //could not find path, start the request from the current location
                    reqPos = this.Position;
                    reqPath.Add(path[0]);
                }

                this.corridor.SetCorridor(reqPos, reqPath);
                this.boundary.Reset();
                this.IsPartial = false;

                if (reqPath[reqPath.Count - 1] == this.targetReference)
                {
                    this.TargetState = TargetState.Valid;
                    this.targetReplanTime = 0.0f;
                }
                else
                {
                    //the path is longer or potentially unreachable, full plan
                    this.TargetState = TargetState.WaitingForQueue;
                }
            }

            if (this.TargetState == TargetState.WaitingForQueue)
            {
                numQueue = AddToPathQueue(this, queue, numQueue, queue.Length);
            }
        }

        public void ResolveWaitingForPath(PathQueue pathQueue)
        {
            //poll path queue
            var status = pathQueue.GetRequestStatus(this.targetPathQueryIndex);
            if (status == Status.Failure)
            {
                //path find failed, retry if the target location is still valid
                this.targetPathQueryIndex = PathQueue.Invalid;
                if (this.targetReference != PolyId.Null)
                {
                    this.TargetState = TargetState.Requesting;
                }
                else
                {
                    this.TargetState = TargetState.Failed;
                }
                this.targetReplanTime = 0.0f;
            }
            else if (status == Status.Success)
            {
                PolygonPath path = this.corridor.NavigationPath;

                //apply results
                Vector3 targetPos = new Vector3();
                targetPos = this.TargetPosition;

                PolygonPath res;
                bool valid = true;
                status = pathQueue.GetPathResult(this.targetPathQueryIndex, out res).ToStatus();
                if (status == Status.Failure || res.Count == 0)
                {
                    valid = false;
                }

                //Merge result and existing path
                if (valid && path[path.Count - 1] != res[0])
                {
                    valid = false;
                }

                if (valid)
                {
                    //put the old path infront of the old path
                    if (path.Count > 1)
                    {
                        //make space for the old path
                        //if ((path.Count - 1) + nres > maxPathResult)
                        //nres = maxPathResult - (npath - 1);

                        for (int j = 0; j < res.Count; j++)
                        {
                            res[path.Count - 1 + j] = res[j];
                        }

                        //copy old path in the beginning
                        for (int j = 0; j < path.Count - 1; j++)
                        {
                            res.Add(path[j]);
                        }

                        //remove trackbacks
                        res.RemoveTrackbacks();
                    }

                    //check for partial path
                    if (res[res.Count - 1] != this.targetReference)
                    {
                        //partial path, constrain target position inside the last polygon
                        Vector3 nearest;
                        bool tempBool = false;
                        status = this.Crowd.NavQuery.ClosestPointOnPoly(res[res.Count - 1], targetPos, out nearest, out tempBool).ToStatus();
                        if (status == Status.Success)
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
                    //set current corridor
                    this.corridor.SetCorridor(targetPos, res);

                    //forced to update boundary
                    this.boundary.Reset();
                    this.TargetState = TargetState.Valid;
                }
                else
                {
                    //something went wrong
                    this.TargetState = TargetState.Failed;
                }

                this.targetReplanTime = 0.0f;
            }
        }

        public void RequestPathUpdate(PathQueue pathQueue)
        {
            var startPoint = new PathPoint(this.corridor.LastPoly, this.corridor.Target);
            var endPoint = new PathPoint(this.targetReference, this.TargetPosition);

            this.targetPathQueryIndex = pathQueue.Request(startPoint, endPoint);
            if (this.targetPathQueryIndex != PathQueue.Invalid)
            {
                this.TargetState = TargetState.WaitingForPath;
            }
        }

        public void OptimizeTopology(float timeDelta, Agent[] queue, ref int numQueue)
        {
            this.topologyOptTime += timeDelta;
            if (this.topologyOptTime >= OptimizationTimeThresshold)
            {
                numQueue = AddToOptQueue(this, queue, numQueue, queue.Length);
            }
        }

        public void OptimizePathTopology()
        {
            this.corridor.OptimizePathTopology(this.Crowd.NavQuery, this.Crowd.NavQueryFilter);
            this.topologyOptTime = 0.0f;
        }


        public StraightPath GetStraightPath()
        {
            return this.corridor.FindCorners(this.Crowd.NavQuery);
        }

        public override string ToString()
        {
            return string.Format("Agent. Position {0}", this.Position);
        }
    }
}
