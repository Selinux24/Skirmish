using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh2
{
    public struct TileCacheContour
    {
        public int nverts;
        public Trianglei[] verts;
        public int reg;
        public TileCacheAreas area;
    }
}
