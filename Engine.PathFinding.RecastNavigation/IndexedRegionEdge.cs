namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Edge with region and area definition of a PolyMesh
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    struct IndexedRegionEdge(int edgeIndexA, int edgeIndexB, int region, int area)
    {
        /// <summary>
        /// Edge index A
        /// </summary>
        public int EdgeIndexA = edgeIndexA;
        /// <summary>
        /// Edge index B
        /// </summary>
        public int EdgeIndexB = edgeIndexB;
        /// <summary>
        /// Region
        /// </summary>
        public int Region = region;
        /// <summary>
        /// Area
        /// </summary>
        public int Area = area;

        /// <summary>
        /// Decrement by one the edge indices greater than rem
        /// </summary>
        /// <param name="edges">Edges list</param>
        /// <param name="nedges">Number of edges in the list</param>
        /// <param name="rem">Index to remove</param>
        public static void RemoveIndex(IndexedRegionEdge[] edges, int nedges, int rem)
        {
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i].EdgeIndexA > rem) edges[i].EdgeIndexA--;
                if (edges[i].EdgeIndexB > rem) edges[i].EdgeIndexB--;
            }
        }
    }
}
