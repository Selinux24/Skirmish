using Engine;
using Engine.PathFinding;
using SharpDX;
using System;

namespace TerrainTest.AI
{
    using TerrainTest.AI.Behaviors;

    class Agent
    {
        private float attackInterval = 5f;
        private float lastAttackTime = 0;
        private float damage = 35f;

        public event AttackBehaviorEventHandler Attacking;
        public event BehaviorEventHandler Damaged;
        public event BehaviorEventHandler Destroyed;

        public Behavior CurrentBehavior { get; set; }
        public Model Model { get; private set; }
        public AgentType AgentType { get; private set; }
        public bool CanAttack
        {
            get
            {
                return this.Life > 0 && this.lastAttackTime > this.attackInterval;
            }
        }
        public float Life { get; private set; }

        public Agent(AgentType agent, Model model)
        {
            this.AgentType = agent;
            this.Model = model;

            this.Life = 100f;
        }

        public void Update(GameTime gameTime)
        {
            if (this.CurrentBehavior != null)
            {
                if (this.CurrentBehavior.Active)
                {
                    this.CurrentBehavior.Update(gameTime);
                }
                else
                {

                }
            }
            else
            {

            }

            this.lastAttackTime += gameTime.ElapsedSeconds;
        }

        public void DoPatrol(Vector3[] route, float velocity, float checkPointTime, Ground ground)
        {
            var behavior = new PatrolBehavior(this);

            behavior.SetRoute(route, velocity, checkPointTime, ground);

            this.CurrentBehavior = behavior;
        }
        public void DoAttack(Agent target)
        {
            var behavior = new AttackBehavior(this);

            behavior.SetTarget(target);

            this.CurrentBehavior = behavior;
        }
        public void Clear()
        {
            this.CurrentBehavior = null;
        }

        public void Attack(Agent target)
        {
            this.FireAttacking(target);

            this.lastAttackTime = 0;
            var d = AgentManager.rnd.NextFloat(0, damage);
            if (AgentManager.rnd.NextFloat(0, 1) > 0.9f) { d *= 2f; }
            target.Life -= AgentManager.rnd.NextFloat(0, d);
            target.lastAttackTime -= d * 0.1f;

            target.FireDamaged();

            if (target.Life <= 0)
            {
                target.FireDestroyed();
            }
        }
        private void FireAttacking(Agent target)
        {
            this.Attacking(this, new AttackEventArgs(target));
        }
        private void FireDamaged()
        {
            this.Damaged(this, new EventArgs());
        }
        private void FireDestroyed()
        {
            this.Destroyed(this, new EventArgs());
        }
    }
}
