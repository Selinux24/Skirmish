using Engine.Shaders.Properties;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Billboard effect
    /// </summary>
    public class EffectShadowBillboard : Drawer
    {
        /// <summary>
        /// Billboard shadow map drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ShadowMapBillboard = null;

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
        public EffectShadowBillboard(Graphics graphics)
            : base(graphics, EffectsResources.ShaderShadowBillboard, true)
        {
            ShadowMapBillboard = Effect.GetTechniqueByName("ShadowMapBillboard");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            eyePositionWorldVar = Effect.GetVariableVector("gEyePositionWorld");
            startRadiusVar = Effect.GetVariableScalar("gStartRadius");
            endRadiusVar = Effect.GetVariableScalar("gEndRadius");
            textureCountVar = Effect.GetVariableScalar("gTextureCount");
            texturesVar = Effect.GetVariableTexture("gTextureArray");

            textureRandomVar = Effect.GetVariableTexture("gTextureRandom");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="randomTexture">Random texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            EngineShaderResourceView randomTexture)
        {
            WorldViewProjection = world * viewProjection;
            EyePositionWorld = eyePositionWorld;

            TextureRandom = randomTexture;
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
            StartRadius = startRadius;
            EndRadius = endRadius;
            TextureCount = textureCount;
            Textures = texture;
        }
    }
}
