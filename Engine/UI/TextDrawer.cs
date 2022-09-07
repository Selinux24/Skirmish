using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Fonts;
    using Engine.Common;

    /// <summary>
    /// Text drawer
    /// </summary>
    class TextDrawer : Drawable<TextDrawerDescription>
    {
        /// <summary>
        /// Maximum text length
        /// </summary>
        public const int MAXTEXTLENGTH = 1024 * 10;

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
        private readonly BuiltInFonts fontDrawer;

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
        public string Font = null;
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

                if (text?.Length > MAXTEXTLENGTH)
                {
                    text = text.Substring(0, MAXTEXTLENGTH);
                }

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
        /// <remarks>Defines an area outside wich all text is clipped</remarks>
        public Rectangle? ClippingRectangle { get; set; } = null;
        /// <summary>
        /// Parent control
        /// </summary>
        public IUIControl Parent { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public TextDrawer(Scene scene, string id, string name)
            : base(scene, id, name)
        {
            fontDrawer = BuiltInShaders.GetDrawer<BuiltInFonts>();
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

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(TextDrawerDescription description)
        {
            await base.InitializeAssets(description);

            Manipulator = new Manipulator2D(Game);
            ShadowManipulator = new Manipulator2D(Game);

            Font = $"{Description.FontFamily} {Description.FontSize}";

            var generator = FontMapKeycodeGenerator.DefaultWithCustom(Description.CustomKeycodes);

            if (!string.IsNullOrWhiteSpace(Description.FontFileName) && !string.IsNullOrWhiteSpace(Description.ContentPath))
            {
                fontMap = await FontMap.FromFile(Game, Description.ContentPath, generator, Description.FontFileName, Description.FontSize, Description.Style);
            }
            else if (Description.FontMapping != null)
            {
                fontMap = await FontMap.FromMap(Game, Description.ContentPath, Description.FontMapping);
            }
            else if (!string.IsNullOrWhiteSpace(Description.FontFamily))
            {
                fontMap = await FontMap.FromFamily(Game, generator, Description.FontFamily, Description.FontSize, Description.Style);
            }

            VertexFont[] verts = new VertexFont[MAXTEXTLENGTH * 4];
            uint[] idx = new uint[MAXTEXTLENGTH * 6];

            vertexBuffer = BufferManager.AddVertexData(Name, true, verts);
            indexBuffer = BufferManager.AddIndexData(Name, true, idx);

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

            Vector2 sca = Vector2.One * (Parent?.AbsoluteScale ?? 1f);
            float rot = Parent?.AbsoluteRotation ?? 0f;
            Vector2 pos = renderArea.Center;
            pos.Y += baseLineThr;

            // Apply scroll if any
            pos = ApplyScroll(pos, renderArea);

            Vector2? parentPos = Parent?.GetTransformationPivot();

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
            if (Parent is IScrollable ta)
            {
                if (ta.Scroll == ScrollModes.None)
                {
                    return pos;
                }

                ClippingRectangle = (Rectangle)renderArea;

                pos.X -= ta.Scroll.HasFlag(ScrollModes.Horizontal) ? ta.ScrollHorizontalOffset : 0f;
                pos.X = (int)pos.X;

                pos.Y -= ta.Scroll.HasFlag(ScrollModes.Vertical) ? ta.ScrollVerticalOffset : 0f;
                pos.Y = (int)pos.Y;
            }

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

            var bufferManager = BufferManager;

            var state = new BuiltInFontState
            {
                Alpha = Alpha * AlphaMultplier,
                UseColor = UseTextureColor,
                UseRectangle = ClippingRectangle.HasValue,
                FineSampling = FineSampling,
                FontTexture = fontMap.Texture,
                ClippingRectangle = ClippingRectangle ?? Rectangle.Empty,
            };
            fontDrawer.UpdateFont(state);

            int count = indexDrawCount;

            if (ShadowColor != Color.Transparent)
            {
                //Draw with shadows
                int offset = indexDrawCount / 2;
                count = indexDrawCount / 2;

                fontDrawer.UpdateText(ShadowManipulator.LocalTransform);

                var shOptions = new DrawOptions
                {
                    Indexed = true,
                    IndexBuffer = indexBuffer,
                    IndexDrawCount = count,
                    IndexBufferOffset = offset,
                    Topology = Topology.TriangleList,
                    VertexBuffer = vertexBuffer,
                };
                fontDrawer.Draw(bufferManager, shOptions);
            }

            fontDrawer.UpdateText(Manipulator.LocalTransform);

            var opOptions = new DrawOptions
            {
                Indexed = true,
                IndexBuffer = indexBuffer,
                IndexDrawCount = count,
                IndexBufferOffset = 0,
                Topology = Topology.TriangleList,
                VertexBuffer = vertexBuffer,
            };
            fontDrawer.Draw(bufferManager, opOptions);
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

            var parsed = FontMapParser.ParseSentence(text, textColor, shadowColor);
            ParsedText = parsed.Text;

            var renderArea = GetRenderArea();

            var colorW = fontMap.MapSentence(
                parsed,
                false,
                renderArea,
                horizontalAlign,
                verticalAlign);

            TextSize = colorW.Size;

            iList.AddRange(colorW.Indices);
            vList.AddRange(colorW.Vertices);

            if (ShadowColor != Color.Transparent)
            {
                var colorS = fontMap.MapSentence(
                    parsed,
                    true,
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

            var parsed = FontMapParser.ParseSentence(text, ForeColor, ShadowColor);

            var renderArea = GetRenderArea();

            var w = fontMap.MapSentence(
                parsed,
                false,
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
                FontMapParsedSentence.FromSample(sampleChar),
                false,
                Game.Form.RenderRectangle,
                TextHorizontalAlign.Left,
                TextVerticalAlign.Top);

            return w.Size.Y;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ParsedText;
        }
    }
}
