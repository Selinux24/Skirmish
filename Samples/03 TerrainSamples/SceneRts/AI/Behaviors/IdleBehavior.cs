using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Behaviors
{
    /// <summary>
    /// Idle behavior
    /// </summary>
    public class IdleBehavior : Behavior
    {
        /// <inheritdoc/>
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
