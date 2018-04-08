
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Span pool
    /// </summary>
    class SpanPool
    {
        /// <summary>
        /// The next span pool.
        /// </summary>
        public SpanPool next;
        /// <summary>
        /// Array of spans in the pool.
        /// </summary>
        public Span[] items;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpanPool()
        {
            next = null;
            items = Helper.CreateArray(Recast.RC_SPANS_PER_POOL, () =>
            {
                return new Span();
            });
        }
    }
}
