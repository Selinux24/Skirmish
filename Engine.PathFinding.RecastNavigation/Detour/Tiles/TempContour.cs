using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public class TempContour
    {
        public Int4[] Verts { get; set; }
        public int Nverts { get; set; }
        public int Cverts { get; set; }
        public IndexedPolygon Poly { get; set; }
        public int Npoly { get; set; }
        public int Cpoly { get; set; }

        public TempContour(Int4[] vbuf, int nvbuf, IndexedPolygon pbuf, int npbuf)
        {
            Verts = vbuf;
            Nverts = 0;
            Cverts = nvbuf;
            Poly = pbuf;
            Npoly = 0;
            Cpoly = npbuf;
        }
    }
}
