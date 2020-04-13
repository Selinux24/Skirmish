
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Shared edge
    /// </summary>
    struct SharedEdge
    {
        /// <summary>
        /// First index
        /// </summary>
        public int A { get; set; }
        /// <summary>
        /// Last index
        /// </summary>
        public int B { get; set; }
        /// <summary>
        /// Share count
        /// </summary>
        public int ShareCount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SharedEdge(int a, int b, int shareCount)
        {
            A = a;
            B = b;
            ShareCount = shareCount;
        }
    }
}
