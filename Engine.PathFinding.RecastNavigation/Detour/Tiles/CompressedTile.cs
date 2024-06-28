
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Compressed tile
    /// </summary>
    public class CompressedTile
    {
        /// <summary>
        /// Tile header
        /// </summary>
        public TileCacheLayerHeader Header { get; set; }
        /// <summary>
        /// Tile data
        /// </summary>
        public TileCacheLayerData Data { get; set; }
        /// <summary>
        /// Counter describing modifications to the tile.
        /// </summary>
        public int Salt { get; set; }
        /// <summary>
        /// Tile flags
        /// </summary>
        public CompressedTileFlagTypes Flags { get; set; }
        /// <summary>
        /// Next tile
        /// </summary>
        public CompressedTile Next { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Salt {Salt}; Flags {Flags}; Header {Header}; Data {Data}; Next {Next?.Header}";
        }
    }
}
