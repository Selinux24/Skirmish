using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh.Crowds
{
    using Engine.Collections;

    /// <summary>
    /// Crowd controller class
    /// </summary>
    class Crowd
    {
        /// <summary>
		/// The maximum number of crowd avoidance configurations supported by the crowd manager
		/// </summary>
		private const int AgentMaxObstacleAvoidanceParams = 8;
        /// <summary>
        /// The maximum number of neighbors that a crowd agent can take into account for steering decisions
        /// </summary>
        private const int AgentMaxNeighbors = 6;
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
            else if (agent.TopologyOptTime <= agents[numAgents - 1].TopologyOptTime)
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
                    if (agent.TopologyOptTime >= agents[i].TopologyOptTime)
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

        private List<Agent> agents = new List<Agent>();
        private PathQueue pathQueue;
        private ProximityGrid<Agent> grid;
        private Vector3 extents;
        private NavigationMeshQuery navQuery;
        private NavigationMeshQueryFilter navQueryFilter;
        private ObstacleAvoidanceQuery obstacleQuery;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxAgentRadius">Maximum agent radius</param>
        /// <param name="navMesh">Tiled navigation mesh</param>
        public Crowd(float maxAgentRadius, ref TiledNavigationMesh navMesh)
        {
            this.extents = new Vector3(maxAgentRadius * 2.0f, maxAgentRadius * 1.5f, maxAgentRadius * 2.0f);

            //initialize proximity grid
            this.grid = new ProximityGrid<Agent>(128 * 4, maxAgentRadius * 3);

            //allocate obstacle avoidance query
            this.obstacleQuery = new ObstacleAvoidanceQuery(6, 8);

            this.pathQueue = new PathQueue(4096, ref navMesh);

            //allocate nav mesh query
            this.navQuery = new NavigationMeshQuery(navMesh, 512);

            //initialize filter
            this.navQueryFilter = null;
        }

        /// <summary>
        /// Updates the crowd pathfinding status
        /// </summary>
        /// <param name="timeDelta">Delta time</param>
        public void Update(float timeDelta)
        {
            //check that all agents have valid paths
            this.CheckPathValidity(timeDelta);

            //update async move requests and path finder
            this.UpdateMoveRequest();

            //optimize path topology
            this.UpdateTopologyOptimization(timeDelta);

            //register agents to proximity grid
            this.RegisterAgents();

            //get nearby navmesh segments and agents to collide with
            for (int i = 0; i < this.agents.Count; i++)
            {
                var agent = this.agents[i];

                if (agent.State != AgentState.Walking)
                {
                    continue;
                }

                //update the collision boundary after certain distance has passed or if it has become invalid
                agent.UpdateCollision(navQuery);

                //query neighbor agents
                Agent[] ids = new Agent[MaxNeighbors];
                int neighborsIds = grid.QueryItems(agent.Position, agent.Parameters.CollisionQueryRange, ids, MaxNeighbors);

                //set the neigbors for the agent
                agent.SetNeighbors(ids, neighborsIds);
            }

            //find the next corner to steer to
            for (int i = 0; i < this.agents.Count; i++)
            {
                var agent = this.agents[i];

                if (agent.State != AgentState.Walking)
                {
                    continue;
                }

                if (agent.TargetState == TargetState.None ||
                    agent.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                //find corners for steering
                agent.Steer1(navQuery);
            }

            //trigger off-mesh connections (depends on corners)
            for (int i = 0; i < this.agents.Count; i++)
            {
                var agent = this.agents[i];

                if (agent.State != AgentState.Walking)
                {
                    continue;
                }

                if (agent.TargetState == TargetState.None || agent.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                //check
                if (OverOffmeshConnection(agent))
                {
                    //adjust the path over the off-mesh connection
                    PolyId[] refs = new PolyId[2];
                    var agentAnim = agent.Animation;
                    if (agent.Corridor.MoveOverOffmeshConnection(agent.Corners[agent.Corners.Count - 1].Point.Polygon, refs, ref agentAnim.StartPos, ref agentAnim.EndPos, navQuery))
                    {
                        agentAnim.InitPos = agent.Position;
                        agentAnim.PolyRef = refs[1];
                        agentAnim.Active = true;
                        agentAnim.T = 0.0f;
                        agentAnim.TMax = (Helper.Distance2D(agentAnim.StartPos, agentAnim.EndPos) / agent.Parameters.MaxSpeed) * 0.5f;

                        agent.TriggerOffmeshConnection();
                        continue;
                    }
                }
            }

            //calculate steering
            for (int i = 0; i < this.agents.Count; i++)
            {
                var agent = this.agents[i];

                if (agent.State != AgentState.Walking)
                {
                    continue;
                }

                if (agent.TargetState == TargetState.None)
                {
                    continue;
                }

                agent.Steer2(this.agents);
            }

            //velocity planning
            for (int i = 0; i < this.agents.Count; i++)
            {
                var agent = this.agents[i];

                if (agent.State != AgentState.Walking)
                {
                    continue;
                }

                agent.VelocityPlanning(this.obstacleQuery);
            }

            //integrate
            for (int i = 0; i < this.agents.Count; i++)
            {
                var agent = agents[i];

                if (agent.State != AgentState.Walking)
                {
                    continue;
                }

                agent.Integrate(timeDelta);
            }

            //handle collisions
            for (int iter = 0; iter < 4; iter++)
            {
                for (int i = 0; i < this.agents.Count; i++)
                {
                    var agent = agents[i];

                    if (agent.State != AgentState.Walking)
                    {
                        continue;
                    }

                    agent.HandleCollisions(this.agents);
                }

                for (int i = 0; i < this.agents.Count; i++)
                {
                    var agent = agents[i];

                    if (agent.State != AgentState.Walking)
                    {
                        continue;
                    }

                    //move along navmesh
                    agent.MovePosition(navQuery);
                }

                //update agents using offmesh connections
                for (int i = 0; i < this.agents.Count; i++)
                {
                    var agent = this.agents[i];
                    var anim = agent.Animation;

                    if (!anim.Active)
                    {
                        continue;
                    }

                    anim.T += timeDelta;
                    if (anim.T > anim.TMax)
                    {
                        //reset animation
                        anim.Active = false;

                        //prepare agent for walking
                        agent.State = AgentState.Walking;

                        continue;
                    }

                    //update position
                    float ta = anim.TMax * 0.15f;
                    float tb = anim.TMax;
                    if (anim.T < ta)
                    {
                        float u = Helper.Normalize(anim.T, 0.0f, ta);
                        agent.Position = Vector3.Lerp(anim.InitPos, anim.StartPos, u);
                    }
                    else
                    {
                        float u = Helper.Normalize(anim.T, ta, tb);
                        agent.Position = Vector3.Lerp(anim.StartPos, anim.EndPos, u);
                    }

                    agent.Velocity = Vector3.Zero;
                    agent.DesiredVelocity = Vector3.Zero;
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
            var agent = new Agent(parameters);

            //Find nearest position on the navmesh and place the agent there
            PathPoint nearest;
            if (this.navQuery.FindNearestPoly(ref pos, ref this.extents, out nearest))
            {
                agent.Reset(nearest.Polygon, nearest.Position);
                agent.IsActive = true;
            }

            this.agents.Add(agent);

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
        /// <summary>
        /// Registers all agents in the proximity grid
        /// </summary>
        private void RegisterAgents()
        {
            this.grid.Clear();

            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = agents[i];

                this.grid.AddItem(a, a.Parameters.Radius, a.Position);
            }
        }

        /// <summary>
        /// Make sure that each agent is taking a valid path
        /// </summary>
        /// <param name="deltaTime">Time until next update</param>
        private void CheckPathValidity(float deltaTime)
        {
            //Iterate through all the agents
            for (int i = 0; i < this.agents.Count; i++)
            {
                Agent agent = agents[i];

                if (agent.State != AgentState.Walking)
                {
                    continue;
                }

                if (agent.TargetState == TargetState.None || agent.TargetState == TargetState.Velocity)
                {
                    continue;
                }

                agent.TargetReplanTime += deltaTime;

                bool replan = false;

                //first check that the current location is valid
                var agentRef = agent.Corridor.GetFirstPoly();
                var agentPos = agent.Position;
                if (!this.navQuery.IsValidPolyRef(agentRef))
                {
                    //current location is not valid, try to reposition
                    Vector3 nearest = agentPos;
                    Vector3 pos = agent.Position;
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
                            agent.SetInvalidState(agentPos);
                            continue;
                        }

                        //make sure the first polygon is valid
                        agent.Reposition(agentRef, agentPos);
                        replan = true;
                    }
                }

                //try to recover move request position
                if (agent.TargetState != TargetState.None &&
                    agent.TargetState != TargetState.Failed)
                {
                    if (!this.navQuery.IsValidPolyRef(agent.TargetReference))
                    {
                        //current target is not valid, try to reposition
                        Vector3 nearest = agent.TargetPosition;
                        Vector3 tpos = agent.TargetPosition;
                        agent.TargetReference = PolyId.Null;
                        PathPoint nearestPt;
                        if (this.navQuery.FindNearestPoly(ref tpos, ref this.extents, out nearestPt))
                        {
                            nearest = nearestPt.Position;
                            agent.SetTarget(nearestPt.Polygon, nearestPt.Position);
                            replan = true;
                        }
                    }

                    if (agent.TargetReference == PolyId.Null)
                    {
                        //failed to reposition target
                        agent.ResetTarget(agentRef, agentPos);
                    }
                }

                //if nearby corridor is not valid, replan
                if (!agent.Corridor.IsValid(CheckLookAhead, this.navQuery))
                {
                    replan = true;
                }

                //if the end of the path is near and it is not the request location, replan
                if (agent.TargetState == TargetState.Valid)
                {
                    if (agent.TargetReplanTime > TargetReplanDelay &&
                        agent.Corridor.NavPath.Count < CheckLookAhead &&
                        agent.Corridor.GetLastPoly() != agent.TargetReference)
                    {
                        replan = true;
                    }
                }

                //try to replan path to goal
                if (replan && agent.TargetState != TargetState.None)
                {
                    agent.RequestMoveTargetReplan();
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
                    PathPoint endPoint = new PathPoint(a.TargetReference, a.TargetPosition);
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
                        if (reqPath[reqPath.Count - 1] != a.TargetReference)
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

                    if (reqPath[reqPath.Count - 1] == a.TargetReference)
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
                queue[i].TargetPathQueryIndex = this.pathQueue.Request(new PathPoint(queue[i].Corridor.GetLastPoly(), queue[i].Corridor.Target), new PathPoint(queue[i].TargetReference, queue[i].TargetPosition));
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
                        if (a.TargetReference != PolyId.Null)
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
                            if (res[res.Count - 1] != a.TargetReference)
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

                a.TopologyOptTime += dt;
                if (a.TopologyOptTime >= OptTimeTHR)
                {
                    nqueue = AddToOptQueue(a, queue, nqueue, OptMaxAgents);
                }
            }

            for (int i = 0; i < nqueue; i++)
            {
                queue[i].Corridor.OptimizePathTopology(this.navQuery, this.navQueryFilter);
                queue[i].TopologyOptTime = 0.0f;
            }
        }

        /// <summary>
        /// Move all agents in the crowd to position
        /// </summary>
        /// <param name="position">Target position</param>
        /// <param name="radius">Radius around target position</param>
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
    }
}
