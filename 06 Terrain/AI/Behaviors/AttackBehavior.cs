using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
    public class AttackBehavior : Behavior
    {
        private AIAgent attackTarget;
        private Vector3? attackPosition = null;
        private float attackVelocity;
        private float attakingDeltaDistance = 10;

        public override Vector3? Target
        {
            get
            {
                return this.attackPosition;
            }
        }

        public AttackBehavior(AIAgent agent) : base(agent)
        {

        }

        public void InitAttackingBehavior(float attackVelocity, float attakingDeltaDistance)
        {
            this.attackTarget = null;
            this.attackPosition = null;
            this.attackVelocity = attackVelocity;
            this.attakingDeltaDistance = attakingDeltaDistance;
        }

        public override bool Test(GameTime gameTime)
        {
            bool res = false;

            if (this.attackTarget != null)
            {
                if (this.attackTarget.Status.Life <= 0)
                {
                    this.attackTarget = null;
                    this.attackPosition = null;
                    return false;
                }

                if (this.Agent.EnemyOnSight(this.attackTarget))
                {
                    return !this.Agent.IsHardEnemy(this.attackTarget);
                }
            }

            var targets = this.Agent.GetEnemiesOnSight();

            if (targets != null && targets.Length > 0)
            {
                this.attackTarget = targets[0];
                this.attackPosition = null;
                res = !this.Agent.IsHardEnemy(this.attackTarget);
            }
            else
            {
                this.attackTarget = null;
                this.attackPosition = null;
                return false;
            }

            return res;
        }

        public override void Task(GameTime gameTime)
        {
            if (this.attackTarget != null)
            {
                if (!this.attackPosition.HasValue)
                {
                    this.attackPosition = this.attackTarget.Manipulator.Position;
                }
                else
                {
                    float d = Vector3.Distance(this.attackTarget.Manipulator.Position, this.attackPosition.Value);
                    if (d > this.attakingDeltaDistance)
                    {
                        this.attackPosition = this.attackTarget.Manipulator.Position;
                    }
                }

                bool onSight = this.Agent.EnemyOnSight(this.attackTarget);
                bool onRange = !onSight ? false : this.Agent.EnemyOnAttackRange(this.attackTarget);

                if (!onRange)
                {
                    float v = this.attackVelocity;

                    float d = Vector3.Distance(this.attackTarget.Manipulator.Position, this.Agent.Manipulator.Position);
                    if (d < 10)
                    {
                        v = this.attackTarget.Controller.Speed;
                    }
                    this.Agent.SetRouteToPoint(this.attackPosition.Value, v, false);
                }
                else
                {
                    this.Agent.Attack(this.attackTarget);

                    this.Agent.Stop();
                }
            }
        }

        public void SetTarget(AIAgent target)
        {
            this.attackTarget = target;
            this.attackPosition = target.Manipulator.Position;
        }
    }
}
