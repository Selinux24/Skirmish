using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Data to update tiles
    /// </summary>
    class UpdateTileData
    {
        /// <summary>
        /// X tile position
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y tile position
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"X:{X}; Y:{Y}; Bounds:{BoundingBox};";
        }
    }
}
