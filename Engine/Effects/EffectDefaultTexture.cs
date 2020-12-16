using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Texture effect
    /// </summary>
    public class EffectDefaultTexture : Drawer
    {
        /// <summary>
        /// Simple texture drawing technique
        /// </summary>
        public readonly EngineEffectTechnique SimpleTexture = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureIndexVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureVar = null;

        /// <summary>
        /// Current texture
        /// </summary>
        private EngineShaderResourceView currentTexture = null;

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
        /// Texture index
        /// </summary>
        protected uint TextureIndex
        {
            get
            {
                return textureIndexVar.GetUInt();
            }
            set
            {
                textureIndexVar.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Texture
        {
            get
            {
                return textureVar.GetResource();
            }
            set
            {
                if (currentTexture != value)
                {
                    textureVar.SetResource(value);

                    currentTexture = value;

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
        public EffectDefaultTexture(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            SimpleTexture = Effect.GetTechniqueByName("SimpleTexture");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            textureIndexVar = Effect.GetVariableScalar("gTextureIndex");
            textureVar = Effect.GetVariableTexture("gTexture");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            WorldViewProjection = world * viewProjection;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(
            uint textureIndex,
            EngineShaderResourceView texture)
        {
            TextureIndex = textureIndex;
            Texture = texture;
        }
    }
}
