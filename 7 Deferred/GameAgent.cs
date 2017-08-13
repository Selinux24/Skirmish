using System;
using Engine;
using Engine.Common;
using Engine.PathFinding;
using Engine.PathFinding.NavMesh;

namespace DeferredTest
{
    /// <summary>
    /// Game agent class
    /// </summary>
    public class GameAgent<T> : IUpdatable, IAgent where T : ManipulatorController
    {
        /// <summary>
        /// Agent type
        /// </summary>
        public NavigationMeshAgentType AgentType { get; set; }
        /// <summary>
        /// Model
        /// </summary>
        private SceneObject model;
        /// <summary>
        /// Controller
        /// </summary>
        private T controller;

        /// <summary>
        /// Gets or sets if the agent is active
        /// </summary>
        public bool Active
        {
            get
            {
                return this.model.Active;
            }
            set
            {
                this.model.Active = value;
            }
        }
        /// <summary>
        /// Gets or sets if the agent is visible
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.model.Visible;
            }
            set
            {
                this.model.Visible = value;
            }
        }

        public bool HasPath
        {
            get
            {
                return this.controller.HasPath;
            }
        }
        /// <summary>
        /// Gets the agent manipulator
        /// </summary>
        public Manipulator3D Manipulator
        {
            get
            {
                return this.model.Get<ITransformable3D>()?.Manipulator;
            }
        }

        public float MaximumSpeed
        {
            get
            {
                return this.controller.MaximumSpeed;
            }
            set
            {
                this.controller.MaximumSpeed = value;
            }
        }
        /// <summary>
        /// Gets the agent lights
        /// </summary>
        public SceneLight[] Lights
        {
            get
            {
                return this.model.Get<Model>()?.Lights;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAgent(NavigationMeshAgentType agentType, SceneObject model, T controller)
        {
            this.model = model;
            this.controller = controller;
            this.AgentType = agentType;
        }
        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Upating context</param>
        public void Update(UpdateContext context)
        {
            this.controller?.UpdateManipulator(context.GameTime, this.Manipulator);
        }
        /// <summary>
        /// Sets a path to follow
        /// </summary>
        /// <param name="path">Path</param>
        public void FollowPath(PathFindingPath path)
        {
            this.controller.Follow(new NormalPath(path.ReturnPath.ToArray(), path.Normals.ToArray()));
        }

        public void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            this.controller.UpdateManipulator(gameTime, manipulator);
        }

        public void Follow(IControllerPath path, float time = 0)
        {
            this.controller.Follow(path, time);
        }

        public void Clear()
        {
            this.controller.Clear();
        }
    }
}
