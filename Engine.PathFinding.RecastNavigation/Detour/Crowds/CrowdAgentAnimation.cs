using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class CrowdAgentAnimation
    {
        public bool Active { get; set; }
        public Vector3 InitPos { get; set; }
        public Vector3 StartPos { get; set; }
        public Vector3 EndPos { get; set; }
        public int PolyRef { get; set; }
        public float T { get; set; }
        public float TMax { get; set; }
    }
}
