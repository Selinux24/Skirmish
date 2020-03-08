using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    public struct SteerTarget
    {
        public Vector3 Position { get; set; }
        public StraightPathFlagTypes Flag { get; set; }
        public int Ref { get; set; }
        public Vector3[] Points { get; set; }
        public int PointCount { get; set; }
    }
}
