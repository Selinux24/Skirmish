﻿
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Sweep span
    /// </summary>
    public struct SweepSpan
    {
        /// <summary>
        /// row id
        /// </summary>
        public int RId { get; set; }
        /// <summary>
        /// region id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// number samples
        /// </summary>
        public int NS { get; set; }
        /// <summary>
        /// neighbour id
        /// </summary>
        public int Nei { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            if (RId != 0 || Id != 0 || NS != 0 || Nei != 0)
            {
                return $"Row Id: {RId}; Region Id: {Id}; Samples: {NS}; Neighbor Id: {Nei};";
            }
            else
            {
                return "Empty";
            }
        }
    }
}
