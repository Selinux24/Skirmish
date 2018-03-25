using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    public class TempContour
    {
        public Int4[] verts;
        public int nverts;
        public int cverts;
        public Polygoni poly;
        public int npoly;
        public int cpoly;

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
