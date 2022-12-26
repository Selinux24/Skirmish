using SharpDX;
using System.Collections.Generic;
using System.IO;

namespace Engine
{
    public interface IImages
    {
        Image FromStream(Stream data);

        void SaveToFile(string fileName, Image image);
    }

    public struct Image
    {
        public readonly Color4[,] Colors;
        public readonly int Width;
        public readonly int Height;

        public Image(Color4[,] colors)
        {
            Colors = colors;
            Width = colors.GetLength(0);
            Height = colors.GetLength(1);
        }

        public Color4 GetPixel(int width, int height)
        {
            return Colors[width, height];
        }

        public IEnumerable<Color4> Flatten()
        {
            Color4[] colors = new Color4[Width * Height];

            int index = 0;
            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    colors[index++] = Colors[w, h];
                }
            }

            return colors;
        }
    }
}
