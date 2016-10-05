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
            public string ContentPath = "Textures";

            /// <summary>
            /// Normal maps
            /// </summary>
            public string[] NormalMaps = null;

            /// <summary>
            /// Gets or sets if use alpha mapping or not
            /// </summary>
            public bool UseAlphaMapping = false;
            /// <summary>
            /// Alpha map
            /// </summary>
            public string AlphaMap = null;
            /// <summary>
            /// Color textures for alpha map
            /// </summary>
            public string[] ColorTextures = null;

            /// <summary>
            /// Gets or sets if use slope texturing or not
            /// </summary>
            public bool UseSlopes = false;
            /// <summary>
            /// Slope ranges
            /// </summary>
            public Vector2 SlopeRanges = Vector2.Zero;
            /// <summary>
            /// High resolution textures
            /// </summary>
            public string[] TexturesHR = null;
            /// <summary>
            /// Low resolution textures
            /// </summary>
            public string[] TexturesLR = null;

            /// <summary>
            /// Lerping proportion between alpha mapping and slope texturing
            /// </summary>
            public float Proportion = 0f;
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
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
        /// <summary>
        /// Textures
        /// </summary>
        public TexturesDescription Textures = null;
    }
}
