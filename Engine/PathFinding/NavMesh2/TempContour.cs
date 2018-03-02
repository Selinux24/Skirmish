using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh2
{
    public class TempContour
    {
        public Trianglei[] verts;
        public int nverts;
        public int cverts;
        public Polygoni poly;
        public int npoly;
        public int cpoly;

        public TempContour(Trianglei[] vbuf, int nvbuf, Polygoni pbuf, int npbuf)
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
