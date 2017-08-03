using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh.Crowds
{
    class Crowd
    {
        /// <summary>
		/// The maximum number of crowd avoidance configurations supported by the crowd manager
		/// </summary>
		private const int AgentMaxObstacleAvoidanceParams = 8;
        /// <summary>
        /// The maximum number of neighbors that a crowd agent can take into account for steering decisions
        /// </summary>
        private const int AgentMaxNeighbors = Agent.AgentMaxNeighbors;
        /// <summary>
        /// The maximum number of corners a crowd agent will look ahead in the path
        /// </summary>
        private const int AgentMaxCorners = 4;

        private const int MaxIteratorsPerUpdate = 100;
        private const int MaxNeighbors = 32;

        private const int MaxIter = 20;

        private const int PathMaxAgents = 8;
        private const int OptMaxAgents = 1;
        private const float OptTimeTHR = 0.5f; //seconds

        private const float CollisionResolveFactor = 0.7f;
        private const int CheckLookAhead = 10;
        private const float TargetReplanDelay = 1.0f; //seconds

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
            else if (agent.TargetReplanTime <= agents[numAgents - 1].TargetReplanTime)
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
                    if (agent.TargetReplanTime >= agents[i].TargetReplanTime)
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

        private static bool OverOffmeshConnection(Agent agent, float radius)
        {
            if (agent.Corners.Count == 0)
            {
                return false;
            }

            bool offmeshConnection = (agent.Corners[agent.Corners.Count - 1].Flags & StraightPathFlags.OffMeshConnection) != 0;
            if (offmeshConnection)
            {
                float dist = Helper.Distance2D(agent.Position, agent.Corners[agent.Corners.Count - 1].Point.Position);
                if (dist * dist < radius * radius)
                {
                    return true;
                }
            }

            return false;
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
        /// <param name="pos">Current position</param>
        /// <param name="height">The height</param>
        /// <param name="range">The range to search within</param>
        /// <param name="skip">The current crowd agent</param>
        /// <param name="result">The neihbors array</param>
        /// <param name="maxResult">The maximum number of neighbors that can be stored</param>
        /// <param name="agents">Array of all crowd agents</param>
        /// <param name="grid">The ProximityGrid</param>
        /// <returns>The number of neighbors</returns>
        private static int GetNeighbors(Vector3 pos, float height, float range, Agent skip, CrowdNeighbor[] result, int maxResult, List<Agent> agents, ProximityGrid<Agent> grid)
        {
            int n = 0;

            Agent[] ids = new Agent[MaxNeighbors];
            int nids = grid.QueryItems(pos, range, ids, MaxNeighbors);

            for (int i = 0; i < nids; i++)
            {
                var ag = ids[i];
                if (ag == skip)
                {
                    continue;
                }

                //check for overlap
                Vector3 diff = pos - ag.Position;
                if (Math.Abs(diff.Y) >= (height + ag.Parameters.Height) / 2.0f)
                {
                    continue;
                }
                diff.Y = 0;
                float distSqr = diff.LengthSquared();
                if (distSqr > range * range)
                {
                    continue;
                }

                n = AddNeighbor(ids[i], distSqr, result, n, maxResult, agents);
            }

            return n;
        }
        /// <summary>
        /// Add a CrowdNeighbor to the array
        /// </summary>
        /// <param name="agent">The neighbor</param>
        /// <param name="dist">Distance from current agent</param>
        /// <param name="neis">The neighbors array</param>
        /// <param name="nneis">The number of neighbors</param>
        /// <param name="maxNeis">The maximum number of neighbors allowed</param>
        /// <param name="agents">Agents</param>
        /// <returns>An updated neighbor count</returns>
        private static int AddNeighbor(Agent agent, float dist, CrowdNeighbor[] neis, int nneis, int maxNeis, List<Agent> agents)
        {
            //insert neighbor based on distance
            int nPos = 0;
            if (nneis == 0)
            {
                nPos = nneis;
            }
            else if (dist >= neis[nneis - 1].Distance)
            {
                if (nneis >= maxNeis)
                {
                    return nneis;
                }
                nPos = nneis;
            }
            else
            {
                int i;
                for (i = 0; i < nneis; i++)
                {
                    if (dist <= neis[i].Distance)
                    {
                        break;
                    }
                }

                int tgt = i + 1;
                int n = Math.Min(nneis - i, maxNeis - tgt);

                if (n > 0)
                {
                    for (int j = 0; j < n; j++)
                    {
                        neis[tgt + j] = neis[i + j];
                    }
                }

                nPos = i;
            }

            //TODO rework Crowd so that Agents are passed around instead of indices
            int index;
            for (index = 0; index < agents.Count; index++)
            {
                if (agent.Equals(agents[index]))
                {
                    break;
                }
            }

            if (index == agents.Count)
            {
                throw new IndexOutOfRangeException("Agent not in crowd.");
            }

            var neighbor = new CrowdNeighbor();
            neighbor.Index = index;
            neighbor.Distance = dist;
            neis[nPos] = neighbor;

            return Math.Min(nneis + 1, maxNeis);
        }

        private List<Agent> agents = new List<Agent>();
        private List<AgentAnimation> agentAnims = new List<AgentAnimation>();
        private PathQueue pathQueue;
        private List<ObstacleAvoidanceParams> obstacleQueryParams = new List<ObstacleAvoidanceParams>();
        private ProximityGrid<Agent> grid;
        private Vector3 extents;
        private int velocitySampleCount;
        private NavigationMeshQuery navQuery;
        private NavigationMeshQueryFilter navQueryFilter;
        private ObstacleAvoidanceQuery obstacleQuery;

        public Crowd(float maxAgentRadius, ref TiledNavigationMesh navMesh)
        {
            this.extents = new Vector3(maxAgentRadius * 2.0f, maxAgentRadius * 1.5f, maxAgentRadius * 2.0f);

            //initialize proximity grid
            this.grid = new ProximityGrid<Agent>(128 * 4, maxAgentRadius * 3);

            //allocate obstacle avoidance query
            this.obstacleQuery = new ObstacleAvoidanceQuery(6, 8);

            //initialize obstancle query params
            for (int i = 0; i < this.obstacleQueryParams.Count; i++)
            {
                var obsQP = new ObstacleAvoidanceParams()
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
                    AdaptiveDepth = 5,
                };

                this.obstacleQueryParams.Add(obsQP);
            }

            this.pathQueue = new PathQueue(4096, ref navMesh);

            //allocate nav mesh query
            this.navQuery = new NavigationMeshQuery(navMesh, 512);

            //initialize filter
            this.navQueryFilter = null;
        }

        /// <summary>
        /// Update the crowd pathfinding periodically 
        /// </summary>
        /// <param name="dt">Th time until the next update</param>
        public void Update(float dt)
        {
            velocitySampleCount = 0;

            //check that all agents have valid paths
            this.CheckPathValidity(dt);

            //update async move requests and path finder
            this.UpdateMoveRequest();

            //optimize path topology
            this.UpdateTopologyOptimization(dt);

            //register agents to proximity grid
            this.grid.Clear();

            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = agents[i];

                this.grid.AddItem(a, a.Parameters.Radius, a.Position);
            }

            //get nearby navmesh segments and agents to collide with
            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = this.agents[i];

                if (a.State != AgentState.Walking)
                {
                    continue;
                }

                //update the collision boundary after certain distance has passed or if it has become invalid
                float updateThr = a.Parameters.CollisionQueryRange * 0.25f;
                if (Helper.Distance2D(a.Position, a.Boundary.Center) > updateThr * updateThr || !a.Boundary.IsValid(navQuery))
                {
                    a.Boundary.Update(a.Corridor.GetFirstPoly(), a.Position, a.Parameters.CollisionQueryRange, navQuery);
                }

                //query neighbor agents
                a.NeighborCount = GetNeighbors(a.Position, a.Parameters.Height, a.Parameters.CollisionQueryRange, a, a.Neighbors, AgentMaxNeighbors, agents, grid);

                for (int j = 0; j < a.NeighborCount; j++)
                {
                    var neighbor = agents[a.Neighbors[j].Index];

                    a.Neighbors[j].Index = this.agents.IndexOf(neighbor);
                }
            }

            //find the next corner to steer to
            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = this.agents[i];

                if (a.State != AgentState.Walking)
                {
                    continue;
                }

                if (a.TargetState == TargetState.None ||
                    a.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                //find corners for steering
                a.Corners = a.Corridor.FindCorners(navQuery);

                //check to see if the corner after the next corner is directly visible 
                if ((a.Parameters.UpdateFlags & UpdateFlags.OptimizeVis) != 0 && a.Corners.Count > 0)
                {
                    Vector3 target = a.Corners[Math.Min(1, a.Corners.Count - 1)].Point.Position;
                    a.Corridor.OptimizePathVisibility(target, a.Parameters.PathOptimizationRange, navQuery);
                }
            }

            //trigger off-mesh connections (depends on corners)
            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = this.agents[i];

                if (a.State != AgentState.Walking)
                {
                    continue;
                }

                if (a.TargetState == TargetState.None || a.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                //check
                float triggerRadius = a.Parameters.Radius * 2.25f;
                if (OverOffmeshConnection(a, triggerRadius))
                {
                    //adjust the path over the off-mesh connection
                    PolyId[] refs = new PolyId[2];
                    var agentAnim = this.agentAnims[i];
                    if (a.Corridor.MoveOverOffmeshConnection(a.Corners[a.Corners.Count - 1].Point.Polygon, refs, ref agentAnim.StartPos, ref agentAnim.EndPos, navQuery))
                    {
                        agentAnim.InitPos = a.Position;
                        agentAnim.PolyRef = refs[1];
                        agentAnim.Active = true;
                        agentAnim.T = 0.0f;
                        agentAnim.TMax = (Helper.Distance2D(agentAnims[i].StartPos, agentAnims[i].EndPos) / a.Parameters.MaxSpeed) * 0.5f;

                        a.State = AgentState.Offmesh;
                        a.Corners.Clear();
                        a.NeighborCount = 0;
                        continue;
                    }
                }
            }

            //calculate steering
            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = this.agents[i];

                if (a.State != AgentState.Walking)
                {
                    continue;
                }

                if (a.TargetState == TargetState.None)
                {
                    continue;
                }

                Vector3 dvel = Vector3.Zero;

                if (a.TargetState == TargetState.Velocity)
                {
                    dvel = a.TargetPosition;
                    a.DesiredSpeed = a.TargetPosition.Length();
                }
                else
                {
                    //calculate steering direction
                    if ((a.Parameters.UpdateFlags & UpdateFlags.AnticipateTurns) != 0)
                    {
                        CalcSmoothSteerDirection(a, ref dvel);
                    }
                    else
                    {
                        CalcStraightSteerDirection(a, ref dvel);
                    }

                    //calculate speed scale, which tells the agent to slowdown at the end of the path
                    float slowDownRadius = a.Parameters.Radius * 2;
                    float speedScale = GetDistanceToGoal(a, slowDownRadius) / slowDownRadius;

                    a.DesiredSpeed = a.Parameters.MaxSpeed;
                    dvel = dvel * (a.DesiredSpeed * speedScale);
                }

                //separation
                if ((a.Parameters.UpdateFlags & UpdateFlags.Separation) != 0)
                {
                    float separationDist = a.Parameters.CollisionQueryRange;
                    float invSeparationDist = 1.0f / separationDist;
                    float separationWeight = a.Parameters.SeparationWeight;

                    float w = 0;
                    Vector3 disp = Vector3.Zero;

                    for (int j = 0; j < a.NeighborCount; j++)
                    {
                        var n = agents[a.Neighbors[j].Index];

                        Vector3 diff = a.Position - n.Position;
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
                        float desiredSqr = a.DesiredSpeed * a.DesiredSpeed;
                        if (speedSqr > desiredSqr)
                        {
                            dvel = dvel * (desiredSqr / speedSqr);
                        }
                    }
                }

                //set the desired velocity
                a.DesiredVel = dvel;
            }

            //velocity planning
            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = this.agents[i];

                if (a.State != AgentState.Walking)
                {
                    continue;
                }

                if ((a.Parameters.UpdateFlags & UpdateFlags.ObstacleAvoidance) != 0)
                {
                    this.obstacleQuery.Reset();

                    //add neighhbors as obstacles
                    for (int j = 0; j < a.NeighborCount; j++)
                    {
                        var n = agents[a.Neighbors[j].Index];
                        obstacleQuery.AddCircle(n.Position, n.Parameters.Radius, n.Vel, n.DesiredVel);
                    }

                    //append neighbor segments as obstacles
                    for (int j = 0; j < a.Boundary.SegCount; j++)
                    {
                        Segment s = a.Boundary.Segs[j];
                        if (Helper.Area2D(a.Position, s.Start, s.End) < 0.0f)
                        {
                            continue;
                        }
                        obstacleQuery.AddSegment(s.Start, s.End);
                    }

                    //sample new safe velocity
                    bool adaptive = true;
                    int ns = 0;
                    var parameters = obstacleQueryParams[a.Parameters.ObstacleAvoidanceType];
                    Vector3 nVel;
                    if (adaptive)
                    {
                        ns = obstacleQuery.SampleVelocityAdaptive(a.Position, a.Parameters.Radius, a.DesiredSpeed, a.Vel, a.DesiredVel, parameters, out nVel);
                    }
                    else
                    {
                        ns = obstacleQuery.SampleVelocityGrid(a.Position, a.Parameters.Radius, a.DesiredSpeed, a.Vel, a.DesiredVel, parameters, out nVel);
                    }
                    a.NVel = nVel;

                    this.velocitySampleCount += ns;
                }
                else
                {
                    //if not using velocity planning, new velocity is directly the desired velocity
                    a.NVel = a.DesiredVel;
                }
            }

            //integrate
            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = agents[i];

                if (a.State != AgentState.Walking)
                {
                    continue;
                }

                a.Integrate(dt);
            }

            //handle collisions
            for (int iter = 0; iter < 4; iter++)
            {
                for (int i = 0; i < this.agents.Count; i++)
                {
                    var a = agents[i];

                    if (a.State != AgentState.Walking)
                    {
                        continue;
                    }

                    int idx0 = this.agents.IndexOf(a);

                    a.Disp = Vector3.Zero;

                    float w = 0;

                    for (int j = 0; j < a.NeighborCount; j++)
                    {
                        Agent neighbor = agents[a.Neighbors[j].Index];
                        int idx1 = this.agents.IndexOf(neighbor);

                        Vector3 diff = a.Position - neighbor.Position;
                        diff.Y = 0;

                        float dist = diff.LengthSquared();
                        if (dist > (a.Parameters.Radius + neighbor.Parameters.Radius) * (a.Parameters.Radius + neighbor.Parameters.Radius))
                        {
                            continue;
                        }

                        dist = (float)Math.Sqrt(dist);
                        float pen = (a.Parameters.Radius + neighbor.Parameters.Radius) - dist;
                        if (dist < 0.0001f)
                        {
                            //agents on top of each other, try to choose diverging separation directions
                            if (idx0 > idx1)
                            {
                                diff = new Vector3(-a.DesiredVel.Z, 0, a.DesiredVel.X);
                            }
                            else
                            {
                                diff = new Vector3(a.DesiredVel.Z, 0, -a.DesiredVel.X);
                            }
                            pen = 0.01f;
                        }
                        else
                        {
                            pen = (1.0f / dist) * (pen * 0.5f) * CollisionResolveFactor;
                        }

                        a.Disp = a.Disp + diff * pen;

                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        float iw = 1.0f / w;
                        a.Disp = a.Disp * iw;
                    }
                }

                for (int i = 0; i < this.agents.Count; i++)
                {
                    var a = agents[i];

                    if (a.State != AgentState.Walking)
                    {
                        continue;
                    }

                    //move along navmesh
                    a.Corridor.MovePosition(a.Position, navQuery);

                    //get valid constrained position back
                    a.Position = a.Corridor.Pos;

                    //if not using path, truncate the corridor to just one poly
                    if (a.TargetState == TargetState.None ||
                        a.TargetState == TargetState.Velocity)
                    {
                        a.Corridor.Reset(a.Corridor.GetFirstPoly(), a.Position);
                        a.IsPartial = false;
                    }
                }

                //update agents using offmesh connections
                for (int i = 0; i < this.agents.Count; i++)
                {
                    var a = this.agents[i];
                    var anim = this.agentAnims[i];

                    if (!agentAnims[i].Active)
                    {
                        continue;
                    }

                    anim.T += dt;
                    if (agentAnims[i].T > agentAnims[i].TMax)
                    {
                        //reset animation
                        anim.Active = false;

                        //prepare agent for walking
                        a.State = AgentState.Walking;

                        continue;
                    }

                    //update position
                    float ta = agentAnims[i].TMax * 0.15f;
                    float tb = agentAnims[i].TMax;
                    if (agentAnims[i].T < ta)
                    {
                        float u = Helper.Normalize(agentAnims[i].T, 0.0f, ta);
                        a.Position = Vector3.Lerp(agentAnims[i].InitPos, agentAnims[i].StartPos, u);
                    }
                    else
                    {
                        float u = Helper.Normalize(agentAnims[i].T, ta, tb);
                        a.Position = Vector3.Lerp(agentAnims[i].StartPos, agentAnims[i].EndPos, u);
                    }

                    a.Vel = Vector3.Zero;
                    a.DesiredVel = Vector3.Zero;
                }
            }
        }
        /// <summary>
        /// Add an agent to the crowd.
        /// </summary>
        /// <param name="pos">The agent's position</param>
        /// <param name="parameters">The settings</param>
        /// <returns>The id of the agent (-1 if there is no empty slot)</returns>
        public Agent AddAgent(Vector3 pos, AgentParams parameters)
        {
            var agent = new Agent()
            {
                Parameters = parameters
            };

            //Find nearest position on the navmesh and place the agent there
            PathPoint nearest;
            if (this.navQuery.FindNearestPoly(ref pos, ref this.extents, out nearest))
            {
                agent.Reset(nearest.Polygon, nearest.Position);
                agent.IsActive = true;
            }

            this.agents.Add(agent);
            this.agentAnims.Add(new AgentAnimation());

            return agent;
        }
        /// <summary>
        /// The agent is deactivated and will no longer be processed. It can still be reused later.
        /// </summary>
        /// <param name="index">The agent's id</param>
        /// <returns>A value indicating whether the agent was successfully removed.</returns>
        public void RemoveAgent(Agent agent)
        {
            if (this.agents.Contains(agent))
            {
                this.agents.Remove(agent);
            }
        }

        public void MoveTo(Vector3 position, float radius)
        {
            //Get the polygon that the starting point is in
            PathPoint startPt;
            if (this.navQuery.FindNearestPoly(ref position, ref this.extents, out startPt))
            {
                for (int i = 0; i < this.agents.Count; i++)
                {
                    //Pick a new random point that is within a certain radius of the current point
                    PathPoint newPt;
                    if (this.navQuery.FindRandomPointAroundCircle(ref startPt, radius, out newPt))
                    {
                        //Give this agent a target point
                        this.agents[i].RequestMoveTarget(newPt.Polygon, newPt.Position);
                    }
                }
            }
        }

        /// <summary>
        /// Make sure that each agent is taking a valid path
        /// </summary>
        /// <param name="agents">The agent array</param>
        /// <param name="agentCount">The number of agents</param>
        /// <param name="dt">Time until next update</param>
        private void CheckPathValidity(float dt)
        {
            //Iterate through all the agents
            for (int i = 0; i < this.agents.Count; i++)
            {
                Agent ag = agents[i];

                if (ag.State != AgentState.Walking)
                {
                    continue;
                }

                if (ag.TargetState == TargetState.None || ag.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                ag.TargetReplanTime += dt;

                bool replan = false;

                //first check that the current location is valid
                PolyId agentRef = ag.Corridor.GetFirstPoly();
                Vector3 agentPos = ag.Position;
                if (!this.navQuery.IsValidPolyRef(agentRef))
                {
                    //current location is not valid, try to reposition
                    Vector3 nearest = agentPos;
                    Vector3 pos = ag.Position;
                    agentRef = PolyId.Null;
                    PathPoint nearestPt;
                    if (this.navQuery.FindNearestPoly(ref pos, ref this.extents, out nearestPt))
                    {
                        nearest = nearestPt.Position;
                        agentRef = nearestPt.Polygon;
                        agentPos = nearestPt.Position;

                        if (agentRef == PolyId.Null)
                        {
                            //could not find location in navmesh, set state to invalid
                            ag.Corridor.Reset(PolyId.Null, agentPos);
                            ag.IsPartial = false;
                            ag.Boundary.Reset();
                            ag.State = AgentState.Invalid;
                            continue;
                        }

                        //make sure the first polygon is valid
                        ag.Corridor.FixPathStart(agentRef, agentPos);
                        ag.Boundary.Reset();
                        ag.Position = agentPos;

                        replan = true;
                    }
                }

                //try to recover move request position
                if (ag.TargetState != TargetState.None &&
                    ag.TargetState != TargetState.Failed)
                {
                    if (!this.navQuery.IsValidPolyRef(ag.TargetRef))
                    {
                        //current target is not valid, try to reposition
                        Vector3 nearest = ag.TargetPosition;
                        Vector3 tpos = ag.TargetPosition;
                        ag.TargetRef = PolyId.Null;
                        PathPoint nearestPt;
                        if (this.navQuery.FindNearestPoly(ref tpos, ref this.extents, out nearestPt))
                        {
                            ag.TargetRef = nearestPt.Polygon;
                            nearest = nearestPt.Position;
                            ag.TargetPosition = nearest;
                            replan = true;
                        }
                    }

                    if (ag.TargetRef == PolyId.Null)
                    {
                        //failed to reposition target
                        ag.Corridor.Reset(agentRef, agentPos);
                        ag.IsPartial = false;
                        ag.TargetState = TargetState.None;
                    }
                }

                //if nearby corridor is not valid, replan
                if (!ag.Corridor.IsValid(CheckLookAhead, this.navQuery))
                {
                    replan = true;
                }

                //if the end of the path is near and it is not the request location, replan
                if (ag.TargetState == TargetState.Valid)
                {
                    if (ag.TargetReplanTime > TargetReplanDelay &&
                        ag.Corridor.NavPath.Count < CheckLookAhead &&
                        ag.Corridor.GetLastPoly() != ag.TargetRef)
                    {
                        replan = true;
                    }
                }

                //try to replan path to goal
                if (replan)
                {
                    if (ag.TargetState != TargetState.None)
                    {
                        ag.RequestMoveTargetReplan(ag.TargetRef, ag.TargetPosition);
                    }
                }
            }
        }
        /// <summary>
        /// Change the move requests for all the agents
        /// </summary>
        private void UpdateMoveRequest()
        {
            Agent[] queue = new Agent[PathMaxAgents];
            int numQueue = 0;
            Status status;

            //fire off new requests
            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = this.agents[i];

                if (!a.IsActive)
                {
                    continue;
                }

                if (a.State == AgentState.Invalid)
                {
                    continue;
                }

                if (a.TargetState == TargetState.None || a.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                if (a.TargetState == TargetState.Requesting)
                {
                    var path = a.Corridor.NavPath;

                    Vector3 reqPos = new Vector3();
                    Path reqPath = new Path();

                    //quick search towards the goal
                    PathPoint startPoint = new PathPoint(path[0], a.Position);
                    PathPoint endPoint = new PathPoint(a.TargetRef, a.TargetPosition);
                    this.navQuery.InitSlicedFindPath(ref startPoint, ref endPoint, this.navQueryFilter, FindPathOptions.None);
                    int tempInt = 0;
                    this.navQuery.UpdateSlicedFindPath(MaxIter, ref tempInt);
                    status = Status.Failure;
                    if (a.TargetReplan)
                    {
                        //try to use an existing steady path during replan if possible
                        status = this.navQuery.FinalizedSlicedPathPartial(path, reqPath).ToStatus();
                    }
                    else
                    {
                        //try to move towards the target when the goal changes
                        status = this.navQuery.FinalizeSlicedFindPath(reqPath).ToStatus();
                    }

                    if (status != Status.Failure && reqPath.Count > 0)
                    {
                        //in progress or succeed
                        if (reqPath[reqPath.Count - 1] != a.TargetRef)
                        {
                            //partial path, constrain target position in last polygon
                            bool tempBool;
                            status = this.navQuery.ClosestPointOnPoly(reqPath[reqPath.Count - 1], a.TargetPosition, out reqPos, out tempBool).ToStatus();
                            if (status == Status.Failure)
                            {
                                reqPath.Clear();
                            }
                        }
                        else
                        {
                            reqPos = a.TargetPosition;
                        }
                    }
                    else
                    {
                        reqPath.Clear();
                    }

                    if (reqPath.Count == 0)
                    {
                        //could not find path, start the request from the current location
                        reqPos = a.Position;
                        reqPath.Add(path[0]);
                    }

                    a.Corridor.SetCorridor(reqPos, reqPath);
                    a.Boundary.Reset();
                    a.IsPartial = false;

                    if (reqPath[reqPath.Count - 1] == a.TargetRef)
                    {
                        a.TargetState = TargetState.Valid;
                        a.TargetReplanTime = 0.0f;
                    }
                    else
                    {
                        //the path is longer or potentially unreachable, full plan
                        a.TargetState = TargetState.WaitingForQueue;
                    }
                }

                if (a.TargetState == TargetState.WaitingForQueue)
                {
                    numQueue = AddToPathQueue(a, queue, numQueue, PathMaxAgents);
                }
            }

            for (int i = 0; i < numQueue; i++)
            {
                queue[i].TargetPathQueryIndex = this.pathQueue.Request(new PathPoint(queue[i].Corridor.GetLastPoly(), queue[i].Corridor.Target), new PathPoint(queue[i].TargetRef, queue[i].TargetPosition));
                if (queue[i].TargetPathQueryIndex != PathQueue.Invalid)
                {
                    queue[i].TargetState = TargetState.WaitingForPath;
                }
            }

            //update requests
            this.pathQueue.Update(MaxIteratorsPerUpdate);

            //process path results
            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = this.agents[i];

                if (!a.IsActive)
                {
                    continue;
                }

                if (a.TargetState == TargetState.None || a.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                if (a.TargetState == TargetState.WaitingForPath)
                {
                    //poll path queue
                    status = this.pathQueue.GetRequestStatus(a.TargetPathQueryIndex);
                    if (status == Status.Failure)
                    {
                        //path find failed, retry if the target location is still valid
                        a.TargetPathQueryIndex = PathQueue.Invalid;
                        if (a.TargetRef != PolyId.Null)
                        {
                            a.TargetState = TargetState.Requesting;
                        }
                        else
                        {
                            a.TargetState = TargetState.Failed;
                        }
                        a.TargetReplanTime = 0.0f;
                    }
                    else if (status == Status.Success)
                    {
                        Path path = a.Corridor.NavPath;

                        //apply results
                        Vector3 targetPos = new Vector3();
                        targetPos = a.TargetPosition;

                        Path res;
                        bool valid = true;
                        status = this.pathQueue.GetPathResult(a.TargetPathQueryIndex, out res).ToStatus();
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
                            if (res[res.Count - 1] != a.TargetRef)
                            {
                                //partial path, constrain target position inside the last polygon
                                Vector3 nearest;
                                bool tempBool = false;
                                status = this.navQuery.ClosestPointOnPoly(res[res.Count - 1], targetPos, out nearest, out tempBool).ToStatus();
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
                            a.Corridor.SetCorridor(targetPos, res);

                            //forced to update boundary
                            a.Boundary.Reset();
                            a.TargetState = TargetState.Valid;
                        }
                        else
                        {
                            //something went wrong
                            a.TargetState = TargetState.Failed;
                        }

                        a.TargetReplanTime = 0.0f;
                    }
                }
            }
        }
        /// <summary>
        /// Reoptimize the path corridor for all agents
        /// </summary>
        /// <param name="agents">The agents array</param>
        /// <param name="numAgents">The number of agents</param>
        /// <param name="dt">Time until next update</param>
        private void UpdateTopologyOptimization(float dt)
        {
            if (this.agents.Count == 0)
            {
                return;
            }

            Agent[] queue = new Agent[OptMaxAgents];
            int nqueue = 0;

            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = agents[i];

                if (a.State != AgentState.Walking)
                {
                    continue;
                }

                if (a.TargetState == TargetState.None ||
                    a.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                if ((a.Parameters.UpdateFlags & UpdateFlags.OptimizeTopo) == 0)
                {
                    continue;
                }

                a.topologyOptTime += dt;
                if (a.topologyOptTime >= OptTimeTHR)
                {
                    nqueue = AddToOptQueue(a, queue, nqueue, OptMaxAgents);
                }
            }

            for (int i = 0; i < nqueue; i++)
            {
                queue[i].Corridor.OptimizePathTopology(this.navQuery, this.navQueryFilter);
                queue[i].topologyOptTime = 0.0f;
            }
        }
    }
}
