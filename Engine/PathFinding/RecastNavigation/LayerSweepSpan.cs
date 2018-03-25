
namespace Engine.PathFinding.RecastNavigation
{
    public struct LayerSweepSpan
    {
        /// <summary>
        /// Number samples
        /// </summary>
        public int ns;
        /// <summary>
        /// Region id
        /// </summary>
        public int id;
        /// <summary>
        /// Neighbour id
        /// </summary>
        public int nei;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Samples {0}; Region {1}; Neighbour {2};", ns, id, nei);
        }
    }
}