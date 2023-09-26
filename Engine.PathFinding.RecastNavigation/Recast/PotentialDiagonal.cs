using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Potential diagonal
    /// </summary>
    public struct PotentialDiagonal
    {
        /// <summary>
        /// Default comparer
        /// </summary>
        public static readonly Comparison<PotentialDiagonal> DefaultComparer = (x, y) =>
        {
            if (x.Dist < y.Dist) return -1;
            if (x.Dist > y.Dist) return 1;
            return 0;
        };

        /// <summary>
        /// Vertex
        /// </summary>
        public int Vert { get; set; }
        /// <summary>
        /// Distance
        /// </summary>
        public int Dist { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Vertex: {Vert}; Distance: {Dist};";
        }
    }
}
