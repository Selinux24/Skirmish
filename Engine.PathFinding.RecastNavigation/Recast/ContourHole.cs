using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contour hole
    /// </summary>
    public class ContourHole
    {
        /// <summary>
        /// Holes comparer
        /// </summary>
        public static readonly Comparison<ContourHole> Comparer = (a, b) =>
        {
            if (a.MinX == b.MinX)
            {
                if (a.MinZ < b.MinZ) return -1;
                if (a.MinZ > b.MinZ) return 1;
            }
            else
            {
                if (a.MinX < b.MinX) return -1;
                if (a.MinX > b.MinX) return 1;
            }
            return 0;
        };

        /// <summary>
        /// Contour
        /// </summary>
        public Contour Contour { get; set; }
        /// <summary>
        /// Min x
        /// </summary>
        public int MinX { get; set; }
        /// <summary>
        /// Min z
        /// </summary>
        public int MinZ { get; set; }
        /// <summary>
        /// Left most
        /// </summary>
        public int Leftmost { get; set; }
    }
}
