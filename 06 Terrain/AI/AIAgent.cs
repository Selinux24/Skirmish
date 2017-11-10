using Engine;
using Engine.Common;
using Engine.PathFinding;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Terrain.AI
{
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

    public abstract class Behavior
    {
        public AIAgent Agent { get; private set; }

        public abstract Vector3? Target { get; }

        public Behavior(AIAgent agent)
        {
            this.Agent = agent;
        }

        public abstract bool Test(GameTime gameTime);

        public abstract void Task(GameTime gameTime);
    }

    public class IdleBehavior : Behavior
    {
        public override Vector3? Target
        {
            get
            {
                return null;
            }
        }

        public IdleBehavior(AIAgent agent) : base(agent)
        {

        }

        public override bool Test(GameTime gameTime)
        {
            return true;
        }

        public override void Task(GameTime gameTime)
        {
            //Do nothing
        }
    }

    public class PatrolBehavior : Behavior
    {
        private Vector3[] checkPoints = null;
        private int currentCheckPoint = -1;
        private float checkPointTime;
        private float lastCheckPointTime = 0;
        private float patrollVelocity;

        public override Vector3? Target
        {
            get
            {
                if (this.currentCheckPoint >= 0)
                {
                    return this.checkPoints[this.currentCheckPoint];
                }

                return null;
            }
        }

        public PatrolBehavior(AIAgent agent) : base(agent)
        {

        }

        public void InitPatrollingBehavior(Vector3[] checkPoints, float checkPointTime, float patrolVelocity)
        {
            this.checkPoints = checkPoints;
            this.currentCheckPoint = -1;
            this.checkPointTime = checkPointTime;
            this.lastCheckPointTime = 0;
            this.patrollVelocity = patrolVelocity;
        }

        public override bool Test(GameTime gameTime)
        {
            if (this.checkPoints != null && this.checkPoints.Length > 0)
            {
                return true;
            }

            return false;
        }

        public override void Task(GameTime gameTime)
        {
            bool navigate = false;

            var currentPosition = this.Agent.Manipulator.Position;

            if (this.currentCheckPoint < 0)
            {
                this.currentCheckPoint = 0;

                navigate = true;
            }

            if (!this.Agent.Controller.HasPath)
            {
                navigate = true;
            }

            float d = Vector3.Distance(currentPosition, this.checkPoints[this.currentCheckPoint]);
            if (d < 10f)
            {
                this.lastCheckPointTime += gameTime.ElapsedSeconds;

                if (this.lastCheckPointTime > this.checkPointTime)
                {
                    this.lastCheckPointTime = 0;

                    this.currentCheckPoint++;
                    if (this.currentCheckPoint > this.checkPoints.Length - 1)
                    {
                        this.currentCheckPoint = 0;
                    }

                    navigate = true;
                }
            }

            if (navigate)
            {
                this.Agent.SetRouteToPoint(this.checkPoints[this.currentCheckPoint], this.patrollVelocity, true);
            }
        }
    }

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
