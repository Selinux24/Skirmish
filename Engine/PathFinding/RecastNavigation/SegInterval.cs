using System;

namespace Engine.PathFinding.RecastNavigation
{
    public class SegInterval
    {
        public int R { get; set; }
        public int TMin { get; set; }
        public int TMax { get; set; }

        public static void InsertInterval(ref SegInterval[] ints, ref int nints, int maxInts, int tmin, int tmax, int r)
        {
            if (nints + 1 > maxInts) return;

            // Find insertion point.
            int idx = 0;
            while (idx < nints)
            {
                if (tmax <= ints[idx].TMin)
                {
                    break;
                }
                idx++;
            }

            // Move current results.
            if (nints - idx != 0)
            {
                Array.Copy(ints, idx, ints, idx + 1, (nints - idx));
            }

            // Store
            ints[idx].R = r;
            ints[idx].TMin = tmin;
            ints[idx].TMax = tmax;
            nints++;
        }
    }
}
