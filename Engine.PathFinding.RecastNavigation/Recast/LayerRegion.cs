using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Layer region
    /// </summary>
    public struct LayerRegion
    {
        public const int MaxLayers = 63;
        public const int MaxNeighbors = 16;

        /// <summary>
        /// Default layer region
        /// </summary>
        public static LayerRegion Default
        {
            get
            {
                return new LayerRegion()
                {
                    Layers = new int[MaxLayers],
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

        /// <summary>
        /// Layer list
        /// </summary>
        public int[] Layers { get; set; }
        /// <summary>
        /// Layer count
        /// </summary>
        public int NLayers { get; set; }
        /// <summary>
        /// Neighbour list
        /// </summary>
        public int[] Neis { get; set; }
        /// <summary>
        ///  Neighbour count
        /// </summary>
        public int NNeis { get; set; }
        /// <summary>
        /// Minimum layer height
        /// </summary>
        public int YMin { get; set; }
        /// <summary>
        /// Maximum layer height
        /// </summary>
        public int YMax { get; set; }
        /// <summary>
        /// Layer ID
        /// </summary>
        public int LayerId { get; set; }
        /// <summary>
        /// Flag indicating if the region is the base of merged regions.
        /// </summary>
        public bool IsBase { get; set; }

        /// <summary>
        /// Adds an unique layer
        /// </summary>
        /// <param name="v">Layer value</param>
        /// <returns>Returns true if the layer were added</returns>
        public bool AddUniqueLayer(int v)
        {
            if (ContainsLayer(v))
            {
                return true;
            }

            if (NLayers >= MaxLayers)
            {
                return false;
            }

            Layers[NLayers++] = v;

            return true;
        }
        /// <summary>
        /// Adds an unique neighbour
        /// </summary>
        /// <param name="v">Neighbour value</param>
        /// <returns>Returns true if the neighbour were added</returns>
        public bool AddUniqueNei(int v)
        {
            if (ContainsNei(v))
            {
                return true;
            }

            if (NNeis >= MaxNeighbors)
            {
                return false;
            }

            Neis[NNeis++] = v;

            return true;
        }
        /// <summary>
        /// Gets whether the layer array has the value or not
        /// </summary>
        /// <param name="v">Layer value</param>
        /// <returns>Returns true if the layer array contains the value</returns>
        public readonly bool ContainsLayer(int v)
        {
            return Layers?.Take(NLayers).Contains(v) ?? false;
        }
        /// <summary>
        /// Gets whether the neighbour array has the value or not
        /// </summary>
        /// <param name="v">Neighbour value</param>
        /// <returns>Returns true if the neighbour array contains the value</returns>
        public readonly bool ContainsNei(int v)
        {
            return Neis?.Take(NNeis).Contains(v) ?? false;
        }
        /// <summary>
        /// Merges the specified region layers into current
        /// </summary>
        /// <param name="reg">Layer region to merge in</param>
        public bool Merge(LayerRegion reg)
        {
            for (int k = 0; k < reg.NLayers; ++k)
            {
                if (!AddUniqueLayer(reg.Layers[k]))
                {
                    return false;
                }
            }

            YMin = Math.Min(YMin, reg.YMin);
            YMax = Math.Max(YMax, reg.YMax);

            return true;
        }
        /// <summary>
        /// Gets the height range between to layer regions
        /// </summary>
        /// <param name="reg1">Layer region 1</param>
        /// <param name="reg2">Layer region 2</param>
        public static int GetHeightRange(LayerRegion reg1, LayerRegion reg2)
        {
            // Skip if the height range would become too large.
            int ymin = Math.Min(reg1.YMin, reg2.YMin);
            int ymax = Math.Max(reg1.YMax, reg2.YMax);
            return ymax - ymin;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Id: {LayerId}; Layers: {NLayers}; Neighbors: {NNeis}; Base: {IsBase}";
        }
    }
}