using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sky scattering description
    /// </summary>
    public class SkyScatteringDescription : DrawableDescription
    {
        /// <summary>
        /// Rayleigh scattering constant.
        /// </summary>
        public const float RayleighScatteringConstant = 0.0035f;
        /// <summary>
        /// Rayleigh scale depth constant.
        /// </summary>
        public const float RayleighScaleDepthConstant = 0.25f;
        /// <summary>
        /// Mie scattering constant.
        /// </summary>
        public const float MieScatteringConstant = 0.0045f;
        /// <summary>
        /// Mie phase asymmetry factor.
        /// </summary>
        public const float MiePhaseAssymetryFactor = -0.75f;
        /// <summary>
        /// Mie scale depth constant.
        /// </summary>
        public const float MieScaleDepthConstant = 0.1f;
        /// <summary>
        /// Earth radius
        /// </summary>
        public const float EarthRadius = (6378.0f * 1000.0f);
        /// <summary>
        /// Earth atmosphere radius
        /// </summary>
        public const float EarthAtmosphereRadius = 200000.0f;
        /// <summary>
        /// Earth sky brightness
        /// </summary>
        public const float EarthSkyBrightness = 25.0f;
        /// <summary>
        /// Sun light wave length
        /// </summary>
        public static readonly Color4 SunLightWaveLength = new Color4(0.650f, 0.570f, 0.475f, 0);

        /// <summary>
        /// Planet radius
        /// </summary>
        public float PlanetRadius { get; set; }
        /// <summary>
        /// Planet atmosphere radius from surface
        /// </summary>
        public float PlanetAtmosphereRadius { get; set; }
        /// <summary>
        /// Rayleigh scattering constant value
        /// </summary>
        public float RayleighScattering { get; set; }
        /// <summary>
        /// Rayleigh scale depth value
        /// </summary>
        public float RayleighScaleDepth { get; set; }
        /// <summary>
        /// Mie scattering constant value
        /// </summary>
        public float MieScattering { get; set; }
        /// <summary>
        /// Mie phase assymetry value
        /// </summary>
        public float MiePhaseAssymetry { get; set; }
        /// <summary>
        /// Mie scale depth value
        /// </summary>
        public float MieScaleDepth { get; set; }
        /// <summary>
        /// Light wave length
        /// </summary>
        public Color4 WaveLength { get; set; }
        /// <summary>
        /// Sky brightness
        /// </summary>
        public float Brightness { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkyScatteringDescription()
            : base()
        {
            this.PlanetRadius = EarthRadius;
            this.PlanetAtmosphereRadius = EarthAtmosphereRadius;

            this.RayleighScattering = RayleighScatteringConstant;
            this.RayleighScaleDepth = RayleighScaleDepthConstant;
            this.MieScattering = MieScatteringConstant;
            this.MiePhaseAssymetry = MiePhaseAssymetryFactor;
            this.MieScaleDepth = MieScaleDepthConstant;

            this.WaveLength = SunLightWaveLength;
            this.Brightness = EarthSkyBrightness;

            this.Static = true;
            this.AlwaysVisible = false;
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.EnableDepthStencil = false;
            this.EnableAlphaBlending = false;
        }
    }
}
