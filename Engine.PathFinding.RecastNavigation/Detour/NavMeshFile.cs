using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;

    /// <summary>
    /// Navigation mesh file
    /// </summary>
    [Serializable]
    public struct NavMeshFile
    {
        /// <summary>
        /// Navigation mesh parameters
        /// </summary>
        public NavMeshParams NavMeshParams { get; set; }
        /// <summary>
        /// Mesh data
        /// </summary>
        public List<MeshData> NavMeshData { get; set; }

        /// <summary>
        /// Has tile cache
        /// </summary>
        public bool HasTileCache { get; set; }
        /// <summary>
        /// Tile cache parameters
        /// </summary>
        public TileCacheParams TileCacheParams { get; set; }
        /// <summary>
        /// Tile cache data
        /// </summary>
        public List<TileCacheData> TileCacheData { get; set; }
    }
}
