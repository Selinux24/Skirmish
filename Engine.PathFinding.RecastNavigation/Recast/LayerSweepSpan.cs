﻿
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
        public override string ToString()
        {
            return string.Format("Samples {0}; Region {1}; Neighbour {2};", NS, Id, Nei);
        }
    }
}