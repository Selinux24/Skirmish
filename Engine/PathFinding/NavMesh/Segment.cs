using SharpDX;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A line segment contains two points
    /// </summary>
    public struct Segment
    {
        /// <summary>
        /// Start point
        /// </summary>
        public Vector3 Start;
        /// <summary>
        /// End point
        /// </summary>
        public Vector3 End;
        /// <summary>
        /// Distance for pruning
        /// </summary>
        public float Dist;
    }
}
