
namespace Engine.PathFinding.NavMesh2
{
    public class TempContour
    {
        public int[][] verts;
        public int nverts;
        public int cverts;
        public uint[] poly;
        public int npoly;
        public int cpoly;

        public TempContour(int[][] vbuf, int nvbuf, uint[] pbuf, int npbuf)
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
