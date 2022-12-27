using SharpDX;
using System.Collections.Generic;
using System.IO;

namespace Engine
{
    /// <summary>
    /// Images helper interface
    /// </summary>
    public interface IImages
    {
        /// <summary>
        /// Gets an image from a stream
        /// </summary>
        /// <param name="data">Stream data</param>
        Image FromStream(Stream data);
        /// <summary>
        /// Saves an image to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="image">Image data</param>
        void SaveToFile(string fileName, Image image);
    }

    /// <summary>
    /// Image
    /// </summary>
    public struct Image
    {
        /// <summary>
        /// Image pixel colors
        /// </summary>
        private readonly Color4[,] colors;

        /// <summary>
        /// Gets the image width in pixels
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the image height in pixels
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="colors">Pixel colors</param>
        public Image(Color4[,] colors)
        {
            this.colors = colors;

            Width = colors.GetLength(0);
            Height = colors.GetLength(1);
        }

        /// <summary>
        /// Gets the pixel color value at position
        /// </summary>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        public Color4 GetPixel(int width, int height)
        {
            return colors[width, height];
        }
        /// <summary>
        /// Flattens the image pixel into a one-dimension array
        /// </summary>
        public IEnumerable<Color4> Flatten()
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
