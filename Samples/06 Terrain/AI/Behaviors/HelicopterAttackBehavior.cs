using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
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

        /// <summary>
        /// Executes the behavior task
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Task(GameTime gameTime)
        {
            if (this.Target != null)
            {
                var model = this.Agent.SceneObject;
                if (model != null)
                {
                    model.Manipulator.RotateTo(this.Target.Value, Vector3.Up, Axis.Y, 0.01f);
                }
            }

            base.Task(gameTime);
        }
    }
}
