using SharpDX;

namespace Engine
{
    /// <summary>
    /// Heightmap description
    /// </summary>
    public class HeightmapDescription
    {
        /// <summary>
        /// Creates a height map descripton from map data
        /// </summary>
        /// <param name="heightmap">Height map</param>
        /// <param name="cellsize">Cell size</param>
        /// <param name="maximumHeight">Maximum height</param>
        public static HeightmapDescription FromMap(float[,] heightmap, float cellsize, float maximumHeight)
        {
            return new HeightmapDescription
            {
                Heightmap = heightmap,
                CellSize = cellsize,
                MaximumHeight = maximumHeight,
            };
        }

        /// <summary>
        /// Terrain textures
        /// </summary>
        public class TexturesDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath { get; set; } = "Textures";

            /// <summary>
            /// Normal maps
            /// </summary>
            public string[] NormalMaps { get; set; } = null;
            /// <summary>
            /// Specular maps
            /// </summary>
            public string[] SpecularMaps { get; set; } = null;

            /// <summary>
            /// Gets or sets if use alpha mapping or not
            /// </summary>
            public bool UseAlphaMapping { get; set; } = false;
            /// <summary>
            /// Alpha map
            /// </summary>
            public string AlphaMap { get; set; } = null;
            /// <summary>
            /// Color textures for alpha map
            /// </summary>
            public string[] ColorTextures { get; set; } = null;

            /// <summary>
            /// Gets or sets if use slope texturing or not
            /// </summary>
            public bool UseSlopes { get; set; } = false;
            /// <summary>
            /// Slope ranges
            /// </summary>
            public Vector2 SlopeRanges { get; set; } = Vector2.Zero;
            /// <summary>
            /// High resolution textures
            /// </summary>
            public string[] TexturesHR { get; set; } = null;
            /// <summary>
            /// Low resolution textures
            /// </summary>
            public string[] TexturesLR { get; set; } = null;

            /// <summary>
            /// Lerping proportion between alpha mapping and slope texturing
            /// </summary>
            public float Proportion { get; set; } = 0f;
            /// <summary>
            /// UV texture scale
            /// </summary>
            public float Scale { get; set; } = 1;
            /// <summary>
            /// UV texture displacement
            /// </summary>
            public Vector2 Displacement { get; set; } = Vector2.Zero;
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Height map
        /// </summary>
        public float[,] Heightmap { get; set; } = null;
        /// <summary>
        /// Height map file name
        /// </summary>
        public string HeightmapFileName { get; set; } = null;
        /// <summary>
        /// Color map
        /// </summary>
        public Color4[,] Colormap { get; set; } = null;
        /// <summary>
        /// Color map file name
        /// </summary>
        public string ColormapFileName { get; set; } = null;
        /// <summary>
        /// Cell size
        /// </summary>
        public float CellSize { get; set; } = 1;
        /// <summary>
        /// Maximum height
        /// </summary>
        public float MaximumHeight { get; set; } = 1;
        /// <summary>
        /// Texture resolution
        /// </summary>
        public float TextureResolution { get; set; } = 10;
        /// <summary>
        /// Textures
        /// </summary>
        public TexturesDescription Textures { get; set; } = new TexturesDescription();
        /// <summary>
        /// Terrain material
        /// </summary>
        public MaterialDescription Material { get; set; } = new MaterialDescription();
    }
}
