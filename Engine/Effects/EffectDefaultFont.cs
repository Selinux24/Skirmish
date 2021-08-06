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
        /// Small size effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar isSmallSizeVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureVar = null;
        /// <summary>
        /// Resolution variable
        /// </summary>
        private readonly EngineEffectVariableVector resolutionVar = null;
        /// <summary>
        /// Clipping rectangle variable
        /// </summary>
        private readonly EngineEffectVariableVector rectangleVar = null;
        /// <summary>
        /// Use clipping rectangle variable
        /// </summary>
        private readonly EngineEffectVariableScalar useRectangleVar = null;

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
        /// Is small size
        /// </summary>
        protected bool IsSmallSize
        {
            get
            {
                return isSmallSizeVar.GetBool();
            }
            set
            {
                isSmallSizeVar.Set(value);
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
        /// Screen resolution in pixels
        /// </summary>
        protected Vector2 Resolution
        {
            get
            {
                return resolutionVar.GetVector<Vector2>();
            }
            set
            {
                resolutionVar.Set(value);
            }
        }
        /// <summary>
        /// Clipping rectangle in pixels
        /// </summary>
        protected Rectangle ClippingRectangle
        {
            get
            {
                Vector4 rect = rectangleVar.GetVector<Vector4>();

                return new Rectangle()
                {
                    X = (int)rect.X,
                    Y = (int)rect.Y,
                    Width = (int)rect.Z,
                    Height = (int)rect.W,
                };
            }
            set
            {
                rectangleVar.Set(new Vector4(value.X, value.Y, value.Width, value.Height));
            }
        }
        /// <summary>
        /// Use clipping rectangle
        /// </summary>
        protected bool UseClippingRectangle
        {
            get
            {
                return useRectangleVar.GetBool();
            }
            set
            {
                useRectangleVar.Set(value);
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
            isSmallSizeVar = Effect.GetVariableScalar("gIsSmall");
            textureVar = Effect.GetVariableTexture("gTexture");
            resolutionVar = Effect.GetVariableVector("gResolution");
            rectangleVar = Effect.GetVariableVector("gRectangle");
            useRectangleVar = Effect.GetVariableScalar("gUseRect");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="alphaMult">Alpha multiplier</param>
        /// <param name="useTextureColor">Use the texture color instead of the specified color</param>
        /// <param name="isSmallSize">The font is a small size font</param>
        /// <param name="texture">Font texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float alphaMult,
            bool useTextureColor,
            bool isSmallSize,
            EngineShaderResourceView texture)
        {
            World = world;
            WorldViewProjection = world * viewProjection;
            Alpha = alphaMult;
            UseTextureColor = useTextureColor;
            Texture = texture;
            IsSmallSize = isSmallSize;
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="useClippingRectangle">Use clipping</param>
        /// <param name="screenResolution">Screen resolution in pixels</param>
        /// <param name="clippingRectangle">Clipping rectangle in pixels</param>
        public void UpdatePerFrame(
            bool useClippingRectangle,
            Vector2 screenResolution,
            Rectangle clippingRectangle)
        {
            UseClippingRectangle = useClippingRectangle;
            Resolution = screenResolution;
            ClippingRectangle = clippingRectangle;
        }
    }
}
