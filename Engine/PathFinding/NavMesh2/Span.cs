
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
        public int smin;
        /// <summary>
        /// The upper limit of the span
        /// </summary>
        public int smax;
        /// <summary>
        /// The area id assigned to the span.
        /// </summary>
        public TileCacheAreas area;
        /// <summary>
        /// The next span higher up in column.
        /// </summary>
        public Span next;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Min {0} Max {1} Area: {2}; Next Span {3};", this.smin, this.smax, this.area, this.next != null);
        }
    }
}
