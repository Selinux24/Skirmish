﻿namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Edge with region and area definition of a PolyMesh
    /// </summary>
    struct IndexedRegionEdge
    {
        /// <summary>
        /// Edge index A
        /// </summary>
        public int EdgeIndexA;
        /// <summary>
        /// Edge index B
        /// </summary>
        public int EdgeIndexB;
        /// <summary>
        /// Region
        /// </summary>
        public int Region;
        /// <summary>
        /// Area
        /// </summary>
        public SamplePolyAreas Area;

        /// <summary>
        /// Constructor
        /// </summary>
        public IndexedRegionEdge(int edgeIndexA, int edgeIndexB, int region, SamplePolyAreas area)
        {
            EdgeIndexA = edgeIndexA;
            EdgeIndexB = edgeIndexB;
            Region = region;
            Area = area;
        }
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