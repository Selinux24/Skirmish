
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Heightfield layer set
    /// </summary>
    class HeightfieldLayerSet
    {
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
                return new HeightfieldLayerSet();
            }

            return StoreLayers(ldata);
        }
        private static HeightfieldLayerSet StoreLayers(HeightfieldLayerData ldata)
        {
            var lset = new HeightfieldLayerSet
            {
                Layers = new HeightfieldLayer[ldata.LayerId],
                NLayers = ldata.LayerId,
            };

            for (int i = 0; i < lset.NLayers; ++i)
            {
                var layer = lset.Layers[i];

                // Copy height and area from compact heightfield. 
                ldata.CopyToLayer(ref layer, i);

                lset.Layers[i] = layer;
            }

            return lset;
        }
    }
}
