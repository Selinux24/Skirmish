using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    using Common;
    using Engine.PathFinding.NavMesh;
    using Engine.PathFinding.NavMesh.Crowds;

    /// <summary>
    /// Agent crowd
    /// </summary>
    public class AgentCrowd
    {
        /// <summary>
        /// Internal crowd
        /// </summary>
        private Crowd crowd = null;
        /// <summary>
        /// Game agent and crowd agend relations
        /// </summary>
        private List<Tuple<SceneObject, Agent>> agents = new List<Tuple<SceneObject, Agent>>();

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

            this.crowd = navigationMesh.AddCrowd(agentType);
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            this.crowd.Update(gameTime.ElapsedSeconds);

            for (int i = 0; i < this.agents.Count; i++)
            {
                var a = this.agents[i];
                var crtl = a.Item1.Get<ManipulatorController>();
                var path = a.Item2.Corners;

                List<Vector3> verts = new List<Vector3>();
                List<Vector3> norms = new List<Vector3>();
                for (int p = 0; p < path.Count; p++)
                {
                    verts.Add(path[p].Point.Position);
                    verts.Add(Vector3.Up);
                }
                NormalPath np = new NormalPath(verts.ToArray(), norms.ToArray());

                crtl.Follow(np);
            }
        }
        /// <summary>
        /// Adds a new agent to agent list
        /// </summary>
        /// <param name="obj">Agent</param>
        /// <param name="agentType">Agent type</param>
        public void AddAgent(SceneObject obj, NavigationMeshAgentType agentType)
        {
            var par = new AgentParams()
            {
                Height = agentType.Height,
                Radius = agentType.Radius,
                MaxSpeed = 13.5f,
                MaxAcceleration = 80f,
                CollisionQueryRange = agentType.Radius * 12f,
                PathOptimizationRange = agentType.Radius * 30f,
            };

            var transform = obj.Get<ITransformable3D>();

            var agent = this.crowd.AddAgent(transform.Manipulator.Position, par);

            this.agents.Add(new Tuple<SceneObject, Agent>(obj, agent));
        }

        public void RemoveAgent(SceneObject obj)
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
            this.crowd.MoveTo(position, radius);
        }
    }
}
