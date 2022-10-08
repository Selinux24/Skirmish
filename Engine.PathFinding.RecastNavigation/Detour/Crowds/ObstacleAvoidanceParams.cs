
namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public class ObstacleAvoidanceParams
    {
        public float VelBias { get; set; }
        public float WeightDesVel { get; set; }
        public float WeightCurVel { get; set; }
        public float WeightSide { get; set; }
        public float WeightToi { get; set; }
        public float HorizTime { get; set; }
        public int GridSize { get; set; }
        public int AdaptiveDivs { get; set; }
        public int AdaptiveRings { get; set; }
        public int AdaptiveDepth { get; set; }
    }
}