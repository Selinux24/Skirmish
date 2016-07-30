using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Represents a cell in a <see cref="CompactHeightField"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CompactHeightFieldCell
    {
        /// <summary>
        /// The starting index of spans in a <see cref="CompactHeightField"/> for this cell.
        /// </summary>
        public int StartIndex;
        /// <summary>
        /// The number of spans in a <see cref="CompactHeightField"/> for this cell.
        /// </summary>
        public int Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompactHeightFieldCell"/> struct.
        /// </summary>
        /// <param name="start">The start index.</param>
        /// <param name="count">The count.</param>
        public CompactHeightFieldCell(int start, int count)
        {
            StartIndex = start;
            Count = count;
        }
    }
}
