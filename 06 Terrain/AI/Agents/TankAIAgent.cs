using Engine;
using Engine.PathFinding;

namespace Terrain.AI.Agents
{
    using Terrain.AI.Behaviors;
    using Terrain.Controllers;

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
}
