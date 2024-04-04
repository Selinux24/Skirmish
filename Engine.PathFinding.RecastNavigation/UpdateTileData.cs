using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Data to update tiles
    /// </summary>
    public class UpdateTileData
    {
        /// <summary>
        /// X tile position
        /// </summary>
        public int TX { get; set; }
        /// <summary>
        /// Y tile position
        /// </summary>
        public int TY { get; set; }
        /// <summary>
        /// Tile bounds
        /// </summary>
        public BoundingBox TileBounds { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"TX: {TX}; TY: {TY}; TileBounds: {TileBounds};";
        }
    }
}
