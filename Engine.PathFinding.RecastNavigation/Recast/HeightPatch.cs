using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Height patch
    /// </summary>
    class HeightPatch
    {
        public const int RC_UNSET_HEIGHT = -1;

        /// <summary>
        /// Data
        /// </summary>
        public int[] Data { get; set; }
        /// <summary>
        /// Rectangle
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HeightPatch()
        {
            Data = null;
            Bounds = new Rectangle(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the patch height at the specified point
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="ics"></param>
        /// <param name="ch"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public int GetHeight(Vector3 p, float ics, float ch, int radius)
        {
            int ix = (int)Math.Floor(p.X * ics + 0.01f);
            int iz = (int)Math.Floor(p.Z * ics + 0.01f);
            ix = MathUtil.Clamp(ix - Bounds.X, 0, Bounds.Width - 1);
            iz = MathUtil.Clamp(iz - Bounds.Y, 0, Bounds.Height - 1);
            int h = Data[ix + iz * Bounds.Width];
            if (h != RC_UNSET_HEIGHT)
            {
                return h;
            }

            // Special case when data might be bad.
            // Walk adjacent cells in a spiral up to 'radius', and look
            // for a pixel which has a valid height.
            int x = 1, z = 0, dx = 1, dz = 0;
            int maxSize = radius * 2 + 1;
            int maxIter = maxSize * maxSize - 1;

            int nextRingIterStart = 8;
            int nextRingIters = 16;

            float dmin = float.MaxValue;
            for (int i = 0; i < maxIter; i++)
            {
                int nx = ix + x;
                int nz = iz + z;

                if (nx >= 0 && nz >= 0 && nx < Bounds.Width && nz < Bounds.Height)
                {
                    int nh = Data[nx + nz * Bounds.Width];
                    if (nh != RC_UNSET_HEIGHT)
                    {
                        float d = Math.Abs(nh * ch - p.Y);
                        if (d < dmin)
                        {
                            h = nh;
                            dmin = d;
                        }
                    }
                }

                // We are searching in a grid which looks approximately like this:
                //  __________
                // |2 ______ 2|
                // | |1 __ 1| |
                // | | |__| | |
                // | |______| |
                // |__________|
                // We want to find the best height as close to the center cell as possible. This means that
                // if we find a height in one of the neighbor cells to the center, we don't want to
                // expand further out than the 8 neighbors - we want to limit our search to the closest
                // of these "rings", but the best height in the ring.
                // For example, the center is just 1 cell. We checked that at the entrance to the function.
                // The next "ring" contains 8 cells (marked 1 above). Those are all the neighbors to the center cell.
                // The next one again contains 16 cells (marked 2). In general each ring has 8 additional cells, which
                // can be thought of as adding 2 cells around the "center" of each side when we expand the ring.
                // Here we detect if we are about to enter the next ring, and if we are and we have found
                // a height, we abort the search.
                if (i + 1 == nextRingIterStart)
                {
                    if (h != RC_UNSET_HEIGHT)
                    {
                        break;
                    }

                    nextRingIterStart += nextRingIters;
                    nextRingIters += 8;
                }

                if ((x == z) || ((x < 0) && (x == -z)) || ((x > 0) && (x == 1 - z)))
                {
                    int tmp = dx;
                    dx = -dz;
                    dz = tmp;
                }
                x += dx;
                z += dz;
            }

            return h;
        }
        /// <summary>
        /// Gets whether the specified magnitudes were into the heightpatch or not
        /// </summary>
        /// <param name="hx">X height</param>
        /// <param name="hy">Y height</param>
        public bool CompareBounds(int hx, int hy)
        {
            if (hx < 0 || hy < 0 || hx >= Bounds.Width || hy >= Bounds.Height)
            {
                return false;
            }

            if (Data[hx + hy * Bounds.Width] != RC_UNSET_HEIGHT)
            {
                return false;
            }

            return true;
        }
    }
}
