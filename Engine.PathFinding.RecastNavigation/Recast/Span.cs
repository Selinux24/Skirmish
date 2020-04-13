
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

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Min {0} Max {1} Area: {2}; Next Span {3};", this.SMin, this.SMax, this.Area, this.Next != null);
        }
    }
}
