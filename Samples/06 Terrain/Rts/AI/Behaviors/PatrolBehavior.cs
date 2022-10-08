﻿using Engine;
using SharpDX;
using System.Linq;

namespace Terrain.Rts.AI.Behaviors
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
        /// Check-point proximity threshold
        /// </summary>
        private readonly float checkPointThr = 10f;

        /// <summary>
        /// Last check point time
        /// </summary>
        protected float LastCheckPointTime { get; set; } = 0;
        /// <summary>
        /// Get the next check point
        /// </summary>
        protected Vector3? NextCheckPoint
        {
            get
            {
                return checkPoints.ElementAtOrDefault(currentCheckPoint);
            }
        }

        /// <summary>
        /// Gets the target position
        /// </summary>
        public override Vector3? Target
        {
            get
            {
                if (currentCheckPoint >= 0)
                {
                    return checkPoints[currentCheckPoint];
                }

                return null;
            }
        }
        /// <summary>
        /// Patrolling velocity
        /// </summary>
        public float PatrollVelocity { get; set; }

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
            currentCheckPoint = -1;
            this.checkPointTime = checkPointTime;
            LastCheckPointTime = 0;
            PatrollVelocity = patrolVelocity;
        }

        /// <summary>
        /// Tests wether the current behavior can be executed
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <returns>Returns true if the behavior can be executed</returns>
        public override bool Test(GameTime gameTime)
        {
            if (checkPoints?.Length > 0)
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

            if (currentCheckPoint < 0)
            {
                currentCheckPoint = 0;

                navigate = true;
            }

            if (!Agent.Controller.HasPath)
            {
                navigate = true;
            }

            float d = Vector3.Distance(Agent.Manipulator.Position, NextCheckPoint.Value);
            if (d < checkPointThr)
            {
                LastCheckPointTime += gameTime.ElapsedSeconds;

                if (LastCheckPointTime > checkPointTime)
                {
                    LastCheckPointTime = 0;

                    Logger.WriteDebug(this, $"Agent {Agent} going to next check-point.");

                    currentCheckPoint++;
                    if (currentCheckPoint > checkPoints.Length - 1)
                    {
                        currentCheckPoint = 0;
                    }

                    navigate = true;
                }
            }

            if (navigate)
            {
                Logger.WriteDebug(this, $"Agent {Agent} looking for path.");

                Agent.SetRouteToPoint(checkPoints[currentCheckPoint], PatrollVelocity, true);
            }
        }

        /// <summary>
        /// Gets whether the agent is waiting in a check-point or not
        /// </summary>
        public bool IsWaitingInCheckPoint()
        {
            float d = Vector3.Distance(Agent.Manipulator.Position, NextCheckPoint.Value);
            if (d < checkPointThr)
            {
                return LastCheckPointTime <= checkPointTime;
            }

            return false;
        }
    }
}
