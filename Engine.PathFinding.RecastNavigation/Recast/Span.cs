
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Span
    /// </summary>
    class Span
    {
        /// <summary>
        /// Span height bits
        /// </summary>
        const int SpanHeightBits = 13;
        /// <summary>
        /// Defines the maximum value for smin and smax.
        /// </summary>
        public const int SpanMaxHeight = (1 << SpanHeightBits) - 1;

        /// <summary>
        /// The lower limit of the span
        /// </summary>
        public int SMin;
        /// <summary>
        /// The upper limit of the span
        /// </summary>
        public int SMax;
        /// <summary>
        /// The area id assigned to the span.
        /// </summary>
        public AreaTypes Area;
        /// <summary>
        /// The next span higher up in column.
        /// </summary>
        public Span Next;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Min {SMin} Max {SMax} Area: {Area}; {(Next != null ? "Next =>" : "")};";
        }
    }
}
