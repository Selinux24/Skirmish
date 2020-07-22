using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
    /// <summary>
    /// Tank attack behavior
    /// </summary>
    public class TankAttackBehavior : AttackBehavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public TankAttackBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <summary>
        /// Attack task
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <remarks>Rotate turret towards target</remarks>
        public override void Task(GameTime gameTime)
        {
            if (this.Target != null)
            {
                var model = this.Agent.SceneObject;
                if (model?.ModelPartCount > 0)
                {
                    model["Turret-mesh"].Manipulator.RotateTo(this.Target.Value, Vector3.Up, Axis.Y, 0.01f);
                }
            }

            base.Task(gameTime);
        }
    }
}
