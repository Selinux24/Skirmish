using Engine;
using Engine.PathFinding;
using SharpDX;

namespace Terrain.AI
{
    public class TankAIAgent : AIAgent
    {
        public TankAIAgent(Brain parent, AgentType agentType, SceneObject sceneObject, AIStatusDescription status) :
            base(parent, agentType, sceneObject, status)
        {
            this.Controller = new TankManipulatorController();

            this.AttackBehavior = new TankAttackBehavior(this);
        }

        protected override void FireDamaged(AIAgent active, AIAgent passive)
        {
            base.FireDamaged(active, passive);

            var model = this.SceneObject.Get<Model>();
            if (model != null)
            {
                if (this.Status.Damage > 0.9f)
                {
                    model.TextureIndex = 2;
                }
                else if (this.Status.Damage > 0.2f)
                {
                    model.TextureIndex = 1;
                }
                else
                {
                    model.TextureIndex = 0;
                }
            }
        }
        protected override void FireDestroyed(AIAgent active, AIAgent passive)
        {
            base.FireDestroyed(active, passive);

            var model = this.SceneObject.Get<Model>();
            if (model != null)
            {
                model.TextureIndex = 2;
            }
        }
    }

    public class TankAttackBehavior : AttackBehavior
    {
        public TankAttackBehavior(TankAIAgent agent) : base(agent)
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
