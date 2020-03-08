
namespace Engine.PathFinding.RecastNavigation.Recast
{
    public class HeightPatch
    {
        public int[] Data { get; set; }
        public int Xmin { get; set; }
        public int Ymin { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public HeightPatch()
        {
            Data = null;
            Xmin = Ymin = Width = Height = 0;
        }
    }
}
