using Engine.Shaders.Properties;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

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
        /// Sprite size in pixels effect variable
        /// </summary>
        private readonly EngineEffectVariableVector sizeVar = null;
        /// <summary>
        /// First color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector color1Var = null;
        /// <summary>
        /// Second color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector color2Var = null;
        /// <summary>
        /// Third color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector color3Var = null;
        /// <summary>
        /// Fourth color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector color4Var = null;
        /// <summary>
        /// Percentage effect variable
        /// </summary>
        private readonly EngineEffectVariableVector percentageVar = null;
        /// <summary>
        /// Direction effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar directionVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture texturesVar = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureIndexVar = null;
        /// <summary>
        /// Clipping rectangle variable
        /// </summary>
        private readonly EngineEffectVariableVector rectangleVar = null;
        /// <summary>
        /// Use clipping rectangle variable
        /// </summary>
        private readonly EngineEffectVariableScalar useRectangleVar = null;

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
        /// First Color
        /// </summary>
        protected Color4 Color1
        {
            get
            {
                return color1Var.GetVector<Color4>();
            }
            set
            {
                color1Var.Set(value);
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
        /// Third color
        /// </summary>
        protected Color4 Color3
        {
            get
            {
                return color3Var.GetVector<Color4>();
            }
            set
            {
                color3Var.Set(value);
            }
        }
        /// <summary>
        /// Fourth color
        /// </summary>
        protected Color4 Color4
        {
            get
            {
                return color4Var.GetVector<Color4>();
            }
            set
            {
                color4Var.Set(value);
            }
        }
        /// <summary>
        /// Percentage
        /// </summary>
        protected Vector3 Percentage
        {
            get
            {
                return percentageVar.GetVector<Vector3>();
            }
            set
            {
                percentageVar.Set(value);
            }
        }
        /// <summary>
        /// Draw direction
        /// </summary>
        protected int Direction
        {
            get
            {
                return directionVar.GetInt();
            }
            set
            {
                directionVar.Set(value);
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
        public EffectDefaultSprite(Graphics graphics)
            : base(graphics, EffectsResources.ShaderDefaultSprite, true)
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
            rectangleVar = Effect.GetVariableVector("gRectangle");
            useRectangleVar = Effect.GetVariableScalar("gUseRect");

            sizeVar = Effect.GetVariableVector("gSize");
            color1Var = Effect.GetVariableVector("gColor1");
            color2Var = Effect.GetVariableVector("gColor2");
            color3Var = Effect.GetVariableVector("gColor3");
            color4Var = Effect.GetVariableVector("gColor4");
            percentageVar = Effect.GetVariableVector("gPct");
            directionVar = Effect.GetVariableScalar("gDirection");
            textureIndexVar = Effect.GetVariableScalar("gTextureIndex");
            texturesVar = Effect.GetVariableTexture("gTextureArray");
        }

        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="channel">Color channel</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        public EngineEffectTechnique GetTechnique(VertexTypes vertexType, ColorChannels channel)
        {
            if (vertexType == VertexTypes.PositionColor)
            {
                return PositionColor;
            }
            else if (vertexType == VertexTypes.PositionTexture)
            {
                if (channel == ColorChannels.All) return PositionTexture;
                else if (channel == ColorChannels.Red) return PositionTextureRED;
                else if (channel == ColorChannels.Green) return PositionTextureGREEN;
                else if (channel == ColorChannels.Blue) return PositionTextureBLUE;
                else if (channel == ColorChannels.Alpha) return PositionTextureALPHA;
                else if (channel == ColorChannels.NoAlpha) return PositionTextureNOALPHA;
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
            UpdatePerFrame(world, viewProjection, screenResolution, false, Rectangle.Empty);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="screenResolution">Screen resolution in pixels</param>
        /// <param name="useClippingRectangle">Use clipping</param>
        /// <param name="clippingRectangle">Clipping rectangle in pixels</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector2 screenResolution,
            bool useClippingRectangle,
            Rectangle clippingRectangle)
        {
            World = world;
            WorldViewProjection = world * viewProjection;
            Resolution = screenResolution;
            UseClippingRectangle = useClippingRectangle;
            ClippingRectangle = clippingRectangle;
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
            Color1 = tintColor;
            Textures = texture;
            TextureIndex = textureIndex;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="spriteParameters">Sprite parameters</param>
        /// <param name="texture">Texture</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObjectPct(
            SpriteEffectParameters spriteParameters,
            EngineShaderResourceView texture,
            int textureIndex)
        {
            Size = spriteParameters.RenderArea;
            Color1 = spriteParameters.Color1;
            Color2 = spriteParameters.Color2;
            Color3 = spriteParameters.Color3;
            Color4 = spriteParameters.Color4;
            Percentage = new Vector3
            {
                X = spriteParameters.Percentage1,
                Y = spriteParameters.Percentage2,
                Z = spriteParameters.Percentage3,
            };
            Direction = spriteParameters.Direction;
            Textures = texture;
            TextureIndex = textureIndex;
        }
    }
}
