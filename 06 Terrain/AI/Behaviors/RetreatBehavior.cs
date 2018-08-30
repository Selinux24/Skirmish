using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
    public class RetreatBehavior : Behavior
    {
        private Vector3 rallyPoint;
        private Vector3? retreatingPosition = null;
        private float retreatVelocity;

        public override Vector3? Target
        {
            get
            {
                return this.retreatingPosition;
            }
        }

        public RetreatBehavior(AIAgent agent) : base(agent)
        {

        }

        public void InitRetreatingBehavior(Vector3 rallyPoint, float retreatVelocity)
        {
            this.rallyPoint = rallyPoint;
            this.retreatingPosition = null;
            this.retreatVelocity = retreatVelocity;
        }

        public override bool Test(GameTime gameTime)
        {
            if (this.Agent.Manipulator.Position == this.Agent.RetreatBehavior.rallyPoint)
            {
                return false;
            }
            else
            {
                var targets = this.Agent.GetEnemiesOnSight();
                for (int i = 0; i < targets.Length; i++)
                {
                    if (this.Agent.IsHardEnemy(targets[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Task(GameTime gameTime)
        {
            bool retreat = false;

            if (!this.retreatingPosition.HasValue)
            {
                this.retreatingPosition = this.rallyPoint;
                retreat = true;
            }

            if (retreat)
            {
                this.Agent.SetRouteToPoint(this.retreatingPosition.Value, this.retreatVelocity, true);
            }
        }
    }
}
