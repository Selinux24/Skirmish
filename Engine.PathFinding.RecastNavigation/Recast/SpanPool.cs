
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
        const int RC_SPANS_PER_POOL = 2048;

        /// <summary>
        /// Array of spans in the pool.
        /// </summary>
        private readonly Span[] items;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpanPool()
        {
            items = Helper.CreateArray(RC_SPANS_PER_POOL, () => { return new Span(); });
        }

        /// <summary>
        /// Adds a new span to the pool
        /// </summary>
        /// <param name="span">Span</param>
        /// <returns>Returns the next free span</returns>
        public Span Add(Span span)
        {
            var freelist = span;

            int itIndex = RC_SPANS_PER_POOL;
            do
            {
                var it = items[--itIndex];
                it.Next = freelist;
                freelist = it;
            }
            while (itIndex > 0);

            return items[itIndex];
        }
    }
}
