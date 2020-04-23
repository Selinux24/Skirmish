using System;

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

        /// <summary>
        /// Decompress the tile
        /// </summary>
        /// <returns>Returns the tile layer</returns>
        public TileCacheLayer Decompress()
        {
            TileCacheLayer layer = new TileCacheLayer()
            {
                Header = Header,
                Areas = null,
                Heights = null,
                Cons = null,
                Regs = null,
                RegCount = 0,
            };

            if (Data.Areas != null && Data.Areas.Length > 0)
            {
                layer.Areas = new AreaTypes[Data.Areas.Length];
                Array.Copy(Data.Areas, layer.Areas, Data.Areas.Length);
            }

            if (Data.Heights != null && Data.Heights.Length > 0)
            {
                layer.Heights = new int[Data.Heights.Length];
                Array.Copy(Data.Heights, layer.Heights, Data.Heights.Length);
            }

            if (Data.Connections != null && Data.Connections.Length > 0)
            {
                layer.Cons = new int[Data.Connections.Length];
                Array.Copy(Data.Connections, layer.Cons, Data.Connections.Length);
            }

            return layer;
        }

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
