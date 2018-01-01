
using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    class Heightfield
    {
        /// <summary>
        /// The width of the heightfield. (Along the x-axis in cell units.)
        /// </summary>
        public int width;
        /// <summary>
        /// The height of the heightfield. (Along the z-axis in cell units.)
        /// </summary>
        public int height;
        /// <summary>
        /// Bounds in world space. [(x, y, z)]
        /// </summary>
        public BoundingBox b;
        /// <summary>
        /// The size of each cell. (On the xz-plane.)
        /// </summary>
        public float cs;
        /// <summary>
        /// The height of each cell. (The minimum increment along the y-axis.)
        /// </summary>
        public float ch;
        /// <summary>
        /// Heightfield of spans (width*height).
        /// </summary>
        public Span[] spans;
        /// <summary>
        /// Linked list of span pools.
        /// </summary>
        public SpanPool[] pools;
        /// <summary>
        /// The next free span.
        /// </summary>
        public Span freelist;
    }
}
