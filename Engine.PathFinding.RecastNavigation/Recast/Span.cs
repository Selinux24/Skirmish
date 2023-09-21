
namespace Engine.PathFinding.RecastNavigation.Recast
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
        public int smin;
        /// <summary>
        /// The upper limit of the span
        /// </summary>
        public int smax;
        /// <summary>
        /// The area id assigned to the span.
        /// </summary>
        public AreaTypes area;
        /// <summary>
        /// The next span higher up in column.
        /// </summary>
        public Span next;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Min {smin} Max {smax} Area: {area}; Next Span {next};";
        }
    }
}
