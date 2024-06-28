
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Height data item
    /// </summary>
    class HeightDataItem
    {
        /// <summary>
        /// X position index
        /// </summary>
        public int X { get; set; } = -1;
        /// <summary>
        /// Y position index
        /// </summary>
        public int Y { get; set; } = -1;
        /// <summary>
        /// Index value
        /// </summary>
        public int I { get; set; } = -1;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"X: {X}; Y: {Y}; Index: {I};";
        }
    }
}
