using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Font effect
    /// </summary>
    public class EffectDefaultFont : Drawer
    {
        /// <summary>
        /// Font drawing technique
        /// </summary>
        public readonly EngineEffectTechnique FontDrawer = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Alpha value effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar alphaVar = null;
        /// <summary>
        /// Use color effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar useTextureColorVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureVar = null;

        /// <summary>
        /// Current font texture
        /// </summary>
        private EngineShaderResourceView currentTexture = null;

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
        /// Alpha value
        /// </summary>
        protected float Alpha
        {
            get
            {
                return alphaVar.GetFloat();
            }
            set
            {
                alphaVar.Set(value);
            }
        }
        /// <summary>
        /// Use texture color
        /// </summary>
        protected bool UseTextureColor
        {
            get
            {
                return useTextureColorVar.GetBool();
            }
            set
            {
                useTextureColorVar.Set(value);
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
        public EffectDefaultFont(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            FontDrawer = Effect.GetTechniqueByName("FontDrawer");

            worldVar = Effect.GetVariableMatrix("gWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            alphaVar = Effect.GetVariableScalar("gAlpha");
            useTextureColorVar = Effect.GetVariableScalar("gUseColor");
            textureVar = Effect.GetVariableTexture("gTexture");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="alphaMult">Alpha multiplier</param>
        /// <param name="useTextureColor">Use the texture color instead of the specified color</param>
        /// <param name="texture">Font texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float alphaMult,
            bool useTextureColor,
            EngineShaderResourceView texture)
        {
            World = world;
            WorldViewProjection = world * viewProjection;
            Alpha = alphaMult;
            UseTextureColor = useTextureColor;
            Texture = texture;
        }
    }
}
