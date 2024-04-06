using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;
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

        /// <summary>
        /// Tile origin
        /// </summary>
        public Vector3 Origin { get; set; }
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// Cell height
        /// </summary>
        public float CellHeight { get; set; }
        /// <summary>
        /// Tile-Cache polygon mesh
        /// </summary>
        public TileCachePolyMesh TileCachePolyMesh { get; set; }
    }
}
