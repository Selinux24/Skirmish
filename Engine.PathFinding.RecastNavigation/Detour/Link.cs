﻿
namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines a link between polygons.
    /// </summary>
    public struct Link
    {
        /// <summary>
        /// Neighbour reference. (The neighbor that is linked to.)
        /// </summary>
        public int NRef { get; set; }
        /// <summary>
        /// Index of the next link.
        /// </summary>
        public int Next { get; set; }
        /// <summary>
        /// Index of the polygon edge that owns this link.
        /// </summary>
        public int Edge { get; set; }
        /// <summary>
        /// If a boundary link, defines on which side the link is.
        /// </summary>
        public int Side { get; set; }
        /// <summary>
        /// If a boundary link, defines the minimum sub-edge area.
        /// </summary>
        public int BMin { get; set; }
        /// <summary>
        /// If a boundary link, defines the maximum sub-edge area.
        /// </summary>
        public int BMax { get; set; }

        /// <summary>
        /// Checks if the link spans the whole edge
        /// </summary>
        public readonly bool ExcedBoundaries()
        {
            return BMin == 0 && BMax == 255;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Ref {NRef}; Next {Next}; Edge {Edge}; Side {Side}; BMin {BMin}; BMax {BMax};";
        }
    }
}
