using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Grid utils
    /// </summary>
    static class GridUtils
    {
        static readonly int[] OffsetsX = [-1, 0, 1, 0,];
        static readonly int[] OffsetsY = [0, 1, 0, -1];
        static readonly int[] OffsetsDir = [3, 0, -1, 2, 1];

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
            return Iterate(0, 0, width, height);
        }
        /// <summary>
        /// Iterates over a grid
        /// </summary>
        /// <param name="rowStart">Starting row</param>
        /// <param name="colStart">Starting column</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns column and row coordinates</returns>
        public static IEnumerable<(int col, int row)> Iterate(int rowStart, int colStart, int width, int height)
        {
            int rowTo = rowStart + height;
            int colTo = colStart + width;

            for (int row = rowStart; row < rowTo; ++row)
            {
                for (int col = colStart; col < colTo; ++col)
                {
                    yield return (col, row);
                }
            }
        }
    }
}
