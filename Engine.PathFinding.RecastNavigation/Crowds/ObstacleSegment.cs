using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Crowds
{
    public class ObstacleSegment
    {
        /// <summary>
        /// End points of the obstacle segment
        /// </summary>
        public Vector3 P { get; set; }
        /// <summary>
        /// End points of the obstacle segment
        /// </summary>
        public Vector3 Q { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Touch { get; set; }
    };
}
