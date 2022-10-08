using SharpDX;

namespace Engine.BuiltIn.Billboard
{
    using Engine.Common;

    /// <summary>
    /// Built-in billboard state
    /// </summary>
    public struct BuiltInBillboardState
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
        /// Tint color
        /// </summary>
        public Color4 TintColor { get; set; }
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
        /// Random texture
        /// </summary>
        public EngineShaderResourceView RandomTexture { get; set; }
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
