using Engine;
using Engine.Common;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using System.Collections.Generic;

namespace TerrainSamples.SceneCrowds
{
    /// <summary>
    /// Game agent class
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    /// <param name="agentType">Agent type</param>
    /// <param name="model">Model</param>
    /// <param name="controller">Controller</param>
    public class GameAgent<TAgent, TController>(string id, string name, TAgent agentType, TController controller, ModelInstance model) : IAgent<TAgent>, IUpdatable, IControllable, ITransformable3D
        where TAgent : GraphAgentType
        where TController : ManipulatorController
    {
        /// <summary>
        /// Model
        /// </summary>
        private readonly ModelInstance model = model;
        /// <summary>
        /// Controller
        /// </summary>
        private readonly TController controller = controller;

        /// <inheritdoc/>
        public string Id { get; private set; } = id;
        /// <inheritdoc/>
        public string Name { get; set; } = name;
        /// <inheritdoc/>
        public Scene Scene { get; private set; }
        /// <inheritdoc/>
        public SceneObjectUsages Usage { get; set; } = SceneObjectUsages.Agent;
        /// <inheritdoc/>
        public int Layer { get; set; } = 0;
        /// <inheritdoc/>
        public bool HasOwner { get { return Owner != null; } }
        /// <inheritdoc/>
        public ISceneObject Owner { get; set; }
        /// <inheritdoc/>
        public TAgent AgentType { get; set; } = agentType;
        /// <summary>
        /// Agent identifier
        /// </summary>
        public CrowdAgent CrowdAgent { get; set; }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public bool HasPath
        {
            get
            {
                return controller.HasPath;
            }
        }
        /// <inheritdoc/>
        public IManipulator3D Manipulator
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
                return model?.Lights ?? [];
            }
        }

        /// <inheritdoc/>
        public void EarlyUpdate(UpdateContext context)
        {
            //Not applicable
        }
        /// <inheritdoc/>
        public void Update(UpdateContext context)
        {
            controller?.UpdateManipulator(context.GameTime, Manipulator);
        }
        /// <inheritdoc/>
        public void LateUpdate(UpdateContext context)
        {
            //Not applicable
        }
        /// <inheritdoc/>
        public void SetManipulator(IManipulator3D manipulator)
        {
            model?.SetManipulator(manipulator);
        }
        /// <inheritdoc/>
        public void UpdateManipulator(IGameTime gameTime, IManipulator3D manipulator)
        {
            controller.UpdateManipulator(gameTime, manipulator);
        }
        /// <inheritdoc/>
        public void Follow(IControllerPath newPath, float time = 0)
        {
            controller.Follow(newPath, time);
        }
        /// <inheritdoc/>
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
