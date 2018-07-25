using System;

namespace Engine.PathFinding.RecastNavigation
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
        public TileCacheLayerHeader Header;
        /// <summary>
        /// Data
        /// </summary>
        public TileCacheLayerData Data;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", this.Header, this.Data);
        }
    }
}
