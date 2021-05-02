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
        /// World matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureIndexVar = null;
        /// <summary>
        /// Color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector colorVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture texturesVar = null;

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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultSprite(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            PositionColor = Effect.GetTechniqueByName("PositionColor");
            PositionTexture = Effect.GetTechniqueByName("PositionTexture");
            PositionTextureNOALPHA = Effect.GetTechniqueByName("PositionTextureNOALPHA");
            PositionTextureRED = Effect.GetTechniqueByName("PositionTextureRED");
            PositionTextureGREEN = Effect.GetTechniqueByName("PositionTextureGREEN");
            PositionTextureBLUE = Effect.GetTechniqueByName("PositionTextureBLUE");
            PositionTextureALPHA = Effect.GetTechniqueByName("PositionTextureALPHA");

            worldVar = Effect.GetVariableMatrix("gWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            textureIndexVar = Effect.GetVariableScalar("gTextureIndex");
            colorVar = Effect.GetVariableVector("gColor");
            texturesVar = Effect.GetVariableTexture("gTextureArray");
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
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            World = world;
            WorldViewProjection = world * viewProjection;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="texture">Texture</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            Color4 color,
            EngineShaderResourceView texture,
            int textureIndex)
        {
            Color = color;
            Textures = texture;
            TextureIndex = textureIndex;
        }
    }
}
