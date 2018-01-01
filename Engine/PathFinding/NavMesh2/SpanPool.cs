
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Span pool
    /// </summary>
    class SpanPool
    {
        /// <summary>
        /// The number of spans allocated per span spool.
        /// </summary>
        public const int SpansPerPool = 2048;

        /// <summary>
        /// The next span pool.
        /// </summary>
        public SpanPool next;
        /// <summary>
        /// Array of spans in the pool.
        /// </summary>
        public Span[] items;
    }
}
