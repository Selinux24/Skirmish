using Engine;
using Engine.PathFinding;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainTest.AI
{
    public class TankAIAgent : AIAgent
    {
        public TankAIAgent(Brain parent, AgentType agentType, SceneObject sceneObject, AIStatusDescription status) :
            base(parent, agentType, sceneObject, status)
        {
            this.Controller = new TankManipulatorController();
        }

        protected override void AttackingTasks(GameTime gameTime)
        {
            if (this.attackTarget != null)
            {
                if (this.Model.ModelPartCount > 0)
                {
                    this.Model["Turret-mesh"].Manipulator.RotateTo(this.attackTarget.Manipulator.Position, Vector3.Up, true, 0.01f);
                }
            }

            base.AttackingTasks(gameTime);
        }
    }
}
