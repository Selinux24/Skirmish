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
        /// <summary>
        /// Unset height default value
        /// </summary>
        public const int RC_UNSET_HEIGHT = 0xffff;

        /// <summary>
        /// Data
        /// </summary>
        private readonly int[] data;

        /// <summary>
        /// Rectangle
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HeightPatch(Rectangle bounds)
        {
            int size = bounds.Width * bounds.Height;
            data = Helper.CreateArray(size, RC_UNSET_HEIGHT);
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
        /// Initializes patch data to value
        /// </summary>
        /// <param name="value">Height value</param>
        public void InitializeData(int value)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = value;
            }
        }
        /// <summary>
        /// Gets the height data at coordinates
        /// </summary>
        /// <param name="hx">X coordinate</param>
        /// <param name="hy">Y coordinate</param>
        /// <returns>Returns the height value</returns>
        public int GetHeight(int hx, int hy)
        {
            return data[hx + hy * Bounds.Width];
        }
        /// <summary>
        /// Sets the height data at coordinates
        /// </summary>
        /// <param name="hx">X coordinate</param>
        /// <param name="hy">Y coordinate</param>
        /// <param name="value">Height value</param>
        public void SetHeight(int hx, int hy, int value)
        {
            data[hx + hy * Bounds.Width] = value;
        }
        /// <summary>
        /// Sets the height data
        /// </summary>
        /// <param name="hdItem">Height data item</param>
        /// <param name="value">Height value</param>
        public void SetHeight(HeightDataItem hdItem, int value)
        {
            SetHeight(hdItem.X - Bounds.X, hdItem.Y - Bounds.Y, value);
        }
        /// <summary>
        /// Calculates the patch height at the specified coordinates
        /// </summary>
        /// <param name="hx">X coordinate</param>
        /// <param name="hy">Y coordinate</param>
        /// <param name="ph">Height value</param>
        /// <param name="ch">Cell height</param>
        /// <param name="radius">Search radius</param>
        /// <returns>Returns the patch height value</returns>
        public int CalculateHeight(int hx, int hy, float ph, float ch, int radius)
        {
            hx = MathUtil.Clamp(hx - Bounds.X, 0, Bounds.Width - 1);
            hy = MathUtil.Clamp(hy - Bounds.Y, 0, Bounds.Height - 1);
            int hmin = data[hx + hy * Bounds.Width];
            if (hmin != RC_UNSET_HEIGHT)
            {
                return hmin;
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
                int nx = hx + x;
                int nz = hy + z;
                if (CalculateDistanceToCeiling(nx, nz, ph, ch, out var h, out var d) && (d < dmin))
                {
                    hmin = h;
                    dmin = d;
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
                    if (hmin != RC_UNSET_HEIGHT)
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

            return hmin;
        }
        /// <summary>
        /// Calculates the distance to ceiling of the specified coordinates
        /// </summary>
        /// <param name="hx">Heightpatch x coordinate</param>
        /// <param name="hy">Heightpatch z coordinate</param>
        /// <param name="ph">Point height</param>
        /// <param name="ch">Cell height</param>
        /// <param name="h">Height value</param>
        /// <param name="dist">Distance to ceiling</param>
        /// <returns>Returns true if the specified coordinates returns a height value</returns>
        private bool CalculateDistanceToCeiling(int hx, int hy, float ph, float ch, out int h, out float dist)
        {
            h = int.MaxValue;
            dist = float.MaxValue;

            if (!Contains(hx, hy))
            {
                return false;
            }

            int nh = GetHeight(hx, hy);
            if (nh == RC_UNSET_HEIGHT)
            {
                return false;
            }

            h = nh;
            dist = MathF.Abs(nh * ch - ph);

            return true;
        }
        /// <summary>
        /// Gets whether the patch contains the specified coordinates
        /// </summary>
        /// <param name="hx">X coordinate</param>
        /// <param name="hy">Y coordinate</param>
        /// <returns>Returns true if the path contains the coordinates</returns>
        public bool Contains(int hx, int hy)
        {
            if (hx < 0 || hy < 0 || hx >= Bounds.Width || hy >= Bounds.Height)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets whether the specified coordinates contains a height value or not
        /// </summary>
        /// <param name="hx">X coordinate</param>
        /// <param name="hy">Y coordinate</param>
        public bool IsSet(int hx, int hy)
        {
            return GetHeight(hx, hy) != RC_UNSET_HEIGHT;
        }

        /// <summary>
        /// Converts the specified point to height patch coordinates
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="cs">Cell size</param>
        public static (int hx, int hy) PointToPatch(Vector3 p, float cs)
        {
            float ics = 1f / cs;
            int x = (int)MathF.Floor(p.X * ics + 0.01f);
            int z = (int)MathF.Floor(p.Z * ics + 0.01f);

            return (x, z);
        }
    }
}
