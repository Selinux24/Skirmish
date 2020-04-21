using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public class TempContour
    {
        public Int4[] verts { get; set; }
        public int nverts { get; set; }
        public int cverts { get; set; }
        public IndexedPolygon poly { get; set; }
        public int npoly { get; set; }
        public int cpoly { get; set; }

        public TempContour(Int4[] vbuf, int nvbuf, IndexedPolygon pbuf, int npbuf)
        {
            verts = vbuf;
            nverts = 0;
            cverts = nvbuf;
            poly = pbuf;
            npoly = 0;
            cpoly = npbuf;
        }
    }
}
