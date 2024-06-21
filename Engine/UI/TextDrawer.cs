using SharpDX;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.BuiltIn.Drawers;
    using Engine.BuiltIn.Drawers.Fonts;
    using Engine.Common;

    /// <summary>
    /// Text drawer
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    /// <param name="maxTextLength">Maximum text length</param>
    class TextDrawer(Scene scene, string id, string name, int maxTextLength) : Drawable<TextDrawerDescription>(scene, id, name)
    {
        /// <summary>
        /// Maximum text length
        /// </summary>
        private readonly int maxTextLength = maxTextLength;

        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexDrawCount = 0;
        /// <summary>
        /// Sentence descriptor
        /// </summary>
        private FontMapSentenceDescriptor sentenceDescriptor = FontMapSentenceDescriptor.Create(maxTextLength);

        /// <summary>
        /// Font map
        /// </summary>
        private FontMap fontMap = null;
        /// <summary>
        /// Base line threshold
        /// </summary>
        private float baseLineThr = 0;
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
        /// Horizontal align
        /// </summary>
        private TextHorizontalAlign horizontalAlign = TextHorizontalAlign.Left;
        /// <summary>
        /// Vertical align
        /// </summary>
        private TextVerticalAlign verticalAlign = TextVerticalAlign.Middle;
        /// <summary>
        /// Update internals flag
        /// </summary>
        private bool updateInternals = false;
        /// <summary>
        /// Font drawer
        /// </summary>
        private readonly BuiltInFonts fontDrawer = BuiltInShaders.GetDrawer<BuiltInFonts>();

        /// <summary>
        /// Manipulator
        /// </summary>
        protected Manipulator2D Manipulator { get; private set; }
        /// <summary>
        /// Shadow manipulator
        /// </summary>
        protected Manipulator2D ShadowManipulator { get; private set; }

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
                if (string.Equals(text, value))
                {
                    return;
                }

                text = value;

                updateInternals = true;
            }
        }
        /// <summary>
        /// Parsed text
        /// </summary>
        public string ParsedText { get; protected set; }
        /// <summary>
        /// Gets or sets the horizontal align
        /// </summary>
        public TextHorizontalAlign HorizontalAlign
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
        public TextVerticalAlign VerticalAlign
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
        /// Gets the text total mapped size
        /// </summary>
        public Vector2 TextSize { get; protected set; }
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
        /// Fine sampling
        /// </summary>
        /// <remarks>
        /// If deactivated, the font will be drawn with a point sampler. Otherwise, a linear sampler will be used.
        /// Deactivate for thin fonts.
        /// </remarks>
        public bool FineSampling { get; set; }
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
        /// Clipping rectangle
        /// </summary>
        /// <remarks>Defines an area outside which all text is clipped</remarks>
        public Rectangle? ClippingRectangle { get; set; } = null;
        /// <summary>
        /// Parent control
        /// </summary>
        public IUIControl Parent { get; set; } = null;

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

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(TextDrawerDescription description)
        {
            await base.ReadAssets(description);

            Manipulator = new Manipulator2D(Game);
            ShadowManipulator = new Manipulator2D(Game);

            var generator = FontMapKeycodeGenerator.DefaultWithCustom(Description.CustomKeycodes);

            if (!string.IsNullOrWhiteSpace(Description.FontFileName) && !string.IsNullOrWhiteSpace(Description.ContentPath))
            {
                fontMap = FontMap.FromFile(Game, Description.ContentPath, generator, Description.FontFileName, Description.FontSize, Description.Style);
            }
            else if (!Description.FontMapping.IsEmpty)
            {
                fontMap = FontMap.FromMap(Game, Description.ContentPath, Description.FontMapping);
            }
            else if (!string.IsNullOrWhiteSpace(Description.FontFamily))
            {
                fontMap = FontMap.FromFamily(Game, generator, Description.FontFamily, Description.FontSize, Description.Style);
            }

            vertexBuffer = BufferManager.AddVertexData(Name, true, sentenceDescriptor.Vertices);
            indexBuffer = BufferManager.AddIndexData(Name, true, sentenceDescriptor.Indices);

            UseTextureColor = Description.UseTextureColor;
            FineSampling = Description.FineSampling;

            if (Description.LineAdjust)
            {
                baseLineThr = GetLineHeight() * 0.1666f; // --> 0.3333f * 0.5f
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

            var sca = Vector2.One * (Parent?.AbsoluteScale ?? 1f);
            float rot = Parent?.AbsoluteRotation ?? 0f;
            var pos = renderArea.Center;
            pos.Y += baseLineThr;

            // Apply scroll if any
            pos = ApplyScroll(pos, renderArea);

            var parentPos = Parent?.GetTransformationPivot();

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
        /// Apply the scroll transformation to the text position
        /// </summary>
        /// <param name="pos">Text position</param>
        /// <param name="renderArea">Text render area</param>
        /// <returns>Returns the transformed position</returns>
        private Vector2 ApplyScroll(Vector2 pos, RectangleF renderArea)
        {
            if (Parent is not IScrollable ta)
            {
                return pos;
            }

            if (ta.Scroll == ScrollModes.None)
            {
                return pos;
            }

            ClippingRectangle = (Rectangle)renderArea;

            pos.X -= ta.Scroll.HasFlag(ScrollModes.Horizontal) ? ta.ScrollHorizontalOffset : 0f;
            pos.X = (int)pos.X;

            pos.Y -= ta.Scroll.HasFlag(ScrollModes.Vertical) ? ta.ScrollVerticalOffset : 0f;
            pos.Y = (int)pos.Y;

            return pos;
        }
        /// <summary>
        /// Sets the update internals flag to true
        /// </summary>
        public void UpdateInternals()
        {
            updateInternals = true;
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (fontMap == null)
            {
                return false;
            }

            if (!BuffersReady)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode, true);
            if (!draw)
            {
                return false;
            }

            var dc = context.DeviceContext;

            WriteBuffers(dc);

            var state = new BuiltInFontState
            {
                Alpha = Alpha * AlphaMultplier,
                UseColor = UseTextureColor,
                UseRectangle = ClippingRectangle.HasValue,
                FineSampling = FineSampling,
                FontTexture = fontMap.Texture,
                ClippingRectangle = ClippingRectangle ?? Rectangle.Empty,
            };
            fontDrawer.UpdateFont(dc, state);

            int count = indexDrawCount;

            bool drawn = false;

            if (ShadowColor != Color.Transparent)
            {
                //Draw with shadows
                int offset = indexDrawCount / 2;
                count = indexDrawCount / 2;

                fontDrawer.UpdateText(dc, ShadowManipulator.LocalTransform);

                var shOptions = new DrawOptions
                {
                    IndexBuffer = indexBuffer,
                    IndexDrawCount = count,
                    IndexBufferOffset = offset,
                    Topology = Topology.TriangleList,
                    VertexBuffer = vertexBuffer,
                };
                drawn = fontDrawer.Draw(context.DeviceContext, shOptions);
            }

            fontDrawer.UpdateText(dc, Manipulator.LocalTransform);

            var opOptions = new DrawOptions
            {
                IndexBuffer = indexBuffer,
                IndexDrawCount = count,
                IndexBufferOffset = 0,
                Topology = Topology.TriangleList,
                VertexBuffer = vertexBuffer,
            };
            return fontDrawer.Draw(dc, opOptions) || drawn;
        }
        /// <summary>
        /// Writes text data into buffers
        /// </summary>
        /// <param name="dc">Device context</param>
        private void WriteBuffers(IEngineDeviceContext dc)
        {
            bool vertsWrited = Game.WriteVertexBuffer(dc, vertexBuffer, sentenceDescriptor.Vertices.Take((int)sentenceDescriptor.VertexCount).ToArray());
            bool idxWrited = Game.WriteIndexBuffer(dc, indexBuffer, sentenceDescriptor.Indices.Take((int)sentenceDescriptor.IndexCount).ToArray());
            if (!vertsWrited || !idxWrited)
            {
                return;
            }

            indexDrawCount = (int)sentenceDescriptor.IndexCount;
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

            sentenceDescriptor.Clear();

            var parsed = FontMapParser.ParseSentence(text, textColor, shadowColor);
            ParsedText = parsed.Text;

            var renderArea = GetRenderArea();

            fontMap.MapSentence(
                parsed,
                false,
                renderArea.Width,
                ref sentenceDescriptor);

            TextSize = sentenceDescriptor.GetSize();

            if (ShadowColor != Color.Transparent)
            {
                fontMap.MapSentence(
                    parsed,
                    true,
                    renderArea.Width,
                    ref sentenceDescriptor);
            }

            sentenceDescriptor.Adjust(renderArea, horizontalAlign, verticalAlign);
        }
        /// <summary>
        /// Gets the render area in absolute coordinates from screen origin
        /// </summary>
        /// <returns>Returns the render area</returns>
        private RectangleF GetRenderArea()
        {
            return Parent?.GetRenderArea(true) ?? Game.Form.RenderRectangle;
        }

        /// <summary>
        /// Measures the specified text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <returns>Returns a size vector where X is the width, and Y is the height</returns>
        public Vector2 MeasureText(string text, TextHorizontalAlign horizontalAlign, TextVerticalAlign verticalAlign)
        {
            if (fontMap == null)
            {
                return Vector2.Zero;
            }

            if (string.IsNullOrEmpty(text))
            {
                return Vector2.Zero;
            }

            var desc = FontMapSentenceDescriptor.Create(maxTextLength);

            var parsed = FontMapParser.ParseSentence(text, ForeColor, ShadowColor);

            var renderArea = GetRenderArea();

            fontMap.MapSentence(
                parsed,
                false,
                renderArea.Width,
                ref desc);

            desc.Adjust(renderArea, horizontalAlign, verticalAlign);

            return desc.GetSize();
        }
        /// <summary>
        /// Gets the single line height
        /// </summary>
        public float GetLineHeight()
        {
            return fontMap?.GetSpaceSize().Y ?? 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ParsedText;
        }
    }
}
