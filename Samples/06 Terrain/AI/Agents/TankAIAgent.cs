using Engine;
using Engine.PathFinding;

namespace Terrain.AI.Agents
{
    using Terrain.AI.Behaviors;
    using Terrain.Controllers;

    /// <summary>
    /// Tank agent
    /// </summary>
    public class TankAIAgent : AIAgent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Brain</param>
        /// <param name="agentType">Agent type</param>
        /// <param name="sceneObject">Scene object</param>
        /// <param name="stats">Agent stats</param>
        public TankAIAgent(Brain parent, AgentType agentType, ISceneObject sceneObject, AIStatsDescription stats) :
            base(parent, agentType, sceneObject, stats)
        {
            this.Controller = new TankManipulatorController()
            {
                ArrivingRadius = 1f,
                ArrivingThreshold = 1f,
                MaximumForce = 0.25f,
            };

            this.AttackBehavior = new TankAttackBehavior(this);
        }

        /// <summary>
        /// Fires the damaged action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Passive</param>
        protected override void FireDamaged(AIAgent active, AIAgent passive)
        {
            base.FireDamaged(active, passive);

            var model = this.SceneObject.Get<Model>();
            if (model != null)
            {
                if (this.Stats.Damage > 0.9f)
                {
                    model.TextureIndex = 2;
                }
                else if (this.Stats.Damage > 0.2f)
                {
                    model.TextureIndex = 1;
                }
                else
                {
                    model.TextureIndex = 0;
                }
            }
        }
        /// <summary>
        /// Fires the destroyed action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Passive</param>
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
