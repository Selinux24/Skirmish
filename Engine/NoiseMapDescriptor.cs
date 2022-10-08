﻿using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Noise map descriptor
    /// </summary>
    public class NoiseMapDescriptor
    {
        /// <summary>
        /// Width
        /// </summary>
        public int MapWidth { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int MapHeight { get; set; }
        /// <summary>
        /// Noise scale
        /// </summary>
        public float Scale { get; set; } = 1;
        /// <summary>
        /// Octaves
        /// </summary>
        public int Octaves { get; set; } = 4;
        /// <summary>
        /// Persistance
        /// </summary>
        public float Persistance { get; set; } = 0.5f;
        /// <summary>
        /// Lacunarity
        /// </summary>
        public float Lacunarity { get; set; } = 2f;
        /// <summary>
        /// Random seed
        /// </summary>
        public int Seed { get; set; } = 0;
        /// <summary>
        /// Position offset
        /// </summary>
        public Vector2 Offset { get; set; } = Vector2.One;

        /// <summary>
        /// Validates the noise map parameters
        /// </summary>
        public void Validate()
        {
            MapWidth = Math.Max(1, MapWidth);
            MapHeight = Math.Max(1, MapHeight);
            Scale = Math.Max(0.0001f, Scale);
            Octaves = Math.Max(1, Octaves);
            Persistance = MathUtil.Clamp(Persistance, 0, 1);
            Lacunarity = Math.Max(1, Lacunarity);
            Seed = Math.Max(0, Seed);
            Offset = new Vector2(Math.Max(1, Offset.X), Math.Max(1, Offset.Y));
        }
    }
}
