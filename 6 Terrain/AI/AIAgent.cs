using Engine;
using Engine.PathFinding;
using SharpDX;
using System;

namespace TerrainTest.AI
{
    public class AIAgent
    {
        protected Brain Parent;
        public AIStatus Status;
        public Model Model;
        public AgentType AgentType;
        public Manipulator3DController Controller;

        delegate void CurrentBehavior(GameTime gameTime);

        CurrentBehavior currentBehavior = null;
        public AIStates CurrentState = AIStates.Idle;

        private Vector3[] checkPoints = null;
        private int currentCheckPoint = -1;
        private float checkPointTime;
        private float lastCheckPointTime;
        private float patrollVelocity;

        private AIAgent attackTarget;
        private Vector3? attackPosition = null;
        private float attackVelocity;
        private float attakingDeltaDistance = 10;

        private Vector3 rallyPoint;
        private Vector3? retreatingPosition = null;
        private float retreatVelocity;

        public Vector3? Target
        {
            get
            {
                if (this.CurrentState == AIStates.Idle)
                {
                    return null;
                }
                else if (this.CurrentState == AIStates.Patrolling)
                {
                    if (currentCheckPoint >= 0)
                    {
                        return this.checkPoints[currentCheckPoint];
                    }
                }
                else if (this.CurrentState == AIStates.Attacking)
                {
                    return this.attackPosition;
                }
                else if (this.CurrentState == AIStates.Retreating)
                {
                    return this.retreatingPosition;
                }

                return null;
            }
        }

        public event BehaviorEventHandler Attacking;
        public event BehaviorEventHandler Damaged;
        public event BehaviorEventHandler Destroyed;

        public AIAgent(Brain parent, AgentType agentType, Model model, AIStatusDescription status)
        {
            this.Parent = parent;
            this.AgentType = agentType;
            this.Model = model;
            this.Status = new AIStatus(status);
            this.Controller = new Manipulator3DController();

            this.ChangeState(AIStates.Idle);
        }

        public void Update(GameTime gameTime)
        {
            if (this.CurrentState == AIStates.None)
            {
                return;
            }

            this.currentBehavior?.Invoke(gameTime);

            if (this.CurrentState == AIStates.Idle)
            {
                if (this.AttackingTest(gameTime))
                {
                    this.ChangeState(AIStates.Attacking);
                }
                else if (this.RetreatingTest(gameTime))
                {
                    this.ChangeState(AIStates.Retreating);
                }
                else if (this.PatrollingTest(gameTime))
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
                if (this.AttackingTest(gameTime))
                {
                    this.ChangeState(AIStates.Attacking);
                }
                else if (this.RetreatingTest(gameTime))
                {
                    this.ChangeState(AIStates.Retreating);
                }
                else if (this.PatrollingTest(gameTime))
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
                if (this.AttackingTest(gameTime))
                {
                    //Continue attacking
                }
                else if (this.RetreatingTest(gameTime))
                {
                    this.ChangeState(AIStates.Retreating);
                }
                else if (this.PatrollingTest(gameTime))
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
                if (this.RetreatingTest(gameTime))
                {
                    //Continue the retreat
                }
                else if (this.PatrollingTest(gameTime))
                {
                    this.ChangeState(AIStates.Patrolling);
                }
                else
                {
                    this.ChangeState(AIStates.Idle);
                }
            }

            this.Status.Update(gameTime);

            this.Controller.UpdateManipulator(gameTime, this.Model.Manipulator);
        }

        public void Clear()
        {
            this.ChangeState(AIStates.None);
        }
        private void ChangeState(AIStates state)
        {
            this.CurrentState = state;

            this.currentBehavior = null;

            if (state == AIStates.Idle)
            {
                this.currentBehavior = this.IdleTasks;
            }
            else if (state == AIStates.Patrolling)
            {
                this.currentBehavior = this.PatrollingTasks;
            }
            else if (state == AIStates.Attacking)
            {
                this.currentBehavior = this.AttackingTasks;
            }
            else if (state == AIStates.Retreating)
            {
                this.currentBehavior = this.RetreatingTasks;
            }
        }


        public void InitPatrollingBehavior(Vector3[] checkPoints, float checkPointTime, float patrolVelocity)
        {
            this.checkPoints = checkPoints;
            this.currentCheckPoint = -1;
            this.checkPointTime = checkPointTime;
            this.lastCheckPointTime = 0;
            this.patrollVelocity = patrolVelocity;
        }
        public void InitAttackingBehavior(float attackVelocity, float attakingDeltaDistance)
        {
            this.attackTarget = null;
            this.attackPosition = null;
            this.attackVelocity = attackVelocity;
            this.attakingDeltaDistance = attakingDeltaDistance;
        }
        public void InitRetreatingBehavior(Vector3 rallyPoint, float retreatVelocity)
        {
            this.rallyPoint = rallyPoint;
            this.retreatingPosition = null;
            this.retreatVelocity = retreatVelocity;
        }

        protected virtual bool PatrollingTest(GameTime gameTime)
        {
            if (this.checkPoints != null && this.checkPoints.Length > 0)
            {
                return true;
            }

            return false;
        }
        protected virtual bool AttackingTest(GameTime gameTime)
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

                if (this.OnSight(this.attackTarget))
                {
                    return !this.IsHardEnemy(this.attackTarget);
                }
            }

            var targets = this.GetEnemiesOnSight();

            if (targets != null && targets.Length > 0)
            {
                this.attackTarget = targets[0];
                this.attackPosition = null;
                res = !this.IsHardEnemy(this.attackTarget);
            }
            else
            {
                this.attackTarget = null;
                this.attackPosition = null;
                return false;
            }

            return res;
        }
        protected virtual bool RetreatingTest(GameTime gameTime)
        {
            if (this.Model.Manipulator.Position == this.rallyPoint)
            {
                return false;
            }
            else
            {
                var targets = this.GetEnemiesOnSight();
                for (int i = 0; i < targets.Length; i++)
                {
                    if (this.IsHardEnemy(targets[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        protected virtual void IdleTasks(GameTime gameTime)
        {
            //Do nothing
        }
        protected virtual void PatrollingTasks(GameTime gameTime)
        {
            bool navigate = false;

            var currentPosition = this.Model.Manipulator.Position;

            if (this.currentCheckPoint < 0)
            {
                this.currentCheckPoint = 0;

                navigate = true;
            }

            if (!this.Controller.HasPath)
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
                this.SetRouteToPoint(this.checkPoints[this.currentCheckPoint], this.patrollVelocity);
            }
        }
        protected virtual void AttackingTasks(GameTime gameTime)
        {
            if (this.attackTarget != null)
            {
                bool chase = false;

                if (!this.attackPosition.HasValue)
                {
                    this.attackPosition = this.attackTarget.Model.Manipulator.Position;
                    chase = true;
                }
                else
                {
                    float d = Vector3.Distance(this.attackTarget.Model.Manipulator.Position, this.attackPosition.Value);
                    if (d > attakingDeltaDistance)
                    {
                        this.attackPosition = this.attackTarget.Model.Manipulator.Position;
                        chase = true;
                    }
                }

                if (chase)
                {
                    this.SetRouteToPoint(this.attackPosition.Value, this.attackVelocity);
                }

                this.Attack(this.attackTarget);
            }
        }
        protected virtual void RetreatingTasks(GameTime gameTime)
        {
            bool retreat = false;

            if (!this.retreatingPosition.HasValue)
            {
                this.retreatingPosition = this.rallyPoint;
                retreat = true;
            }

            if (retreat)
            {
                this.SetRouteToPoint(this.retreatingPosition.Value, this.retreatVelocity);
            }
        }


        protected virtual AIAgent[] GetEnemiesOnSight()
        {
            var targets = this.Parent.GetTargetsForAgent(this);

            return Array.FindAll(targets, target =>
            {
                if (target.Status.Life > 0)
                {
                    return this.OnSight(target);
                }

                return false;
            });
        }
        protected virtual bool IsHardEnemy(AIAgent target)
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
        protected virtual void SetRouteToPoint(Vector3 point, float velocity)
        {
            if (this.AgentType != null & this.Parent.Ground != null)
            {
                var p = this.Parent.Ground.FindPath(this.AgentType, this.Model.Manipulator.Position, point);

                this.Controller.Follow(new SegmentPath(p.ReturnPath.ToArray()));

                this.Model.Manipulator.LinearVelocity = velocity;
            }
        }


        public bool OnSight(AIAgent target)
        {
            var p1 = this.Model.Manipulator.Position;
            var p2 = target.Model.Manipulator.Position;

            var s = p2 - p1;
            if (s.Length() < this.Status.SightDistance)
            {
                float a = Helper.Angle(s, this.Model.Manipulator.Forward);
                if (a < this.Status.SightAngle)
                {
                    return true;
                }
            }

            return false;
        }
        public void Attack(AIAgent target)
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
        public void GetDamage(AIAgent attacker, float damage)
        {
            if (this.attackTarget == null)
            {
                this.attackTarget = attacker;
                this.attackPosition = attacker.Model.Manipulator.Position;
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


        private void FireAttacking(AIAgent active, AIAgent passive)
        {
            this.Attacking?.Invoke(new BehaviorEventArgs(active, passive));
        }
        private void FireDamaged(AIAgent active, AIAgent passive)
        {
            this.Damaged?.Invoke(new BehaviorEventArgs(active, passive));
        }
        private void FireDestroyed(AIAgent active, AIAgent passive)
        {
            this.Destroyed?.Invoke(new BehaviorEventArgs(active, passive));
        }
    }
}
