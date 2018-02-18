
namespace Engine.PathFinding.NavMesh2
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
                    layers = new byte[63],
                    nlayers = 0,

                    neis = new byte[MaxNeighbors],
                    nneis = 0,

                    ymin = 0xffff,
                    ymax = 0,

                    layerId = 0xff,

                    isBase = false,
                };
            }
        }

        public byte[] layers;
        public byte[] neis;
        public ushort ymin;
        public ushort ymax;
        /// <summary>
        /// Layer ID
        /// </summary>
        public byte layerId;
        /// <summary>
        /// Layer count
        /// </summary>
        public byte nlayers;
        /// <summary>
        ///  Neighbour count
        /// </summary>
        public byte nneis;
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