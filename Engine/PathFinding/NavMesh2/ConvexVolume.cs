
namespace Engine.PathFinding.NavMesh2
{
    public struct ConvexVolume
    {
        public const int MaxConvexVolPoints = 12;

        public float[] verts;
        public float hmin;
        public float hmax;
        public int nverts;
        public int area;
    }
}
