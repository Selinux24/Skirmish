using Engine;
using SharpDX;
using System;

namespace TerrainSamples.SceneRts.AI.Behaviors
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

        /// <inheritdoc/>
        public override Vector3? Target
        {
            get
            {
                return attackPosition;
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
            attackTarget = null;
            attackPosition = null;
            this.attackVelocity = attackVelocity;
            this.attakingDeltaDistance = attakingDeltaDistance;
        }

        /// <inheritdoc/>
        public override bool Test(IGameTime gameTime)
        {
            if (attackTarget != null)
            {
                if (attackTarget.Stats.Life <= 0)
                {
                    attackTarget = null;
                    attackPosition = null;
                }
                else
                {
                    var onSight = Agent.EnemyOnSight(attackTarget);
                    if (onSight)
                    {
                        Logger.WriteDebug(this, $"Agent {Agent} target on sight.");

                        var inRange = Agent.EnemyOnAttackRange(attackTarget);
                        if (inRange)
                        {
                            bool hardEnemy = Agent.IsHardEnemy(attackTarget);

                            Logger.WriteDebug(this, $"Agent {Agent} target in range. Hard enemy {hardEnemy}");

                            return !hardEnemy;
                        }
                        else
                        {
                            Logger.WriteDebug(this, $"Agent {Agent} target not in range.");

                            return false;
                        }
                    }
                }
            }

            Logger.WriteDebug(this, $"Agent {Agent} looking for valid targets.");

            var target = Array.Find(Agent.GetEnemiesOnSight(), e => e != attackTarget);
            if (target != null)
            {
                Logger.WriteDebug(this, $"Agent {Agent} selected target {target}.");

                attackTarget = target;
                attackPosition = null;
                return !Agent.IsHardEnemy(target);
            }
            else
            {
                attackTarget = null;
                attackPosition = null;
                return false;
            }
        }
        /// <inheritdoc/>
        public override void Task(IGameTime gameTime)
        {
            if (attackTarget != null)
            {
                bool onSight = Agent.EnemyOnSight(attackTarget);
                bool onRange = onSight && Agent.EnemyOnAttackRange(attackTarget);

                if (!attackPosition.HasValue)
                {
                    attackPosition = attackTarget.Manipulator.Position;
                }
                else
                {
                    float d = Vector3.Distance(attackTarget.Manipulator.Position, attackPosition.Value);
                    if (d > attakingDeltaDistance || !onSight || !onRange)
                    {
                        attackPosition = attackTarget.Manipulator.Position;
                    }
                }

                if (!onRange)
                {
                    float v = attackVelocity;

                    float d = Vector3.Distance(attackTarget.Manipulator.Position, Agent.Manipulator.Position);
                    if (d < 10)
                    {
                        v = attackTarget.Controller.Speed;
                    }

                    Logger.WriteDebug(this, $"Agent {Agent} not on range. Going to target position.");
                    Agent.SetRouteToPoint(attackPosition.Value, v, false);
                }
                else
                {
                    Logger.WriteDebug(this, $"Agent {Agent} attacking target {attackTarget}.");
                    Agent.Attack(attackTarget);

                    Agent.Stop();
                }
            }
        }

        /// <summary>
        /// Sets the attakcing target
        /// </summary>
        /// <param name="target">Attakcing target</param>
        public void SetTarget(AIAgent target)
        {
            attackTarget = target;
            attackPosition = target.Manipulator.Position;
        }
    }
}
