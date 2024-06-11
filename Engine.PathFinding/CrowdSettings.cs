
namespace Engine.PathFinding
{
    /// <summary>
    /// Crowd settings
    /// </summary>
    public struct CrowdSettings
    {
        /// <summary>
        /// Default settings
        /// </summary>
        public static CrowdSettings Default
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
        /// <summary>
        /// Obstacle avoidance
        /// </summary>
        public bool ObstacleAvoidance { get; set; }
        /// <summary>
        /// Avoidance quality
        /// </summary>
        public int AvoidanceQuality { get; set; }
        /// <summary>
        /// Separation
        /// </summary>
        public bool Separation { get; set; }
        /// <summary>
        /// Separation weight
        /// </summary>
        public float SeparationWeight { get; set; }
    }
}
