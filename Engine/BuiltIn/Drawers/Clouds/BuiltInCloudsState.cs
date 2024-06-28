using SharpDX;

namespace Engine.BuiltIn.Drawers.Clouds
{
    using Engine.Common;

    /// <summary>
    /// Clouds drawer state
    /// </summary>
    public struct BuiltInCloudsState
    {
        /// <summary>
        /// Perturbed clouds
        /// </summary>
        public bool Perturbed { get; set; }
        /// <summary>
        /// Translation velocity
        /// </summary>
        public float Translation { get; set; }
        /// <summary>
        /// Scale
        /// </summary>
        public float Scale { get; set; }
        /// <summary>
        /// Fadding distance
        /// </summary>
        public float FadingDistance { get; set; }
        /// <summary>
        /// First layer translation velocity
        /// </summary>
        public Vector2 FirstTranslation { get; set; }
        /// <summary>
        /// Second layer translation velocity
        /// </summary>
        public Vector2 SecondTranslation { get; set; }
        /// <summary>
        /// Color
        /// </summary>
        public Color3 Color { get; set; }
        /// <summary>
        /// Brightness
        /// </summary>
        public float Brightness { get; set; }
        /// <summary>
        /// First cloud layer
        /// </summary>
        public EngineShaderResourceView Clouds1 { get; set; }
        /// <summary>
        /// Second cloud layer
        /// </summary>
        public EngineShaderResourceView Clouds2 { get; set; }
    }
}
