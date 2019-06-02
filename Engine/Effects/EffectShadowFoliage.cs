using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Foliage shadows effect
    /// </summary>
    public class EffectShadowFoliage : Drawer
    {
        /// <summary>
        /// Foliage shadow map drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ShadowMapFoliage4 = null;
        /// <summary>
        /// Foliage shadow map drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ShadowMapFoliage8 = null;
        /// <summary>
        /// Foliage shadow map drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ShadowMapFoliage16 = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private readonly EngineEffectVariableVector eyePositionWorldVar = null;
        /// <summary>
        /// Start radius
        /// </summary>
        private readonly EngineEffectVariableScalar startRadiusVar = null;
        /// <summary>
        /// End radius
        /// </summary>
        private readonly EngineEffectVariableScalar endRadiusVar = null;
        /// <summary>
        /// Wind direction effect variable
        /// </summary>
        private readonly EngineEffectVariableVector windDirectionVar = null;
        /// <summary>
        /// Wind strength effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar windStrengthVar = null;
        /// <summary>
        /// Time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar totalTimeVar = null;
        /// <summary>
        /// Position delta
        /// </summary>
        private readonly EngineEffectVariableVector deltaVar = null;
        /// <summary>
        /// Texture count variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureCountVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture texturesVar = null;
        /// <summary>
        /// Random texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureRandomVar = null;

        /// <summary>
        /// Current texture array
        /// </summary>
        private EngineShaderResourceView currentTextures = null;
        /// <summary>
        /// Current random texture
        /// </summary>
        private EngineShaderResourceView currentTextureRandom = null;

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
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return this.eyePositionWorldVar.GetVector<Vector3>();
            }
            set
            {
                this.eyePositionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// Start radius
        /// </summary>
        protected float StartRadius
        {
            get
            {
                return this.startRadiusVar.GetFloat();
            }
            set
            {
                this.startRadiusVar.Set(value);
            }
        }
        /// <summary>
        /// End radius
        /// </summary>
        protected float EndRadius
        {
            get
            {
                return this.endRadiusVar.GetFloat();
            }
            set
            {
                this.endRadiusVar.Set(value);
            }
        }
        /// <summary>
        /// Wind direction
        /// </summary>
        protected Vector3 WindDirection
        {
            get
            {
                return this.windDirectionVar.GetVector<Vector3>();
            }
            set
            {
                this.windDirectionVar.Set(value);
            }
        }
        /// <summary>
        /// Wind strength
        /// </summary>
        protected float WindStrength
        {
            get
            {
                return this.windStrengthVar.GetFloat();
            }
            set
            {
                this.windStrengthVar.Set(value);
            }
        }
        /// <summary>
        /// Time
        /// </summary>
        protected float TotalTime
        {
            get
            {
                return this.totalTimeVar.GetFloat();
            }
            set
            {
                this.totalTimeVar.Set(value);
            }
        }
        /// <summary>
        /// Position delta
        /// </summary>
        protected Vector3 Delta
        {
            get
            {
                return this.deltaVar.GetVector<Vector3>();
            }
            set
            {
                this.deltaVar.Set(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return this.textureCountVar.GetUInt();
            }
            set
            {
                this.textureCountVar.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Textures
        {
            get
            {
                return this.texturesVar.GetResource();
            }
            set
            {
                if (this.currentTextures != value)
                {
                    this.texturesVar.SetResource(value);

                    this.currentTextures = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Random texture
        /// </summary>
        protected EngineShaderResourceView TextureRandom
        {
            get
            {
                return this.textureRandomVar.GetResource();
            }
            set
            {
                if (this.currentTextureRandom != value)
                {
                    this.textureRandomVar.SetResource(value);

                    this.currentTextureRandom = value;

                    Counters.TextureUpdates++;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowFoliage(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.ShadowMapFoliage4 = this.Effect.GetTechniqueByName("ShadowMapFoliage4");
            this.ShadowMapFoliage8 = this.Effect.GetTechniqueByName("ShadowMapFoliage8");
            this.ShadowMapFoliage16 = this.Effect.GetTechniqueByName("ShadowMapFoliage16");

            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.eyePositionWorldVar = this.Effect.GetVariableVector("gEyePositionWorld");
            this.startRadiusVar = this.Effect.GetVariableScalar("gStartRadius");
            this.endRadiusVar = this.Effect.GetVariableScalar("gEndRadius");
            this.textureCountVar = this.Effect.GetVariableScalar("gTextureCount");
            this.texturesVar = this.Effect.GetVariableTexture("gTextureArray");

            this.windDirectionVar = this.Effect.GetVariableVector("gWindDirection");
            this.windStrengthVar = this.Effect.GetVariableScalar("gWindStrength");
            this.totalTimeVar = this.Effect.GetVariableScalar("gTotalTime");
            this.deltaVar = this.Effect.GetVariableVector("gDelta");
            this.textureRandomVar = this.Effect.GetVariableTexture("gTextureRandom");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="windDirection">Wind direction</param>
        /// <param name="windStrength">Wind strength</param>
        /// <param name="totalTime">Total time</param>
        /// <param name="delta">Delta</param>
        /// <param name="randomTexture">Random texture</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            Vector3 windDirection,
            float windStrength,
            float totalTime,
            Vector3 delta,
            EngineShaderResourceView randomTexture)
        {
            this.WorldViewProjection = viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.WindDirection = windDirection;
            this.WindStrength = windStrength;
            this.TotalTime = totalTime;
            this.Delta = delta;
            this.TextureRandom = randomTexture;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="startRadius">Drawing start radius</param>
        /// <param name="endRadius">Drawing end radius</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(
            float startRadius,
            float endRadius,
            uint textureCount,
            EngineShaderResourceView texture)
        {
            this.StartRadius = startRadius;
            this.EndRadius = endRadius;
            this.TextureCount = textureCount;
            this.Textures = texture;
        }
    }
}
