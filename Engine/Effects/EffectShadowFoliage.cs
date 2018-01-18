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
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EngineEffectVariableVector eyePositionWorld = null;
        /// <summary>
        /// Start radius
        /// </summary>
        private EngineEffectVariableScalar startRadius = null;
        /// <summary>
        /// End radius
        /// </summary>
        private EngineEffectVariableScalar endRadius = null;
        /// <summary>
        /// Wind direction effect variable
        /// </summary>
        private EngineEffectVariableVector windDirection = null;
        /// <summary>
        /// Wind strength effect variable
        /// </summary>
        private EngineEffectVariableScalar windStrength = null;
        /// <summary>
        /// Time effect variable
        /// </summary>
        private EngineEffectVariableScalar totalTime = null;
        /// <summary>
        /// Position delta
        /// </summary>
        private EngineEffectVariableVector delta = null;
        /// <summary>
        /// Texture count variable
        /// </summary>
        private EngineEffectVariableScalar textureCount = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EngineEffectVariableTexture textures = null;
        /// <summary>
        /// Random texture effect variable
        /// </summary>
        private EngineEffectVariableTexture textureRandom = null;

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
                return this.worldViewProjection.GetMatrix();
            }
            set
            {
                this.worldViewProjection.SetMatrix(value);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return this.eyePositionWorld.GetVector<Vector3>();
            }
            set
            {
                this.eyePositionWorld.Set(value);
            }
        }
        /// <summary>
        /// Start radius
        /// </summary>
        protected float StartRadius
        {
            get
            {
                return this.startRadius.GetFloat();
            }
            set
            {
                this.startRadius.Set(value);
            }
        }
        /// <summary>
        /// End radius
        /// </summary>
        protected float EndRadius
        {
            get
            {
                return this.endRadius.GetFloat();
            }
            set
            {
                this.endRadius.Set(value);
            }
        }
        /// <summary>
        /// Wind direction
        /// </summary>
        protected Vector3 WindDirection
        {
            get
            {
                return this.windDirection.GetVector<Vector3>();
            }
            set
            {
                this.windDirection.Set(value);
            }
        }
        /// <summary>
        /// Wind strength
        /// </summary>
        protected float WindStrength
        {
            get
            {
                return this.windStrength.GetFloat();
            }
            set
            {
                this.windStrength.Set(value);
            }
        }
        /// <summary>
        /// Time
        /// </summary>
        protected float TotalTime
        {
            get
            {
                return this.totalTime.GetFloat();
            }
            set
            {
                this.totalTime.Set(value);
            }
        }
        /// <summary>
        /// Position delta
        /// </summary>
        protected Vector3 Delta
        {
            get
            {
                return this.delta.GetVector<Vector3>();
            }
            set
            {
                this.delta.Set(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return this.textureCount.GetUInt();
            }
            set
            {
                this.textureCount.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Textures
        {
            get
            {
                return this.textures.GetResource();
            }
            set
            {
                if (this.currentTextures != value)
                {
                    this.textures.SetResource(value);

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
                return this.textureRandom.GetResource();
            }
            set
            {
                if (this.currentTextureRandom != value)
                {
                    this.textureRandom.SetResource(value);

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

            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.eyePositionWorld = this.Effect.GetVariableVector("gEyePositionWorld");
            this.startRadius = this.Effect.GetVariableScalar("gStartRadius");
            this.endRadius = this.Effect.GetVariableScalar("gEndRadius");
            this.textureCount = this.Effect.GetVariableScalar("gTextureCount");
            this.textures = this.Effect.GetVariableTexture("gTextureArray");

            this.windDirection = this.Effect.GetVariableVector("gWindDirection");
            this.windStrength = this.Effect.GetVariableScalar("gWindStrength");
            this.totalTime = this.Effect.GetVariableScalar("gTotalTime");
            this.delta = this.Effect.GetVariableVector("gDelta");
            this.textureRandom = this.Effect.GetVariableTexture("gTextureRandom");
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
