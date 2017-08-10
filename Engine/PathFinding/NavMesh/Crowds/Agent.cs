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

        private const int CheckLookAhead = 10;

        private const float TargetReplanDelay = 1.0f; //seconds

        private const int MaxIter = 20;

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
        /// <summary>
        /// Gets if the specified agent is over an Offmesh connection
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns true if the agent is over an Offmesh connection</returns>
        private static bool OverOffmeshConnection(Agent agent)
        {
            if (agent.Corners.Count == 0)
            {
                return false;
            }

            bool offmeshConnection = (agent.Corners[agent.Corners.Count - 1].Flags & StraightPathFlags.OffMeshConnection) != 0;
            if (offmeshConnection)
            {
                float dist = Helper.Distance2D(agent.Position, agent.Corners[agent.Corners.Count - 1].Point.Position);
                float radius = agent.Parameters.TriggerRadius;
                if (dist * dist < radius * radius)
                {
                    return true;
                }
            }

            return false;
        }

        public float TopologyOptTime { get; set; }
        public float DesiredSpeed { get; set; }

        public Vector3 Disp { get; set; }
        public Vector3 DesiredVelocity { get; set; }
        public Vector3 NVelocity { get; set; }
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
            Vector3 dv = this.NVelocity - this.Velocity;
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

        internal void Reset(PolyId reference, Vector3 nearest)
        {
            this.Corridor.Reset(reference, nearest);
            this.Boundary.Reset();
            this.IsPartial = false;

            this.TopologyOptTime = 0;
            this.TargetReplanTime = 0;

            this.DesiredVelocity = Vector3.Zero;
            this.NVelocity = Vector3.Zero;
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
        internal void RequestMoveTargetReplan()
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
        internal bool RequestMoveTarget(PolyId reference, Vector3 pos)
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
        internal void SetInvalidState(Vector3 position)
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
        internal void SetTarget(PolyId reference, Vector3 position)
        {
            this.TargetReference = reference;
            this.TargetPosition = position;
        }

        internal void CheckPlan(NavigationMeshQuery navQuery, Vector3 extents, float deltaTime)
        {
            this.TargetReplanTime += deltaTime;

            bool replan = false;

            //first check that the current location is valid
            var agentRef = this.Corridor.GetFirstPoly();
            var agentPos = this.Position;
            if (!navQuery.IsValidPolyRef(agentRef))
            {
                //current location is not valid, try to reposition
                Vector3 nearest = agentPos;
                Vector3 pos = this.Position;
                agentRef = PolyId.Null;
                PathPoint nearestPt;
                if (navQuery.FindNearestPoly(ref pos, ref extents, out nearestPt))
                {
                    nearest = nearestPt.Position;
                    agentRef = nearestPt.Polygon;
                    agentPos = nearestPt.Position;

                    if (agentRef == PolyId.Null)
                    {
                        //could not find location in navmesh, set state to invalid
                        this.SetInvalidState(agentPos);
                        return;
                    }

                    //make sure the first polygon is valid
                    this.Reposition(agentRef, agentPos);
                    replan = true;
                }
            }

            //try to recover move request position
            if (this.TargetState != TargetState.None &&
                this.TargetState != TargetState.Failed)
            {
                if (!navQuery.IsValidPolyRef(this.TargetReference))
                {
                    //current target is not valid, try to reposition
                    Vector3 nearest = this.TargetPosition;
                    Vector3 tpos = this.TargetPosition;
                    this.TargetReference = PolyId.Null;
                    PathPoint nearestPt;
                    if (navQuery.FindNearestPoly(ref tpos, ref extents, out nearestPt))
                    {
                        nearest = nearestPt.Position;
                        this.SetTarget(nearestPt.Polygon, nearestPt.Position);
                        replan = true;
                    }
                }

                if (this.TargetReference == PolyId.Null)
                {
                    //failed to reposition target
                    this.ResetTarget(agentRef, agentPos);
                }
            }

            //if nearby corridor is not valid, replan
            if (!this.Corridor.IsValid(CheckLookAhead, navQuery))
            {
                replan = true;
            }

            //if the end of the path is near and it is not the request location, replan
            if (this.TargetState == TargetState.Valid)
            {
                if (this.TargetReplanTime > TargetReplanDelay &&
                    this.Corridor.NavPath.Count < CheckLookAhead &&
                    this.Corridor.GetLastPoly() != this.TargetReference)
                {
                    replan = true;
                }
            }

            //try to replan path to goal
            if (replan && this.TargetState != TargetState.None)
            {
                this.RequestMoveTargetReplan();
            }
        }

        internal void ResetTarget(PolyId reference, Vector3 position)
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
        internal void Reposition(PolyId reference, Vector3 position)
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

        internal void SetNeighbors(Agent[] neighborAgents, int count)
        {
            this.Neighbors = GetNeighbors(this, neighborAgents, count);
        }

        internal bool TriggerOffmeshConnection(NavigationMeshQuery navQuery)
        {
            if (OverOffmeshConnection(this))
            {
                //adjust the path over the off-mesh connection
                PolyId[] refs = new PolyId[2];
                var agentAnim = this.Animation;
                if (this.Corridor.MoveOverOffmeshConnection(this.Corners[this.Corners.Count - 1].Point.Polygon, refs, ref agentAnim.StartPos, ref agentAnim.EndPos, navQuery))
                {
                    agentAnim.InitPos = this.Position;
                    agentAnim.PolyRef = refs[1];
                    agentAnim.Active = true;
                    agentAnim.T = 0.0f;
                    agentAnim.TMax = (Helper.Distance2D(agentAnim.StartPos, agentAnim.EndPos) / this.Parameters.MaxSpeed) * 0.5f;

                    this.State = AgentState.Offmesh;
                    this.Corners.Clear();

                    return true;
                }
            }

            return false;
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

        internal void Steer2(List<Agent> agents)
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

        internal void VelocityPlanning(ObstacleAvoidanceQuery obstacleQuery)
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
                this.NVelocity = nVel;
            }
            else
            {
                //if not using velocity planning, new velocity is directly the desired velocity
                this.NVelocity = this.DesiredVelocity;
            }
        }

        internal void HandleCollisions(List<Agent> agents)
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

        internal void UpdateOffmeshConnections(float timeDelta)
        {
            var anim = this.Animation;

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

                    this.Velocity = Vector3.Zero;
                    this.DesiredVelocity = Vector3.Zero;
                }
            }
        }

        internal void ResolveRequesting(NavigationMeshQuery navQuery, NavigationMeshQueryFilter navQueryFilter)
        {
            var path = this.Corridor.NavPath;

            Vector3 reqPos = new Vector3();
            Path reqPath = new Path();

            //quick search towards the goal
            PathPoint startPoint = new PathPoint(path[0], this.Position);
            PathPoint endPoint = new PathPoint(this.TargetReference, this.TargetPosition);
            navQuery.InitSlicedFindPath(ref startPoint, ref endPoint, navQueryFilter, FindPathOptions.None);
            int tempInt = 0;
            navQuery.UpdateSlicedFindPath(MaxIter, ref tempInt);
            var status = Status.Failure;
            if (this.TargetReplan)
            {
                //try to use an existing steady path during replan if possible
                status = navQuery.FinalizedSlicedPathPartial(path, reqPath).ToStatus();
            }
            else
            {
                //try to move towards the target when the goal changes
                status = navQuery.FinalizeSlicedFindPath(reqPath).ToStatus();
            }

            if (status != Status.Failure && reqPath.Count > 0)
            {
                //in progress or succeed
                if (reqPath[reqPath.Count - 1] != this.TargetReference)
                {
                    //partial path, constrain target position in last polygon
                    bool tempBool;
                    status = navQuery.ClosestPointOnPoly(reqPath[reqPath.Count - 1], this.TargetPosition, out reqPos, out tempBool).ToStatus();
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

            this.Corridor.SetCorridor(reqPos, reqPath);
            this.Boundary.Reset();
            this.IsPartial = false;

            if (reqPath[reqPath.Count - 1] == this.TargetReference)
            {
                this.TargetState = TargetState.Valid;
                this.TargetReplanTime = 0.0f;
            }
            else
            {
                //the path is longer or potentially unreachable, full plan
                this.TargetState = TargetState.WaitingForQueue;
            }
        }

        internal void ResolveWaitingForPath(NavigationMeshQuery navQuery, PathQueue pathQueue)
        {
            //poll path queue
            var status = pathQueue.GetRequestStatus(this.TargetPathQueryIndex);
            if (status == Status.Failure)
            {
                //path find failed, retry if the target location is still valid
                this.TargetPathQueryIndex = PathQueue.Invalid;
                if (this.TargetReference != PolyId.Null)
                {
                    this.TargetState = TargetState.Requesting;
                }
                else
                {
                    this.TargetState = TargetState.Failed;
                }
                this.TargetReplanTime = 0.0f;
            }
            else if (status == Status.Success)
            {
                Path path = this.Corridor.NavPath;

                //apply results
                Vector3 targetPos = new Vector3();
                targetPos = this.TargetPosition;

                Path res;
                bool valid = true;
                status = pathQueue.GetPathResult(this.TargetPathQueryIndex, out res).ToStatus();
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
                    if (res[res.Count - 1] != this.TargetReference)
                    {
                        //partial path, constrain target position inside the last polygon
                        Vector3 nearest;
                        bool tempBool = false;
                        status = navQuery.ClosestPointOnPoly(res[res.Count - 1], targetPos, out nearest, out tempBool).ToStatus();
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
                    this.Corridor.SetCorridor(targetPos, res);

                    //forced to update boundary
                    this.Boundary.Reset();
                    this.TargetState = TargetState.Valid;
                }
                else
                {
                    //something went wrong
                    this.TargetState = TargetState.Failed;
                }

                this.TargetReplanTime = 0.0f;
            }
        }
    }
}
