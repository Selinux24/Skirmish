using SharpDX;

namespace Engine.PathFinding.NavMesh.Crowds
{
    public struct AgentAnimation
    {
        public bool Active { get; set; }
        public Vector3 InitPos;
        public Vector3 StartPos;
        public Vector3 EndPos;
        public PolyId PolyRef;
        public float T;
        public float TMax;
    }
}
