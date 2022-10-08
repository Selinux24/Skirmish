using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    public class SegInterval
    {
        public int R { get; set; }
        public int TMin { get; set; }
        public int TMax { get; set; }

        public static void InsertInterval(List<SegInterval> ints, int maxInts, int tmin, int tmax, int r)
        {
            if (ints.Count + 1 > maxInts) return;

            // Find insertion point.
            int idx = 0;
            while (idx < ints.Count)
            {
                if (tmax <= ints[idx].TMin)
                {
                    break;
                }
                idx++;
            }

            ints.Insert(idx, new SegInterval()
            {
                TMin = tmin,
                TMax = tmax,
                R = r,
            });
        }
    }
}
