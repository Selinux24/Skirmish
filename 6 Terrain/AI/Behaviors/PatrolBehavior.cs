using Engine;
using SharpDX;

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

        public PatrolBehavior(Agent agent) : base(agent)
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (this.currentCheckPoint >= 0)
            {
                var currentPosition = this.Agent.Model.Manipulator.Position;

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

        public void SetRoute(Vector3[] checkPoints, float velocity, float checkPointTime, Ground ground)
        {
            this.Route = checkPoints;
            this.Ground = ground;
            this.Velocity = velocity;
            this.CheckPointTime = checkPointTime;
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
            var p = this.Ground.FindPath(this.Agent.AgentType, this.Agent.Model.Manipulator.Position, this.Route[this.currentCheckPoint]);

            this.Agent.Model.Manipulator.Follow(p.ReturnPath.ToArray(), this.Velocity);
            this.Agent.Model.Manipulator.LinearVelocity = this.Velocity;
        }

        public override string ToString()
        {
            return "Patrol";
        }
    }
}
