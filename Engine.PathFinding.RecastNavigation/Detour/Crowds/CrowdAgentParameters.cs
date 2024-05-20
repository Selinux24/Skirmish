
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Configuration parameters for a crowd agent.
    /// </summary>
    public class CrowdAgentParameters : ICrowdAgentParameters
    {
        /// <inheritdoc/>
        public float Radius { get; set; }
        /// <inheritdoc/>
        public float SlowDownRadiusFactor { get; set; } = 2;
        /// <inheritdoc/>
        public float Height { get; set; }
        /// <inheritdoc/>
        public float MaxAcceleration { get; set; }
        /// <inheritdoc/>
        public float MaxSpeed { get; set; }

        /// <inheritdoc/>
        public float CollisionQueryRange { get; set; }
        /// <inheritdoc/>
        public float PathOptimizationRange { get; set; }

        /// <inheritdoc/>
        public float SeparationWeight { get; set; }

        /// <summary>
        /// Flags that impact steering behavior.
        /// </summary>
        public UpdateFlagTypes UpdateFlags { get; set; }

        /// <inheritdoc/>
        public int ObstacleAvoidanceType { get; set; }

        /// <inheritdoc/>
        public int QueryFilterTypeIndex { get; set; }

        /// <inheritdoc/>
        public object UserData { get; set; }
    }
}
