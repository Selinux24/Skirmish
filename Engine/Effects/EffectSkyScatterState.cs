using SharpDX;

namespace Engine.Effects
{
    /// <summary>
    /// Effect sky scatter state
    /// </summary>
    public struct EffectSkyScatterState
    {
        /// <summary>
        /// Planet radius
        /// </summary>
        public float PlanetRadius { get; set; }
        /// <summary>
        /// Planet atmosphere radius from surface
        /// </summary>
        public float PlanetAtmosphereRadius { get; set; }
        /// <summary>
        /// Sphere outer radius
        /// </summary>
        public float SphereOuterRadius { get; set; }
        /// <summary>
        /// Sphere inner radius
        /// </summary>
        public float SphereInnerRadius { get; set; }
        /// <summary>
        /// Sky brightness
        /// </summary>
        public float SkyBrightness { get; set; }
        /// <summary>
        /// Rayleigh scattering constant
        /// </summary>
        public float RayleighScattering { get; set; }
        /// <summary>
        /// Rayleigh scattering constant * 4 * PI
        /// </summary>
        public float RayleighScattering4PI { get; set; }
        /// <summary>
        /// Mie scattering constant
        /// </summary>
        public float MieScattering { get; set; }
        /// <summary>
        /// Mie scattering constant * 4 * PI
        /// </summary>
        public float MieScattering4PI { get; set; }
        /// <summary>
        /// Inverse light wave length
        /// </summary>
        public Color3 InvWaveLength4 { get; set; }
        /// <summary>
        /// Scale
        /// </summary>
        public float Scale { get; set; }
        /// <summary>
        /// Rayleigh scale depth
        /// </summary>
        public float RayleighScaleDepth { get; set; }
        /// <summary>
        /// Back color
        /// </summary>
        public Color4 BackColor { get; set; }
        /// <summary>
        /// HDR exposure
        /// </summary>
        public float HdrExposure { get; set; }
    }
}
