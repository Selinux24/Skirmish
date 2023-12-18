using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Behaviors
{
    /// <summary>
    /// Helicopter attack behavior
    /// </summary>
    public class HelicopterAttackBehavior : AttackBehavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public HelicopterAttackBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <inheritdoc/>
        public override void Task(IGameTime gameTime)
        {
            if (Target != null)
            {
                var model = Agent.SceneObject;
                model?.Manipulator.RotateTo(Target.Value, Vector3.Up, Axis.Y, 0.01f);
            }

            base.Task(gameTime);
        }
    }
}
