using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public struct TileCacheContour
    {
        public int nverts;
        public Int4[] verts;
        public int reg;
        public TileCacheAreas area;
    }
}
