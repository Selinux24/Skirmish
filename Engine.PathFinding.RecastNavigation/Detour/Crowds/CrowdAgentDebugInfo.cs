using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Crowd agent debug information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="maxSamples">Maximum debug samples</param>
    public struct CrowdAgentDebugInfo(int maxSamples)
    {
        private readonly ObstacleAvoidanceDebugData vod = new(maxSamples);

        /// <summary>
        /// Optimization start position
        /// </summary>
        public Vector3 OptStart { get; set; }
        /// <summary>
        /// Optimization end position
        /// </summary>
        public Vector3 OptEnd { get; set; }
        /// <summary>
        /// Obstacle avoidance debug data
        /// </summary>
        public readonly ObstacleAvoidanceDebugData Vod { get => vod; }
    }
}
