using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.Common;

    public struct BasicFontState
    {
        public float Alpha { get; set; }
        public bool UseColor { get; set; }
        public bool UseRectangle { get; set; }
        public bool FineSampling { get; set; }
        public Rectangle ClippingRectangle { get; set; }
        public EngineShaderResourceView FontTexture { get; set; }
    }
}
