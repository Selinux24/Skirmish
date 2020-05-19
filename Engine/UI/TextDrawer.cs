using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
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
        /// Text translation matrix
        /// </summary>
        private Matrix local;
        /// <summary>
        /// Text shadow translarion matrix
        /// </summary>
        private Matrix localShadow;

        /// <summary>
        /// Font map
        /// </summary>
        private FontMap fontMap = null;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        private Matrix viewProjection;
        /// <summary>
        /// Top
        /// </summary>
        private int top = 0;
        /// <summary>
        /// Left
        /// </summary>
        private int left = 0;
        /// <summary>
        /// Text area rectangle limit
        /// </summary>
        /// <remarks>Used for text positioning</remarks>
        private Rectangle? textArea = null;
        /// <summary>
        /// Text
        /// </summary>
        private string text = null;
        /// <summary>
        /// Center text into the defined text area (if any)
        /// </summary>
        private bool centered = false;

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
        public Vector2 ShadowDelta { get; set; }
        /// <summary>
        /// Gets or sets the text color alpha multiplier
        /// </summary>
        public float AlphaMultplier { get; set; } = 1.2f;

        /// <summary>
        /// Gets or sets the text area rectangle limit
        /// </summary>
        /// <remarks>Used for text positioning</remarks>
        public Rectangle? TextArea
        {
            get
            {
                return this.textArea;
            }
            set
            {
                this.textArea = value;
                this.centered = value.HasValue && this.centered;

                this.MapText();
            }
        }
        /// <summary>
        /// Gets or sets the text position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(this.left, this.top);
            }
            set
            {
                this.left = (int)value.X;
                this.top = (int)value.Y;
                this.centered = false;
                this.textArea = null;

                this.MapText();
            }
        }
        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public int Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
                this.centered = false;
                this.textArea = null;

                this.MapText();
            }
        }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public int Top
        {
            get
            {
                return this.top;
            }
            set
            {
                this.top = value;
                this.centered = false;
                this.textArea = null;

                this.MapText();
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
            this.Font = string.Format("{0} {1}", description.Font, description.FontSize);

            this.viewProjection = this.Game.Form.GetOrthoProjectionMatrix();

            if (!string.IsNullOrWhiteSpace(description.Font))
            {
                this.fontMap = FontMap.Map(this.Game, description.Font, description.FontSize, description.Style);
            }
            else if (!string.IsNullOrWhiteSpace(description.FontFileName) && !string.IsNullOrWhiteSpace(description.ContentPath))
            {
                this.fontMap = FontMap.MapFromFile(this.Game, description.ContentPath, description.FontFileName, description.FontSize, description.Style);
            }

            VertexPositionTexture[] verts = new VertexPositionTexture[FontMap.MAXTEXTLENGTH * 4];
            uint[] idx = new uint[FontMap.MAXTEXTLENGTH * 6];

            this.vertexBuffer = this.BufferManager.AddVertexData(description.Name, true, verts);
            this.indexBuffer = this.BufferManager.AddIndexData(description.Name, true, idx);

            this.TextColor = description.TextColor;
            this.ShadowColor = description.ShadowColor;
            this.ShadowDelta = description.ShadowDelta;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TextDrawer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                this.BufferManager?.RemoveVertexData(this.vertexBuffer);
                this.BufferManager?.RemoveIndexData(this.indexBuffer);

                //Remove the font map reference
                this.fontMap = null;
            }
        }

        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (!BuffersReady)
            {
                return;
            }

            var mode = context.DrawerMode;

            if (mode.HasFlag(DrawerModes.TransparentOnly) && !string.IsNullOrWhiteSpace(this.text))
            {
                if (this.updateBuffers)
                {
                    this.BufferManager.WriteVertexBuffer(this.vertexBuffer, this.vertices);
                    this.BufferManager.WriteIndexBuffer(this.indexBuffer, this.indices);

                    this.indexDrawCount = this.indices.Length;

                    this.updateBuffers = false;
                }

                this.BufferManager.SetIndexBuffer(this.indexBuffer);

                var effect = DrawerPool.EffectDefaultFont;
                var technique = effect.FontDrawer;

                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer, Topology.TriangleList);

                if (this.ShadowColor != Color.Transparent)
                {
                    //Draw shadow
                    this.DrawText(effect, technique, this.localShadow, this.ShadowColor);
                }

                //Draw text
                this.DrawText(effect, technique, this.local, this.TextColor);
            }
        }
        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="technique">Technique</param>
        /// <param name="local">Local transform</param>
        /// <param name="color">Text color</param>
        private void DrawText(EffectDefaultFont effect, EngineEffectTechnique technique, Matrix local, Color4 color)
        {
            effect.UpdatePerFrame(
                local,
                this.viewProjection,
                color.RGB(),
                color.Alpha * AlphaMultplier,
                this.fontMap.Texture);

            var graphics = this.Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(this.indexDrawCount, this.indexBuffer.BufferOffset, this.vertexBuffer.BufferOffset);
            }
        }

        /// <summary>
        /// Resize
        /// </summary>
        public void Resize()
        {
            this.viewProjection = this.Game.Form.GetOrthoProjectionMatrix();
        }

        /// <summary>
        /// Centers the text into the screen
        /// </summary>
        public void CenterScreen()
        {
            CenterRectangle(this.Game.Form.RenderRectangle);
        }
        /// <summary>
        /// Centers the text into the rectangle
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        public void CenterRectangle(Rectangle rectangle)
        {
            this.textArea = rectangle;
            this.centered = true;

            this.MapText();
        }
        /// <summary>
        /// Map text
        /// </summary>
        private void MapText()
        {
            var rect = textArea ?? new Rectangle(this.left, this.top, this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);

            this.fontMap.MapSentence(
                this.text,
                rect,
                out this.vertices, out this.indices, out Vector2 size);

            this.updateBuffers = true;

            // Adjust text bounds
            this.Width = (int)size.X;
            this.Height = (int)size.Y;

            // Adjust position
            if (centered && textArea.HasValue)
            {
                var targetCenter = textArea.Value.Center();

                this.left = (int)targetCenter.X - (int)(Width * 0.5f);
                this.top = (int)targetCenter.Y - (int)(Height * 0.5f);
            }
            else
            {
                this.top = rect.Y;
                this.left = rect.X;
            }

            // Adjust to screen
            int x = +(this.left - this.Game.Form.RelativeCenter.X);
            int y = -(this.top - this.Game.Form.RelativeCenter.Y);

            // Calculate new transforms
            this.local = Matrix.Translation(x, y, 0f);
            this.localShadow = Matrix.Translation(x + this.ShadowDelta.X, y + this.ShadowDelta.Y, 0f);
        }
    }

    /// <summary>
    /// Text drawer extensions
    /// </summary>
    public static class TextDrawerExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<TextDrawer> AddComponentTextDrawer(this Scene scene, TextDrawerDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            TextDrawer component = null;

            await Task.Run(() =>
            {
                component = new TextDrawer(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
