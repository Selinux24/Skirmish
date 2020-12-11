using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Decals effect
    /// </summary>
    public class EffectDefaultDecals : Drawer
    {
        /// <summary>
        /// Decal drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Decal = null;
        /// <summary>
        /// Rotated decals drawing technique
        /// </summary>
        public readonly EngineEffectTechnique DecalRotated = null;

        /// <summary>
        /// World effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureArrayVar = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureCountVar = null;
        /// <summary>
        /// Game time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar totalTimeVar = null;

        /// <summary>
        /// Current texture array
        /// </summary>
        private EngineShaderResourceView currentTextureArray = null;

        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return worldVar.GetMatrix();
            }
            set
            {
                worldVar.SetMatrix(value);
            }
        }
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
        /// Textures
        /// </summary>
        protected EngineShaderResourceView TextureArray
        {
            get
            {
                return textureArrayVar.GetResource();
            }
            set
            {
                if (currentTextureArray != value)
                {
                    textureArrayVar.SetResource(value);

                    currentTextureArray = value;

                    Counters.TextureUpdates++;
                }
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
        /// Game time
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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultDecals(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            Decal = Effect.GetTechniqueByName("Decal");
            DecalRotated = Effect.GetTechniqueByName("DecalRotated");

            worldVar = Effect.GetVariableMatrix("gWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            textureArrayVar = Effect.GetVariableTexture("gTextureArray");
            textureCountVar = Effect.GetVariableScalar("gTextureCount");
            totalTimeVar = Effect.GetVariableScalar("gTotalTime");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="totalTime">Total time</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            float totalTime,
            uint textureCount,
            EngineShaderResourceView textures)
        {
            World = Matrix.Identity;
            WorldViewProjection = viewProjection;
            TotalTime = totalTime;
            TextureCount = textureCount;
            TextureArray = textures;
        }
    }
}
