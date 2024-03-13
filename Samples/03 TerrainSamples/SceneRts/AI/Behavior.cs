using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI
{
    /// <summary>
    /// Base behavior
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="agent">Agent</param>
    public abstract class Behavior(AIAgent agent)
    {
        /// <summary>
        /// Agent
        /// </summary>
        public AIAgent Agent { get; private set; } = agent;
        /// <summary>
        /// Gets the target position
        /// </summary>
        public abstract Vector3? Target { get; }

        /// <summary>
        /// Tests whether the current behavior can be executed
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <returns>Returns true if the behavior can be executed</returns>
        public abstract bool Test(IGameTime gameTime);
        /// <summary>
        /// Executes the behavior task
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public abstract void Task(IGameTime gameTime);
    }
}
