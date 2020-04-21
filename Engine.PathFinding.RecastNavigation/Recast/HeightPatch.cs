using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    public class HeightPatch
    {
        public int[] Data { get; set; }
        public Rectangle Bounds { get; set; }

        public HeightPatch()
        {
            Data = null;
            Bounds = new Rectangle(0, 0, 0, 0);
        }
    }
}
