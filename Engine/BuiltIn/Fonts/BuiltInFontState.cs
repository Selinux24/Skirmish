using SharpDX;

namespace Engine.BuiltIn.Fonts
{
    using Engine.Common;

    /// <summary>
    /// Font drawer state
    /// </summary>
    public struct BuiltInFontState
    {
        /// <summary>
        /// Alpha modifier
        /// </summary>
        public float Alpha { get; set; }
        /// <summary>
        /// Use texture color
        /// </summary>
        public bool UseColor { get; set; }
        /// <summary>
        /// Use clipping rectangle
        /// </summary>
        public bool UseRectangle { get; set; }
        /// <summary>
        /// Fine sampling
        /// </summary>
        public bool FineSampling { get; set; }
        /// <summary>
        /// Clipping rectangle
        /// </summary>
        public Rectangle ClippingRectangle { get; set; }
        /// <summary>
        /// Font texture
        /// </summary>
        public EngineShaderResourceView FontTexture { get; set; }
    }
}
