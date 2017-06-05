using Engine;
using Engine.Common;
using Engine.PathFinding;
using Engine.PathFinding.NavMesh;

namespace DeferredTest
{
    /// <summary>
    /// Game agent class
    /// </summary>
    public class GameAgent<T> : IUpdatable, ITransformable3D where T : ManipulatorController
    {
        /// <summary>
        /// Agent type
        /// </summary>
        public NavigationMeshAgentType AgentType;
        /// <summary>
        /// Model
        /// </summary>
        private Model model;
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
        /// <summary>
        /// Gets the agent manipulator
        /// </summary>
        public Manipulator3D Manipulator
        {
            get
            {
                return this.model.Manipulator;
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
                return this.model.Lights;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAgent(NavigationMeshAgentType agentType, Model model, T controller)
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
            this.controller?.UpdateManipulator(context.GameTime, this.model?.Manipulator);
        }
        /// <summary>
        /// Sets a path to follow
        /// </summary>
        /// <param name="path">Path</param>
        public void FollowPath(PathFindingPath path)
        {
            this.controller.Follow(new NormalPath(path.ReturnPath.ToArray(), path.Normals.ToArray()));
        }
    }
}
