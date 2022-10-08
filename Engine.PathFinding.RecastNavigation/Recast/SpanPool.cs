
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Span pool
    /// </summary>
    class SpanPool
    {
        /// <summary>
        /// The number of spans allocated per span spool.
        /// </summary>
        public const int RC_SPANS_PER_POOL = 2048;

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
            items = Helper.CreateArray(RC_SPANS_PER_POOL, () =>
            {
                return new Span();
            });
        }
    }
}
