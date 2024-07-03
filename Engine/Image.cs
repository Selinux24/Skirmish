using SharpDX;
using System.Collections.Generic;

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
        private readonly Color4[,] colors = new Color4[width, height];

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
        public readonly void SetPixel(int width, int height, Color color)
        {
            colors[width, height] = color;
        }
        /// <summary>
        /// Sets the pixel color value at position
        /// </summary>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        /// <param name="color">Color value</param>
        public readonly void SetPixel(int width, int height, Color4 color)
        {
            colors[width, height] = color;
        }
        /// <summary>
        /// Gets the pixel color value at position
        /// </summary>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        public readonly Color4 GetPixel(int width, int height)
        {
            return colors[width, height];
        }
        /// <summary>
        /// Flattens the image pixel into a one-dimension array
        /// </summary>
        public readonly IEnumerable<Color4> Flatten()
        {
            Color4[] flatColors = new Color4[Width * Height];

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
