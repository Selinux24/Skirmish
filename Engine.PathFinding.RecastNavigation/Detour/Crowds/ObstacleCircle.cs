using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class ObstacleCircle
    {
        /// <summary>
        /// Position of the obstacle
        /// </summary>
        public Vector3 P { get; set; }
        /// <summary>
        /// Velocity of the obstacle
        /// </summary>
        public Vector3 Vel { get; set; }
        /// <summary>
        /// Desired velocity of the obstacle
        /// </summary>
        public Vector3 DVel { get; set; }
        /// <summary>
        /// Radius of the obstacle
        /// </summary>
        public float Rad { get; set; }
        /// <summary>
        /// Use for side selection during sampling.
        /// </summary>
        public Vector3 Dp { get; set; }
        /// <summary>
        /// Use for side selection during sampling.
        /// </summary>
        public Vector3 Np { get; set; }
    };
}
