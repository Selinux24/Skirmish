using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
    /// <summary>
    /// Attack behavior
    /// </summary>
    public class AttackBehavior : Behavior
    {
        /// <summary>
        /// Target
        /// </summary>
        private AIAgent attackTarget;
        /// <summary>
        /// Target position
        /// </summary>
        private Vector3? attackPosition = null;
        /// <summary>
        /// Attack velocity
        /// </summary>
        private float attackVelocity;
        /// <summary>
        /// Attacking delta distance
        /// </summary>
        private float attakingDeltaDistance = 10;

        /// <summary>
        /// Gets the target position
        /// </summary>
        public override Vector3? Target
        {
            get
            {
                return this.attackPosition;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public AttackBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <summary>
        /// Initilializes the behavior
        /// </summary>
        /// <param name="attackVelocity">Velocity</param>
        /// <param name="attakingDeltaDistance">Delta</param>
        public void InitAttackingBehavior(float attackVelocity, float attakingDeltaDistance)
        {
            this.attackTarget = null;
            this.attackPosition = null;
            this.attackVelocity = attackVelocity;
            this.attakingDeltaDistance = attakingDeltaDistance;
        }

        /// <summary>
        /// Tests wether the current behavior can be executed
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <returns>Returns true if the behavior can be executed</returns>
        public override bool Test(GameTime gameTime)
        {
            bool res = false;

            if (this.attackTarget != null)
            {
                if (this.attackTarget.Stats.Life <= 0)
                {
                    this.attackTarget = null;
                    this.attackPosition = null;
                    return false;
                }

                var onSight = this.Agent.EnemyOnSight(this.attackTarget);
                if (onSight)
                {
                    var onRange = this.Agent.EnemyOnAttackRange(this.attackTarget);
                    if (onRange)
                    {
                        return !this.Agent.IsHardEnemy(this.attackTarget);
                    }
                    else
                    {
                        return false;
                    }
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
        /// <summary>
        /// Executes the behavior task
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Task(GameTime gameTime)
        {
            if (this.attackTarget != null)
            {
                bool onSight = this.Agent.EnemyOnSight(this.attackTarget);
                bool onRange = onSight && this.Agent.EnemyOnAttackRange(this.attackTarget);

                if (!this.attackPosition.HasValue)
                {
                    this.attackPosition = this.attackTarget.Manipulator.Position;
                }
                else
                {
                    float d = Vector3.Distance(this.attackTarget.Manipulator.Position, this.attackPosition.Value);
                    if (d > this.attakingDeltaDistance || !onSight || !onRange)
                    {
                        this.attackPosition = this.attackTarget.Manipulator.Position;
                    }
                }

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

        /// <summary>
        /// Sets the attakcing target
        /// </summary>
        /// <param name="target">Attakcing target</param>
        public void SetTarget(AIAgent target)
        {
            this.attackTarget = target;
            this.attackPosition = target.Manipulator.Position;
        }
    }
}
