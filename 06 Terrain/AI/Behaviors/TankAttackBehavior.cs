using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
    public class TankAttackBehavior : AttackBehavior
    {
        public TankAttackBehavior(AIAgent agent) : base(agent)
        {

        }

        public override void Task(GameTime gameTime)
        {
            if (this.Target != null)
            {
                var model = this.Agent.SceneObject.Get<Model>();
                if (model != null)
                {
                    if (model.ModelPartCount > 0)
                    {
                        model["Turret-mesh"].Manipulator.RotateTo(this.Target.Value, Vector3.Up, true, 0.01f);
                    }
                }
            }

            base.Task(gameTime);
        }
    }
}
