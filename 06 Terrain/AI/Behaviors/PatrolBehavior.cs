using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
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
}
