using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDefaultSkyScattering : Drawer
    {
        /// <summary>
        /// Low sky scattering technique
        /// </summary>
        public readonly EngineEffectTechnique SkyScatteringLow = null;
        /// <summary>
        /// Medium sky scattering technique
        /// </summary>
        public readonly EngineEffectTechnique SkyScatteringMedium = null;
        /// <summary>
        /// High sky scattering technique
        /// </summary>
        public readonly EngineEffectTechnique SkyScatteringHigh = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Sphere radii effect variable
        /// </summary>
        private readonly EngineEffectVariableVector sphereRadii = null;
        /// <summary>
        /// 
        /// </summary>
        private readonly EngineEffectVariableVector scatteringCoefficients = null;
        /// <summary>
        /// 
        /// </summary>
        private readonly EngineEffectVariableVector inverseWaveLength = null;
        /// <summary>
        /// 
        /// </summary>
        private readonly EngineEffectVariableVector misc = null;
        /// <summary>
        /// Back color variable
        /// </summary>
        private readonly EngineEffectVariableVector backColor = null;
        /// <summary>
        /// Light direction effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lightDirectionWorld = null;
        /// <summary>
        /// HDR exposure effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar hdrExposure = null;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjection.GetMatrix();
            }
            set
            {
                this.worldViewProjection.SetMatrix(value);
            }
        }
        /// <summary>
        /// Sphere radii
        /// </summary>
        protected Vector4 SphereRadii
        {
            get
            {
                return this.sphereRadii.GetVector<Vector4>();
            }
            set
            {
                this.sphereRadii.Set(value);
            }
        }
        /// <summary>
        /// Scattering coefficients
        /// </summary>
        protected Vector4 ScatteringCoefficients
        {
            get
            {
                return this.scatteringCoefficients.GetVector<Vector4>();
            }
            set
            {
                this.scatteringCoefficients.Set(value);
            }
        }
        /// <summary>
        /// Inverse waveLength
        /// </summary>
        protected Vector4 InverseWaveLength
        {
            get
            {
                return this.inverseWaveLength.GetVector<Vector4>();
            }
            set
            {
                this.inverseWaveLength.Set(value);
            }
        }
        /// <summary>
        /// Misc: camera height, squared camera height, scale and scale over scale depth
        /// </summary>
        protected Vector4 Misc
        {
            get
            {
                return this.misc.GetVector<Vector4>();
            }
            set
            {
                this.misc.Set(value);
            }
        }
        /// <summary>
        /// Back color
        /// </summary>
        protected Color4 BackColor
        {
            get
            {
                return this.backColor.GetVector<Color4>();
            }
            set
            {
                this.backColor.Set(value);
            }
        }
        /// <summary>
        /// Light direction
        /// </summary>
        protected Vector3 LightDirectionWorld
        {
            get
            {
                return this.lightDirectionWorld.GetVector<Vector3>();
            }
            set
            {
                this.lightDirectionWorld.Set(value);
            }
        }
        /// <summary>
        /// HDR exposure
        /// </summary>
        protected float HDRExposure
        {
            get
            {
                return this.hdrExposure.GetFloat();
            }
            set
            {
                this.hdrExposure.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultSkyScattering(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.SkyScatteringLow = this.Effect.GetTechniqueByName("SkyScatteringLow");
            this.SkyScatteringMedium = this.Effect.GetTechniqueByName("SkyScatteringMedium");
            this.SkyScatteringHigh = this.Effect.GetTechniqueByName("SkyScatteringHigh");

            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.sphereRadii = this.Effect.GetVariableVector("gSphereRadii");
            this.scatteringCoefficients = this.Effect.GetVariableVector("gScatteringCoeffs");
            this.inverseWaveLength = this.Effect.GetVariableVector("gInvWaveLength");
            this.misc = this.Effect.GetVariableVector("gMisc");
            this.backColor = this.Effect.GetVariableVector("gBackColor");
            this.lightDirectionWorld = this.Effect.GetVariableVector("gLightDirection");
            this.hdrExposure = this.Effect.GetVariableScalar("gHDRExposure");
        }

        /// <summary>
        /// Update per frame
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="lightDirection">Light direction</param>
        /// <param name="state">State</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 lightDirection,
            EffectSkyScatterState state)
        {
            this.WorldViewProjection = world * viewProjection;

            this.SphereRadii = new Vector4(
                state.SphereOuterRadius, state.SphereOuterRadius * state.SphereOuterRadius,
                state.SphereInnerRadius, state.SphereInnerRadius * state.SphereInnerRadius);

            this.ScatteringCoefficients = new Vector4(
                state.RayleighScattering * state.SkyBrightness, state.RayleighScattering4PI,
                state.MieScattering * state.SkyBrightness, state.MieScattering4PI);

            this.InverseWaveLength = state.InvWaveLength4;

            this.Misc = new Vector4(state.PlanetRadius, state.PlanetAtmosphereRadius, state.Scale, state.Scale / state.RayleighScaleDepth);

            this.BackColor = state.BackColor;

            this.LightDirectionWorld = -lightDirection;
            this.HDRExposure = state.HdrExposure;
        }
    }
}
