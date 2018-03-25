
namespace Engine.PathFinding.RecastNavigation
{
    public struct LayerRegion
    {
        public const int MaxLayers = 63;
        public const int MaxNeighbors = 16;

        public static LayerRegion Default
        {
            get
            {
                return new LayerRegion()
                {
                    layers = new int[63],
                    nlayers = 0,

                    neis = new int[MaxNeighbors],
                    nneis = 0,

                    ymin = 0xffff,
                    ymax = 0,

                    layerId = 0xff,

                    isBase = false,
                };
            }
        }

        public int[] layers;
        public int[] neis;
        public int ymin;
        public int ymax;
        /// <summary>
        /// Layer ID
        /// </summary>
        public int layerId;
        /// <summary>
        /// Layer count
        /// </summary>
        public int nlayers;
        /// <summary>
        ///  Neighbour count
        /// </summary>
        public int nneis;
        /// <summary>
        /// Flag indicating if the region is the base of merged regions.
        /// </summary>
        public bool isBase;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Id: {0}; Layers: {1}; Neighbors: {2}; Base: {3}", layerId, nlayers, nneis, isBase);
        }
    }
}