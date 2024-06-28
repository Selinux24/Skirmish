using Engine;
using Engine.Common;
using Engine.PathFinding;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace TerrainSamples.SceneRts.AI
{
    using Engine.BuiltIn.Components.Models;
    using TerrainSamples.SceneRts.AI.Behaviors;

    /// <summary>
    /// Artificial intelligence Agent
    /// </summary>
    public abstract class AIAgent
    {
        /// <summary>
        /// Current agent behavior
        /// </summary>
        private Behavior currentBehavior = null;
        /// <summary>
        /// Last distance
        /// </summary>
        protected float LastDistance = 0f;

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
        public Model SceneObject { get; protected set; }
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
        public IManipulator3D Manipulator
        {
            get
            {
                return SceneObject.Manipulator;
            }
        }
        /// <summary>
        /// Gets the target position
        /// </summary>
        public Vector3? Target
        {
            get
            {
                if (CurrentState == AIStates.Idle)
                {
                    return IdleBehavior.Target;
                }
                else if (CurrentState == AIStates.Patrolling)
                {
                    return PatrolBehavior.Target;
                }
                else if (CurrentState == AIStates.Attacking)
                {
                    return AttackBehavior.Target;
                }
                else if (CurrentState == AIStates.Retreating)
                {
                    return RetreatBehavior.Target;
                }

                return null;
            }
        }
        /// <summary>
        /// Gets whether the agent AI is active
        /// </summary>
        public bool ActiveAI { get; set; }
        /// <summary>
        /// Gets whether the agent is active
        /// </summary>
        public bool Active
        {
            get
            {
                return SceneObject.Active;
            }
            set
            {
                SceneObject.Active = value;
            }
        }
        /// <summary>
        /// Gets whether the agent is visible
        /// </summary>
        public bool Visible
        {
            get
            {
                return SceneObject.Visible;
            }
            set
            {
                SceneObject.Visible = value;
            }
        }
        /// <summary>
        /// Gets or sets the agent maximum speed magnitude
        /// </summary>
        public float MaximumSpeed
        {
            get
            {
                return Controller.MaximumSpeed;
            }
            set
            {
                Controller.MaximumSpeed = value;
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
        protected AIAgent(Brain parent, AgentType agentType, Model sceneObject, AIStatsDescription stats)
        {
            Parent = parent;
            AgentType = agentType;
            SceneObject = sceneObject;
            Stats = new AIStats(stats);
            Controller = new SteerManipulatorController();

            IdleBehavior = new IdleBehavior(this);
            PatrolBehavior = new PatrolBehavior(this);
            AttackBehavior = new AttackBehavior(this);
            RetreatBehavior = new RetreatBehavior(this);

            CurrentState = AIStates.None;
            ChangeState(AIStates.Idle);
        }

        /// <summary>
        /// Updates agent state
        /// </summary>
        /// <param name="context">Updating context</param>
        public void Update(IGameTime gameTime)
        {
            Stats.Update(gameTime);

            UpdateController(gameTime);

            if (!ActiveAI)
            {
                return;
            }

            if (CurrentState != AIStates.None)
            {
                currentBehavior?.Task(gameTime);

                if (CurrentState == AIStates.Idle)
                {
                    UpdateIdle(gameTime);
                }
                else if (CurrentState == AIStates.Patrolling)
                {
                    UpdatePatrolling(gameTime);
                }
                else if (CurrentState == AIStates.Attacking)
                {
                    UpdateAttacking(gameTime);
                }
                else if (CurrentState == AIStates.Retreating)
                {
                    UpdateRetreating(gameTime);
                }
            }
            else if (Stats.Life > 0)
            {
                // If it's alive, set idle state
                ChangeState(AIStates.Idle);
            }
        }
        /// <summary>
        /// Updates agent state when on idle state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdateIdle(IGameTime gameTime)
        {
            if (AttackBehavior.Test(gameTime))
            {
                ChangeState(AIStates.Attacking);
            }
            else if (RetreatBehavior.Test(gameTime))
            {
                ChangeState(AIStates.Retreating);
            }
            else if (PatrolBehavior.Test(gameTime))
            {
                ChangeState(AIStates.Patrolling);
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
        private void UpdatePatrolling(IGameTime gameTime)
        {
            if (AttackBehavior.Test(gameTime))
            {
                ChangeState(AIStates.Attacking);
            }
            else if (RetreatBehavior.Test(gameTime))
            {
                ChangeState(AIStates.Retreating);
            }
            else if (PatrolBehavior.Test(gameTime))
            {
                //Continue patrolling
            }
            else
            {
                ChangeState(AIStates.Idle);
            }
        }
        /// <summary>
        /// Updates agent state when on attacking state state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdateAttacking(IGameTime gameTime)
        {
            if (AttackBehavior.Test(gameTime))
            {
                //Continue attacking
            }
            else if (RetreatBehavior.Test(gameTime))
            {
                ChangeState(AIStates.Retreating);
            }
            else if (PatrolBehavior.Test(gameTime))
            {
                ChangeState(AIStates.Patrolling);
            }
            else
            {
                ChangeState(AIStates.Idle);
            }
        }
        /// <summary>
        /// Updates agent state when on retreating state
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdateRetreating(IGameTime gameTime)
        {
            if (RetreatBehavior.Test(gameTime))
            {
                //Continue the retreat
            }
            else if (PatrolBehavior.Test(gameTime))
            {
                ChangeState(AIStates.Patrolling);
            }
            else
            {
                ChangeState(AIStates.Idle);
            }
        }
        /// <summary>
        /// Updates model controller
        /// </summary>
        /// <param name="context">Updating context</param>
        private void UpdateController(IGameTime gameTime)
        {
            var lastPosition = Manipulator.Position;

            Controller.UpdateManipulator(gameTime, Manipulator);

            LastDistance += Vector3.Distance(lastPosition, Manipulator.Position);
            if (LastDistance > 0.2f)
            {
                FireMoving(this, null);

                LastDistance -= 0.2f;
            }
        }
        /// <summary>
        /// Change state
        /// </summary>
        /// <param name="state"></param>
        private void ChangeState(AIStates state)
        {
            CurrentState = state;

            currentBehavior = null;

            if (state == AIStates.Idle)
            {
                currentBehavior = IdleBehavior;
            }
            else if (state == AIStates.Patrolling)
            {
                currentBehavior = PatrolBehavior;
            }
            else if (state == AIStates.Attacking)
            {
                currentBehavior = AttackBehavior;
            }
            else if (state == AIStates.Retreating)
            {
                currentBehavior = RetreatBehavior;
            }
            else if (state == AIStates.None)
            {
                Controller.Clear();
            }
        }
        /// <summary>
        /// Clear state
        /// </summary>
        public void Clear()
        {
            ChangeState(AIStates.None);
        }

        /// <summary>
        /// Gets enemy agents on sight
        /// </summary>
        /// <returns>Returns a list with the enemy agents on sight range</returns>
        public virtual AIAgent[] GetEnemiesOnSight()
        {
            var targets = Parent.GetTargetsForAgent(this);

            return Array.FindAll(targets, target =>
            {
                if (target.Stats.Life > 0)
                {
                    return EnemyOnSight(target);
                }

                return false;
            });
        }
        /// <summary>
        /// Gets whether the specified target is on sight
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Returns true if the target is on sight</returns>
        public virtual bool EnemyOnSight(AIAgent target)
        {
            var p1 = Manipulator.Position;
            var p2 = target.Manipulator.Position;

            var s = p1 - p2;
            if (s.Length() < Stats.SightDistance)
            {
                float a = Helper.Angle(s, Manipulator.Forward);
                if (a < Stats.SightAngle)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets whether the specified target is on range
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Returns true if the target is on range</returns>
        public virtual bool EnemyOnAttackRange(AIAgent target)
        {
            if (Stats.CurrentWeapon != null)
            {
                var p1 = Manipulator.Position;
                var p2 = target.Manipulator.Position;

                var s = p2 - p1;
                if (s.Length() < Stats.CurrentWeapon.Range)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets whether the specified target is too hard
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>Returns true if the target is too hard</returns>
        public virtual bool IsHardEnemy(AIAgent target)
        {
            if (target == null)
            {
                return false;
            }

            if (target.Stats.CurrentWeapon != null &&
                target.Stats.CurrentWeapon.Damage > Stats.Life &&
                Stats.Damage > 0.9f)
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
            if (Stats.CurrentWeapon != null)
            {
                float d = Stats.CurrentWeapon.Shoot(this, target);
                if (d > 0f)
                {
                    target.GetDamage(this, d);

                    FireAttacking(this, target);
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
            if (EnemyOnAttackRange(attacker) && AttackBehavior.Target == null)
            {
                AttackBehavior.SetTarget(attacker);
                ChangeState(AIStates.Attacking);
            }

            Stats.Life -= Helper.RandomGenerator.NextFloat(0, damage);

            Stats.PrimaryWeapon?.Delay(damage * 0.1f);
            Stats.SecondaryWeapon?.Delay(damage * 0.1f);

            FireDamaged(attacker, this);

            if (Stats.Life <= 0)
            {
                Stats.Life = 0;

                Clear();

                FireDestroyed(attacker, this);
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
            if (AgentType == null || Parent.Scene == null)
            {
                return;
            }

            var refineDelta = refine ? MathF.Max(speed * 0.1f, 0.25f) : 0f;

            Task.Run(async () =>
            {
                Logger.WriteDebug(this, $"Agent {AgentType} FindPathAsync.");

                var path = await Parent.Scene.FindPathAsync(AgentType, Manipulator.Position, point, true);
                if (path != null)
                {
                    path.RefinePath(refineDelta);

                    FollowPath(path, speed);
                }
            });
        }
        /// <summary>
        /// Follows the specified path
        /// </summary>
        /// <param name="path">Path to follow</param>
        /// <param name="speed">Speed</param>
        public virtual void FollowPath(PathFindingPath path, float speed)
        {
            Controller.Follow(new NormalPath(path.Positions, path.Normals));
            Controller.MaximumSpeed = speed;
        }
        /// <summary>
        /// Stops the movement controller actions
        /// </summary>
        public virtual void Stop()
        {
            Controller.Clear();
            Controller.MaximumSpeed = 0f;
        }

        /// <summary>
        /// Fires the moving action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Pasive</param>
        protected virtual void FireMoving(AIAgent active, AIAgent passive)
        {
            Moving?.Invoke(this, new BehaviorEventArgs(active, passive));
        }
        /// <summary>
        /// Fires the attacking action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Passive</param>
        protected virtual void FireAttacking(AIAgent active, AIAgent passive)
        {
            Attacking?.Invoke(this, new BehaviorEventArgs(active, passive));
        }
        /// <summary>
        /// Fires the damage action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Pasive</param>
        protected virtual void FireDamaged(AIAgent active, AIAgent passive)
        {
            Damaged?.Invoke(this, new BehaviorEventArgs(active, passive));
        }
        /// <summary>
        /// Fires the destroyed action
        /// </summary>
        /// <param name="active">Active</param>
        /// <param name="passive">Passive</param>
        protected virtual void FireDestroyed(AIAgent active, AIAgent passive)
        {
            Destroyed?.Invoke(this, new BehaviorEventArgs(active, passive));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{CurrentState} -> {Stats.Life:000.00}";
        }
    }
}
