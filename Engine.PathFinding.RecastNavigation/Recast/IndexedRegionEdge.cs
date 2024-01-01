
namespace Engine.PathFinding.RecastNavigation.Recast
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
    }
}
