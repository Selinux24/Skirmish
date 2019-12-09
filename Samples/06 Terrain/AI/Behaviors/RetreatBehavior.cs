using Engine;
using SharpDX;

namespace Terrain.AI.Behaviors
{
    /// <summary>
    /// Retreat behavior
    /// </summary>
    public class RetreatBehavior : Behavior
    {
        /// <summary>
        /// Rally point
        /// </summary>
        private Vector3 rallyPoint;
        /// <summary>
        /// Retreating position
        /// </summary>
        private Vector3? retreatingPosition = null;
        /// <summary>
        /// Velocity
        /// </summary>
        private float retreatVelocity;

        /// <summary>
        /// Gets the target position
        /// </summary>
        public override Vector3? Target
        {
            get
            {
                return this.retreatingPosition;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="agent">Agent</param>
        public RetreatBehavior(AIAgent agent) : base(agent)
        {

        }

        /// <summary>
        /// Initializes the behavior
        /// </summary>
        /// <param name="rallyPoint">Rally point</param>
        /// <param name="retreatVelocity">Retreat velocity</param>
        public void InitRetreatingBehavior(Vector3 rallyPoint, float retreatVelocity)
        {
            this.rallyPoint = rallyPoint;
            this.retreatingPosition = null;
            this.retreatVelocity = retreatVelocity;
        }

        /// <summary>
        /// Tests wether the current behavior can be executed
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <returns>Returns true if the behavior can be executed</returns>
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
        /// <summary>
        /// Executes the behavior task
        /// </summary>
        /// <param name="gameTime">Game time</param>
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
