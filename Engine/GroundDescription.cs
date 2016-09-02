﻿using System;
using SharpDX;

namespace Engine
{
    using Engine.PathFinding;

    /// <summary>
    /// Terrain description
    /// </summary>
    public class GroundDescription
    {
        /// <summary>
        /// Model description
        /// </summary>
        public class ModelDescription
        {
            /// <summary>
            /// Model file name
            /// </summary>
            public string ModelFileName = null;
        }
        /// <summary>
        /// Heightmap description
        /// </summary>
        public class HeightmapDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Heightmap";
            /// <summary>
            /// Height map file name
            /// </summary>
            public string HeightmapFileName = null;
            /// <summary>
            /// Color map file name
            /// </summary>
            public string ColormapFileName = null;
            /// <summary>
            /// Cell size
            /// </summary>
            public float CellSize = 1;
            /// <summary>
            /// Maximum height
            /// </summary>
            public float MaximumHeight = 1;
        }
        /// <summary>
        /// Terrain textures
        /// </summary>
        public class TexturesDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Textures";
            /// <summary>
            /// High resolution textures
            /// </summary>
            public string[] TexturesHR = null;
            /// <summary>
            /// Low resolution textures
            /// </summary>
            public string[] TexturesLR = null;
            /// <summary>
            /// Normal maps
            /// </summary>
            public string[] NormalMaps = null;
            /// <summary>
            /// Color textures for alpha map
            /// </summary>
            public string[] ColorTextures = null;
            /// <summary>
            /// Alpha map
            /// </summary>
            public string AlphaMap = null;
            /// <summary>
            /// Slope ranges
            /// </summary>
            public Vector2 SlopeRanges = Vector2.Zero;
        }
        /// <summary>
        /// Vegetation
        /// </summary>
        public class VegetationDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Vegetation";
            /// <summary>
            /// Drawing radius for vegetation
            /// </summary>
            public float StartRadius = 0f;
            /// <summary>
            /// Drawing radius for vegetation
            /// </summary>
            public float EndRadius = 0f;
            /// <summary>
            /// Seed for random position generation
            /// </summary>
            public int Seed = 0;
            /// <summary>
            /// Vegetation saturation per triangle
            /// </summary>
            public float Saturation = 0.1f;
            /// <summary>
            /// Is opaque
            /// </summary>
            public bool Opaque = true;
            /// <summary>
            /// Can be renderer by the deferred renderer
            /// </summary>
            public bool DeferredEnabled = true;

            /// <summary>
            /// Texture names array for vegetation
            /// </summary>
            public string[] VegetarionTextures = null;
            /// <summary>
            /// Vegetation sprite minimum size
            /// </summary>
            public Vector2 MinSize = Vector2.One;
            /// <summary>
            /// Vegetation sprite maximum size
            /// </summary>
            public Vector2 MaxSize = Vector2.One * 2f;
        }
        /// <summary>
        /// Quadtree description
        /// </summary>
        public class QuadtreeDescription
        {
            /// <summary>
            /// Maximum depth
            /// </summary>
            public int MaximumDepth { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public QuadtreeDescription()
            {
                this.MaximumDepth = 3;
            }
        }
        /// <summary>
        /// Path finder grid description
        /// </summary>
        public class PathFinderDescription
        {
            /// <summary>
            /// Graph type
            /// </summary>
            public PathFinderSettings Settings = null;
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Model
        /// </summary>
        public ModelDescription Model = null;
        /// <summary>
        /// Heightmap
        /// </summary>
        public HeightmapDescription Heightmap = null;
        /// <summary>
        /// Textures
        /// </summary>
        public TexturesDescription Textures = null;
        /// <summary>
        /// Vegetation collection
        /// </summary>
        public VegetationDescription Vegetation = null;
        /// <summary>
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree = null;
        /// <summary>
        /// Path finder
        /// </summary>
        public PathFinderDescription PathFinder = null;

        /// <summary>
        /// Name
        /// </summary>
        public string Name = null;
        /// <summary>
        /// Is Opaque
        /// </summary>
        public bool Opaque = true;
        /// <summary>
        /// Is Static
        /// </summary>
        public bool Static = true;
        /// <summary>
        /// Can be renderer by the deferred renderer
        /// </summary>
        public bool DeferredEnabled = true;
        /// <summary>
        /// Delay internal generation
        /// </summary>
        public bool DelayGeneration = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundDescription()
        {

        }
    }
}
