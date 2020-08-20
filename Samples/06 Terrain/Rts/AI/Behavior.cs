using Engine;
using SharpDX;

namespace Terrain.Rts.AI
{
    /// <summary>
    /// Base behavior
    /// </summary>
    public abstract class Behavior
    {
        /// <summary>
        /// Agent
        /// </summary>
        public AIAgent Agent { get; private set; }
        /// <summary>
        /// Gets the target position
        /// </summary>
        public abstract Vector3? Target { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        protected Behavior(AIAgent agent)
        {
            this.Agent = agent;
        }

        /// <summary>
        /// Tests wether the current behavior can be executed
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <returns>Returns true if the behavior can be executed</returns>
        public abstract bool Test(GameTime gameTime);
        /// <summary>
        /// Executes the behavior task
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public abstract void Task(GameTime gameTime);
    }
}
