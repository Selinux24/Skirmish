
namespace Engine.PathFinding.NavMesh2
{
    public struct CompactCell
    {
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
    }
}
