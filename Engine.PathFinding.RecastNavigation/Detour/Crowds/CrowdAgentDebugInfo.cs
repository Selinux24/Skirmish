using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class CrowdAgentDebugInfo
    {
        public int Idx { get; set; }
        public Vector3 OptStart { get; set; }
        public Vector3 OptEnd { get; set; }
        public ObstacleAvoidanceDebugData Vod { get; set; }
    }
}
