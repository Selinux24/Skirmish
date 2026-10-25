using System.Collections.Generic;
using System.Numerics;

namespace Engine
{
    /// <summary>
    /// Image
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public struct Image(int width, int height)
    {
        /// <summary>
        /// Image pixel colors
        /// </summary>
        private readonly Vector4[,] colors = new Vector4[width, height];

        /// <summary>
        /// Gets the image width in pixels
        /// </summary>
        public int Width { get; private set; } = width;
        /// <summary>
        /// Gets the image height in pixels
        /// </summary>
        public int Height { get; private set; } = height;

        /// <summary>
        /// Sets the pixel color value at position
        /// </summary>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        /// <param name="color">Color value</param>
        public readonly void SetPixel(int width, int height, Vector3 color)
        {
            colors[width, height] = new(color, 1f);
        }
        /// <summary>
        /// Sets the pixel color value at position
        /// </summary>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        /// <param name="color">Color value</param>
        public readonly void SetPixel(int width, int height, Vector4 color)
        {
            colors[width, height] = color;
        }
        /// <summary>
        /// Gets the pixel color value at position
        /// </summary>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        public readonly Vector4 GetPixel(int width, int height)
        {
            return colors[width, height];
        }
        /// <summary>
        /// Flattens the image pixel into a one-dimension array
        /// </summary>
        public readonly IEnumerable<Vector4> Flatten()
        {
            Vector4[] flatColors = new Vector4[Width * Height];

            int index = 0;
            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    flatColors[index++] = colors[w, h];
                }
            }

            return flatColors;
        }
    }
}
