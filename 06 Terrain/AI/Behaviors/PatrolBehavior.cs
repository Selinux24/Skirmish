using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
    /// <summary>
    /// Patrolling behavior
    /// </summary>
    public class PatrolBehavior : Behavior
    {
        /// <summary>
        /// Patrol check points
        /// </summary>
        private Vector3[] checkPoints = null;
        /// <summary>
        /// Current check point index
        /// </summary>
        private int currentCheckPoint = -1;
        /// <summary>
        /// Time to wait in the check point
        /// </summary>
        private float checkPointTime;
        /// <summary>
        /// Patrolling velocity
        /// </summary>
        private float patrollVelocity;

        /// <summary>
        /// Last check point time
        /// </summary>
        protected float LastCheckPointTime { get; set; } = 0;

        /// <summary>
        /// Gets the target position
        /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public PatrolBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <summary>
        /// Initializes the behavior
        /// </summary>
        /// <param name="checkPoints">Check point list</param>
        /// <param name="checkPointTime">Time to wait in the check point</param>
        /// <param name="patrolVelocity">Velocity</param>
        public void InitPatrollingBehavior(Vector3[] checkPoints, float checkPointTime, float patrolVelocity)
        {
            this.checkPoints = checkPoints;
            this.currentCheckPoint = -1;
            this.checkPointTime = checkPointTime;
            this.LastCheckPointTime = 0;
            this.patrollVelocity = patrolVelocity;
        }

        /// <summary>
        /// Tests wether the current behavior can be executed
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <returns>Returns true if the behavior can be executed</returns>
        public override bool Test(GameTime gameTime)
        {
            if (this.checkPoints != null && this.checkPoints.Length > 0)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Executes the behavior task
        /// </summary>
        /// <param name="gameTime">Game time</param>
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
                this.LastCheckPointTime += gameTime.ElapsedSeconds;

                if (this.LastCheckPointTime > this.checkPointTime)
                {
                    this.LastCheckPointTime = 0;

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
}
