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
                return worldViewProjectionVar.GetMatrix();
            }
            set
            {
                worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Sphere radii
        /// </summary>
        protected Vector4 SphereRadii
        {
            get
            {
                return sphereRadiiVar.GetVector<Vector4>();
            }
            set
            {
                sphereRadiiVar.Set(value);
            }
        }
        /// <summary>
        /// Scattering coefficients
        /// </summary>
        protected Vector4 ScatteringCoefficients
        {
            get
            {
                return scatteringCoefficientsVar.GetVector<Vector4>();
            }
            set
            {
                scatteringCoefficientsVar.Set(value);
            }
        }
        /// <summary>
        /// Inverse waveLength
        /// </summary>
        protected Vector4 InverseWaveLength
        {
            get
            {
                return inverseWaveLengthVar.GetVector<Vector4>();
            }
            set
            {
                inverseWaveLengthVar.Set(value);
            }
        }
        /// <summary>
        /// Misc: camera height, squared camera height, scale and scale over scale depth
        /// </summary>
        protected Vector4 Misc
        {
            get
            {
                return miscVar.GetVector<Vector4>();
            }
            set
            {
                miscVar.Set(value);
            }
        }
        /// <summary>
        /// Back color
        /// </summary>
        protected Color4 BackColor
        {
            get
            {
                return backColorVar.GetVector<Color4>();
            }
            set
            {
                backColorVar.Set(value);
            }
        }
        /// <summary>
        /// Light direction
        /// </summary>
        protected Vector3 LightDirectionWorld
        {
            get
            {
                return lightDirectionWorldVar.GetVector<Vector3>();
            }
            set
            {
                lightDirectionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// HDR exposure
        /// </summary>
        protected float HDRExposure
        {
            get
            {
                return hdrExposureVar.GetFloat();
            }
            set
            {
                hdrExposureVar.Set(value);
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
            SkyScatteringLow = Effect.GetTechniqueByName("SkyScatteringLow");
            SkyScatteringMedium = Effect.GetTechniqueByName("SkyScatteringMedium");
            SkyScatteringHigh = Effect.GetTechniqueByName("SkyScatteringHigh");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            sphereRadiiVar = Effect.GetVariableVector("gSphereRadii");
            scatteringCoefficientsVar = Effect.GetVariableVector("gScatteringCoeffs");
            inverseWaveLengthVar = Effect.GetVariableVector("gInvWaveLength");
            miscVar = Effect.GetVariableVector("gMisc");
            backColorVar = Effect.GetVariableVector("gBackColor");
            lightDirectionWorldVar = Effect.GetVariableVector("gLightDirection");
            hdrExposureVar = Effect.GetVariableScalar("gHDRExposure");
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
            WorldViewProjection = world * viewProjection;

            SphereRadii = new Vector4(
                state.SphereOuterRadius, state.SphereOuterRadius * state.SphereOuterRadius,
                state.SphereInnerRadius, state.SphereInnerRadius * state.SphereInnerRadius);

            ScatteringCoefficients = new Vector4(
                state.RayleighScattering * state.SkyBrightness, state.RayleighScattering4PI,
                state.MieScattering * state.SkyBrightness, state.MieScattering4PI);

            InverseWaveLength = new Vector4(state.InvWaveLength4, 1f);

            Misc = new Vector4(state.PlanetRadius, state.PlanetAtmosphereRadius, state.Scale, state.Scale / state.RayleighScaleDepth);

            BackColor = state.BackColor;

            LightDirectionWorld = -lightDirection;
            HDRExposure = state.HdrExposure;
        }
    }
}
