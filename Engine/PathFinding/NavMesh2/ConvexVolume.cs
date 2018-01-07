using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    public struct ConvexVolume
    {
        public const int MaxConvexVolPoints = 12;

        public Vector3[] verts;
        public float hmin;
        public float hmax;
        public int nverts;
        public byte area;
    }
}
