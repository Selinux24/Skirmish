using SharpDX;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;
    using Engine.Effects;
    using System.Linq;

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
        /// Parent control
        /// </summary>
        private UIControl parent = null;
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
        private float top = 0;
        /// <summary>
        /// Left
        /// </summary>
        private float left = 0;
        /// <summary>
        /// Text area rectangle limit
        /// </summary>
        /// <remarks>Used for text positioning</remarks>
        private RectangleF? textArea = null;
        /// <summary>
        /// Text
        /// </summary>
        private string text = null;
        /// <summary>
        /// Horizontal centering flag
        /// </summary>
        private TextCenteringTargets centerHorizontally = TextCenteringTargets.None;
        /// <summary>
        /// Vertical centering flag
        /// </summary>
        private TextCenteringTargets centerVertically = TextCenteringTargets.None;

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
        public UIControl Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;

                this.MapText();
            }
        }

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
                this.left = value.X;
                this.top = value.Y;
                this.centerHorizontally = TextCenteringTargets.None;
                this.centerVertically = TextCenteringTargets.None;
                this.textArea = null;

                this.MapText();
            }
        }
        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public float Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
                this.centerHorizontally = TextCenteringTargets.None;
                this.centerVertically = TextCenteringTargets.None;
                this.textArea = null;

                this.MapText();
            }
        }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public float Top
        {
            get
            {
                return this.top;
            }
            set
            {
                this.top = value;
                this.centerHorizontally = TextCenteringTargets.None;
                this.centerVertically = TextCenteringTargets.None;
                this.textArea = null;

                this.MapText();
            }
        }
        /// <summary>
        /// Gets text width
        /// </summary>
        public float Width { get; private set; }
        /// <summary>
        /// Gets text height
        /// </summary>
        public float Height { get; private set; }
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
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!this.Active)
            {
                return;
            }

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
            if (centerHorizontally != TextCenteringTargets.None)
            {
                var rectH = this.GetCenteringArea(centerHorizontally);
                pos.X = rectH.Center.X - (Width * 0.5f);
            }
            else
            {
                var rect = this.GetRenderArea();
                pos.X = rect.X;
            }

            if (centerVertically != TextCenteringTargets.None)
            {
                var rectV = this.GetCenteringArea(centerVertically);
                pos.Y = rectV.Center.Y - (Height * 0.5f);
            }
            else
            {
                var rect = this.GetRenderArea();
                pos.Y = rect.Y;
            }

            // Calculate new transforms
            this.Manipulator.SetScale(sca);
            this.Manipulator.SetRotation(rot);
            this.Manipulator.SetPosition(pos);
            this.Manipulator.Update(parentCenter, parentScale);

            this.ShadowManipulator.SetScale(sca);
            this.ShadowManipulator.SetRotation(rot);
            this.ShadowManipulator.SetPosition(pos + this.ShadowDelta);
            this.ShadowManipulator.Update(parentCenter, parentScale);
        }

        /// <inheritdoc/>
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

            if (fontMap == null)
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
                    this.DrawText(effect, technique, this.ShadowManipulator.LocalTransform, this.ShadowColor);
                }

                //Draw text
                this.DrawText(effect, technique, this.Manipulator.LocalTransform, this.TextColor);
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
        /// Centers vertically the text
        /// </summary>
        /// <param name="target">Center target</param>
        public void CenterVertically(TextCenteringTargets target)
        {
            this.centerVertically = target;
        }
        /// <summary>
        /// Centers horinzontally the text
        /// </summary>
        /// <param name="target">Center target</param>
        public void CenterHorizontally(TextCenteringTargets target)
        {
            this.centerHorizontally = target;
        }
        /// <summary>
        /// Centers the text into the screen
        /// </summary>
        public void CenterScreen()
        {
            this.textArea = null;
            this.centerHorizontally = TextCenteringTargets.Screen;
            this.centerVertically = TextCenteringTargets.Screen;

            this.MapText();
        }
        /// <summary>
        /// Centers the text into the parent rectangle
        /// </summary>
        public void CenterParent()
        {
            this.textArea = null;
            this.centerHorizontally = TextCenteringTargets.Parent;
            this.centerVertically = TextCenteringTargets.Parent;

            this.MapText();
        }
        /// <summary>
        /// Centers the text into the rectangle
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        public void CenterRectangle(RectangleF rectangle)
        {
            this.textArea = rectangle;
            this.centerHorizontally = TextCenteringTargets.Area;
            this.centerVertically = TextCenteringTargets.Area;

            this.MapText();
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

            var rect = this.GetRenderArea();

            this.fontMap.MapSentence(
                this.text,
                rect,
                out this.vertices, out this.indices, out Vector2 size);

            this.updateBuffers = true;

            // Adjust text bounds
            this.Width = size.X;
            this.Height = size.Y;
        }
        /// <summary>
        /// Gets the text render area
        /// </summary>
        /// <returns>Returns the text render area</returns>
        private RectangleF GetRenderArea()
        {
            return this.Parent?.GetRenderArea() ?? textArea ?? this.Game.Form.RenderRectangle;
        }
        /// <summary>
        /// Gets the area used for text centering calculation
        /// </summary>
        /// <param name="target">Center target</param>
        /// <returns>Returns the text centering area</returns>
        private RectangleF GetCenteringArea(TextCenteringTargets target)
        {
            if (target == TextCenteringTargets.Parent)
            {
                return this.Parent?.GetRenderArea() ?? this.textArea ?? this.Game.Form.RenderRectangle;
            }
            else if (target == TextCenteringTargets.Area)
            {
                return this.textArea ?? this.Game.Form.RenderRectangle;
            }
            else
            {
                return this.Game.Form.RenderRectangle;
            }
        }
    }

    /// <summary>
    /// Text centering targets
    /// </summary>
    public enum TextCenteringTargets
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Parent
        /// </summary>
        Parent = 1,
        /// <summary>
        /// Screen
        /// </summary>
        Screen = 2,
        /// <summary>
        /// Area
        /// </summary>
        Area = 3,
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
