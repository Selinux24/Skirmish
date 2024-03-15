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
        /// <summary>
        /// Previous polygon reference
        /// </summary>
        public int? PrevReference { get; set; }

        /// <summary>
        /// Gets whether the request is valid or not
        /// </summary>
        /// <param name="navMesh">Navigation mesh</param>
        public readonly bool IsValid(NavMesh navMesh)
        {
            // Validate input
            if (Filter == null ||
                StartPos.IsInfinity() ||
                EndPos.IsInfinity() ||
                !navMesh.IsValidPolyRef(StartRef) ||
                PrevReference.HasValue && !navMesh.IsValidPolyRef(PrevReference.Value))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Get current, previous and next tile references from the start reference
        /// </summary>
        /// <param name="navMesh">Navigation mesh</param>
        public readonly (TileRef cur, TileRef prev, TileRef next) GetTiles(NavMesh navMesh)
        {
            var cur = navMesh.GetTileAndPolyByRefUnsafe(StartRef);
            var prev = cur;
            var next = cur;

            if (PrevReference.HasValue)
            {
                prev = navMesh.GetTileAndPolyByRefUnsafe(PrevReference.Value);
            }

            return (cur, prev, next);
        }
    }
}
