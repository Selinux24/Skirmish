
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Span
    /// </summary>
    class Span
    {
        public const int SpanHeightBits = 13;
        /// <summary>
        /// Defines the maximum value for smin and smax.
        /// </summary>
        public const int SpanMaxHeight = (1 << SpanHeightBits) - 1;

        /// <summary>
        /// The lower limit of the span
        /// </summary>
        public uint smin;
        /// <summary>
        /// The upper limit of the span
        /// </summary>
        public uint smax;
        /// <summary>
        /// The area id assigned to the span.
        /// </summary>
        public byte area;
        /// <summary>
        /// The next span higher up in column.
        /// </summary>
        public Span next;
    }
}
