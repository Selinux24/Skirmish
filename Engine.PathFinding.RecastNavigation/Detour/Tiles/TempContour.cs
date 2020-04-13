using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public class TempContour
    {
        public Int4[] Verts { get; set; }
        public int NVerts { get; set; }
        public int CVerts { get; set; }
        public IndexedPolygon Poly { get; set; }
        public int NPoly { get; set; }
        public int CPoly { get; set; }

        public TempContour(Int4[] vbuf, int nvbuf, IndexedPolygon pbuf, int npbuf)
        {
            Verts = vbuf;
            NVerts = 0;
            CVerts = nvbuf;
            Poly = pbuf;
            NPoly = 0;
            CPoly = npbuf;
        }
    }
}
