
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Rasterize item
    /// </summary>
    public struct RasterizeItem
    {
        /// <summary>
        /// Triangle
        /// </summary>
        public Triangle Triangle { get; set; }
        /// <summary>
        /// Area type
        /// </summary>
        public AreaTypes AreaType { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{AreaType} => {Triangle}";
        }
    }
}
