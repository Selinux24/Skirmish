
namespace Engine.PathFinding.RecastNavigation.Recast
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
                    Layers = new int[63],
                    NLayers = 0,

                    Neis = new int[MaxNeighbors],
                    NNeis = 0,

                    YMin = int.MaxValue,
                    YMax = int.MinValue,

                    LayerId = 0xff,

                    IsBase = false,
                };
            }
        }

        public int[] Layers { get; set; }
        public int[] Neis { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        /// <summary>
        /// Layer ID
        /// </summary>
        public int LayerId { get; set; }
        /// <summary>
        /// Layer count
        /// </summary>
        public int NLayers { get; set; }
        /// <summary>
        ///  Neighbour count
        /// </summary>
        public int NNeis { get; set; }
        /// <summary>
        /// Flag indicating if the region is the base of merged regions.
        /// </summary>
        public bool IsBase { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Id: {0}; Layers: {1}; Neighbors: {2}; Base: {3}", LayerId, NLayers, NNeis, IsBase);
        }
    }
}