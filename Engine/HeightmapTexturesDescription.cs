using SharpDX;

namespace Engine
{
    /// <summary>
    /// Terrain textures
    /// </summary>
    public class HeightmapTexturesDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";

        /// <summary>
        /// Normal maps
        /// </summary>
        public string[] NormalMaps { get; set; } = null;

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
        /// <summary>
        /// Texture resolution
        /// </summary>
        public float Resolution { get; set; } = 10;
    }
}
