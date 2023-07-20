using System;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache data
    /// </summary>
    [Serializable]
    public struct TileCacheData
    {
        /// <summary>
        /// Header
        /// </summary>
        public TileCacheLayerHeader Header { get; set; }
        /// <summary>
        /// Data
        /// </summary>
        public TileCacheLayerData Data { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override readonly string ToString()
        {
            return $"{Header} {Data}";
        }
    }
}
