using System;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Recast navigation file header
    /// </summary>
    [Serializable]
    public class RecastNavigationFileHeader
    {
        /// <summary>
        /// Magin number
        /// </summary>
        public const int MAGIC = 'M' << 24 | 'S' << 16 | 'E' << 8 | 'T';
        /// <summary>
        /// File version
        /// </summary>
        public const int VERSION = 1;

        /// <summary>
        /// Magic
        /// </summary>
        public int Magic;
        /// <summary>
        /// Version
        /// </summary>
        public int Version;
        /// <summary>
        /// Navigation mesh parameters
        /// </summary>
        public NavMeshParams NavMeshParams;
        /// <summary>
        /// Navigation mesh tile count
        /// </summary>
        public int NavMeshTileCount;

        /// <summary>
        /// Has tile cache
        /// </summary>
        public bool WithTileCache;
        /// <summary>
        /// Tile cache parameters
        /// </summary>
        public TileCacheParams TileCacheParams;
        /// <summary>
        /// Tile cache tile count
        /// </summary>
        public int TileCacheTileCount;
    }
}
