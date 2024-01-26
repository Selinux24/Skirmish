
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Grid utils
    /// </summary>
    static class GridUtils
    {
        static readonly int[] OffsetsX = new[] { -1, 0, 1, 0, };
        static readonly int[] OffsetsY = new[] { 0, 1, 0, -1 };
        static readonly int[] OffsetsDir = new[] { 3, 0, -1, 2, 1 };

        public static int GetDirOffsetX(int dir)
        {
            return OffsetsX[dir & 3];
        }
        public static int GetDirOffsetY(int dir)
        {
            return OffsetsY[dir & 3];
        }
        public static int GetDirForOffset(int x, int y)
        {
            return OffsetsDir[((y + 1) << 1) + x];
        }
        public static int Rotate(int dir, int amount)
        {
            return (dir + amount) & 3;
        }
        public static int RotateCW(int dir)
        {
            return Rotate(dir, 1);
        }
        public static int RotateCCW(int dir)
        {
            return Rotate(dir, 3);
        }

        /// <summary>
        /// Iterates over a grid
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns column and row coordinates</returns>
        public static IEnumerable<(int col, int row)> Iterate(int width, int height)
        {
            for (int row = 0; row < height; ++row)
            {
                for (int col = 0; col < width; ++col)
                {
                    yield return (col, row);
                }
            }
        }
    }
}
