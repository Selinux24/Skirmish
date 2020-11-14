using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Effect terrain state
    /// </summary>
    public struct EffectTerrainState
    {
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic { get; set; }
        /// <summary>
        /// Normal map
        /// </summary>
        public EngineShaderResourceView NormalMap { get; set; }
        /// <summary>
        /// Use alpha mapping
        /// </summary>
        public bool UseAlphaMap { get; set; }
        /// <summary>
        /// Alpha map
        /// </summary>
        public EngineShaderResourceView AlphaMap { get; set; }
        /// <summary>
        /// Color textures
        /// </summary>
        public EngineShaderResourceView ColorTextures { get; set; }
        /// <summary>
        /// Use slope texturing
        /// </summary>
        public bool UseSlopes { get; set; }
        /// <summary>
        /// Slope ranges
        /// </summary>
        public Vector2 SlopeRanges { get; set; }
        /// <summary>
        /// Low resolution textures
        /// </summary>
        public EngineShaderResourceView DiffuseMapLR { get; set; }
        /// <summary>
        /// High resolution textures
        /// </summary>
        public EngineShaderResourceView DiffuseMapHR { get; set; }
        /// <summary>
        /// Lerping proportion
        /// </summary>
        public float Proportion { get; set; }
        /// <summary>
        /// Marerial index
        /// </summary>
        public uint MaterialIndex { get; set; }
    }
}
