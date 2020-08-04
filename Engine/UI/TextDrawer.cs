using SharpDX;

namespace Engine.UI
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Text drawer
    /// </summary>
    class TextDrawer : Drawable, IScreenFitted
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private readonly BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private readonly BufferDescriptor indexBuffer = null;
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
        /// View * projection matrix
        /// </summary>
        private Matrix viewProjection;

        /// <summary>
        /// Font map
        /// </summary>
        private readonly FontMap fontMap = null;
        /// <summary>
        /// Base line threshold
        /// </summary>
        private readonly float baseLineThr = 0;
        /// <summary>
        /// Text
        /// </summary>
        private string text = null;

        /// <summary>
        /// Parent control
        /// </summary>
        private UIControl parent = null;
        /// <summary>
        /// Text width
        /// </summary>
        private float width;
        /// <summary>
        /// Text height
        /// </summary>
        private float height;
        /// <summary>
        /// Text area rectangle limit
        /// </summary>
        /// <remarks>Used for text positioning</remarks>
        private RectangleF? textArea = null;

        /// <summary>
        /// Manipulator
        /// </summary>
        protected Manipulator2D Manipulator { get; private set; }
        /// <summary>
        /// Shadow manipulator
        /// </summary>
        protected Manipulator2D ShadowManipulator { get; private set; }

        /// <summary>
        /// Parent control
        /// </summary>
        public UIControl Parent { get; set; }

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
        /// Use the texure color flag
        /// </summary>
        public bool UseTextureColor { get; set; } = false;
        /// <summary>
        /// Gets or sets text fore color
        /// </summary>
        public Color4 TextColor { get; set; }
        /// <summary>
        /// Gets or sets text shadow color
        /// </summary>
        public Color4 ShadowColor { get; set; }
        /// <summary>
        /// Alpha color component
        /// </summary>
        public float Alpha { get; set; } = 1f;
        /// <summary>
        /// Gets or sets relative position of shadow
        /// </summary>
        public Vector2 ShadowDelta { get; set; }
        /// <summary>
        /// Gets or sets the text color alpha multiplier
        /// </summary>
        public float AlphaMultplier { get; set; } = 1.2f;
        /// <summary>
        /// Gets or sets the horizontal align
        /// </summary>
        public TextAlign HorizontalAlign { get; set; } = TextAlign.Left;
        /// <summary>
        /// Gets or sets the vertical align
        /// </summary>
        public VerticalAlign VerticalAlign { get; set; } = VerticalAlign.Middle;

        /// <summary>
        /// Gets whether the internal buffers were ready or not
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                return this.vertexBuffer?.Ready == true && this.indexBuffer?.Ready == true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Text description</param>
        public TextDrawer(Scene scene, TextDrawerDescription description)
            : base(scene, description)
        {
            this.Manipulator = new Manipulator2D(this.Game);
            this.ShadowManipulator = new Manipulator2D(this.Game);

            this.Font = string.Format("{0} {1}", description.Font, description.FontSize);

            this.viewProjection = this.Game.Form.GetOrthoProjectionMatrix();

            if (!string.IsNullOrWhiteSpace(description.FontFileName) && !string.IsNullOrWhiteSpace(description.ContentPath))
            {
                this.fontMap = FontMap.FromFile(this.Game, description.ContentPath, description.FontFileName, description.FontSize, description.Style);
            }
            else if (description.FontMapping != null)
            {
                this.fontMap = FontMap.FromMap(this.Game, description.ContentPath, description.FontMapping);
            }
            else if (!string.IsNullOrWhiteSpace(description.Font))
            {
                this.fontMap = FontMap.FromFamily(this.Game, description.Font, description.FontSize, description.Style);
            }

            VertexPositionTexture[] verts = new VertexPositionTexture[FontMap.MAXTEXTLENGTH * 4];
            uint[] idx = new uint[FontMap.MAXTEXTLENGTH * 6];

            this.vertexBuffer = this.BufferManager.AddVertexData(description.Name, true, verts);
            this.indexBuffer = this.BufferManager.AddIndexData(description.Name, true, idx);

            this.TextColor = description.TextColor;
            this.UseTextureColor = description.UseTextureColor;
            this.ShadowColor = description.ShadowColor;
            this.ShadowDelta = description.ShadowDelta;
            this.HorizontalAlign = description.HorizontalAlign;
            this.VerticalAlign = description.VerticalAlign;

            this.MapText();

            if (description.LineAdjust)
            {
                // Set base line threshold
                this.baseLineThr = this.height * 0.1666f; // --> 0.3333f * 0.5f
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TextDrawer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                this.BufferManager?.RemoveVertexData(this.vertexBuffer);
                this.BufferManager?.RemoveIndexData(this.indexBuffer);
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!this.Active)
            {
                return;
            }

            this.MapText();

            Vector2 sca;
            Vector2 pos;
            float rot;
            Vector2 parentCenter;
            float parentScale;

            if (this.Parent != null)
            {
                sca = Vector2.One * this.Parent.AbsoluteScale;
                pos = new Vector2(this.Parent.AbsoluteLeft, this.Parent.AbsoluteTop);
                rot = this.Parent.AbsoluteRotation;
                parentCenter = this.Parent.GrandpaRectangle.Center;
                parentScale = this.Parent.GrandpaScale;
            }
            else
            {
                sca = Vector2.One;
                pos = Vector2.Zero;
                rot = 0f;
                parentCenter = Vector2.Zero;
                parentScale = 1f;
            }

            // Adjust position
            var rect = this.GetRenderArea().Scale(this.Parent?.Scale ?? 1f);

            pos.X = rect.X;

            if (VerticalAlign == VerticalAlign.Middle)
            {
                pos.Y = rect.Center.Y - (height * 0.5f);
            }
            else if (VerticalAlign == VerticalAlign.Bottom)
            {
                pos.Y = rect.Bottom - height;
            }
            else
            {
                pos.Y = rect.Y;
            }

            pos.Y += this.baseLineThr;

            // Calculate new transforms
            this.Manipulator.SetScale(sca);
            this.Manipulator.SetRotation(rot);
            this.Manipulator.SetPosition(pos);
            this.Manipulator.Update(parentCenter, parentScale);

            if (this.ShadowColor != Color.Transparent)
            {
                this.ShadowManipulator.SetScale(sca);
                this.ShadowManipulator.SetRotation(rot);
                this.ShadowManipulator.SetPosition(pos + this.ShadowDelta);
                this.ShadowManipulator.Update(parentCenter, parentScale);
            }
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(this.text))
            {
                return;
            }

            if (fontMap == null)
            {
                return;
            }

            if (!BuffersReady)
            {
                return;
            }

            bool draw = context.ValidateDraw(this.BlendMode, true);
            if (!draw)
            {
                return;
            }

            this.WriteBuffers();

            this.BufferManager.SetIndexBuffer(this.indexBuffer);

            var effect = DrawerPool.EffectDefaultFont;
            var technique = effect.FontDrawer;

            this.BufferManager.SetInputAssembler(technique, this.vertexBuffer, Topology.TriangleList);

            if (this.ShadowColor != Color.Transparent)
            {
                //Draw shadow
                this.DrawText(effect, technique, this.ShadowManipulator.LocalTransform, this.ShadowColor, false);
            }

            //Draw text
            this.DrawText(effect, technique, this.Manipulator.LocalTransform, this.TextColor, this.UseTextureColor);
        }
        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="technique">Technique</param>
        /// <param name="local">Local transform</param>
        /// <param name="color">Text color</param>
        /// <param name="useTextureColor">Use the texture color</param>
        private void DrawText(EffectDefaultFont effect, EngineEffectTechnique technique, Matrix local, Color4 color, bool useTextureColor)
        {
            effect.UpdatePerFrame(
                local,
                this.viewProjection,
                color.RGB(),
                color.Alpha * Alpha * AlphaMultplier,
                useTextureColor,
                this.fontMap.Texture);

            var graphics = this.Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(this.indexDrawCount, this.indexBuffer.BufferOffset, this.vertexBuffer.BufferOffset);
            }
        }
        /// <summary>
        /// Writes text data into buffers
        /// </summary>
        private void WriteBuffers()
        {
            if (!this.updateBuffers)
            {
                return;
            }

            this.BufferManager.WriteVertexBuffer(this.vertexBuffer, this.vertices);
            this.BufferManager.WriteIndexBuffer(this.indexBuffer, this.indices);

            this.indexDrawCount = this.indices.Length;

            this.updateBuffers = false;
        }

        /// <summary>
        /// Resize
        /// </summary>
        public void Resize()
        {
            this.viewProjection = this.Game.Form.GetOrthoProjectionMatrix();
        }

        /// <summary>
        /// Centers the text into the parent rectangle
        /// </summary>
        public void CenterParent()
        {
            this.textArea = null;
            this.HorizontalAlign = TextAlign.Center;
            this.VerticalAlign = VerticalAlign.Middle;
        }
        /// <summary>
        /// Centers the text into the rectangle
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        public void CenterRectangle(RectangleF rectangle)
        {
            this.textArea = rectangle;
            this.HorizontalAlign = TextAlign.Center;
            this.VerticalAlign = VerticalAlign.Middle;
        }

        /// <summary>
        /// Map text
        /// </summary>
        private void MapText()
        {
            if (this.fontMap == null)
            {
                return;
            }

            this.fontMap.MapSentence(
                this.text,
                this.GetCurrentWidth(),
                this.HorizontalAlign,
                this.VerticalAlign,
                out this.vertices, out this.indices, out Vector2 size);

            this.updateBuffers = true;

            // Adjust text bounds
            this.width = size.X;
            this.height = size.Y;
        }
        /// <summary>
        /// Gets the text render area
        /// </summary>
        /// <returns>Returns the text render area</returns>
        private RectangleF GetRenderArea()
        {
            return this.Parent?.GetRenderArea() ?? this.textArea ?? this.Game.Form.RenderRectangle;
        }

        /// <summary>
        /// Measures the specified text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="width">Maximum text width</param>
        /// <returns>Returns a size vector where X is the width, and Y is the height</returns>
        public Vector2 MeasureText(string text, float width)
        {
            if (this.fontMap == null)
            {
                return Vector2.Zero;
            }

            this.fontMap.MapSentence(
                text,
                width,
                this.HorizontalAlign,
                this.VerticalAlign,
                out _, out _, out Vector2 size);

            return size;
        }


        private float GetCurrentWidth()
        {
            var rect = this.GetRenderArea();

            return rect.Width * (this.Parent?.Scale ?? 1f);
        }

        private float GetCurrentHeight()
        {
            var rect = this.GetRenderArea();

            return rect.Height * (this.Parent?.Scale ?? 1f);
        }
    }
}
