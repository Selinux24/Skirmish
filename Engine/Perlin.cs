﻿using SharpDX;

namespace Engine
{
    /// <summary>
    /// Perlin Noise
    /// </summary>
    /// <remarks>
    /// From https://github.com/keijiro/PerlinNoise/blob/master/Assets/Perlin.cs
    /// </remarks>
    public static class Perlin
    {
        static readonly int[] perm =
        {
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
            151
        };

        /// <summary>
        /// Gets a perlin noise value at 1D coordinate
        /// </summary>
        /// <param name="x">X coordinate</param>
        public static float Noise(float x)
        {
            var X = (int)x & 0xff;
            x -= (int)x;

            var u = Fade(x);

            return Lerp(u, Grad(perm[X], x), Grad(perm[X + 1], x - 1)) * 2;
        }
        /// <summary>
        /// Gets a perlin noise value at 2D coordinate
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public static float Noise(float x, float y)
        {
            int X = (int)x & 0xff;
            int Y = (int)y & 0xff;
            x -= (int)x;
            y -= (int)y;

            float u = Fade(x);
            float v = Fade(y);

            int A = (perm[X + 0] + Y) & 0xff;
            int B = (perm[X + 1] + Y) & 0xff;

            float l1 = Lerp(u, Grad(perm[A + 0], x, y + 0), Grad(perm[B + 0], x - 1, y + 0));
            float l2 = Lerp(u, Grad(perm[A + 1], x, y - 1), Grad(perm[B + 1], x - 1, y - 1));
            float res = Lerp(v, l1, l2);
            return res;
        }
        /// <summary>
        /// Gets a perlin noise value at 2D coordinate
        /// </summary>
        /// <param name="coord">2D coordinate</param>
        public static float Noise(Vector2 coord)
        {
            return Noise(coord.X, coord.Y);
        }
        /// <summary>
        /// Gets a perlin noise value at 3D coordinate
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public static float Noise(float x, float y, float z)
        {
            var X = (int)x & 0xff;
            var Y = (int)y & 0xff;
            var Z = (int)z & 0xff;
            x -= (int)x;
            y -= (int)y;
            z -= (int)z;

            var u = Fade(x);
            var v = Fade(y);
            var w = Fade(z);

            var A = (perm[X + 0] + Y) & 0xff;
            var B = (perm[X + 1] + Y) & 0xff;

            var AA = (perm[A + 0] + Z) & 0xff;
            var BA = (perm[B + 0] + Z) & 0xff;
            var AB = (perm[A + 1] + Z) & 0xff;
            var BB = (perm[B + 1] + Z) & 0xff;

            return Lerp(w, Lerp(v, Lerp(u, Grad(perm[AA + 0], x, y + 0, z + 0), Grad(perm[BA + 0], x - 1, y + 0, z + 0)),
                                   Lerp(u, Grad(perm[AB + 0], x, y - 1, z + 0), Grad(perm[BB + 0], x - 1, y - 1, z + 0))),
                           Lerp(v, Lerp(u, Grad(perm[AA + 1], x, y + 0, z - 1), Grad(perm[BA + 1], x - 1, y + 0, z - 1)),
                                   Lerp(u, Grad(perm[AB + 1], x, y - 1, z - 1), Grad(perm[BB + 1], x - 1, y - 1, z - 1))));
        }
        /// <summary>
        /// Gets a perlin noise value at 3D coordinate
        /// </summary>
        /// <param name="coord">3D coordinate</param>
        public static float Noise(Vector3 coord)
        {
            return Noise(coord.X, coord.Y, coord.Z);
        }

        /// <summary>
        /// Gets a fractional brownian noise value at 1D coordinate
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="octave">Octave</param>
        public static float Fbm(float x, int octave)
        {
            var f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * Noise(x);
                x *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }
        /// <summary>
        /// Gets a fractional brownian noise value at 2D coordinate
        /// </summary>
        /// <param name="coord">2D coordinate</param>
        /// <param name="octave">Octave</param>
        public static float Fbm(Vector2 coord, int octave)
        {
            var f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * Noise(coord);
                coord *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }
        /// <summary>
        /// Gets a fractional brownian noise value at 2D coordinate
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="octave">Octave</param>
        public static float Fbm(float x, float y, int octave)
        {
            return Fbm(new Vector2(x, y), octave);
        }
        /// <summary>
        /// Gets a fractional brownian noise value at 3D coordinate
        /// </summary>
        /// <param name="coord">3D coordinate</param>
        /// <param name="octave">Octave</param>
        public static float Fbm(Vector3 coord, int octave)
        {
            var f = 0.0f;
            var w = 0.5f;
            for (var i = 0; i < octave; i++)
            {
                f += w * Noise(coord);
                coord *= 2.0f;
                w *= 0.5f;
            }
            return f;
        }
        /// <summary>
        /// Gets a fractional brownian noise value at 3D coordinate
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="octave">Octave</param>
        public static float Fbm(float x, float y, float z, int octave)
        {
            return Fbm(new Vector3(x, y, z), octave);
        }


        static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        static float Grad(int hash, float x)
        {
            return (hash & 1) == 0 ? x : -x;
        }

        static float Grad(int hash, float x, float y)
        {
            return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
        }

        static float Grad(int hash, float x, float y, float z)
        {
            var h = hash & 15;
            var hv = h == 12 || h == 14 ? x : z;
            var u = h < 8 ? x : y;
            var v = h < 4 ? y : hv;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }
}
