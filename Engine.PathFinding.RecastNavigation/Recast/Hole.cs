
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Hole
    /// </summary>
    struct Hole
    {
        public int[] Indices { get; set; }
        public int[] Region { get; set; }
        public SamplePolyAreas[] Area { get; set; }
        public int NIndices;
        public int NRegion;
        public int NArea;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Size</param>
        public Hole(int size)
        {
            Indices = new int[size];
            Region = new int[size];
            Area = new SamplePolyAreas[size];
            NIndices = 0;
            NRegion = 0;
            NArea = 0;
        }
    }
}
