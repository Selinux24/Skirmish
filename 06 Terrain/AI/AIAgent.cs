using Engine;
using Engine.Common;
using Engine.PathFinding;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Terrain.AI
{
    using Terrain.AI.Behaviors;

    public abstract class AIAgent : IUpdatable
    {
        private Behavior currentBehavior = null;

        private float lastDistance = 0f;

        private bool lookingForRoute = false;

        public Brain Parent { get; protected set; }
        public AIStatus Status { get; protected set; }
        public SceneObject SceneObject { get; protected set; }
        public AgentType AgentType { get; protected set; }
        public ManipulatorController Controller { get; protected set; }
        public AIStates CurrentState { get; protected set; }
        public Manipulator3D Manipulator
        {
            get
            {
                return this.SceneObject.Get<ITransformable3D>().Manipulator;
            }
        }
        public Vector3? Target
        {
            get
            {
                if (this.CurrentState == AIStates.Idle)
                {
                    return this.IdleBehavior.Target;
                }
                else if (this.CurrentState == AIStates.Patrolling)
                {
                    return this.PatrolBehavior.Target;
                }
                else if (this.CurrentState == AIStates.Attacking)
                {
                    return this.AttackBehavior.Target;
                }
                else if (this.CurrentState == AIStates.Retreating)
                {
                    return this.RetreatBehavior.Target;
                }

                return null;
            }
        }
        public bool Active
        {
            get
            {
                return this.SceneObject.Active;
            }
            set
            {
                this.SceneObject.Active = value;
            }
        }
        public bool Visible
        {
            get
            {
                return this.SceneObject.Visible;
            }
            set
            {
                this.SceneObject.Visible = value;
            }
        }
        public float Speed
        {
            get
            {
                return this.Controller.MaximumSpeed;
            }
            set
            {
                this.Controller.MaximumSpeed = value;
            }
        }

        public IdleBehavior IdleBehavior { get; protected set; }
        public PatrolBehavior PatrolBehavior { get; protected set; }
        public AttackBehavior AttackBehavior { get; protected set; }
        public RetreatBehavior RetreatBehavior { get; protected set; }

        public event BehaviorEventHandler Moving;
        public event BehaviorEventHandler Attacking;
        public event BehaviorEventHandler Damaged;
        public event BehaviorEventHandler Destroyed;

        public AIAgent(Brain parent, AgentType agentType, SceneObject sceneObject, AIStatusDescription status)
        {
            this.Parent = parent;
            this.AgentType = agentType;
            this.SceneObject = sceneObject;
            this.Status = new AIStatus(status);
            this.Controller = new SteerManipulatorController();

            this.IdleBehavior = new IdleBehavior(this);
            this.PatrolBehavior = new PatrolBehavior(this);
            this.AttackBehavior = new AttackBehavior(this);
            this.RetreatBehavior = new RetreatBehavior(this);

            this.CurrentState = AIStates.None;
            this.ChangeState(AIStates.Idle);
        }

        public void Update(UpdateContext context)
        {
            if (this.CurrentState != AIStates.None)
            {
                this.currentBehavior?.Task(context.GameTime);

                if (this.CurrentState == AIStates.Idle)
                {
                    if (this.AttackBehavior.Test(context.GameTime))
                    {
                        this.ChangeState(AIStates.Attacking);
                    }
                    else if (this.RetreatBehavior.Test(context.GameTime))
                    {
                        this.ChangeState(AIStates.Retreating);
                    }
                    else if (this.PatrolBehavior.Test(context.GameTime))
                    {
                        this.ChangeState(AIStates.Patrolling);
                    }
                    else
                    {
                        //Continue doing nothing
                    }
                }
                else if (this.CurrentState == AIStates.Patrolling)
                {
                    if (this.AttackBehavior.Test(context.GameTime))
                    {
                        this.ChangeState(AIStates.Attacking);
                    }
                    else if (this.RetreatBehavior.Test(context.GameTime))
                    {
                        this.ChangeState(AIStates.Retreating);
                    }
                    else if (this.PatrolBehavior.Test(context.GameTime))
                    {
                        //Continue patrolling
                    }
                    else
                    {
                        this.ChangeState(AIStates.Idle);
                    }
                }
                else if (this.CurrentState == AIStates.Attacking)
                {
                    if (this.AttackBehavior.Test(context.GameTime))
                    {
                        //Continue attacking
                    }
                    else if (this.RetreatBehavior.Test(context.GameTime))
                    {
                        this.ChangeState(AIStates.Retreating);
                    }
                    else if (this.PatrolBehavior.Test(context.GameTime))
                    {
                        this.ChangeState(AIStates.Patrolling);
                    }
                    else
                    {
                        this.ChangeState(AIStates.Idle);
                    }
                }
                else if (this.CurrentState == AIStates.Retreating)
                {
                    if (this.RetreatBehavior.Test(context.GameTime))
                    {
                        //Continue the retreat
                    }
                    else if (this.PatrolBehavior.Test(context.GameTime))
                    {
                        this.ChangeState(AIStates.Patrolling);
                    }
                    else
                    {
                        this.ChangeState(AIStates.Idle);
                    }
                }
            }

            this.Status.Update(context.GameTime);

            var lastPosition = this.Manipulator.Position;

            this.Controller.UpdateManipulator(context.GameTime, this.Manipulator);

            this.lastDistance += Vector3.Distance(lastPosition, this.Manipulator.Position);
            if (this.lastDistance > 0.2f)
            {
                this.FireMoving(this, null);

                this.lastDistance -= 0.2f;
            }
        }
        private void ChangeState(AIStates state)
        {
            this.CurrentState = state;

            this.currentBehavior = null;

            if (state == AIStates.Idle)
            {
                this.currentBehavior = this.IdleBehavior;
            }
            else if (state == AIStates.Patrolling)
            {
                this.currentBehavior = this.PatrolBehavior;
            }
            else if (state == AIStates.Attacking)
            {
                this.currentBehavior = this.AttackBehavior;
            }
            else if (state == AIStates.Retreating)
            {
                this.currentBehavior = this.RetreatBehavior;
            }
            else if (state == AIStates.None)
            {
                this.Controller.Clear();
            }
        }
        public void Clear()
        {
            this.ChangeState(AIStates.None);
        }


        public virtual AIAgent[] GetEnemiesOnSight()
        {
            var targets = this.Parent.GetTargetsForAgent(this);

            return Array.FindAll(targets, target =>
            {
                if (target.Status.Life > 0)
                {
                    return this.EnemyOnSight(target);
                }

                return false;
            });
        }
        public virtual bool EnemyOnSight(AIAgent target)
        {
            var p1 = this.Manipulator.Position;
            var p2 = target.Manipulator.Position;

            var s = p2 - p1;
            if (s.Length() < this.Status.SightDistance)
            {
                float a = Helper.Angle(s, this.Manipulator.Forward);
                if (a < this.Status.SightAngle)
                {
                    return true;
                }
            }

            return false;
        }
        public virtual bool EnemyOnAttackRange(AIAgent target)
        {
            if (this.Status.CurrentWeapon != null)
            {
                var p1 = this.Manipulator.Position;
                var p2 = target.Manipulator.Position;

                var s = p2 - p1;
                if (s.Length() < this.Status.CurrentWeapon.Range)
                {
                    return true;
                }
            }

            return false;
        }
        public virtual bool IsHardEnemy(AIAgent target)
        {
            if (target != null)
            {
                if (target.Status.CurrentWeapon != null && target.Status.CurrentWeapon.Damage > this.Status.Life && this.Status.Damage > 0.9f)
                {
                    return true;
                }
            }

            return false;
        }
        public virtual void Attack(AIAgent target)
        {
            if (this.Status.CurrentWeapon != null)
            {
                float d = this.Status.CurrentWeapon.Shoot(this.Parent, this, target);
                if (d > 0f)
                {
                    target.GetDamage(this, d);

                    this.FireAttacking(this, target);
                }
            }
        }
        public virtual void GetDamage(AIAgent attacker, float damage)
        {
            if (this.AttackBehavior.Target == null)
            {
                this.AttackBehavior.SetTarget(attacker);
                this.ChangeState(AIStates.Attacking);
            }

            this.Status.Life -= Helper.RandomGenerator.NextFloat(0, damage);

            if (this.Status.PrimaryWeapon != null) this.Status.PrimaryWeapon.Delay(damage * 0.1f);
            if (this.Status.SecondaryWeapon != null) this.Status.SecondaryWeapon.Delay(damage * 0.1f);

            this.FireDamaged(attacker, this);

            if (this.Status.Life <= 0)
            {
                this.Status.Life = 0;

                this.Clear();

                this.FireDestroyed(attacker, this);
            }
        }

        public virtual void SetRouteToPoint(Vector3 point, float speed, bool refine)
        {
            if (this.AgentType != null & this.Parent.Scene != null)
            {
                var refineDelta = refine ? speed * 0.1f : 0f;

                if (this.lookingForRoute == false)
                {
                    this.lookingForRoute = true;

                    var task = Task.Run(async () =>
                    {
                        await Task.Delay(100);

                        var r = this.Parent.Scene.FindPath(this.AgentType, this.Manipulator.Position, point, true, refineDelta);
                        if (r != null)
                        {
                            this.FollowPath(r, speed);
                        }

                        this.lookingForRoute = false;
                    });
                }
            }
        }
        public virtual void FollowPath(PathFindingPath path, float speed)
        {
            this.Controller.Follow(new NormalPath(path.ReturnPath.ToArray(), path.Normals.ToArray()));
            this.Controller.MaximumSpeed = speed;
        }
        public virtual void Stop()
        {
            this.Controller.Clear();
            this.Controller.MaximumSpeed = 0f;
        }

        protected virtual void FireMoving(AIAgent active, AIAgent passive)
        {
            this.Moving?.Invoke(new BehaviorEventArgs(active, passive));
        }
        protected virtual void FireAttacking(AIAgent active, AIAgent passive)
        {
            this.Attacking?.Invoke(new BehaviorEventArgs(active, passive));
        }
        protected virtual void FireDamaged(AIAgent active, AIAgent passive)
        {
            this.Damaged?.Invoke(new BehaviorEventArgs(active, passive));
        }
        protected virtual void FireDestroyed(AIAgent active, AIAgent passive)
        {
            this.Destroyed?.Invoke(new BehaviorEventArgs(active, passive));
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1:000.00}", this.CurrentState, this.Status.Life);
        }
    }
}
