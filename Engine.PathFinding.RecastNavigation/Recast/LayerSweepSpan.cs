
namespace Engine.PathFinding.RecastNavigation.Recast
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

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override readonly string ToString()
        {
            return $"Samples {NS}; Region {Id}; Neighbour {Nei};";
        }
    }
}