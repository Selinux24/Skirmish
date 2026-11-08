
namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Indexed edge of PolyMesh
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    struct IndexedEdge(int edgeIndex, int shareCount)
    {
        /// <summary>
        /// Edge index
        /// </summary>
        public int EdgeIndex = edgeIndex;
        /// <summary>
        /// Shared edge count
        /// </summary>
        public int ShareCount = shareCount;

        /// <summary>
        /// Gets whether the specified edge index, exist in the edge definition
        /// </summary>
        /// <param name="edges">Indexed edge list</param>
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
        /// <summary>
        /// Gets the number of open edges in the specified collection
        /// </summary>
        /// <param name="edges">Indexed edge list</param>
        /// <param name="nedges">Number of edges</param>
        /// <returns>Returns the number of edges with a ShareCount smaller than 2</returns>
        public static int CountOpenEdges(IndexedEdge[] edges, int nedges)
        {
            int numOpenEdges = 0;
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].ShareCount < 2)
                {
                    numOpenEdges++;
                }
            }
            return numOpenEdges;
        }
    }
}
