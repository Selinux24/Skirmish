using Engine;
using Engine.Common;
using Engine.PathFinding.NavMesh;

namespace DeferredTest
{
    /// <summary>
    /// Game agent class
    /// </summary>
    public class GameAgent : IUpdatable
    {
        /// <summary>
        /// Model
        /// </summary>
        public Model Model;
        /// <summary>
        /// Controller
        /// </summary>
        public ManipulatorController Controller;
        /// <summary>
        /// Agent type
        /// </summary>
        public NavigationMeshAgentType AgentType;

        /// <summary>
        /// Gets or sets if the agent is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAgent()
        {

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
            this.Controller?.UpdateManipulator(context.GameTime, this.Model?.Manipulator);
        }
    }
}
