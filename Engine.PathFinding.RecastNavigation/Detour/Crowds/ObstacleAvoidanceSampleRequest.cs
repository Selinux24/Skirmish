using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class ObstacleAvoidanceSampleRequest
    {
        public Vector3 Pos { get; set; }
        public float Rad { get; set; }
        public float VMax { get; set; }
        public Vector3 Vel { get; set; }
        public Vector3 DVel { get; set; }
        public ObstacleAvoidanceParams Param { get; set; }
        public ObstacleAvoidanceDebugData Debug { get; set; }
    }
}
