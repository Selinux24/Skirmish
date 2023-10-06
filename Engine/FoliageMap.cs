using SharpDX;
using System;
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
            var bitmap = Game.Images.FromStream(data);

            var colors = new Color4[bitmap.Height + 1, bitmap.Width + 1];

            for (int w = 0; w < bitmap.Width + 1; w++)
            {
                int ww = w < bitmap.Width ? w : w - 1;

                for (int h = 0; h < bitmap.Height + 1; h++)
                {
                    int hh = h < bitmap.Height ? h : h - 1;

                    //Flip coordinates
                    colors[h, w] = bitmap.GetPixel(ww, hh);
                }
            }

            return new FoliageMap(colors);
        }
        /// <summary>
        /// Converts specified number relative to a total size and min/max magnitudes
        /// </summary>
        /// <param name="n">Number</param>
        /// <param name="size">Total size</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Returns the relative value</returns>
        public static float ConvertRelative(float n, float size, float min, float max)
        {
            float f = size / (max - min);

            return (n + (max - min) - max) * f;
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
            m_Data = data;

            MaxX = data.GetUpperBound(0);
            MaxY = data.GetUpperBound(1);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FoliageMap()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Data = null;
            }
        }

        /// <summary>
        /// Gets the pixel data by texture coordinates
        /// </summary>
        /// <param name="x">X coordinate from 0 to 1</param>
        /// <param name="y">Y coordinate from 0 to 1</param>
        /// <returns>Returns the pixel color</returns>
        public Color4 Get(float x, float y)
        {
            if (m_Data != null)
            {
                float pX = ConvertRelative(x, MaxX, 0, 1);
                float pZ = ConvertRelative(y, MaxY, 0, 1);

                return m_Data[(int)pX, (int)pZ];
            }

            return Color4.Black;
        }
        /// <summary>
        /// Gets the pixel data by absolute coordinates
        /// </summary>
        /// <param name="x">X coordinate from 0 to maximum X pixel count (MaxX)</param>
        /// <param name="y">Y coordinate from 0 to maximum Y pixel count (MaxY)</param>
        /// <returns>Returns the pixel color</returns>
        public Color4 GetAbsolute(int x, int y)
        {
            if (m_Data != null)
            {
                return m_Data[x, y];
            }

            return Color4.Black;
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
            if (m_Data != null)
            {
                float x = ConvertRelative(pos.X, MaxX, min.X, max.X);
                float z = ConvertRelative(pos.Z, MaxY, min.Y, max.Y);

                return m_Data[(int)x, (int)z];
            }

            return Color4.Black;
        }
    }
}
