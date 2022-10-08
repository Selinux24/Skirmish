using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    public struct LocalBoundarySegment
    {
        /// <summary>
        /// Segment start
        /// </summary>
        public Vector3 S1 { get; set; }
        /// <summary>
        /// Segment end
        /// </summary>
        public Vector3 S2 { get; set; }
        /// <summary>
        /// Distance for pruning.
        /// </summary>
        public float D { get; set; }
    };
}
