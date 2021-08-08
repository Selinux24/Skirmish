using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.UI;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDefaultSprite : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionColor = null;
        /// <summary>
        /// Position color by percentage drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionColorPct = null;
        /// <summary>
        /// Position texture technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTexture = null;
        /// <summary>
        /// Position texture using red channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureRED = null;
        /// <summary>
        /// Position texture using green channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureGREEN = null;
        /// <summary>
        /// Position texture using blue channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureBLUE = null;
        /// <summary>
        /// Position texture using alpha channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureALPHA = null;
        /// <summary>
        /// Position texture without alpha channel
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureNOALPHA = null;
        /// <summary>
        /// Position texture by percentage technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTexturePct = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Screen resolution in pixels effect variable
        /// </summary>
        private readonly EngineEffectVariableVector resolutionVar = null;
        /// <summary>
        /// Color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector colorVar = null;
        /// <summary>
        /// Sprite size in pixels effect variable
        /// </summary>
        private readonly EngineEffectVariableVector sizeVar = null;
        /// <summary>
        /// Second color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector color2Var = null;
        /// <summary>
        /// Percentage effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar percentageVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture texturesVar = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureIndexVar = null;

        /// <summary>
        /// Current texture array
        /// </summary>
        private EngineShaderResourceView currentTextures = null;

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
        /// Color
        /// </summary>
        protected Color4 Color
        {
            get
            {
                return colorVar.GetVector<Color4>();
            }
            set
            {
                colorVar.Set(value);
            }
        }
        /// <summary>
        /// Clipping rectangle in pixels
        /// </summary>
        protected RectangleF Size
        {
            get
            {
                Vector4 rect = sizeVar.GetVector<Vector4>();

                return new RectangleF()
                {
                    X = rect.X,
                    Y = rect.Y,
                    Width = rect.Z,
                    Height = rect.W,
                };
            }
            set
            {
                sizeVar.Set(new Vector4(value.X, value.Y, value.Width, value.Height));
            }
        }
        /// <summary>
        /// Second color
        /// </summary>
        protected Color4 Color2
        {
            get
            {
                return color2Var.GetVector<Color4>();
            }
            set
            {
                color2Var.Set(value);
            }
        }
        /// <summary>
        /// Percentage
        /// </summary>
        protected float Percentage
        {
            get
            {
                return percentageVar.GetFloat();
            }
            set
            {
                percentageVar.Set(value);
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
        /// Texture index
        /// </summary>
        protected int TextureIndex
        {
            get
            {
                return textureIndexVar.GetInt();
            }
            set
            {
                textureIndexVar.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultSprite(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            PositionColor = Effect.GetTechniqueByName("PositionColor");
            PositionColorPct = Effect.GetTechniqueByName("PositionColorPct");
            PositionTexture = Effect.GetTechniqueByName("PositionTexture");
            PositionTextureNOALPHA = Effect.GetTechniqueByName("PositionTextureNOALPHA");
            PositionTextureRED = Effect.GetTechniqueByName("PositionTextureRED");
            PositionTextureGREEN = Effect.GetTechniqueByName("PositionTextureGREEN");
            PositionTextureBLUE = Effect.GetTechniqueByName("PositionTextureBLUE");
            PositionTextureALPHA = Effect.GetTechniqueByName("PositionTextureALPHA");
            PositionTexturePct = Effect.GetTechniqueByName("PositionTexturePct");

            worldVar = Effect.GetVariableMatrix("gWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            resolutionVar = Effect.GetVariableVector("gResolution");

            colorVar = Effect.GetVariableVector("gColor");
            sizeVar = Effect.GetVariableVector("gSize");
            color2Var = Effect.GetVariableVector("gColor2");
            percentageVar = Effect.GetVariableScalar("gPct");
            texturesVar = Effect.GetVariableTexture("gTextureArray");
            textureIndexVar = Effect.GetVariableScalar("gTextureIndex");
        }

        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="channel">Color channel</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        public EngineEffectTechnique GetTechnique(VertexTypes vertexType, UITextureRendererChannels channel)
        {
            if (vertexType == VertexTypes.PositionColor)
            {
                return PositionColor;
            }
            else if (vertexType == VertexTypes.PositionTexture)
            {
                if (channel == UITextureRendererChannels.All) return PositionTexture;
                else if (channel == UITextureRendererChannels.Red) return PositionTextureRED;
                else if (channel == UITextureRendererChannels.Green) return PositionTextureGREEN;
                else if (channel == UITextureRendererChannels.Blue) return PositionTextureBLUE;
                else if (channel == UITextureRendererChannels.Alpha) return PositionTextureALPHA;
                else if (channel == UITextureRendererChannels.NoAlpha) return PositionTextureNOALPHA;
                else return PositionTexture;
            }
            else
            {
                throw new EngineException(string.Format("Bad vertex type for effect: {0}", vertexType));
            }
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        public EngineEffectTechnique GetTechniquePct(VertexTypes vertexType)
        {
            if (vertexType == VertexTypes.PositionColor)
            {
                return PositionColorPct;
            }
            else if (vertexType == VertexTypes.PositionTexture)
            {
                return PositionTexturePct;
            }
            else
            {
                throw new EngineException(string.Format("Bad vertex type for effect: {0}", vertexType));
            }
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="screenResolution">Screen resolution in pixels</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector2 screenResolution)
        {
            World = world;
            WorldViewProjection = world * viewProjection;
            Resolution = screenResolution;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="tintColor">Color</param>
        /// <param name="texture">Texture</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            Color4 tintColor,
            EngineShaderResourceView texture,
            int textureIndex)
        {
            Color = tintColor;
            Textures = texture;
            TextureIndex = textureIndex;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="leftColor">Left color</param>
        /// <param name="rightColor">Right color</param>
        /// <param name="pct">Percentage</param>
        /// <param name="texture">Texture</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObjectPct(
            RectangleF renderArea,
            Color4 leftColor,
            Color4 rightColor,
            float pct,
            EngineShaderResourceView texture,
            int textureIndex)
        {
            Size = renderArea;
            Color = leftColor;
            Color2 = rightColor;
            Percentage = pct;
            Textures = texture;
            TextureIndex = textureIndex;
        }
    }
}
