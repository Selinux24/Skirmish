using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// References a <see cref="HeightFieldSpan"/> within a <see cref="HeightField"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HeightFieldSpanReference
    {
        private int x;
        private int y;
        private int index;

        /// <summary>
        /// Gets the X coordinate of the <see cref="HeightFieldCell"/> that contains the referenced <see cref="HeightFieldSpan"/>.
        /// </summary>
        public int X
        {
            get
            {
                return this.x;
            }
        }
        /// <summary>
        /// Gets the Y coordinate of the <see cref="HeightFieldCell"/> that contains the referenced <see cref="HeightFieldSpan"/>.
        /// </summary>
        public int Y
        {
            get
            {
                return this.y;
            }
        }
        /// <summary>
        /// Gets the index of the <see cref="HeightFieldSpan"/> within the <see cref="HeightFieldCell"/> it is contained in.
        /// </summary>
        public int Index
        {
            get
            {
                return this.index;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightFieldSpanReference"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate of the <see cref="HeightFieldCell"/> the <see cref="HeightFieldSpan"/> is contained in.</param>
        /// <param name="y">The Y coordinate of the <see cref="HeightFieldCell"/> the <see cref="HeightFieldSpan"/> is contained in.</param>
        /// <param name="i">The index of the <see cref="HeightFieldSpan"/> within the specified <see cref="HeightFieldCell"/>.</param>
        public HeightFieldSpanReference(int x, int y, int i)
        {
            this.x = x;
            this.y = y;
            this.index = i;
        }
    }
}
