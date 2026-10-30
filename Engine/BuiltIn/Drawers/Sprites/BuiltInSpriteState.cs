using Engine.Common;

namespace Engine.BuiltIn.Drawers.Sprites
{
    using Engine.Mathematics;

    /// <summary>
    /// Texture sprite drawer state
    /// </summary>
    public struct BuiltInSpriteState
    {
        /// <summary>
        /// Local transform
        /// </summary>
        public Matrix4x4 Local { get; set; }
        /// <summary>
        /// First color
        /// </summary>
        public Vector4 Color1 { get; set; }
        /// <summary>
        /// Secondo color
        /// </summary>
        public Vector4 Color2 { get; set; }
        /// <summary>
        /// Third color
        /// </summary>
        public Vector4 Color3 { get; set; }
        /// <summary>
        /// Fourth color
        /// </summary>
        public Vector4 Color4 { get; set; }
        /// <summary>
        /// Color channel
        /// </summary>
        public ColorChannels Channel { get; set; }
        /// <summary>
        /// Use percentages
        /// </summary>
        public bool UsePercentage { get; set; }
        /// <summary>
        /// First percentage
        /// </summary>
        public float Percentage1 { get; set; }
        /// <summary>
        /// Second percentage
        /// </summary>
        public float Percentage2 { get; set; }
        /// <summary>
        /// Third percentage
        /// </summary>
        public float Percentage3 { get; set; }
        /// <summary>
        /// Render area in pixels
        /// </summary>
        public Rect RenderArea { get; set; }
        /// <summary>
        /// Draw direction
        /// </summary>
        /// <remarks>0 is horizontal, 1 is vertical</remarks>
        public uint Direction { get; set; }
        /// <summary>
        /// Texture
        /// </summary>
        public EngineShaderResourceView Texture { get; set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; }
        /// <summary>
        /// Use clipping rectangle
        /// </summary>
        public bool UseRect { get; set; }
        /// <summary>
        /// Clipping rectangle
        /// </summary>
        public Rect ClippingRecangle { get; set; }
    }
}
