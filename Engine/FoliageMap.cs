using SharpDX;
using System;
using System.Drawing;
using System.IO;

namespace Engine
{
    /// <summary>
    /// Foliage map
    /// </summary>
    public class FoliageMap : IDisposable
    {
        /// <summary>
        /// Generates a new foliage map from stream data
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Returns the new generated foliage map</returns>
        public static FoliageMap FromStream(Stream data)
        {
            Bitmap bitmap = Bitmap.FromStream(data) as Bitmap;

            var heights = new float[bitmap.Height + 1, bitmap.Width + 1];
            var colors = new Color4[bitmap.Height + 1, bitmap.Width + 1];

            using (bitmap)
            {
                for (int x = 0; x < bitmap.Width + 1; x++)
                {
                    int xx = x < bitmap.Width ? x : x - 1;

                    for (int y = 0; y < bitmap.Height + 1; y++)
                    {
                        int yy = y < bitmap.Height ? y : y - 1;

                        var color = bitmap.GetPixel(xx, yy);

                        colors[x, y] = new SharpDX.Color(color.R, color.G, color.B, color.A);
                    }
                }
            }

            return new FoliageMap(colors);
        }

        /// <summary>
        /// Channle data
        /// </summary>
        private Color4[,] m_Data;
        /// <summary>
        /// Maximum X value
        /// </summary>
        public readonly int MaxX = 0;
        /// <summary>
        /// Maximum Y value
        /// </summary>
        public readonly int MaxY = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Channel data</param>
        public FoliageMap(Color4[,] data)
        {
            this.m_Data = data;

            this.MaxX = data.GetUpperBound(0);
            this.MaxY = data.GetUpperBound(1);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.m_Data = null;
        }

        /// <summary>
        /// Gets the pixel data by texture coordinates
        /// </summary>
        /// <param name="x">X coordinate from 0 to 1</param>
        /// <param name="y">Y coordinate from 0 to 1</param>
        /// <returns>Returns the pixel color</returns>
        public Color4 Get(float x, float y)
        {
            float pX = x.GetRelative(this.MaxX, 0, 1);
            float pZ = y.GetRelative(this.MaxY, 0, 1);

            return this.m_Data[(int)pX, (int)pZ];
        }
        /// <summary>
        /// Gets the pixel data by absolute coordinates
        /// </summary>
        /// <param name="x">X coordinate from 0 to maximum X pixel count (MaxX)</param>
        /// <param name="y">Y coordinate from 0 to maximum Y pixel count (MaxY)</param>
        /// <returns>Returns the pixel color</returns>
        public Color4 GetAbsolute(int x, int y)
        {
            return this.m_Data[x, y];
        }
        /// <summary>
        /// Gets the pixel data by relative coordinates
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="min">Source minimum dimensions</param>
        /// <param name="max">Source maximum dimensions</param>
        /// <returns>Returns the pixel color</returns>
        public Color4 GetRelative(Vector3 pos, Vector2 min, Vector2 max)
        {
            float x = pos.X.GetRelative(this.MaxX, min.X, max.X);
            float z = pos.Z.GetRelative(this.MaxY, min.Y, max.Y);

            return this.m_Data[(int)x, (int)z];
        }
    }
}
