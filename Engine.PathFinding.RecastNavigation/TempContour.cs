using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class TempContour
    {
        public Int4[] verts { get; set; }
        public int nverts { get; set; }
        public int cverts { get; set; }
        public Polygoni poly { get; set; }
        public int npoly { get; set; }
        public int cpoly { get; set; }

        public TempContour(Int4[] vbuf, int nvbuf, Polygoni pbuf, int npbuf)
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
