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
                return worldViewProjectionVar.GetMatrix();
            }
            set
            {
                worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return eyePositionWorldVar.GetVector<Vector3>();
            }
            set
            {
                eyePositionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// Start radius
        /// </summary>
        protected float StartRadius
        {
            get
            {
                return startRadiusVar.GetFloat();
            }
            set
            {
                startRadiusVar.Set(value);
            }
        }
        /// <summary>
        /// End radius
        /// </summary>
        protected float EndRadius
        {
            get
            {
                return endRadiusVar.GetFloat();
            }
            set
            {
                endRadiusVar.Set(value);
            }
        }
        /// <summary>
        /// Wind direction
        /// </summary>
        protected Vector3 WindDirection
        {
            get
            {
                return windDirectionVar.GetVector<Vector3>();
            }
            set
            {
                windDirectionVar.Set(value);
            }
        }
        /// <summary>
        /// Wind strength
        /// </summary>
        protected float WindStrength
        {
            get
            {
                return windStrengthVar.GetFloat();
            }
            set
            {
                windStrengthVar.Set(value);
            }
        }
        /// <summary>
        /// Time
        /// </summary>
        protected float TotalTime
        {
            get
            {
                return totalTimeVar.GetFloat();
            }
            set
            {
                totalTimeVar.Set(value);
            }
        }
        /// <summary>
        /// Position delta
        /// </summary>
        protected Vector3 Delta
        {
            get
            {
                return deltaVar.GetVector<Vector3>();
            }
            set
            {
                deltaVar.Set(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return textureCountVar.GetUInt();
            }
            set
            {
                textureCountVar.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Textures
        {
            get
            {
                return texturesVar.GetResource();
            }
            set
            {
                if (currentTextures != value)
                {
                    texturesVar.SetResource(value);

                    currentTextures = value;

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
                return textureRandomVar.GetResource();
            }
            set
            {
                if (currentTextureRandom != value)
                {
                    textureRandomVar.SetResource(value);

                    currentTextureRandom = value;

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
            ShadowMapFoliage4 = Effect.GetTechniqueByName("ShadowMapFoliage4");
            ShadowMapFoliage8 = Effect.GetTechniqueByName("ShadowMapFoliage8");
            ShadowMapFoliage16 = Effect.GetTechniqueByName("ShadowMapFoliage16");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            eyePositionWorldVar = Effect.GetVariableVector("gEyePositionWorld");
            startRadiusVar = Effect.GetVariableScalar("gStartRadius");
            endRadiusVar = Effect.GetVariableScalar("gEndRadius");
            textureCountVar = Effect.GetVariableScalar("gTextureCount");
            texturesVar = Effect.GetVariableTexture("gTextureArray");

            windDirectionVar = Effect.GetVariableVector("gWindDirection");
            windStrengthVar = Effect.GetVariableScalar("gWindStrength");
            totalTimeVar = Effect.GetVariableScalar("gTotalTime");
            deltaVar = Effect.GetVariableVector("gDelta");
            textureRandomVar = Effect.GetVariableTexture("gTextureRandom");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="state">State</param>
        public void UpdatePerFrame(
            DrawContextShadows context,
            EffectShadowFoliageState state)
        {
            WorldViewProjection = context.ViewProjection;
            EyePositionWorld = context.EyePosition;

            StartRadius = state.StartRadius;
            EndRadius = state.EndRadius;
            TextureCount = state.TextureCount;
            Textures = state.Texture;

            WindDirection = state.WindDirection;
            WindStrength = state.WindStrength;
            TotalTime = state.TotalTime;
            Delta = state.Delta;
            TextureRandom = state.RandomTexture;
        }
    }
}
