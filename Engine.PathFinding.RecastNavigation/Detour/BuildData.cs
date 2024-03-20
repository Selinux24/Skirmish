using Engine.PathFinding.RecastNavigation.Recast;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Navigation mesh build data
    /// </summary>
    struct BuildData
    {
        /// <summary>
        /// Height field
        /// </summary>
        internal Heightfield Heightfield { get; set; }
        /// <summary>
        /// Polygon mesh
        /// </summary>
        internal PolyMesh PolyMesh { get; set; }
        /// <summary>
        /// Polygon detail mesh
        /// </summary>
        internal PolyMeshDetail PolyMeshDetail { get; set; }
    }
}
