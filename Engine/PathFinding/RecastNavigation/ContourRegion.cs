
namespace Engine.PathFinding.RecastNavigation
{
    public class ContourRegion
    {
        public Contour outline { get; set; }
        public ContourHole[] holes { get; set; }
        public int nholes { get; set; }
    }
}
