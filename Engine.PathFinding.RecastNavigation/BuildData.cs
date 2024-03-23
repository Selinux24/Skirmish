
namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Navigation mesh build data
    /// </summary>
    struct BuildData
    {
        /// <summary>
        /// Height field
        /// </summary>
        public Heightfield Heightfield { get; set; }
        /// <summary>
        /// Contour set
        /// </summary>
        public ContourSet CountourSet { get; set; }
        /// <summary>
        /// Polygon mesh
        /// </summary>
        public PolyMesh PolyMesh { get; set; }
        /// <summary>
        /// Polygon detail mesh
        /// </summary>
        public PolyMeshDetail PolyMeshDetail { get; set; }
    }
}
