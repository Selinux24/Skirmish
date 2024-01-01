
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Indexed edge of PolyMesh
    /// </summary>
    struct IndexedEdge
    {
        /// <summary>
        /// Edge index
        /// </summary>
        public int EdgeIndex;
        /// <summary>
        /// Shared edge count
        /// </summary>
        public int ShareCount;

        /// <summary>
        /// Constructor
        /// </summary>
        public IndexedEdge(int edgeIndex, int shareCount)
        {
            EdgeIndex = edgeIndex;
            ShareCount = shareCount;
        }

        /// <summary>
        /// Gets whether the specified edge index, exist in the edge definition
        /// </summary>
        /// <param name="edges">Index list</param>
        /// <param name="nedges">Number of edges</param>
        /// <param name="edgeIndex">Edge index</param>
        /// <remarks>Increments the edge share count</remarks>
        public static bool Exists(IndexedEdge[] edges, int nedges, int edgeIndex)
        {
            for (int m = 0; m < nedges; ++m)
            {
                var e = edges[m];
                if (e.EdgeIndex == edgeIndex)
                {
                    // Exists, increment vertex share count.
                    e.ShareCount++;
                    return true;
                }
            }

            return false;
        }
    }
}
