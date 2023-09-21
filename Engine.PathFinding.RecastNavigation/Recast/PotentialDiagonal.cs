using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    public struct PotentialDiagonal
    {
        public class CompareDiagDist : IComparer<PotentialDiagonal>
        {
            public int Compare(PotentialDiagonal x, PotentialDiagonal y)
            {
                if (x.Dist < y.Dist) return -1;
                if (x.Dist > y.Dist) return 1;
                return 0;
            }
        }

        public static readonly CompareDiagDist DefaultComparer = new();

        public int Vert { get; set; }
        public int Dist { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Vertex: {Vert}; Distance: {Dist};";
        }
    }
}
