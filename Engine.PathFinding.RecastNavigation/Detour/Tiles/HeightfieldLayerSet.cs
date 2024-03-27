using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    using Engine.PathFinding.RecastNavigation.Recast;

    /// <summary>
    /// Heightfield layer set
    /// </summary>
    class HeightfieldLayerSet
    {
        /// <summary>
        /// Maximum number of layers
        /// </summary>
        const int MAX_LAYERS = 32;

        /// <summary>
        /// Empty heightfield layer set
        /// </summary>
        public static HeightfieldLayerSet Empty
        {
            get
            {
                return new()
                {
                    Layers = [],
                    NLayers = 0,
                };
            }
        }

        /// <summary>
        /// Layer list
        /// </summary>
        public HeightfieldLayer[] Layers { get; set; }
        /// <summary>
        /// Number of layers
        /// </summary>
        public int NLayers { get; set; }

        /// <summary>
        /// Builds a new heightfield layer set
        /// </summary>
        /// <param name="chf">Compact heightfield</param>
        /// <param name="borderSize">Border size</param>
        /// <param name="walkableHeight">Walkable height</param>
        /// <returns>Returns the new heightfield layer set</returns>
        public static HeightfieldLayerSet Build(CompactHeightfield chf, int borderSize, int walkableHeight)
        {
            var ldata = HeightfieldLayerData.Create(chf, borderSize, walkableHeight);
            if (ldata == null)
            {
                return Empty;
            }

            var lset = new HeightfieldLayerSet
            {
                Layers = new HeightfieldLayer[ldata.LayerId],
                NLayers = ldata.LayerId,
            };

            for (int i = 0; i < lset.NLayers; ++i)
            {
                var layer = lset.Layers[i];

                // Copy height and area from compact heightfield. 
                layer.CopyToLayer(ldata, i);

                lset.Layers[i] = layer;
            }

            return lset;
        }
        /// <summary>
        /// Allocates voxel heightfield where we rasterize our input data to 
        /// </summary>
        public IEnumerable<TileCacheData> AllocateTiles(int x, int y)
        {
            int count = Math.Min(NLayers, MAX_LAYERS);

            for (int i = 0; i < count; i++)
            {
                yield return Layers[i].Create(x, y, i);
            }
        }
    }
}
