using SharpDX;
using SharpDX.Direct3D;

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
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// Vertex couunt
        /// </summary>
        private int vertexDrawCount = 0;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexDrawCount = 0;
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
        /// <param name="scene">Scene</param>
        /// <param name="description">Text description</param>
        public TextDrawer(Scene scene, TextDrawerDescription description)
            : base(scene, description)
        {
            this.Font = string.Format("{0} {1}", description.Font, description.FontSize);

            this.viewProjection = Sprite.CreateViewOrthoProjection(
                this.Game.Form.RenderWidth.NextPair(),
                this.Game.Form.RenderHeight.NextPair());

            this.fontMap = FontMap.Map(this.Game, description.Font, description.FontSize);

            VertexPositionTexture[] vertices = new VertexPositionTexture[FontMap.MAXTEXTLENGTH * 4];
            uint[] indices = new uint[FontMap.MAXTEXTLENGTH * 6];

            this.vertexBuffer = this.BufferManager.Add(description.Name, vertices, true, 0);
            this.indexBuffer = this.BufferManager.Add(description.Name, indices, true);

            this.TextColor = description.TextColor;
            this.ShadowColor = description.ShadowColor;
            this.ShadowRelative = new Vector2(1, -1);

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
                    this.BufferManager.WriteBuffer(this.vertexBuffer.Slot, this.vertexBuffer.Offset, this.vertices);
                    this.BufferManager.WriteBuffer(this.indexBuffer.Slot, this.indexBuffer.Offset, this.indices);

                    this.vertexDrawCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 4;
                    this.indexDrawCount = string.IsNullOrWhiteSpace(this.text) ? 0 : this.text.Length * 6;

                    this.updateBuffers = false;
                }

                this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);

                var technique = DrawerPool.EffectDefaultFont.FontDrawer;

                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.TriangleList);

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
        private void DrawText(DrawContext context, EngineEffectTechnique technique, Matrix local, Color4 color)
        {
            #region Per frame update

            DrawerPool.EffectDefaultFont.UpdatePerFrame(
                local,
                this.viewProjection,
                color,
                this.fontMap.Texture);

            #endregion

            for (int p = 0; p < technique.PassCount; p++)
            {
                technique.Apply(this.Game.Graphics, p, 0);

                this.Game.Graphics.DeviceContext.DrawIndexed(this.indexDrawCount, this.indexBuffer.Offset, this.vertexBuffer.Offset);
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
