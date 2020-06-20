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
        /// Color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector colorVar = null;
        /// <summary>
        /// Use color effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorVar = null;
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
                return this.worldVar.GetMatrix();
            }
            set
            {
                this.worldVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjectionVar.GetMatrix();
            }
            set
            {
                this.worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Color
        /// </summary>
        protected Color4 Color
        {
            get
            {
                return this.colorVar.GetVector<Color4>();
            }
            set
            {
                this.colorVar.Set(value);
            }
        }
        /// <summary>
        /// Use texture color
        /// </summary>
        protected bool UseColor
        {
            get
            {
                return this.useColorVar.GetBool();
            }
            set
            {
                this.useColorVar.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Texture
        {
            get
            {
                return this.textureVar.GetResource();
            }
            set
            {
                if (this.currentTexture != value)
                {
                    this.textureVar.SetResource(value);

                    this.currentTexture = value;

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
            this.FontDrawer = this.Effect.GetTechniqueByName("FontDrawer");

            this.worldVar = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.colorVar = this.Effect.GetVariableVector("gColor");
            this.useColorVar = this.Effect.GetVariableScalar("gUseColor");
            this.textureVar = this.Effect.GetVariableTexture("gTexture");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="color">Text color</param>
        /// <param name="alphaMult">Alpha multiplier</param>
        /// <param name="useTextureColor">Use the texture color instead of the specified color</param>
        /// <param name="texture">Font texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Color3 color,
            float alphaMult,
            bool useTextureColor,
            EngineShaderResourceView texture)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.Color = new Color4(color, alphaMult);
            this.UseColor = useTextureColor;
            this.Texture = texture;
        }
    }
}
