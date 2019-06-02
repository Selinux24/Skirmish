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
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Sphere radii effect variable
        /// </summary>
        private readonly EngineEffectVariableVector sphereRadiiVar = null;
        /// <summary>
        /// 
        /// </summary>
        private readonly EngineEffectVariableVector scatteringCoefficientsVar = null;
        /// <summary>
        /// 
        /// </summary>
        private readonly EngineEffectVariableVector inverseWaveLengthVar = null;
        /// <summary>
        /// 
        /// </summary>
        private readonly EngineEffectVariableVector miscVar = null;
        /// <summary>
        /// Back color variable
        /// </summary>
        private readonly EngineEffectVariableVector backColorVar = null;
        /// <summary>
        /// Light direction effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lightDirectionWorldVar = null;
        /// <summary>
        /// HDR exposure effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar hdrExposureVar = null;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjectionVar.GetMatrix();
            }
            set
            {
                this.worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Sphere radii
        /// </summary>
        protected Vector4 SphereRadii
        {
            get
            {
                return this.sphereRadiiVar.GetVector<Vector4>();
            }
            set
            {
                this.sphereRadiiVar.Set(value);
            }
        }
        /// <summary>
        /// Scattering coefficients
        /// </summary>
        protected Vector4 ScatteringCoefficients
        {
            get
            {
                return this.scatteringCoefficientsVar.GetVector<Vector4>();
            }
            set
            {
                this.scatteringCoefficientsVar.Set(value);
            }
        }
        /// <summary>
        /// Inverse waveLength
        /// </summary>
        protected Vector4 InverseWaveLength
        {
            get
            {
                return this.inverseWaveLengthVar.GetVector<Vector4>();
            }
            set
            {
                this.inverseWaveLengthVar.Set(value);
            }
        }
        /// <summary>
        /// Misc: camera height, squared camera height, scale and scale over scale depth
        /// </summary>
        protected Vector4 Misc
        {
            get
            {
                return this.miscVar.GetVector<Vector4>();
            }
            set
            {
                this.miscVar.Set(value);
            }
        }
        /// <summary>
        /// Back color
        /// </summary>
        protected Color4 BackColor
        {
            get
            {
                return this.backColorVar.GetVector<Color4>();
            }
            set
            {
                this.backColorVar.Set(value);
            }
        }
        /// <summary>
        /// Light direction
        /// </summary>
        protected Vector3 LightDirectionWorld
        {
            get
            {
                return this.lightDirectionWorldVar.GetVector<Vector3>();
            }
            set
            {
                this.lightDirectionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// HDR exposure
        /// </summary>
        protected float HDRExposure
        {
            get
            {
                return this.hdrExposureVar.GetFloat();
            }
            set
            {
                this.hdrExposureVar.Set(value);
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

            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.sphereRadiiVar = this.Effect.GetVariableVector("gSphereRadii");
            this.scatteringCoefficientsVar = this.Effect.GetVariableVector("gScatteringCoeffs");
            this.inverseWaveLengthVar = this.Effect.GetVariableVector("gInvWaveLength");
            this.miscVar = this.Effect.GetVariableVector("gMisc");
            this.backColorVar = this.Effect.GetVariableVector("gBackColor");
            this.lightDirectionWorldVar = this.Effect.GetVariableVector("gLightDirection");
            this.hdrExposureVar = this.Effect.GetVariableScalar("gHDRExposure");
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
