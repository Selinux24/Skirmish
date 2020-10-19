using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Bitmap = System.Drawing.Bitmap;
    using GCHandle = System.Runtime.InteropServices.GCHandle;
    using GCHandleType = System.Runtime.InteropServices.GCHandleType;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;

    /// <summary>
    /// Noise map
    /// </summary>
    public class NoiseMap
    {
        /// <summary>
        /// Creates a noise map
        /// </summary>
        /// <param name="descriptor">Noise map descriptor</param>
        public static NoiseMap CreateNoiseMap(NoiseMapDescriptor descriptor)
        {
            descriptor.Validate();

            int mapWidth = descriptor.MapWidth;
            int mapHeight = descriptor.MapHeight;
            float scale = descriptor.Scale;
            int octaves = descriptor.Octaves;
            float persistance = descriptor.Persistance;
            float lacunarity = descriptor.Lacunarity;
            int seed = descriptor.Seed;
            Vector2 offset = descriptor.Offset;

            float[,] noiseMap = new float[mapWidth, mapHeight];

            Random rnd = new Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float sampleX = rnd.NextFloat(-1f, 1f) + offset.X;
                float sampleY = rnd.NextFloat(-1f, 1f) + offset.Y;
                octaveOffsets[i] = new Vector2(sampleX, sampleY);
            }

            float fX = 1f / mapWidth;
            float fY = 1f / mapHeight;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x * fX + octaveOffsets[i].X) / scale * frequency;
                        float sampleY = (y * fY + octaveOffsets[i].Y) / scale * frequency;

                        float perlinValue = Perlin.Noise(sampleX, sampleY) * 2f - 1f; //-1 to 1
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    maxNoiseHeight = Math.Max(maxNoiseHeight, noiseHeight);
                    minNoiseHeight = Math.Min(minNoiseHeight, noiseHeight);

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return new NoiseMap { Map = noiseMap };
        }
        /// <summary>
        /// Clamps smoothly the vale to a and b limits
        /// </summary>
        /// <param name="a">Min value</param>
        /// <param name="b">Max value</param>
        /// <param name="value">Value to clamp</param>
        /// <returns>Returns the clamped value</returns>
        private static float InverseLerp(float a, float b, float value)
        {
            if (a != b)
            {
                return MathUtil.Clamp((value - a) / (b - a), 0f, 1f);
            }
            else
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// Map
        /// </summary>
        public float[,] Map { get; private set; }

        /// <summary>
        /// Creates a color map for a texture from a noise map
        /// </summary>
        public IEnumerable<Color4> CreateColors()
        {
            int width = Map.GetLength(0);
            int height = Map.GetLength(1);

            Color4[] colors = new Color4[width * height];
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colors[index++] = Color4.Lerp(Color4.White, Color4.Black, Map[x, y]);
                }
            }

            return colors;
        }
        /// <summary>
        /// Saves the noise map to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void SaveMapToFile(string fileName)
        {
            int width = Map.GetLength(0);
            int height = Map.GetLength(1);

            var colors = CreateColors();

            int stride = sizeof(int);
            int[] bits = colors.Select(c => c.ToRgba()).ToArray();

            var bitsHandle = GCHandle.Alloc(bits, GCHandleType.Pinned);
            using (var bitmap = new Bitmap(width, height, width * stride, PixelFormat.Format32bppPArgb, bitsHandle.AddrOfPinnedObject()))
            {
                bitmap.Save(fileName);
            }
        }
    }
}
