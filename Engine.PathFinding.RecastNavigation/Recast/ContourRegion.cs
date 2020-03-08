
namespace Engine.PathFinding.RecastNavigation.Recast
{
    public class ContourRegion
    {
        public Contour Outline { get; set; }
        public ContourHole[] Holes { get; set; }
        public int NHoles { get; set; }
    }
}
