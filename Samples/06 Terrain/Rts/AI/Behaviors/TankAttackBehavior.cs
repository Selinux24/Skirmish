using Engine;
using SharpDX;

namespace Terrain.Rts.AI.Behaviors
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
            if (Target.HasValue)
            {
                var model = Agent.SceneObject;
                if (model?.ModelPartCount > 0)
                {
                    model.GetModelPartByName("Turret-mesh").Manipulator.RotateTo(Target.Value, Vector3.Up, Axis.Y, 0.01f);
                    model.GetModelPartByName("Barrel-mesh").Manipulator.RotateTo(Target.Value, Vector3.Up, Axis.X, 0.01f);
                }
            }

            base.Task(gameTime);
        }
    }
}
