
namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Effect billboard state
    /// </summary>
    public struct EffectBillboardState
    {
        /// <summary>
        /// Drawing start radius
        /// </summary>
        public float StartRadius { get; set; }
        /// <summary>
        /// Drawing end radius
        /// </summary>
        public float EndRadius { get; set; }
        /// <summary>
        /// Random texture
        /// </summary>
        public EngineShaderResourceView RandomTexture { get; set; }
        /// <summary>
        /// Material index
        /// </summary>
        public uint MaterialIndex { get; set; }
        /// <summary>
        /// Texture count
        /// </summary>
        public uint TextureCount { get; set; }
        /// <summary>
        /// Normal map count
        /// </summary>
        public uint NormalMapCount { get; set; }
        /// <summary>
        /// Texture
        /// </summary>
        public EngineShaderResourceView Texture { get; set; }
        /// <summary>
        /// Normal maps
        /// </summary>
        public EngineShaderResourceView NormalMaps { get; set; }
    }
}
