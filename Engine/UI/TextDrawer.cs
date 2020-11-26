using SharpDX;
using System.Collections.Generic;
using System.Linq;

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
        private VertexFont[] vertices;
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
        /// Text fore color
        /// </summary>
        private Color4 textColor;
        /// <summary>
        /// Text shadow color
        /// </summary>
        private Color4 shadowColor;

        /// <summary>
        /// Parent control
        /// </summary>
        private readonly UIControl parent = null;
        /// <summary>
        /// Horizontal align
        /// </summary>
        private HorizontalTextAlign horizontalAlign = HorizontalTextAlign.Left;
        /// <summary>
        /// Vertical align
        /// </summary>
        private VerticalTextAlign verticalAlign = VerticalTextAlign.Middle;
        /// <summary>
        /// Update internals flag
        /// </summary>
        private bool updateInternals = false;

        /// <summary>
        /// Manipulator
        /// </summary>
        protected Manipulator2D Manipulator { get; private set; }
        /// <summary>
        /// Shadow manipulator
        /// </summary>
        protected Manipulator2D ShadowManipulator { get; private set; }

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
                return text;
            }
            set
            {
                if (!string.Equals(text, value))
                {
                    text = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Parsed text
        /// </summary>
        public string ParsedText { get; protected set; }
        /// <summary>
        /// Gets or sets the horizontal align
        /// </summary>
        public HorizontalTextAlign HorizontalAlign
        {
            get
            {
                return horizontalAlign;
            }
            set
            {
                if (horizontalAlign != value)
                {
                    horizontalAlign = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the vertical align
        /// </summary>
        public VerticalTextAlign VerticalAlign
        {
            get
            {
                return verticalAlign;
            }
            set
            {
                if (verticalAlign != value)
                {
                    verticalAlign = value;

                    updateInternals = true;
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
        public Color4 ForeColor
        {
            get
            {
                return textColor;
            }
            set
            {
                if (textColor != value)
                {
                    textColor = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets text shadow color
        /// </summary>
        public Color4 ShadowColor
        {
            get
            {
                return shadowColor;
            }
            set
            {
                if (shadowColor != value)
                {
                    shadowColor = value;

                    updateInternals = true;
                }
            }
        }
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
        /// Gets whether the internal buffers were ready or not
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                return vertexBuffer?.Ready == true && indexBuffer?.Ready == true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="parent">Parent control</param>
        /// <param name="description">Text description</param>
        public TextDrawer(string name, Scene scene, UIControl parent, TextDrawerDescription description)
            : base(name, scene, description)
        {
            this.parent = parent;

            Manipulator = new Manipulator2D(Game);
            ShadowManipulator = new Manipulator2D(Game);

            Font = $"{description.FontFamily} {description.FontSize}";

            viewProjection = Game.Form.GetOrthoProjectionMatrix();

            if (!string.IsNullOrWhiteSpace(description.FontFileName) && !string.IsNullOrWhiteSpace(description.ContentPath))
            {
                fontMap = FontMap.FromFile(Game, description.ContentPath, description.FontFileName, description.FontSize, description.Style);
            }
            else if (description.FontMapping != null)
            {
                fontMap = FontMap.FromMap(Game, description.ContentPath, description.FontMapping);
            }
            else if (!string.IsNullOrWhiteSpace(description.FontFamily))
            {
                fontMap = FontMap.FromFamily(Game, description.FontFamily, description.FontSize, description.Style);
            }

            VertexFont[] verts = new VertexFont[FontMap.MAXTEXTLENGTH * 4];
            uint[] idx = new uint[FontMap.MAXTEXTLENGTH * 6];

            vertexBuffer = BufferManager.AddVertexData(name, true, verts);
            indexBuffer = BufferManager.AddIndexData(name, true, idx);

            UseTextureColor = description.UseTextureColor;

            if (description.LineAdjust)
            {
                baseLineThr = GetLineHeight() * 0.1666f; // --> 0.3333f * 0.5f
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
                BufferManager?.RemoveVertexData(vertexBuffer);
                BufferManager?.RemoveIndexData(indexBuffer);
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            if (updateInternals)
            {
                MapText();

                updateInternals = false;
            }

            var renderArea = GetRenderArea();

            Vector2 sca = Vector2.One * (parent?.AbsoluteScale ?? 1f);
            Vector2 pos = new Vector2(renderArea.X, renderArea.Y + baseLineThr);
            float rot = parent?.AbsoluteRotation ?? 0f;

            Vector2? parentPos = parent?.GetTransformationPivot();

            // Calculate new transforms
            Manipulator.SetScale(sca);
            Manipulator.SetRotation(rot);
            Manipulator.SetPosition(pos);
            Manipulator.Update2D(parentPos);

            if (ShadowColor != Color.Transparent)
            {
                ShadowManipulator.SetScale(sca);
                ShadowManipulator.SetRotation(rot);
                ShadowManipulator.SetPosition(pos + ShadowDelta);
                ShadowManipulator.Update2D(parentPos);
            }
        }
        /// <summary>
        /// Sets the update internals flag to true
        /// </summary>
        public void UpdateInternals()
        {
            updateInternals = true;
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
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

            bool draw = context.ValidateDraw(BlendMode, true);
            if (!draw)
            {
                return;
            }

            WriteBuffers();

            BufferManager.SetIndexBuffer(indexBuffer);

            var effect = DrawerPool.EffectDefaultFont;
            var technique = effect.FontDrawer;

            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            if (ShadowColor != Color.Transparent)
            {
                //Draw with shadows
                int offset = indexDrawCount / 2;
                int count = indexDrawCount / 2;
                DrawText(effect, technique, ShadowManipulator.LocalTransform, UseTextureColor, offset, count);
                DrawText(effect, technique, Manipulator.LocalTransform, UseTextureColor, 0, count);
            }
            else
            {
                //Draw fore color only
                DrawText(effect, technique, Manipulator.LocalTransform, UseTextureColor, 0, indexDrawCount);
            }
        }
        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="technique">Technique</param>
        /// <param name="local">Local transform</param>
        /// <param name="useTextureColor">Use the texture color</param>
        /// <param name="index">Primitive index</param>
        /// <param name="count">Index count</param>
        private void DrawText(EffectDefaultFont effect, EngineEffectTechnique technique, Matrix local, bool useTextureColor, int index, int count)
        {
            effect.UpdatePerFrame(
                local,
                viewProjection,
                Alpha * AlphaMultplier,
                useTextureColor,
                fontMap.Texture);

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(count, indexBuffer.BufferOffset + index, vertexBuffer.BufferOffset);
            }
        }
        /// <summary>
        /// Writes text data into buffers
        /// </summary>
        private void WriteBuffers()
        {
            if (!updateBuffers)
            {
                return;
            }

            bool vertsWrited = BufferManager.WriteVertexBuffer(vertexBuffer, vertices);
            bool idxWrited = BufferManager.WriteIndexBuffer(indexBuffer, indices);
            if (!vertsWrited || !idxWrited)
            {
                return;
            }

            indexDrawCount = indices?.Length ?? 0;

            updateBuffers = false;
        }

        /// <summary>
        /// Resize
        /// </summary>
        public void Resize()
        {
            viewProjection = Game.Form.GetOrthoProjectionMatrix();

            updateInternals = true;
        }

        /// <summary>
        /// Map text
        /// </summary>
        private void MapText()
        {
            if (fontMap == null)
            {
                return;
            }

            List<VertexFont> vList = new List<VertexFont>();
            List<uint> iList = new List<uint>();

            ParsedText = FontMap.ParseSentence(
                text,
                textColor,
                shadowColor,
                out var words, out var colors, out var shadowColors);

            var renderArea = GetRenderArea();

            var colorW = fontMap.MapSentence(
                words,
                colors,
                renderArea,
                horizontalAlign,
                verticalAlign);

            iList.AddRange(colorW.Indices);
            vList.AddRange(colorW.Vertices);

            if (ShadowColor != Color.Transparent)
            {
                var colorS = fontMap.MapSentence(
                    words,
                    shadowColors,
                    renderArea,
                    horizontalAlign,
                    verticalAlign);

                colorS.Indices.ToList().ForEach((i) => { iList.Add(i + (uint)vList.Count); });
                vList.AddRange(colorS.Vertices);
            }

            vertices = vList.ToArray();
            indices = iList.ToArray();

            updateBuffers = true;
        }
        /// <summary>
        /// Gets the render area in absolute coordinates from screen origin
        /// </summary>
        /// <returns>Returns the render area</returns>
        private RectangleF GetRenderArea()
        {
            return parent?.GetRenderArea(true) ?? Game.Form.RenderRectangle;
        }

        /// <summary>
        /// Measures the specified text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <returns>Returns a size vector where X is the width, and Y is the height</returns>
        public Vector2 MeasureText(string text, HorizontalTextAlign horizontalAlign, VerticalTextAlign verticalAlign)
        {
            if (fontMap == null)
            {
                return Vector2.Zero;
            }

            FontMap.ParseSentence(text, ForeColor, ShadowColor, out var words, out _, out _);

            var renderArea = GetRenderArea();

            var w = fontMap.MapSentence(
                words,
                null,
                renderArea,
                horizontalAlign,
                verticalAlign);

            return w.Size;
        }
        /// <summary>
        /// Gets the single line height
        /// </summary>
        public float GetLineHeight()
        {
            if (fontMap == null)
            {
                return 0;
            }

            string sampleChar = $"{fontMap.GetSampleCharacter()}";

            var w = fontMap.MapSentence(
                new[] { sampleChar },
                null,
                Game.Form.RenderRectangle,
                HorizontalTextAlign.Left,
                VerticalTextAlign.Top);

            return w.Size.Y;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ParsedText;
        }
    }
}
