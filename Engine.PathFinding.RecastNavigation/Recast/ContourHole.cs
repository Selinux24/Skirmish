using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    public class ContourHole
    {
        public static readonly CompareHoles DefaultComparer = new CompareHoles();


        public Contour Contour { get; set; }
        public int MinX { get; set; }
        public int MinZ { get; set; }
        public int Leftmost { get; set; }


        public class CompareHoles : IComparer<ContourHole>
        {
            public int Compare(ContourHole x, ContourHole y)
            {
                var a = x;
                var b = y;
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
            }
        }
    }
}
