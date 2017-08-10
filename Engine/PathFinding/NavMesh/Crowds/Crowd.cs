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
        private const int MaxIteratorsPerUpdate = 100;
        private const int MaxNeighbors = 32;

        private const int PathMaxAgents = 8;
        private const int OptMaxAgents = 1;
        private const float OptTimeTHR = 0.5f; //seconds

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
            this.agents
                .FindAll(agent => agent.State == AgentState.Walking && (agent.TargetState != TargetState.None && agent.TargetState != TargetState.Velocity))
                .ForEach(agent =>
                {
                    agent.CheckPlan(navQuery, extents, timeDelta);
                });

            //update async move requests and path finder
            this.UpdateMoveRequest();

            //optimize path topology
            this.UpdateTopologyOptimization(timeDelta);

            //register agents to proximity grid
            this.grid.Clear();
            this.agents
                .ForEach(agent =>
                {
                    this.grid.AddItem(agent, agent.Parameters.Radius, agent.Position);
                });

            //get nearby navmesh segments and agents to collide with
            this.agents
                .FindAll(agent => agent.State == AgentState.Walking)
                .ForEach(agent =>
                {
                    //update the collision boundary after certain distance has passed or if it has become invalid
                    agent.UpdateCollision(navQuery);

                    //query neighbor agents
                    Agent[] ids;
                    int neighborsIds = grid.QueryItems(agent.Position, agent.Parameters.CollisionQueryRange, MaxNeighbors, out ids);

                    //set the neigbors for the agent
                    agent.SetNeighbors(ids, neighborsIds);
                });

            //find the next corner to steer to
            this.agents
                .FindAll(agent => agent.State == AgentState.Walking && (agent.TargetState != TargetState.None && agent.TargetState != TargetState.Velocity))
                .ForEach(agent =>
                {
                    //find corners for steering
                    agent.Steer1(navQuery);
                });

            //trigger off-mesh connections (depends on corners)
            this.agents
                .FindAll(agent => agent.State == AgentState.Walking && (agent.TargetState != TargetState.None && agent.TargetState != TargetState.Velocity))
                .ForEach(agent =>
                {
                    agent.TriggerOffmeshConnection(navQuery);
                });

            //calculate steering
            this.agents
                .FindAll(agent => agent.State == AgentState.Walking && (agent.TargetState != TargetState.None))
                .ForEach(agent =>
                {
                    agent.Steer2(this.agents);
                });

            //velocity planning
            this.agents
                .FindAll(agent => agent.State == AgentState.Walking)
                .ForEach(agent =>
                {
                    agent.VelocityPlanning(this.obstacleQuery);
                });

            //integrate
            this.agents
                .FindAll(agent => agent.State == AgentState.Walking)
                .ForEach(agent =>
                {
                    agent.Integrate(timeDelta);
                });

            //handle collisions
            for (int i = 0; i < 4; i++)
            {
                this.agents
                    .FindAll(agent => agent.State == AgentState.Walking)
                    .ForEach(agent =>
                    {
                        agent.HandleCollisions(this.agents);
                    });

                this.agents
                    .FindAll(agent => agent.State == AgentState.Walking)
                    .ForEach(agent =>
                    {
                        //move along navmesh
                        agent.MovePosition(navQuery);
                    });

                //update agents using offmesh connections
                this.agents
                    .ForEach(agent =>
                    {
                        agent.UpdateOffmeshConnections(timeDelta);
                    });
            }
        }
        /// <summary>
        /// Change the move requests for all the agents
        /// </summary>
        private void UpdateMoveRequest()
        {
            int numQueue = 0;
            Agent[] queue = new Agent[PathMaxAgents];

            //fire off new requests
            this.agents
                .FindAll(agent => agent.IsActive && agent.State != AgentState.Invalid)
                .ForEach(agent =>
                {
                    if (agent.TargetState == TargetState.Requesting)
                    {
                        agent.ResolveRequesting(navQuery, navQueryFilter);
                    }

                    if (agent.TargetState == TargetState.WaitingForQueue)
                    {
                        numQueue = AddToPathQueue(agent, queue, numQueue, PathMaxAgents);
                    }
                });

            for (int i = 0; i < numQueue; i++)
            {
                var startPoint = new PathPoint(queue[i].Corridor.GetLastPoly(), queue[i].Corridor.Target);
                var endPoint = new PathPoint(queue[i].TargetReference, queue[i].TargetPosition);

                queue[i].TargetPathQueryIndex = this.pathQueue.Request(startPoint, endPoint);
                if (queue[i].TargetPathQueryIndex != PathQueue.Invalid)
                {
                    queue[i].TargetState = TargetState.WaitingForPath;
                }
            }

            //update requests
            this.pathQueue.Update(MaxIteratorsPerUpdate);

            //process path results
            this.agents
                .FindAll(agent => agent.IsActive && agent.TargetState == TargetState.WaitingForPath)
                .ForEach(agent =>
                {
                    agent.ResolveWaitingForPath(navQuery, pathQueue);
                });
        }
        /// <summary>
        /// Reoptimize the path corridor for all agents
        /// </summary>
        /// <param name="agents">The agents array</param>
        /// <param name="numAgents">The number of agents</param>
        /// <param name="timeDelta">Time until next update</param>
        private void UpdateTopologyOptimization(float timeDelta)
        {
            if (this.agents.Count > 0)
            {
                Agent[] queue = new Agent[OptMaxAgents];
                int nqueue = 0;

                for (int i = 0; i < this.agents.Count; i++)
                {
                    var agent = agents[i];

                    if (agent.State != AgentState.Walking)
                    {
                        continue;
                    }

                    if (agent.TargetState == TargetState.None ||
                        agent.TargetState == TargetState.Velocity)
                    {
                        continue;
                    }

                    if ((agent.Parameters.UpdateFlags & UpdateFlags.OptimizeTopo) == 0)
                    {
                        continue;
                    }

                    agent.TopologyOptTime += timeDelta;
                    if (agent.TopologyOptTime >= OptTimeTHR)
                    {
                        nqueue = AddToOptQueue(agent, queue, nqueue, OptMaxAgents);
                    }
                }

                for (int i = 0; i < nqueue; i++)
                {
                    queue[i].Corridor.OptimizePathTopology(this.navQuery, this.navQueryFilter);
                    queue[i].TopologyOptTime = 0.0f;
                }
            }
        }

        /// <summary>
        /// Add an agent to the crowd.
        /// </summary>
        /// <param name="position">The agent's position</param>
        /// <param name="parameters">The settings</param>
        /// <returns>The id of the agent (-1 if there is no empty slot)</returns>
        public Agent AddAgent(Vector3 position, AgentParams parameters)
        {
            var agent = new Agent(parameters);

            //Find nearest position on the navmesh and place the agent there
            PathPoint nearest;
            if (this.navQuery.FindNearestPoly(ref position, ref this.extents, out nearest))
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
