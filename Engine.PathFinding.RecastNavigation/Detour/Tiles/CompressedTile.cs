
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Compressed tile
    /// </summary>
    public class CompressedTile
    {
        /// <summary>
        /// Header
        /// </summary>
        public TileCacheLayerHeader Header { get; set; }
        /// <summary>
        /// Counter describing modifications to the tile.
        /// </summary>
        public int Salt { get; set; }
        /// <summary>
        /// Layer data
        /// </summary>
        public TileCacheLayerData Data { get; set; }
        /// <summary>
        /// Flags
        /// </summary>
        public CompressedTileFlagTypes Flags { get; set; }
        /// <summary>
        /// Next tile
        /// </summary>
        public CompressedTile Next { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Salt {0}; Flags {1}; Header {2} Data {3} Next {4}",
                this.Salt, this.Flags,
                this.Header, this.Data,
                this.Next != null);
        }
    }
}
