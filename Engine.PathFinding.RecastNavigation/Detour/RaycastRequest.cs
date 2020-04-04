using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Raycast request data
    /// </summary>
    public struct RaycastRequest
    {
        /// <summary>
        /// Start reference
        /// </summary>
        public int StartRef { get; set; }
        /// <summary>
        /// Start position
        /// </summary>
        public Vector3 StartPos { get; set; }
        /// <summary>
        /// End position
        /// </summary>
        public Vector3 EndPos { get; set; }
        /// <summary>
        /// Query filter
        /// </summary>
        public QueryFilter Filter { get; set; }
        /// <summary>
        /// Raycast options
        /// </summary>
        public RaycastOptions Options { get; set; }
        /// <summary>
        /// Maximum nodes in result path
        /// </summary>
        public int MaxPath { get; set; }
    }
}
