using Engine;
using Engine.Common;
using Engine.PathFinding;
using System.Collections.Generic;

namespace Deferred
{
    /// <summary>
    /// Game agent class
    /// </summary>
    public class GameAgent<T> : IUpdatable, IAgent where T : ManipulatorController
    {
        /// <summary>
        /// Model
        /// </summary>
        private readonly ModelInstance model;
        /// <summary>
        /// Controller
        /// </summary>
        private readonly T controller;

        /// <summary>
        /// Agent type
        /// </summary>
        public AgentType AgentType { get; set; }
        /// <summary>
        /// Agent identifier
        /// </summary>
        public int Id { get; set; }
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
        /// <summary>
        /// Gets if the agent has a path to follow
        /// </summary>
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
                return this.model?.Manipulator;
            }
        }
        /// <summary>
        /// Maximum speed
        /// </summary>
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
        public IEnumerable<ISceneLight> Lights
        {
            get
            {
                return this.model?.Lights ?? new ISceneLight[] { };
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAgent(AgentType agentType, ModelInstance model, T controller)
        {
            this.model = model;
            this.controller = controller;
            this.AgentType = agentType;
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
        /// Updates the specified manipulator
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator</param>
        public void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            this.controller.UpdateManipulator(gameTime, manipulator);
        }
        /// <summary>
        /// Follow the specified path
        /// </summary>
        /// <param name="path">Path to follow</param>
        /// <param name="time">Path time</param>
        public void Follow(IControllerPath path, float time = 0)
        {
            this.controller.Follow(path, time);
        }
        /// <summary>
        /// Clears the path
        /// </summary>
        public void Clear()
        {
            this.controller.Clear();
        }
    }
}
