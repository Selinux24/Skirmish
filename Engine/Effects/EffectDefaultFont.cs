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
        private EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Color effect variable
        /// </summary>
        private EngineEffectVariableVector color = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EngineEffectVariableTexture texture = null;

        /// <summary>
        /// Current font texture
        /// </summary>
        private EngineTexture currentTexture = null;

        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return this.world.GetMatrix();
            }
            set
            {
                this.world.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjection.GetMatrix();
            }
            set
            {
                this.worldViewProjection.SetMatrix(value);
            }
        }
        /// <summary>
        /// Color
        /// </summary>
        protected Color4 Color
        {
            get
            {
                return this.color.GetVector<Color4>();
            }
            set
            {
                this.color.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineTexture Texture
        {
            get
            {
                return this.texture.GetResource();
            }
            set
            {
                if (this.currentTexture != value)
                {
                    this.texture.SetResource(value);

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

            this.world = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.color = this.Effect.GetVariableVector("gColor");
            this.texture = this.Effect.GetVariableTexture("gTexture");
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.PositionTexture && mode == DrawerModesEnum.Forward)
                {
                    return this.FontDrawer;
                }
                else
                {
                    throw new EngineException(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                }
            }
            else
            {
                throw new EngineException(string.Format("Bad stage for effect: {0}", stage));
            }
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="color">Color</param>
        /// <param name="texture">Font texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Color4 color,
            EngineTexture texture)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.Color = color;
            this.Texture = texture;
        }
    }
}
