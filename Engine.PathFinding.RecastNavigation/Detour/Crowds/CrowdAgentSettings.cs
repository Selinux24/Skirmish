
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Agent group settings
    /// </summary>
    public struct CrowdAgentSettings : IGroupAgentSettings
    {
        /// <summary>
        /// Creates a settings set from an agent type.
        /// </summary>
        /// <param name="ag">Agent type</param>
        public static CrowdAgentSettings FromAgent(GraphAgentType ag)
        {
            return new()
            {
                Height = ag.Height,
                Radius = ag.Radius,

                MaxAcceleration = 1f,
                MaxSpeed = 1f,
                SlowDownRadiusFactor = 0f,

                CollisionQueryRange = ag.Radius * 12,
                PathOptimizationRange = ag.Radius * 30,
                QueryFilterTypeIndex = 0,

                ObstacleAvoidance = true,
                AvoidanceQuality = 0,
                Separation = true,
                SeparationWeight = 3,

                OptimizeVisibility = false,
                OptimizeTopology = false,
                AnticipateTurns = true,
            };
        }

        /// <summary>
        /// Agent height.
        /// </summary>
        public float Height { get; set; }
        /// <summary>
        /// Agent radius.
        /// </summary>
        public float Radius { get; set; }

        /// <inheritdoc/>
        public float MaxAcceleration { get; set; }
        /// <inheritdoc/>
        public float MaxSpeed { get; set; }
        /// <summary>
        /// Slow down agent radius factor
        /// </summary>
        /// <remarks>Multiplied by Readius</remarks>
        public float SlowDownRadiusFactor { get; set; }

        /// <summary>
        /// Defines how close a collision element must be before it is considered for steering behaviors.
        /// </summary>
        public float CollisionQueryRange { get; set; }
        /// <summary>
        /// The path visibility optimization range.
        /// </summary>
        public float PathOptimizationRange { get; set; }
        /// <summary>
        /// The index of the query filter used by this agent.
        /// </summary>
        public int QueryFilterTypeIndex { get; set; }

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
