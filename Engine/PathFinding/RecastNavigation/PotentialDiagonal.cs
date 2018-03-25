using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    public struct PotentialDiagonal
    {
        public static readonly Comparer DefaultComparer = new Comparer();

        public int vert;
        public int dist;

        public override string ToString()
        {
            return string.Format("Vertex: {0}; Distance: {1};", vert, dist);
        }

        public class Comparer : IComparer<PotentialDiagonal>
        {
            public int Compare(PotentialDiagonal a, PotentialDiagonal b)
            {
                if (a.dist < b.dist) return -1;
                if (a.dist > b.dist) return 1;
                return 0;
            }
        }
    }
}
