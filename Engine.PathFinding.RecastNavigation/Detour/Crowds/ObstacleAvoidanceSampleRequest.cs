using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Obstacle avoidance sample request
    /// </summary>
    public class ObstacleAvoidanceSampleRequest
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Pos { get; set; }
        /// <summary>
        /// Radius
        /// </summary>
        public float Rad { get; set; }
        /// <summary>
        /// Maximum speed
        /// </summary>
        public float MaxSpeed { get; set; }
        /// <summary>
        /// Velocity vector
        /// </summary>
        public Vector3 Vel { get; set; }
        /// <summary>
        /// Desired velocity vector
        /// </summary>
        public Vector3 DVel { get; set; }
        /// <summary>
        /// Avoidance parameters
        /// </summary>
        public ObstacleAvoidanceParams Param { get; set; }
        /// <summary>
        /// Avoidance debug data
        /// </summary>
        public ObstacleAvoidanceDebugData Debug { get; set; }
    }
}
