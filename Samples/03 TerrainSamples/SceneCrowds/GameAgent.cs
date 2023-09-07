using Engine;
using Engine.Common;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using System;
using System.Collections.Generic;

namespace TerrainSamples.SceneCrowds
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

        /// <inheritdoc/>
        public string Id { get; private set; }
        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public Scene Scene { get; private set; }
        /// <inheritdoc/>
        public SceneObjectUsages Usage { get; set; }
        /// <inheritdoc/>
        public int Layer { get; set; }
        /// <inheritdoc/>
        public bool HasOwner { get { return Owner != null; } }
        /// <inheritdoc/>
        public ISceneObject Owner { get; set; }
        /// <summary>
        /// Agent type
        /// </summary>
        public AgentType AgentType { get; set; }
        /// <summary>
        /// Agent identifier
        /// </summary>
        public CrowdAgent CrowdAgent { get; set; }
        /// <summary>
        /// Gets or sets if the agent is active
        /// </summary>
        public bool Active
        {
            get
            {
                return model.Active;
            }
            set
            {
                model.Active = value;
            }
        }
        /// <summary>
        /// Gets or sets if the agent is visible
        /// </summary>
        public bool Visible
        {
            get
            {
                return model.Visible;
            }
            set
            {
                model.Visible = value;
            }
        }
        /// <summary>
        /// Gets if the agent has a path to follow
        /// </summary>
        public bool HasPath
        {
            get
            {
                return controller.HasPath;
            }
        }
        /// <summary>
        /// Gets the agent manipulator
        /// </summary>
        public Manipulator3D Manipulator
        {
            get
            {
                return model?.Manipulator;
            }
        }
        /// <summary>
        /// Maximum speed
        /// </summary>
        public float MaximumSpeed
        {
            get
            {
                return controller.MaximumSpeed;
            }
            set
            {
                controller.MaximumSpeed = value;
            }
        }
        /// <summary>
        /// Gets the agent lights
        /// </summary>
        public IEnumerable<ISceneLight> Lights
        {
            get
            {
                return model?.Lights ?? Array.Empty<ISceneLight>();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="agentType">Agent type</param>
        /// <param name="model">Model</param>
        /// <param name="controller">Controller</param>
        public GameAgent(string id, string name, AgentType agentType, ModelInstance model, T controller)
        {
            Id = id;
            Name = name;
            AgentType = agentType;
            Usage = SceneObjectUsages.Agent;
            Layer = 0;
            this.model = model;
            this.controller = controller;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Upating context</param>
        public void EarlyUpdate(UpdateContext context)
        {
            //Not applicable
        }
        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Upating context</param>
        public void Update(UpdateContext context)
        {
            controller?.UpdateManipulator(context.GameTime, Manipulator);
        }
        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Upating context</param>
        public void LateUpdate(UpdateContext context)
        {
            //Not applicable
        }
        /// <summary>
        /// Updates the specified manipulator
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator</param>
        public void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            controller.UpdateManipulator(gameTime, manipulator);
        }
        /// <summary>
        /// Follow the specified path
        /// </summary>
        /// <param name="newPath">Path to follow</param>
        /// <param name="time">Path time</param>
        public void Follow(IControllerPath newPath, float time = 0)
        {
            controller.Follow(newPath, time);
        }
        /// <summary>
        /// Clears the path
        /// </summary>
        public void Clear()
        {
            controller.Clear();
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new GameAgentState
            {
                Id = Id,
                Controller = controller.GetState(),
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not GameAgentState gameAgentState)
            {
                return;
            }

            Id = gameAgentState.Id;
            controller?.SetState(gameAgentState.Controller);
        }
    }
}
