
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Compact cell
    /// </summary>
    public struct CompactCell
    {
        /// <summary>
        /// Default compact cell
        /// </summary>
        public CompactCell Default
        {
            get
            {
                return new CompactCell()
                {
                    index = 24,
                    count = 8,
                };
            }
        }

        /// <summary>
        /// Index to the first span in the column.
        /// </summary>
        public uint index;
        /// <summary>
        /// Number of spans in the column.
        /// </summary>
        public uint count;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Index {0}; Count {1}", this.index, this.count);
        }
    }
}
