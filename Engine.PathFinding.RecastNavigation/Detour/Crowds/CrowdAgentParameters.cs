
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Configuration parameters for a crowd agent.
    /// </summary>
    public struct CrowdAgentParameters
    {
        /// <summary>
        /// Creates a new crowd agent parameters from the specified agent type.
        /// </summary>
        /// <param name="ag">Agent type</param>
        public static CrowdAgentParameters FromAgent(GraphAgentType ag)
        {
            return new()
            {
                Height = ag.Height,
                Radius = ag.Radius,

                MaxAcceleration = 1f,
                MaxSpeed = 1f,
                SlowDownRadiusFactor = 0f,

                UpdateFlags =
                    UpdateFlagTypes.DT_CROWD_OBSTACLE_AVOIDANCE |
                    UpdateFlagTypes.DT_CROWD_ANTICIPATE_TURNS,
                SeparationWeight = 3,
                ObstacleAvoidanceType = 0,

                CollisionQueryRange = ag.Radius * 12,
                PathOptimizationRange = ag.Radius * 30,
                QueryFilterTypeIndex = 0
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

        /// <summary>
        /// Maximum allowed acceleration.
        /// </summary>
        public float MaxAcceleration { get; set; }
        /// <summary>
        /// Maximum allowed speed.
        /// </summary>
        public float MaxSpeed { get; set; }
        /// <summary>
        /// Slow down agent radius factor
        /// </summary>
        /// <remarks>Multiplied by Readius</remarks>
        public float SlowDownRadiusFactor { get; set; }

        /// <summary>
        /// Flags that impact steering behavior.
        /// </summary>
        public UpdateFlagTypes UpdateFlags { get; set; }
        /// <summary>
        /// How aggresive the agent manager should be at avoiding collisions with this agent.
        /// </summary>
        public float SeparationWeight { get; set; }
        /// <summary>
        /// The index of the avoidance configuration to use for the agent. 
        /// </summary>
        public int ObstacleAvoidanceType { get; set; }

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

        /// <summary>
        /// Updates agent parameters
        /// </summary>
        /// <param name="agentSettings">Agent settings</param>
        public void UpdateSettings(CrowdAgentSettings agentSettings)
        {
            MaxAcceleration = agentSettings.MaxAcceleration;
            MaxSpeed = agentSettings.MaxSpeed;

            UpdateFlagTypes updateFlags = 0;
            int avoidanceQuality = 0;
            float separationWeight = 0;

            if (agentSettings.AnticipateTurns)
            {
                updateFlags |= UpdateFlagTypes.DT_CROWD_ANTICIPATE_TURNS;
            }
            if (agentSettings.OptimizeVisibility)
            {
                updateFlags |= UpdateFlagTypes.DT_CROWD_OPTIMIZE_VIS;
            }
            if (agentSettings.OptimizeTopology)
            {
                updateFlags |= UpdateFlagTypes.DT_CROWD_OPTIMIZE_TOPO;
            }
            if (agentSettings.ObstacleAvoidance)
            {
                updateFlags |= UpdateFlagTypes.DT_CROWD_OBSTACLE_AVOIDANCE;
                avoidanceQuality = agentSettings.AvoidanceQuality;
            }
            if (agentSettings.Separation)
            {
                updateFlags |= UpdateFlagTypes.DT_CROWD_SEPARATION;
                separationWeight = agentSettings.SeparationWeight;
            }

            UpdateFlags = updateFlags;
            ObstacleAvoidanceType = avoidanceQuality;
            SeparationWeight = separationWeight;
        }
    }
}
