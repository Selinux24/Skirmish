using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class ObstacleAvoidanceProcessSampleRequest
    {
        public Vector3 VCand { get; set; }
        public float Cs { get; set; }
        public Vector3 Pos { get; set; }
        public float Rad { get; set; }
        public Vector3 Vel { get; set; }
        public Vector3 DVel { get; set; }
        public float MinPenalty { get; set; }
        public ObstacleAvoidanceDebugData Debug { get; set; }
    }
}
