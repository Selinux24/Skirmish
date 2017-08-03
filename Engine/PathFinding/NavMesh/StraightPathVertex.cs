namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Straight path vertex
    /// </summary>
    public struct StraightPathVertex
    {
        /// <summary>
        /// Path point
        /// </summary>
        public PathPoint Point;
        /// <summary>
        /// Flags
        /// </summary>
        public StraightPathFlags Flags;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="flags">Flags</param>
        public StraightPathVertex(PathPoint point, StraightPathFlags flags)
        {
            Point = point;
            Flags = flags;
        }

        /// <summary>
        /// Gets the string representation of the instance
        /// </summary>
        public override string ToString()
        {
            return string.Format("Point: {0}; Flags: {1}", this.Point, this.Flags);
        }
    }
}
