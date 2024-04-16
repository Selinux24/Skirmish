using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Helpers
{
    /// <summary>
    /// Sliced path query data
    /// </summary>
    public class SlicedPathQueryData
    {
        /// <summary>
        /// Status
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Last best node
        /// </summary>
        public Node LastBestNode { get; set; }
        /// <summary>
        /// Last best node cost
        /// </summary>
        public float LastBestNodeCost { get; set; }
        /// <summary>
        /// Squared ray cast limit distance
        /// </summary>
        public float RaycastLimitSqr { get; set; }

        /// <summary>
        /// Start position
        /// </summary>
        public Vector3 StartPos { get; set; }
        /// <summary>
        /// Start reference
        /// </summary>
        public int StartRef { get; set; }
        /// <summary>
        /// End position
        /// </summary>
        public Vector3 EndPos { get; set; }
        /// <summary>
        /// End reference
        /// </summary>
        public int EndRef { get; set; }

        /// <summary>
        /// Query filter
        /// </summary>
        public IGraphQueryFilter Filter { get; set; }
        /// <summary>
        /// Find path options
        /// </summary>
        public FindPathOptions Options { get; set; }
    }
}
