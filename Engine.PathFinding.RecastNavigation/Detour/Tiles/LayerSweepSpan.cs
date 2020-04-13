
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public struct LayerSweepSpan
    {
        /// <summary>
        /// Number samples
        /// </summary>
        public int NSamples { get; set; }
        /// <summary>
        /// Region id
        /// </summary>
        public int Region { get; set; }
        /// <summary>
        /// Neighbour id
        /// </summary>
        public int Neigbour { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Samples {0}; Region {1}; Neighbour {2};", NSamples, Region, Neigbour);
        }
    }
}