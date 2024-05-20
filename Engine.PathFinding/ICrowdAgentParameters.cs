﻿
namespace Engine.PathFinding
{
    /// <summary>
    /// Crowd agent parameters interface
    /// </summary>
    public interface ICrowdAgentParameters
    {
        /// <summary>
        /// Agent radius.
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Slow down agent radius factor
        /// </summary>
        /// <remarks>Multiplied by Readius</remarks>
        public float SlowDownRadiusFactor { get; set; }
        /// <summary>
        /// Agent height.
        /// </summary>
        public float Height { get; set; }
        /// <summary>
        /// Maximum allowed acceleration.
        /// </summary>
        public float MaxAcceleration { get; set; }
        /// <summary>
        /// Maximum allowed speed.
        /// </summary>
        public float MaxSpeed { get; set; }

        /// <summary>
        /// Defines how close a collision element must be before it is considered for steering behaviors.
        /// </summary>
        public float CollisionQueryRange { get; set; }
        /// <summary>
        /// The path visibility optimization range.
        /// </summary>
        public float PathOptimizationRange { get; set; }

        /// <summary>
        /// How aggresive the agent manager should be at avoiding collisions with this agent.
        /// </summary>
        public float SeparationWeight { get; set; }

        /// <summary>
        /// The index of the avoidance configuration to use for the agent. 
        /// </summary>
        public int ObstacleAvoidanceType { get; set; }

        /// <summary>
        /// The index of the query filter used by this agent.
        /// </summary>
        public int QueryFilterTypeIndex { get; set; }

        /// <summary>
        /// User defined data attached to the agent.
        /// </summary>
        public object UserData { get; set; }
    }
}
