using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    using Engine.PathFinding.NavMesh;
    using Engine.PathFinding.NavMesh.Crowds;

    /// <summary>
    /// Agent crowd
    /// </summary>
    public class AgentCrowd
    {
        private static int CrowdUpdates = 0;
        private const float CrowdLatencySecs = 10;
        private float CrowdLatency = -1;

        /// <summary>
        /// Internal crowd
        /// </summary>
        private Crowd crowd = null;
        /// <summary>
        /// Game agent and crowd agend relations
        /// </summary>
        private List<Tuple<IAgent, Agent>> agents = new List<Tuple<IAgent, Agent>>();

        /// <summary>
        /// Scene
        /// </summary>
        protected Scene scene = null;
        /// <summary>
        /// Navigation mesh
        /// </summary>
        protected NavigationMesh navigationMesh = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public AgentCrowd(Scene scene, NavigationMesh navigationMesh, AgentType agentType)
        {
            this.scene = scene;
            this.navigationMesh = navigationMesh;

            this.crowd = navigationMesh.AddCrowd(true, agentType);
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            this.agents.ForEach(agent =>
            {
                agent.Item2.Position = agent.Item1.Manipulator.Position;
            });

            this.CrowdLatency += gameTime.ElapsedSeconds;

            if (this.CrowdLatency > CrowdLatencySecs || this.CrowdLatency < 0)
            {
                Console.WriteLine("AgentCrowd.Update {0}", ++CrowdUpdates);

                this.CrowdLatency = 0;

                this.crowd.Update(gameTime.ElapsedSeconds);

                for (int i = 0; i < this.agents.Count; i++)
                {
                    var crowdAgent = this.agents[i];
                    var item = crowdAgent.Item1;
                    var agent = crowdAgent.Item2;

                    if (agent.State == AgentState.Walking && agent.TargetState == TargetState.Valid)
                    {
                        if (!item.HasPath)
                        {
                            List<Vector3> verts = new List<Vector3>();
                            List<Vector3> norms = new List<Vector3>();

                            verts.Add(item.Manipulator.Position);
                            norms.Add(item.Manipulator.Up);

                            var path = agent.GetStraightPath();

                            for (int p = 0; p < path.Count; p++)
                            {
                                var point = path[p];

                                verts.Add(point.Point.Position);
                                norms.Add(Vector3.Up);
                            }

                            var np = new NormalPath(verts.ToArray(), norms.ToArray());
                            item.Follow(np);

                            Console.WriteLine("AgentCrowd[{0}].Follow", i);
                        }
                    }
                    else
                    {
                        item.Clear();

                        Console.WriteLine("AgentCrowd[{0}].Clear", i);
                    }
                }
            }
        }
        /// <summary>
        /// Adds a new agent to agent list
        /// </summary>
        /// <param name="obj">Agent</param>
        /// <param name="agentType">Agent type</param>
        public void AddAgent(IAgent obj)
        {
            var par = new AgentParams()
            {
                Height = obj.AgentType.Height,
                Radius = obj.AgentType.Radius,
                MaxSpeed = 13.5f,
                MaxAcceleration = 80f,
                CollisionQueryRange = obj.AgentType.Radius * 12f,
                PathOptimizationRange = obj.AgentType.Radius * 30f,
            };

            var agent = this.crowd.AddAgent(obj.Manipulator.Position, par);

            this.agents.Add(new Tuple<IAgent, Agent>(obj, agent));
        }
        /// <summary>
        /// Remove agent
        /// </summary>
        /// <param name="obj">Scene object to remove from agent list</param>
        public void RemoveAgent(IAgent obj)
        {
            var agent = this.agents.Find(a => a.Item1 == obj);

            this.crowd.RemoveAgent(agent.Item2);

            this.agents.Remove(agent);
        }
        /// <summary>
        /// Move agents to position
        /// </summary>
        /// <param name="position">New position</param>
        /// <param name="radius">Separation radius between crowd agents</param>
        public void MoveTo(Vector3 position, float radius)
        {
            this.CrowdLatency = -1;

            for (int i = 0; i < this.agents.Count; i++)
            {
                this.agents[i].Item1.Clear();
            }

            this.crowd.MoveTo(position, radius);
        }
    }
}
