using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    public struct PotentialDiagonal
    {
        public static readonly CompareDiagDist DefaultComparer = new CompareDiagDist();

        public int Vert { get; set; }
        public int Dist { get; set; }

        public override string ToString()
        {
            return string.Format("Vertex: {0}; Distance: {1};", Vert, Dist);
        }

        public class CompareDiagDist : IComparer<PotentialDiagonal>
        {
            public int Compare(PotentialDiagonal x, PotentialDiagonal y)
            {
                if (x.Dist < y.Dist) return -1;
                if (x.Dist > y.Dist) return 1;
                return 0;
            }
        }
    }
}
