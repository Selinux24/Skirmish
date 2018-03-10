
namespace Engine.PathFinding.NavMesh2
{
    public class HeightPatch
    {
        public int[] data;
        public int xmin;
        public int ymin;
        public int width;
        public int height;

        public HeightPatch()
        {
            data = null;
            xmin = ymin = width = height = 0;
        }
    }
}
