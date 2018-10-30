using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public struct TileCacheContour
    {
        public int NVerts { get; set; }
        public Int4[] Verts { get; set; }
        public int Reg { get; set; }
        public TileCacheAreas Area { get; set; }
    }
}
