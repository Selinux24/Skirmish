
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Compact cell
    /// </summary>
    public struct CompactCell
    {
        /// <summary>
        /// Default compact cell
        /// </summary>
        public static CompactCell Default
        {
            get
            {
                return new CompactCell()
                {
                    Index = 24,
                    Count = 8,
                };
            }
        }

        /// <summary>
        /// Index to the first span in the column.
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Number of spans in the column.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override readonly string ToString()
        {
            return $"Index {Index}; Count {Count}";
        }
    }
}
