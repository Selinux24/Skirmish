using Engine;
using Engine.PathFinding;
using SharpDX;
using System;

namespace TerrainTest.AI
{
    using TerrainTest.AI.Behaviors;

    class Agent
    {
        public event BehaviorEventHandler Attacking;
        public event BehaviorEventHandler Damaged;
        public event BehaviorEventHandler Destroyed;
        public event BehaviorChangingEventHandler BehaviorBeforeChange;
        public event BehaviorChangingEventHandler BehaviorAfterChange;

        private Behavior currentBehavior = null;
        private float initialLife = 0;

        public Behavior CurrentBehavior
        {
            get
            {
                return this.currentBehavior;
            }
            set
            {
                if (this.currentBehavior != value)
                {
                    var prev = this.currentBehavior;

                    this.currentBehavior = value;

                    this.FireBehaviorAfterChange(this, prev, this.currentBehavior);
                }
            }
        }
        public Model Model { get; private set; }
        public AgentType AgentType { get; private set; }
        public bool CanAttack
        {
            get
            {
                return this.Life > 0 && this.CurrentWeapon.CanShoot;
            }
        }
        public float Life { get; private set; }
        public Weapon PrimaryWeapon { get; set; }
        public Weapon SecondaryWeapon { get; set; }
        public Weapon CurrentWeapon { get; private set; }

        public Agent(AgentType agent, Model model, float life, WeaponDescription primary, WeaponDescription secondary)
        {
            this.AgentType = agent;
            this.Model = model;

            this.initialLife = life;
            this.Life = life;
            this.PrimaryWeapon = new Weapon(primary);
            this.SecondaryWeapon = new Weapon(secondary);

            this.CurrentWeapon = this.PrimaryWeapon;
        }

        public void Update(GameTime gameTime)
        {
            if (this.Life > 0)
            {
                if (this.CurrentBehavior != null && this.CurrentBehavior.Active)
                {
                    this.CurrentBehavior.Update(gameTime);
                }
                else
                {
                    this.FireBehaviorBeforeChange(this, this.CurrentBehavior, null);
                }

                if (this.PrimaryWeapon != null) this.PrimaryWeapon.Update(gameTime);
                if (this.SecondaryWeapon != null) this.SecondaryWeapon.Update(gameTime);
            }
        }

        public void DoNothing()
        {
            this.CurrentBehavior = null;
        }
        public void DoPatrol(Vector3[] route, float velocity, float checkPointTime, Ground ground, Agent[] targets)
        {
            var behavior = new PatrolBehavior(this);

            behavior.SetRoute(route, velocity, checkPointTime, ground, targets);

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
            if (this.CurrentWeapon != null)
            {
                float d = this.CurrentWeapon.Shoot();

                target.GetDamage(this, d);

                this.FireAttacking(this, target);
            }
        }

        public void GetDamage(Agent attacker, float damage)
        {
            this.Life -= AgentManager.rnd.NextFloat(0, damage);

            if (this.PrimaryWeapon != null) this.PrimaryWeapon.Delay(damage * 0.1f);
            if (this.SecondaryWeapon != null) this.SecondaryWeapon.Delay(damage * 0.1f);

            if (this.Life / this.initialLife < 0.1f)
            {
                this.Model.TextureIndex = 2;
            }
            else if (this.Life / this.initialLife < 0.9f)
            {
                this.Model.TextureIndex = 1;
            }
            else
            {
                this.Model.TextureIndex = 0;
            }

            Array.ForEach(this.Model.Lights, l =>
            {
                if (AgentManager.rnd.NextFloat(0, 1) > 0.8f)
                {
                    l.Enabled = false;
                }
            });

            this.FireDamaged(attacker, this);

            if (this.Life <= 0)
            {
                this.Life = 0;

                this.Model.Manipulator.Stop();
                this.Model.TextureIndex = 2;

                Array.ForEach(this.Model.Lights, l => l.Enabled = false);

                this.FireDestroyed(attacker, this);
            }
        }

        private void FireAttacking(Agent active, Agent passive)
        {
            this.Attacking?.Invoke(new BehaviorEventArgs(active, passive));
        }
        private void FireDamaged(Agent active, Agent passive)
        {
            this.Damaged?.Invoke(new BehaviorEventArgs(active, passive));
        }
        private void FireDestroyed(Agent active, Agent passive)
        {
            this.Destroyed?.Invoke(new BehaviorEventArgs(active, passive));
        }

        private void FireBehaviorBeforeChange(Agent active, Behavior previous, Behavior next)
        {
            this.BehaviorBeforeChange?.Invoke(new BehaviorChangingEventArgs(active, previous, next));
        }
        private void FireBehaviorAfterChange(Agent active, Behavior previous, Behavior next)
        {
            this.BehaviorAfterChange?.Invoke(new BehaviorChangingEventArgs(active, previous, next));
        }
    }
}
