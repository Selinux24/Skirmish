using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Contains information about a navigation mesh
    /// </summary>
    public class NavMeshInfo
    {
        public int X;
        public int Y;
        public int Layer;
        public int PolyCount;
        public int VertCount;
        public int MaxLinkCount;

        public int DetailMeshCount;
        public int DetailVertCount;
        public int DetailTriCount;

        public int BvNodeCount;

        public int OffMeshConCount;
        public int OffMeshBase; //index of first polygon which is off-mesh connection

        public float WalkableHeight;
        public float WalkableRadius;
        public float WalkableClimb;
        public BoundingBox Bounds;
        public float BvQuantFactor;
    }
}
