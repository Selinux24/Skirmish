using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Behaviors
{
    /// <summary>
    /// Idle behavior
    /// </summary>
    public class IdleBehavior : Behavior
    {
        /// <summary>
        /// Gets the target position
        /// </summary>
        public override Vector3? Target
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public IdleBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <summary>
        /// Tests wether the current behavior can be executed
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <returns>Returns true if the behavior can be executed</returns>
        public override bool Test(GameTime gameTime)
        {
            return true;
        }
        /// <summary>
        /// Executes the behavior task
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Task(GameTime gameTime)
        {
            //Do nothing
        }
    }
}
