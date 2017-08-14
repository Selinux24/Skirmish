using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh.Crowds
{
    using Engine.Collections;

    /// <summary>
    /// Crowd controller class
    /// </summary>
    class Crowd
    {
        private const int MaximumIteratorsPerUpdate = 100;
        private const int MaximumNeighbors = 32;
        private const int PathMaximumAgents = 8;
        private const int TopologyOptimizationMaximumAgents = 1;

        private List<Agent> agents = new List<Agent>();
        private PathQueue pathQueue;
        private ProximityGrid<Agent> proximityGrid;

        public readonly Vector3 Extents;
        public readonly NavigationMeshQuery NavQuery;
        public readonly NavigationMeshQueryFilter NavQueryFilter;
        public readonly ObstacleAvoidanceQuery ObstacleQuery;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxAgentRadius">Maximum agent radius</param>
        /// <param name="navMesh">Tiled navigation mesh</param>
        public Crowd(float maxAgentRadius, ref TiledNavigationMesh navMesh)
        {
            this.Extents = new Vector3(maxAgentRadius * 2.0f, maxAgentRadius * 1.5f, maxAgentRadius * 2.0f);

            //initialize proximity grid
            this.proximityGrid = new ProximityGrid<Agent>(128 * 4, maxAgentRadius * 3);

            //allocate obstacle avoidance query
            this.ObstacleQuery = new ObstacleAvoidanceQuery(6, 8);

            this.pathQueue = new PathQueue(4096, ref navMesh);

            //allocate nav mesh query
            this.NavQuery = new NavigationMeshQuery(navMesh, 512);

            //initialize filter
            this.NavQueryFilter = null;
        }

        /// <summary>
        /// Updates the crowd pathfinding status
        /// </summary>
        /// <param name="timeDelta">Delta time</param>
        public void Update(float timeDelta)
        {
            if (this.agents.Count > 0)
            {
                //check that all agents have valid paths
                this.agents
                    .FindAll(agent => agent.State == AgentState.Walking && (agent.TargetState != TargetState.None && agent.TargetState != TargetState.Velocity))
                    .ForEach(agent =>
                    {
                        agent.CheckPlan(timeDelta);
                    });

                //update async move requests and path finder
                {
                    int numQueue = 0;
                    Agent[] queue = new Agent[PathMaximumAgents];

                    //fire off new requests
                    this.agents
                        .FindAll(agent => agent.IsActive && agent.State != AgentState.Invalid)
                        .ForEach(agent =>
                        {
                            agent.ResolveRequesting(queue, ref numQueue);
                        });

                    //update requests
                    this.UpdatePathQueue(queue, numQueue);

                    //process path results
                    this.agents
                        .FindAll(agent => agent.IsActive && agent.TargetState == TargetState.WaitingForPath)
                        .ForEach(agent =>
                        {
                            agent.ResolveWaitingForPath(pathQueue);
                        });
                }

                //optimize path topology
                {
                    int numQueue = 0;
                    Agent[] queue = new Agent[TopologyOptimizationMaximumAgents];

                    this.agents
                        .FindAll(agent =>
                            agent.State == AgentState.Walking &&
                            (agent.TargetState != TargetState.None && agent.TargetState != TargetState.Velocity) &&
                            ((agent.Parameters.UpdateFlags & UpdateFlags.OptimizeTopo) != 0))
                        .ForEach(agent =>
                        {
                            agent.OptimizeTopology(timeDelta, queue, ref numQueue);
                        });

                    this.UpdateOptimizationQueue(queue, numQueue);
                }

                //register agents to proximity grid
                this.proximityGrid.Clear();
                this.agents
                    .ForEach(agent =>
                    {
                        this.proximityGrid.AddItem(agent, agent.Parameters.Radius, agent.Position);
                    });

                //get nearby navmesh segments and agents to collide with
                this.agents
                    .FindAll(agent => agent.State == AgentState.Walking)
                    .ForEach(agent =>
                    {
                        //update the collision boundary after certain distance has passed or if it has become invalid
                        agent.UpdateCollision();

                        //query neighbor agents
                        Agent[] ids;
                        int neighborsIds = proximityGrid.QueryItems(agent.Position, agent.Parameters.CollisionQueryRange, MaximumNeighbors, out ids);

                        //set the neigbors for the agent
                        agent.SetNeighbors(ids, neighborsIds);
                    });

                //find the next corner to steer to
                this.agents
                    .FindAll(agent => agent.State == AgentState.Walking && (agent.TargetState != TargetState.None && agent.TargetState != TargetState.Velocity))
                    .ForEach(agent =>
                    {
                        //find corners for steering
                        agent.Steer1();
                    });

                //trigger off-mesh connections (depends on corners)
                this.agents
                    .FindAll(agent => agent.State == AgentState.Walking && (agent.TargetState != TargetState.None && agent.TargetState != TargetState.Velocity))
                    .ForEach(agent =>
                    {
                        agent.TriggerOffmeshConnection();
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
                        agent.VelocityPlanning();
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
                            agent.MovePosition();
                        });

                    //update agents using offmesh connections
                    this.agents
                        .ForEach(agent =>
                        {
                            agent.UpdateOffmeshConnections(timeDelta);
                        });
                }
            }
        }
        /// <summary>
        /// Updates the path update requests queue
        /// </summary>
        /// <param name="queue">The queue to request for path update</param>
        /// <param name="numQueue">Number of elements in the queue</param>
        private void UpdatePathQueue(Agent[] queue, int numQueue)
        {
            for (int i = 0; i < numQueue; i++)
            {
                queue[i].RequestPathUpdate(this.pathQueue);
            }

            this.pathQueue.Update(MaximumIteratorsPerUpdate);
        }
        /// <summary>
        /// Updates the topology optimization queue
        /// </summary>
        /// <param name="queue">The queue</param>
        /// <param name="numQueue">Number of elements in the queue</param>
        private void UpdateOptimizationQueue(Agent[] queue, int numQueue)
        {
            for (int i = 0; i < numQueue; i++)
            {
                queue[i].OptimizePathTopology();
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
            var agent = new Agent(this, parameters);

            //Find nearest position on the navmesh and place the agent there
            PathPoint nearest;
            Vector3 extents = this.Extents;
            if (this.NavQuery.FindNearestPoly(ref position, ref extents, out nearest))
            {
                agent.ResetToPosition(nearest.Polygon, nearest.Position);
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
            Vector3 extents = this.Extents;
            if (this.NavQuery.FindNearestPoly(ref position, ref extents, out startPt))
            {
                for (int i = 0; i < this.agents.Count; i++)
                {
                    //Pick a new random point that is within a certain radius of the current point
                    PathPoint newPt;
                    if (this.NavQuery.FindRandomPointAroundCircle(ref startPt, radius, out newPt))
                    {
                        //Give this agent a target point
                        this.agents[i].RequestMoveTarget(newPt.Polygon, newPt.Position);
                    }
                }
            }
        }
    }
}
