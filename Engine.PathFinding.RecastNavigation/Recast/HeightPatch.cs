using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Height patch
    /// </summary>
    class HeightPatch
    {
        const int RC_UNSET_HEIGHT = -1;

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
        public HeightPatch(Rectangle bounds)
        {
            int size = bounds.Width * bounds.Height;
            Data = Helper.CreateArray(size, RC_UNSET_HEIGHT);
            Bounds = bounds;
        }

        /// <summary>
        /// Iterates over the height patch bounds
        /// </summary>
        /// <param name="borderSize">Border size</param>
        /// <returns>Returns the height patch coordinates, and the related cell coordinates</returns>
        public IEnumerable<(int hx, int hy, int x, int y)> IterateBounds(int borderSize)
        {
            for (int hy = 0; hy < Bounds.Height; hy++)
            {
                int y = Bounds.Y + hy + borderSize;

                for (int hx = 0; hx < Bounds.Width; hx++)
                {
                    int x = Bounds.X + hx + borderSize;

                    yield return (hx, hy, x, y);
                }
            }
        }

        /// <summary>
        /// Initializes patch data to <see cref="RC_UNSET_HEIGHT"/>
        /// </summary>
        /// <param name="size">Data array size</param>
        public void InitializeData(int size)
        {
            // Set all heights to RC_UNSET_HEIGHT.
            Data = Helper.CreateArray(size, RC_UNSET_HEIGHT);
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
                GetMinDistance(nx, nz, ch, p.Y, ref h, ref dmin);

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
        /// Updates the minimum distance
        /// </summary>
        /// <param name="x">Heightpatch x coordinate</param>
        /// <param name="z">Heightpatch z coordinate</param>
        /// <param name="ch">Cell height</param>
        /// <param name="pY">Point height</param>
        /// <param name="h">Height value</param>
        /// <param name="dmin">Minimum distance value</param>
        /// <remarks>Only updates the height and distance if it is less</remarks>
        private void GetMinDistance(int x, int z, float ch, float pY, ref int h, ref float dmin)
        {
            Bounds.Contains(x, z);

            if (x < 0 || z < 0 || x >= Bounds.Width || z >= Bounds.Height)
            {
                return;
            }

            int nh = Data[x + z * Bounds.Width];
            if (nh == RC_UNSET_HEIGHT)
            {
                return;
            }

            float d = Math.Abs(nh * ch - pY);
            if (d < dmin)
            {
                h = nh;
                dmin = d;
            }
        }
        /// <summary>
        /// Gets whether the specified magnitudes were into the heightpatch or not
        /// </summary>
        /// <param name="x">X height</param>
        /// <param name="z">Y height</param>
        public bool CompareBounds(int x, int z)
        {
            if (x < 0 || z < 0 || x >= Bounds.Width || z >= Bounds.Height)
            {
                return false;
            }

            if (Data[x + z * Bounds.Width] != RC_UNSET_HEIGHT)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Sets the height data at coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="value">Hight value</param>
        public void SetHeight(int x, int z, int value)
        {
            Data[x + z * Bounds.Width] = value;
        }
    }
}
