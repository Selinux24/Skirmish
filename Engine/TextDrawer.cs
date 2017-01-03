using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Text drawer
    /// </summary>
    public class TextDrawer : Drawable, IScreenFitted
    {
        /// <summary>
        /// Technique name
        /// </summary>
        private EffectTechnique technique = null;
        /// <summary>
        /// Input layout
        /// </summary>
        private InputLayout inputLayout = null;

        /// <summary>
        /// Vertex buffer
        /// </summary>
        private Buffer vertexBuffer = null;
        /// <summary>
        /// Vertex couunt
        /// </summary>
        private int vertexCount = 0;
        /// <summary>
        /// Vertex stride
        /// </summary>
        private int vertexBufferStride = 0;
        /// <summary>
        /// Vertex offset
        /// </summary>
        private int vertexBufferOffset = 0;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding[] vertexBufferBinding = null;
        /// <summary>
        /// Index buffer
        /// </summary>
        private Buffer indexBuffer = null;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;

        /// <summary>
        /// View * projection matrix
        /// </summary>
        private Matrix viewProjection;
        /// <summary>
        /// Font map
        /// </summary>
        private FontMap fontMap = null;
        /// <summary>
        /// Text position in 2D screen
        /// </summary>
        private Vector2 position = Vector2.Zero;
        /// <summary>
        /// Text
        /// </summary>
        private string text = null;

        /// <summary>
        /// Font name
        /// </summary>
        public readonly string Font = null;
        /// <summary>
        /// Gets or sets text to draw
        /// </summary>
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (!string.Equals(this.text, value))
                {
                    this.text = value;

                    this.MapText();
                }
            }
        }
        /// <summary>
        /// Gets character count
        /// </summary>
        public int CharacterCount
        {
            get
            {
                if (!string.IsNullOrEmpty(this.text))
                {
                    return this.text.Length;
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// Gets or sets text fore color
        /// </summary>
        public Color4 TextColor { get; set; }
        /// <summary>
        /// Gets or sets text shadow color
        /// </summary>
        public Color4 ShadowColor { get; set; }
        /// <summary>
        /// Gets or sets relative position of shadow
        /// </summary>
        public Vector2 ShadowRelative { get; set; }

        /// <summary>
        /// Gets or sest text position in 2D screen
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }
        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public int Left
        {
            get
            {
                return (int)this.position.X;
            }
            set
            {
                this.position.X = value;
            }
        }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public int Top
        {
            get
            {
                return (int)this.position.Y;
            }
            set
            {
                this.position.Y = value;
            }
        }
        /// <summary>
        /// Gets text width
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets text height
        /// </summary>
        public int Height { get; private set; }
      
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="description">Text description</param>
        public TextDrawer(Game game, TextDrawerDescription description)
            : base(game, description)
        {
            this.Font = string.Format("{0} {1}", description.Font, description.FontSize);

            this.technique = DrawerPool.EffectDefaultFont.GetTechnique(VertexTypes.PositionTexture, false, DrawingStages.Drawing, DrawerModesEnum.Forward);
            this.inputLayout = DrawerPool.EffectDefaultFont.GetInputLayout(this.technique);

            this.viewProjection = Sprite.CreateViewOrthoProjection(
                game.Form.RenderWidth.NextPair(),
                game.Form.RenderHeight.NextPair());

            this.fontMap = FontMap.Map(game.Graphics.Device, description.Font, description.FontSize);

            VertexPositionTexture[] vertices = new VertexPositionTexture[FontMap.MAXTEXTLENGTH * 4];

            this.vertexBuffer = this.Game.Graphics.Device.CreateVertexBufferWrite(vertices);
            this.vertexBufferStride = vertices[0].Stride;
            this.vertexBufferOffset = 0;
            this.vertexCount = 0;
            this.vertexBufferBinding = new VertexBufferBinding[]
            {
                new VertexBufferBinding(this.vertexBuffer, this.vertexBufferStride, this.vertexBufferOffset),
            };

            this.indexBuffer = this.Game.Graphics.Device.CreateIndexBufferWrite(new uint[FontMap.MAXTEXTLENGTH * 6]);
            this.indexCount = 0;

            this.TextColor = description.TextColor;
            this.ShadowColor = description.ShadowColor;
            this.ShadowRelative = Vector2.One * 1f;

            this.MapText();
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {

        }
        /// <summary>
        /// Update component state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {

        }
        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (!string.IsNullOrWhiteSpace(this.text))
            {
                this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = inputLayout;
                Counters.IAInputLayoutSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;
                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
                Counters.IAVertexBuffersSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;

                if (this.ShadowColor != Color.Transparent)
                {
                    //Draw shadow
                    this.DrawText(context, this.Position + this.ShadowRelative, this.ShadowColor);
                }

                //Draw text
                this.DrawText(context, this.Position, this.TextColor);
            }
        }
        /// <summary>
        /// Resize
        /// </summary>
        public void Resize()
        {
            this.viewProjection = Sprite.CreateViewOrthoProjection(
                this.Game.Form.RenderWidth.NextPair(),
                this.Game.Form.RenderHeight.NextPair());
        }

        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="position">Position</param>
        /// <param name="color">Color</param>
        private void DrawText(DrawContext context, Vector2 position, Color4 color)
        {
            #region Per frame update

            float x = +position.X - this.Game.Form.RelativeCenter.X;
            float y = -position.Y + this.Game.Form.RelativeCenter.Y;

            Matrix world = Matrix.Translation(x, y, 0f);

            DrawerPool.EffectDefaultFont.UpdatePerFrame(
                world,
                this.viewProjection,
                color,
                this.fontMap.Texture);

            #endregion

            for (int p = 0; p < this.technique.Description.PassCount; p++)
            {
                this.technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                if (this.indexBuffer != null)
                {
                    this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, 0, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexCount / 3;
                }
                else
                {
                    this.Game.Graphics.DeviceContext.Draw(this.vertexCount, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.vertexCount / 3;
                }
            }
        }
        /// <summary>
        /// Map text
        /// </summary>
        private void MapText()
        {
            VertexPositionTexture[] v;
            uint[] i;
            Vector2 size;
            this.fontMap.MapSentence(
                this.text,
                out v, out i, out size);

            this.Game.Graphics.DeviceContext.WriteBuffer(this.vertexBuffer, v);
            this.Game.Graphics.DeviceContext.WriteBuffer(this.indexBuffer, i);

            this.vertexCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 4;
            this.indexCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 6;

            this.Width = (int)size.X;
            this.Height = (int)size.Y;
        }
    }
}
