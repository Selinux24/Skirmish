
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public struct LayerSweepSpan
    {
        /// <summary>
        /// Number samples
        /// </summary>
        public int NS { get; set; }
        /// <summary>
        /// Region id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Neighbour id
        /// </summary>
        public int Nei { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Samples {NS}; Region {Id}; Neighbour {Nei};";
        }
    }
}