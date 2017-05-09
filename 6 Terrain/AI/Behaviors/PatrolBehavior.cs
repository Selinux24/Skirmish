using Engine;
using SharpDX;
using System;

namespace TerrainTest.AI.Behaviors
{
    /// <summary>
    /// Patrol behavior
    /// </summary>
    /// <remarks>
    /// The agent follows the specified patrolling route
    /// </remarks>
    class PatrolBehavior : Behavior
    {
        private int currentCheckPoint = -1;

        private Vector3 lastPosition;
        private float lastPositionElapsed;

        public Vector3[] Route { get; private set; }
        public Ground Ground { get; private set; }
        public float Velocity { get; private set; }
        public float CheckPointTime { get; private set; }
        public Agent[] Targets { get; private set; }
        public Agent CurrentTarget { get; private set; }

        public PatrolBehavior(Agent agent) : base(agent)
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (this.currentCheckPoint >= 0)
            {
                var currentPosition = this.Agent.Model.Manipulator.Position;

                var tList = Array.FindAll(this.Targets, target =>
                {
                    if (target.Life > 0)
                    {
                        var p1 = this.Agent.Model.Manipulator.Position;
                        var p2 = target.Model.Manipulator.Position;

                        return Vector3.Distance(p1, p2) < this.Agent.CurrentWeapon.Range;
                    }

                    return false;
                });

                if (tList.Length > 0)
                {
                    this.CurrentTarget = tList[0];
                    this.Active = false;

                    this.Agent.Model.Manipulator.Stop();
                }
                else
                {
                    if (this.lastPosition != currentPosition)
                    {
                        this.lastPosition = currentPosition;
                        this.lastPositionElapsed = 0;
                    }
                    else
                    {
                        this.lastPositionElapsed += gameTime.ElapsedSeconds;
                    }

                    if (this.lastPositionElapsed > this.CheckPointTime)
                    {
                        this.lastPositionElapsed = 0;

                        this.NextCheckPoint();
                    }
                }
            }
        }

        public void SetRoute(Vector3[] checkPoints, float velocity, float checkPointTime, Ground ground, Agent[] targets)
        {
            this.Route = checkPoints;
            this.Ground = ground;
            this.Velocity = velocity;
            this.CheckPointTime = checkPointTime;
            this.Targets = targets;
            this.Active = true;

            this.currentCheckPoint = 0;

            this.SetRouteToCheckPoint();
        }

        private void NextCheckPoint()
        {
            this.currentCheckPoint++;
            if (this.currentCheckPoint > this.Route.Length - 1)
            {
                this.currentCheckPoint = 0;
            }

            this.SetRouteToCheckPoint();
        }

        private void SetRouteToCheckPoint()
        {
            if (this.Ground != null)
            {
                var p = this.Ground.FindPath(this.Agent.AgentType, this.Agent.Model.Manipulator.Position, this.Route[this.currentCheckPoint]);

                this.Agent.Model.Manipulator.Follow(p.ReturnPath.ToArray(), this.Velocity);
                this.Agent.Model.Manipulator.LinearVelocity = this.Velocity;
            }
            else
            {
                this.Agent.Model.Manipulator.Follow(this.Route, this.Velocity);
                this.Agent.Model.Manipulator.LinearVelocity = this.Velocity;
            }
        }

        public override string ToString()
        {
            return "Patrol";
        }
    }
}
