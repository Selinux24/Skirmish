using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Obstacle avoidance process sample request
    /// </summary>
    public struct ObstacleAvoidanceProcessSampleRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public Vector3 VCand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float Cs { get; set; }
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Pos { get; set; }
        /// <summary>
        /// Radius
        /// </summary>
        public float Rad { get; set; }
        /// <summary>
        /// Velocity vector
        /// </summary>
        public Vector3 Vel { get; set; }
        /// <summary>
        /// Desired velocity vector
        /// </summary>
        public Vector3 DVel { get; set; }
        /// <summary>
        /// Minimum penalty
        /// </summary>
        public float MinPenalty { get; set; }
    }
}
