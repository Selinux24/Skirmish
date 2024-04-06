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
                    layers = [],
                    nlayers = 0,
                };
            }
        }

        /// <summary>
        /// Number of allocated layers
        /// </summary>
        private int nlayers;
        /// <summary>
        /// Layer list
        /// </summary>
        private HeightfieldLayer[] layers;

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
                layers = new HeightfieldLayer[ldata.LayerId],
                nlayers = ldata.LayerId,
            };

            for (int i = 0; i < lset.nlayers; ++i)
            {
                var layer = lset.layers[i];

                // Copy height and area from compact heightfield. 
                layer.CopyFromLayerData(ldata, i);

                lset.layers[i] = layer;
            }

            return lset;
        }
       
        /// <summary>
        /// Allocates voxel heightfield where we rasterize our input data to 
        /// </summary>
        public IEnumerable<TileCacheData> AllocateTiles(int x, int y)
        {
            int count = Math.Min(nlayers, MAX_LAYERS);

            for (int i = 0; i < count; i++)
            {
                yield return layers[i].Create(x, y, i);
            }
        }
    }
}
