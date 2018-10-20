using SharpDX;

namespace Engine
{
    /// <summary>
    /// Heightmap description
    /// </summary>
    public class HeightmapDescription
    {
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
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Height map file name
        /// </summary>
        public string HeightmapFileName { get; set; } = null;
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
