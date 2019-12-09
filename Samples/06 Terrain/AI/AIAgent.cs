using Engine;
using Engine.Common;
using Engine.PathFinding;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Terrain.AI
{
    using Terrain.AI.Behaviors;

    /// <summary>
    /// Artificial intelligence Agent
    /// </summary>
    public abstract class AIAgent : IUpdatable
    {
        /// <summary>
        /// Maximum ticks looking for a route
        /// </summary>
        private const int MaxLookingForRouteTicks = 1000;

        /// <summary>
        /// Current agent behavior
        /// </summary>
        private Behavior currentBehavior = null;
        /// <summary>
        /// Last distance
        /// </summary>
        protected float LastDistance = 0f;
        /// <summary>
        /// Looking for route
        /// </summary>
        private bool lookingForRoute = false;
        /// <summary>
        /// Number of ticks looking for route
        /// </summary>
        private int lookingForRouteTicks = 0;

        /// <summary>
        /// Parent brain
        /// </summary>
        public Brain Parent { get; protected set; }
        /// <summary>
        /// Current stats
        /// </summary>
        public AIStats Stats { get; protected set; }
        /// <summary>
        /// Scene object to manage
        /// </summary>
        public SceneObject SceneObject { get; protected set; }
        /// <summary>
        /// Agent type
        /// </summary>
        public AgentType AgentType { get; protected set; }
        /// <summary>
        /// Controller
        /// </summary>
        public ManipulatorController Controller { get; protected set; }
        /// <summary>
        /// Current AI state
        /// </summary>
        public AIStates CurrentState { get; protected set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator
        {
            get
            {
                return this.SceneObject.Get<ITransformable3D>().Manipulator;
            }
        }
        /// <summary>
        /// Gets the target position
        /// </summary>
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
        /// <summary>
        /// Gets wether the agent is active
        /// </summary>
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
        /// <summary>
        /// Gets wether the agent is visible
        /// </summary>
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
        /// <summary>
        /// Gets or sets the agent maximum speed magnitude
        /// </summary>
        public float MaximumSpeed
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

        /// <summary>
        /// Idle behavior
        /// </summary>
        public IdleBehavior IdleBehavior { get; protected set; }
        /// <summary>
        /// Patrolling behavior
        /// </summary>
        public PatrolBehavior PatrolBehavior { get; protected set; }
        /// <summary>
        /// Attack behavior
        /// </summary>
        public AttackBehavior AttackBehavior { get; protected set; }
        /// <summary>
        /// Retreat behavior
        /// </summary>
        public RetreatBehavior RetreatBehavior { get; protected set; }

        /// <summary>
        /// Moving event handler
        /// </summary>
        public event BehaviorEventHandler Moving;
        /// <summary>
        /// Attacking event handler
        /// </summary>
        public event BehaviorEventHandler Attacking;
        /// <summary>
        /// Damaged event handler
        /// </summary>
        public event BehaviorEventHandler Damaged;
        /// <summary>
        /// Destroyed event handler
        /// </summary>
        public event BehaviorEventHandler Destroyed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="agentType"></param>
        /// <param name="sceneObject"></param>
        /// <param name="stats"></param>
        protected AIAgent(Brain parent, AgentType agentType, SceneObject sceneObject, AIStatsDescription stats)
        {
            this.Parent = parent;
            this.AgentType = agentType;
            this.SceneObject = sceneObject;
            this.Stats = new AIStats(stats);
            this.Controller = new SteerManipulatorController();

            this.IdleBehavior = new IdleBehavior(this);
            this.PatrolBehavior = new PatrolBehavior(this);
            this.AttackBehavior = new AttackBehavior(this);
            this.RetreatBehavior = new RetreatBehavior(this);

            this.CurrentState = AIStates.None;
            this.ChangeState(AIStates.Idle);
        }

        /// <summary>
        /// Updates agent state
        /// </summary>
        /// <param name="context">Updating context</param>
        public void Update(UpdateContext context)
        {
            if (this.CurrentState != AIStates.None)
            {
                this.currentBehavior?.Task(context.GameTime);

                if (this.CurrentState == AIStates.Idle)
                {
                    this.UpdateIdle(context);
                }
                else if (this.CurrentState == AIStates.Patrolling)
                {
                    this.UpdatePatrolling(context);
                }
                else if (this.CurrentState == AIStates.Attacking)
                {
                    this.UpdateAttacking(context);
                }
                else if (this.CurrentState == AIStates.Retreating)
                {
                    this.UpdateRetreating(context);
                }
            }
            else if (this.Stats.Life > 0)
            {
                // If it's alive, set idle state
                this.ChangeState(AIStates.Idle);
            }

            this.Stats.Update(context.GameTime);

            this.UpdateController(context);
        }
        /// <summary>
        /// Updates agent state when on idle state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdateIdle(UpdateContext context)
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
        /// <summary>
        /// Updates agent state when on patrolling state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdatePatrolling(UpdateContext context)
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
        /// <summary>
        /// Updates agent state when on attacking state state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdateAttacking(UpdateContext context)
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
        /// <summary>
        /// Updates agent state when on retreating state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdateRetreating(UpdateContext context)
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
        /// <summary>
        /// Updates model controller
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdateController(UpdateContext context)
        {
            var lastPosition = this.Manipulator.Position;

            this.Controller.UpdateManipulator(context.GameTime, this.Manipulator);

            this.LastDistance += Vector3.Distance(lastPosition, this.Manipulator.Position);
            if (this.LastDistance > 0.2f)
            {
                this.FireMoving(this, null);

                this.LastDistance -= 0.2f;
            }
        }
        /// <summary>
        /// Change state
        /// </summary>
        /// <param name="state"></param>
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
        /// <summary>
        /// Clear state
        /// </summary>
        public void Clear()
        {
            this.ChangeState(AIStates.None);
        }

        /// <summary>
        /// Gets enemy agents on sight
        /// </summary>
        /// <returns>Returns a list with the enemy agents on sight range</returns>
        public virtual AIAgent[] GetEnemiesOnSight()
        {
            var targets = this.Parent.GetTargetsForAgent(this);

            return Array.FindAll(targets, target =>
            {
                if (target.Stats.Life > 0)
                {
                    return this.EnemyOnSight(target);
                }

                return false;
            });
        }
        /// <summary>
        /// Gets wether the specified target is on sight
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Returns true if the target is on sight</returns>
        public virtual bool EnemyOnSight(AIAgent target)
        {
            var p1 = this.Manipulator.Position;
            var p2 = target.Manipulator.Position;

            var s = p2 - p1;
            if (s.Length() < this.Stats.SightDistance)
            {
                float a = Helper.Angle(s, this.Manipulator.Forward);
                if (a < this.Stats.SightAngle)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets wether the specified target is on range
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Returns true if the target is on range</returns>
        public virtual bool EnemyOnAttackRange(AIAgent target)
        {
            if (this.Stats.CurrentWeapon != null)
            {
                var p1 = this.Manipulator.Position;
                var p2 = target.Manipulator.Position;

                var s = p2 - p1;
                if (s.Length() < this.Stats.CurrentWeapon.Range)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets wether the specified target is too hard
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Returns true if the target is too hard</returns>
        public virtual bool IsHardEnemy(AIAgent target)
        {
            if (target?.Stats.CurrentWeapon != null &&
                target?.Stats.CurrentWeapon.Damage > this.Stats.Life &&
                this.Stats.Damage > 0.9f)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Perfomrs an attack to the specified target
        /// </summary>
        /// <param name="target">Target</param>
        public virtual void Attack(AIAgent target)
        {
            if (this.Stats.CurrentWeapon != null)
            {
                float d = this.Stats.CurrentWeapon.Shoot(this.Parent, this, target);
                if (d > 0f)
                {
                    target.GetDamage(this, d);

                    this.FireAttacking(this, target);
                }
            }
        }
        /// <summary>
        /// Gets damage from the specified attacker
        /// </summary>
        /// <param name="attacker">Attacker</param>
        /// <param name="damage">Damage amount</param>
        public virtual void GetDamage(AIAgent attacker, float damage)
        {
            if (this.AttackBehavior.Target == null)
            {
                this.AttackBehavior.SetTarget(attacker);
                this.ChangeState(AIStates.Attacking);
            }

            this.Stats.Life -= Helper.RandomGenerator.NextFloat(0, damage);

            if (this.Stats.PrimaryWeapon != null) this.Stats.PrimaryWeapon.Delay(damage * 0.1f);
            if (this.Stats.SecondaryWeapon != null) this.Stats.SecondaryWeapon.Delay(damage * 0.1f);

            this.FireDamaged(attacker, this);

            if (this.Stats.Life <= 0)
            {
                this.Stats.Life = 0;

                this.Clear();

                this.FireDestroyed(attacker, this);
            }
        }

        /// <summary>
        /// Sets a route to a point
        /// </summary>
        /// <param name="point">Target point</param>
        /// <param name="speed">Speed</param>
        /// <param name="refine">Refine route</param>
        public virtual void SetRouteToPoint(Vector3 point, float speed, bool refine)
        {
            if (this.AgentType != null && this.Parent.Scene != null)
            {
                if (!this.lookingForRoute || this.lookingForRouteTicks > MaxLookingForRouteTicks)
                {
                    this.lookingForRoute = true;
                    this.lookingForRouteTicks = 0;

                    var refineDelta = refine ? speed * 0.1f : 0f;

                    Task.Run(async () =>
                    {
                        await Task.Delay(100);

                        var r = this.Parent.Scene.FindPath(this.AgentType, this.Manipulator.Position, point, true, refineDelta);
                        if (r != null)
                        {
                            this.FollowPath(r, speed);
                        }

                        this.lookingForRoute = false;
                        this.lookingForRouteTicks = 0;
                    });
                }

                this.lookingForRouteTicks++;
            }
        }
        /// <summary>
        /// Follows the specified path
        /// </summary>
        /// <param name="path">Path to follow</param>
        /// <param name="speed">Speed</param>
        public virtual void FollowPath(PathFindingPath path, float speed)
        {
            this.Controller.Follow(new NormalPath(path.ReturnPath.ToArray(), path.Normals.ToArray()));
            this.Controller.MaximumSpeed = speed;
        }
        /// <summary>
        /// Stops the movement controller actions
        /// </summary>
        public virtual void Stop()
        {
            this.Controller.Clear();
            this.Controller.MaximumSpeed = 0f;
        }

        /// <summary>
        /// Fires the moving action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Pasive</param>
        protected virtual void FireMoving(AIAgent active, AIAgent passive)
        {
            this.Moving?.Invoke(this, new BehaviorEventArgs(active, passive));
        }
        /// <summary>
        /// Fires the attacking action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Passive</param>
        protected virtual void FireAttacking(AIAgent active, AIAgent passive)
        {
            this.Attacking?.Invoke(this, new BehaviorEventArgs(active, passive));
        }
        /// <summary>
        /// Fires the damage action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Pasive</param>
        protected virtual void FireDamaged(AIAgent active, AIAgent passive)
        {
            this.Damaged?.Invoke(this, new BehaviorEventArgs(active, passive));
        }
        /// <summary>
        /// Fires the destroyed action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Passive</param>
        protected virtual void FireDestroyed(AIAgent active, AIAgent passive)
        {
            this.Destroyed?.Invoke(this, new BehaviorEventArgs(active, passive));
        }

        /// <summary>
        /// Gets the text representation of the agent
        /// </summary>
        /// <returns>Returns the text representation of the agent</returns>
        public override string ToString()
        {
            return string.Format("{0} -> {1:000.00}", this.CurrentState, this.Stats.Life);
        }
    }
}
