using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Behaviors
{
    /// <summary>
    /// Helicopter attack behavior
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="agent">Agent</param>
    public class HelicopterAttackBehavior(AIAgent agent) : AttackBehavior(agent)
    {
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
