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
        /// Default sky scattering technique
        /// </summary>
        protected readonly EngineEffectTechnique SkyScattering = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Sphere radii effect variable
        /// </summary>
        private EngineEffectVariableVector sphereRadii = null;
        /// <summary>
        /// 
        /// </summary>
        private EngineEffectVariableVector scatteringCoefficients = null;
        /// <summary>
        /// 
        /// </summary>
        private EngineEffectVariableVector inverseWaveLength = null;
        /// <summary>
        /// 
        /// </summary>
        private EngineEffectVariableVector misc = null;
        /// <summary>
        /// Back color variable
        /// </summary>
        private EngineEffectVariableVector backColor = null;
        /// <summary>
        /// Light direction effect variable
        /// </summary>
        private EngineEffectVariableVector lightDirectionWorld = null;
        /// <summary>
        /// HDR exposure effect variable
        /// </summary>
        private EngineEffectVariableScalar hdrExposure = null;

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
            this.SkyScattering = this.Effect.GetTechniqueByName("SkyScattering");

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
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Position)
                {
                    if (mode.HasFlag(DrawerModesEnum.Forward))
                    {
                        return this.SkyScattering;
                    }
                    else if (mode.HasFlag(DrawerModesEnum.Deferred))
                    {
                        //TODO: build a proper deferred scattering effect
                        return this.SkyScattering;
                    }
                    else
                    {
                        throw new EngineException(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                    }
                }
                else
                {
                    throw new EngineException(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                }
            }
            else
            {
                throw new EngineException(string.Format("Bad stage for effect: {0}", stage));
            }
        }

        /// <summary>
        /// Update per frame
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="planetRadius">Planet radius</param>
        /// <param name="planetAtmosphereRadius">Planet atmosphere radius from surface</param>
        /// <param name="sphereOuterRadius">Sphere inner radius</param>
        /// <param name="sphereInnerRadius">Sphere outer radius</param>
        /// <param name="skyBrightness">Sky brightness</param>
        /// <param name="rayleighScattering">Rayleigh scattering constant</param>
        /// <param name="rayleighScattering4PI">Rayleigh scattering constant * 4 * PI</param>
        /// <param name="mieScattering">Mie scattering constant</param>
        /// <param name="mieScattering4PI">Mie scattering constant * 4 * PI</param>
        /// <param name="invWaveLength4">Inverse light wave length</param>
        /// <param name="scale">Scale</param>
        /// <param name="rayleighScaleDepth">Rayleigh scale depth</param>
        /// <param name="backColor">Back color</param>
        /// <param name="lightDirection">Light direction</param>
        /// <param name="hdrExposure">HDR exposure</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float planetRadius,
            float planetAtmosphereRadius,
            float sphereOuterRadius,
            float sphereInnerRadius,
            float skyBrightness,
            float rayleighScattering,
            float rayleighScattering4PI,
            float mieScattering,
            float mieScattering4PI,
            Color4 invWaveLength4,
            float scale,
            float rayleighScaleDepth,
            Color4 backColor,
            Vector3 lightDirection,
            float hdrExposure)
        {
            this.WorldViewProjection = world * viewProjection;

            this.SphereRadii = new Vector4(
                sphereOuterRadius, sphereOuterRadius * sphereOuterRadius,
                sphereInnerRadius, sphereInnerRadius * sphereInnerRadius);

            this.ScatteringCoefficients = new Vector4(
                rayleighScattering * skyBrightness, rayleighScattering4PI,
                mieScattering * skyBrightness, mieScattering4PI);

            this.InverseWaveLength = invWaveLength4;

            this.Misc = new Vector4(planetRadius, planetAtmosphereRadius, scale, scale / rayleighScaleDepth);

            this.BackColor = backColor;

            this.LightDirectionWorld = -lightDirection;
            this.HDRExposure = hdrExposure;
        }
    }
}
