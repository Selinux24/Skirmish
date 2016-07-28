using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// References a <see cref="Span"/> within a <see cref="Heightfield"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SpanReference
    {
        private int x;
        private int y;
        private int index;

        /// <summary>
        /// Gets the X coordinate of the <see cref="Cell"/> that contains the referenced <see cref="Span"/>.
        /// </summary>
        public int X
        {
            get
            {
                return this.x;
            }
        }
        /// <summary>
        /// Gets the Y coordinate of the <see cref="Cell"/> that contains the referenced <see cref="Span"/>.
        /// </summary>
        public int Y
        {
            get
            {
                return this.y;
            }
        }
        /// <summary>
        /// Gets the index of the <see cref="Span"/> within the <see cref="Cell"/> it is contained in.
        /// </summary>
        public int Index
        {
            get
            {
                return this.index;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpanReference"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate of the <see cref="Cell"/> the <see cref="Span"/> is contained in.</param>
        /// <param name="y">The Y coordinate of the <see cref="Cell"/> the <see cref="Span"/> is contained in.</param>
        /// <param name="i">The index of the <see cref="Span"/> within the specified <see cref="Cell"/>.</param>
        public SpanReference(int x, int y, int i)
        {
            this.x = x;
            this.y = y;
            this.index = i;
        }
    }
}
