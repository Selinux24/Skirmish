using SharpDX;
using SharpDX.Direct3D;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Text drawer
    /// </summary>
    public class TextDrawer : Drawable, IScreenFitted
    {
        /// <summary>
        /// Vertex couunt
        /// </summary>
        private int vertexCount = 0;
        /// <summary>
        /// Vertex offset
        /// </summary>
        private int vertexBufferOffset = -1;
        /// <summary>
        /// Vertex buffer slot
        /// </summary>
        private int vertexBufferSlot = -1;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;
        /// <summary>
        /// Index buffer offset
        /// </summary>
        private int indexBufferOffset = -1;
        /// <summary>
        /// Index buffer slot
        /// </summary>
        private int indexBufferSlot = -1;
        /// <summary>
        /// Vertices
        /// </summary>
        private VertexPositionTexture[] vertices;
        /// <summary>
        /// Indices
        /// </summary>
        private uint[] indices;
        /// <summary>
        /// Update buffers flag
        /// </summary>
        private bool updateBuffers = false;
        /// <summary>
        /// Text translation matrix
        /// </summary>
        private Matrix local;
        /// <summary>
        /// Text shadow translarion matrix
        /// </summary>
        private Matrix localShadow;
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
        /// Maximum number of instances
        /// </summary>
        public override int MaxInstances
        {
            get
            {
                return 0;
            }
        }

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

                this.UpdatePosition();
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

                this.UpdatePosition();
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

                this.UpdatePosition();
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
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Text description</param>
        public TextDrawer(Game game, BufferManager bufferManager, TextDrawerDescription description)
            : base(game, bufferManager, description)
        {
            this.Font = string.Format("{0} {1}", description.Font, description.FontSize);

            this.viewProjection = Sprite.CreateViewOrthoProjection(
                game.Form.RenderWidth.NextPair(),
                game.Form.RenderHeight.NextPair());

            this.fontMap = FontMap.Map(game.Graphics.Device, description.Font, description.FontSize);

            VertexPositionTexture[] vertices = new VertexPositionTexture[FontMap.MAXTEXTLENGTH * 4];
            uint[] indices = new uint[FontMap.MAXTEXTLENGTH * 6];

            this.BufferManager.Add(this.Name, vertices, true, 0, out this.vertexBufferOffset, out this.vertexBufferSlot);
            this.BufferManager.Add(this.Name, indices, true, out this.indexBufferOffset, out this.indexBufferSlot);

            this.vertexCount = 0;
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
                if (this.updateBuffers)
                {
                    this.BufferManager.WriteBuffer(this.vertexBufferSlot, this.vertexBufferOffset, this.vertices);
                    this.BufferManager.WriteBuffer(this.indexBufferSlot, this.indexBufferOffset, this.indices);

                    this.vertexCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 4;
                    this.indexCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 6;

                    this.updateBuffers = false;
                }

                this.BufferManager.SetIndexBuffer(this.indexBufferSlot);

                var technique = DrawerPool.EffectDefaultFont.FontDrawer;

                this.BufferManager.SetInputAssembler(technique, this.vertexBufferSlot, PrimitiveTopology.TriangleList);

                if (this.ShadowColor != Color.Transparent)
                {
                    //Draw shadow
                    this.DrawText(context, technique, this.localShadow, this.ShadowColor);
                }

                //Draw text
                this.DrawText(context, technique, this.local, this.TextColor);
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
        /// <param name="technique">Technique</param>
        /// <param name="position">Position</param>
        /// <param name="color">Color</param>
        private void DrawText(DrawContext context, EffectTechnique technique, Matrix local, Color4 color)
        {
            #region Per frame update

            DrawerPool.EffectDefaultFont.UpdatePerFrame(
                local,
                this.viewProjection,
                color,
                this.fontMap.Texture);

            #endregion

            for (int p = 0; p < technique.Description.PassCount; p++)
            {
                technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, this.indexBufferOffset, this.vertexBufferOffset);
            }
        }
        /// <summary>
        /// Map text
        /// </summary>
        private void MapText()
        {
            Vector2 size;
            this.fontMap.MapSentence(
                this.text,
                out this.vertices, out this.indices, out size);

            this.Width = (int)size.X;
            this.Height = (int)size.Y;

            this.updateBuffers = true;
        }
        /// <summary>
        /// Update text translation matrices
        /// </summary>
        private void UpdatePosition()
        {
            float x = +this.position.X - this.Game.Form.RelativeCenter.X;
            float y = -this.position.Y + this.Game.Form.RelativeCenter.Y;

            this.local = Matrix.Translation(x, y, 0f);

            x = +this.position.X + this.ShadowRelative.X - this.Game.Form.RelativeCenter.X;
            y = -this.position.Y + this.ShadowRelative.Y + this.Game.Form.RelativeCenter.Y;

            this.localShadow = Matrix.Translation(x, y, 0f);
        }
    }
}
