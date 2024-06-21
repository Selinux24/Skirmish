using SharpDX;

namespace Engine.BuiltIn.Drawers.Common
{
    using Engine.Common;

    /// <summary>
    /// Effect sky scatter state
    /// </summary>
    public struct BuiltInTerrainState
    {
        /// <summary>
        /// Tint color
        /// </summary>
        public Color TintColor { get; set; }
        /// <summary>
        /// Scattering coefficients
        /// </summary>
        public uint MaterialIndex { get; set; }
        /// <summary>
        /// Planet radius
        /// </summary>
        public BuiltInTerrainModes Mode { get; set; }
        /// <summary>
        /// Close texture resolution
        /// </summary>
        public float TextureResolution { get; set; }
        /// <summary>
        /// Proportion between alpha mapping and sloped terrain, when Mode is Full
        /// </summary>
        public float Proportion { get; set; }
        /// <summary>
        /// Slope ranges
        /// </summary>
        public Vector2 SlopeRanges { get; set; }
        /// <summary>
        /// Alpha map texture
        /// </summary>
        public EngineShaderResourceView AlphaMap { get; set; }
        /// <summary>
        /// Normal map
        /// </summary>
        public EngineShaderResourceView MormalMap { get; set; }
        /// <summary>
        /// Color texture
        /// </summary>
        public EngineShaderResourceView ColorTexture { get; set; }
        /// <summary>
        /// Low resolution texture
        /// </summary>
        public EngineShaderResourceView LowResolutionTexture { get; set; }
        /// <summary>
        /// High resolution texture
        /// </summary>
        public EngineShaderResourceView HighResolutionTexture { get; set; }
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic { get; set; }
    }

    /// <summary>
    /// Terrain render modes
    /// </summary>
    public enum BuiltInTerrainModes : uint
    {
        /// <summary>
        /// Alpha mapping
        /// </summary>
        AlphaMap,
        /// <summary>
        /// Slope terrain
        /// </summary>
        Slopes,
        /// <summary>
        /// Both alpha mapping and slopes
        /// </summary>
        Full,
    }
}
