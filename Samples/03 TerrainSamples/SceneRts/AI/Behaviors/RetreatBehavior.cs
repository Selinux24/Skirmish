﻿using Engine;
using SharpDX;

namespace TerrainSamples.SceneRts.AI.Behaviors
{
    /// <summary>
    /// Retreat behavior
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="agent">Agent</param>
    public class RetreatBehavior(AIAgent agent) : Behavior(agent)
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

        /// <inheritdoc/>
        public override Vector3? Target
        {
            get
            {
                return retreatingPosition;
            }
        }

        /// <summary>
        /// Initializes the behavior
        /// </summary>
        /// <param name="rallyPoint">Rally point</param>
        /// <param name="retreatVelocity">Retreat velocity</param>
        public void InitRetreatingBehavior(Vector3 rallyPoint, float retreatVelocity)
        {
            this.rallyPoint = rallyPoint;
            retreatingPosition = null;
            this.retreatVelocity = retreatVelocity;
        }

        /// <inheritdoc/>
        public override bool Test(IGameTime gameTime)
        {
            if (Agent.Manipulator.Position == Agent.RetreatBehavior.rallyPoint)
            {
                return false;
            }
            else
            {
                var targets = Agent.GetEnemiesOnSight();
                for (int i = 0; i < targets.Length; i++)
                {
                    if (Agent.IsHardEnemy(targets[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        /// <inheritdoc/>
        public override void Task(IGameTime gameTime)
        {
            bool retreat = false;

            if (!retreatingPosition.HasValue)
            {
                retreatingPosition = rallyPoint;
                retreat = true;
            }

            if (retreat)
            {
                Agent.SetRouteToPoint(retreatingPosition.Value, retreatVelocity, true);
            }
        }
    }
}
