using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Noise map
    /// </summary>
    public class NoiseMap
    {
        /// <summary>
        /// Creates a simple noise map
        /// </summary>
        /// <param name="descriptor">Noise map descriptor</param>
        /// <remarks>Taking account width, height and scale only</remarks>
        public static NoiseMap CreateSimpleNoiseMap(NoiseMapDescriptor descriptor)
        {
            int mapWidth = descriptor.MapWidth;
            int mapHeight = descriptor.MapHeight;
            float scale = descriptor.Scale;

            if (mapWidth < 1)
            {
                mapWidth = 1;
            }
            if (mapHeight < 1)
            {
                mapHeight = 1;
            }
            if (scale <= 0)
            {
                scale = 0.0001f;
            }

            float[,] noiseMap = new float[mapWidth, mapHeight];

            float fX = 1f / mapWidth;
            float fY = 1f / mapHeight;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float sampleX = x * fX / scale;
                    float sampleY = y * fY / scale;

                    float perlinValue = Perlin.Noise(sampleX, sampleY);

                    maxNoiseHeight = Math.Max(maxNoiseHeight, perlinValue);
                    minNoiseHeight = Math.Min(minNoiseHeight, perlinValue);

                    noiseMap[x, y] = perlinValue;
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
        /// Creates a noise map
        /// </summary>
        /// <param name="descriptor">Noise map descriptor</param>
        public static NoiseMap CreateNoiseMap(NoiseMapDescriptor descriptor)
        {
            int mapWidth = descriptor.MapWidth;
            int mapHeight = descriptor.MapHeight;
            float scale = descriptor.Scale;
            int octaves = descriptor.Octaves;
            float persistance = descriptor.Persistance;
            float lacunarity = descriptor.Lacunarity;
            int seed = descriptor.Seed;
            Vector2 offset = descriptor.Offset;

            if (mapWidth < 1)
            {
                mapWidth = 1;
            }
            if (mapHeight < 1)
            {
                mapHeight = 1;
            }
            if (scale <= 0)
            {
                scale = 0.0001f;
            }
            if (octaves < 0)
            {
                octaves = 0;
            }
            persistance = MathUtil.Clamp(persistance, 0, 1);
            if (lacunarity < 1)
            {
                lacunarity = 1f;
            }

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
                        float sampleX = x * fX / scale * frequency + octaveOffsets[i].X;
                        float sampleY = y * fY / scale * frequency + octaveOffsets[i].Y;

                        float perlinValue = Perlin.Noise(sampleX, sampleY) * 2f - 1f; //-0.5 to 0.5
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
        /// Creates a simple noise color map for a texture
        /// </summary>
        /// <param name="descriptor">Noise map descriptor</param>
        public static IEnumerable<Color4> CreateSimpleNoiseTexture(NoiseMapDescriptor descriptor)
        {
            return CreateSimpleNoiseMap(descriptor).CreateColors();
        }
        /// <summary>
        /// Creates a noise color map for a texture
        /// </summary>
        /// <param name="descriptor">Noise map descriptor</param>
        public static IEnumerable<Color4> CreateNoiseTexture(NoiseMapDescriptor descriptor)
        {
            return CreateNoiseMap(descriptor).CreateColors();
        }

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
    }
}
