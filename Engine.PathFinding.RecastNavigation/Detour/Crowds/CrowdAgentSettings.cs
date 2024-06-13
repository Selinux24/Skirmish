
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Agent group settings
    /// </summary>
    public struct CrowdAgentSettings : IGroupAgentSettings
    {
        /// <summary>
        /// Default settings
        /// </summary>
        public static CrowdAgentSettings Default
        {
            get
            {
                return new()
                {
                    OptimizeVisibility = true,
                    OptimizeTopology = true,
                    AnticipateTurns = true,
                    ObstacleAvoidance = true,
                    AvoidanceQuality = 3,
                    Separation = false,
                    SeparationWeight = 0f
                };
            }
        }

        /// <inheritdoc/>
        public float MaxAcceleration { get; set; }
        /// <inheritdoc/>
        public float MaxSpeed { get; set; }

        /// <inheritdoc/>
        public bool ObstacleAvoidance { get; set; }
        /// <inheritdoc/>
        public int AvoidanceQuality { get; set; }
        /// <inheritdoc/>
        public bool Separation { get; set; }
        /// <inheritdoc/>
        public float SeparationWeight { get; set; }

        /// <summary>
        /// Optimize visibility
        /// </summary>
        public bool OptimizeVisibility { get; set; }
        /// <summary>
        /// Optimize topology
        /// </summary>
        public bool OptimizeTopology { get; set; }
        /// <summary>
        /// Anticipate turns
        /// </summary>
        public bool AnticipateTurns { get; set; }
    }
}
