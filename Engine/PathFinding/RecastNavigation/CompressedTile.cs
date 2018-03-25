
namespace Engine.PathFinding.RecastNavigation
{
    public class CompressedTile
    {
        public TileCacheLayerHeader Header;
        /// <summary>
        /// Counter describing modifications to the tile.
        /// </summary>
        public int Salt;
        public TileCacheLayerData Compressed;
        public int CompressedSize;
        public TileCacheLayerData Data;
        public int DataSize;
        public TileFlags Flags;
        public CompressedTile Next;

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
