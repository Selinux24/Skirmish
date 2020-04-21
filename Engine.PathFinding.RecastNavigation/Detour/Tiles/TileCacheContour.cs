using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public struct TileCacheContour
    {
        public int NVerts { get; set; }
        public Int4[] Verts { get; set; }
        public int Reg { get; set; }
        public AreaTypes Area { get; set; }
    }
}
