using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Behaviors
{
    /// <summary>
    /// Idle behavior
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="agent">Agent</param>
    public class IdleBehavior(AIAgent agent) : Behavior(agent)
    {
        /// <inheritdoc/>
        public override Vector3? Target
        {
            get
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override bool Test(IGameTime gameTime)
        {
            return true;
        }
        /// <inheritdoc/>
        public override void Task(IGameTime gameTime)
        {
            //Do nothing
        }
    }
}
