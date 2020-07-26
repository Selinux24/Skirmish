using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Text area
    /// </summary>
    public class UITextArea : UIControl
    {
        /// <summary>
        /// Button text drawer
        /// </summary>
        private readonly TextDrawer textDrawer = null;

        /// <summary>
        /// Gets or sets the control text
        /// </summary>
        public string Text
        {
            get
            {
                return this.textDrawer?.Text;
            }
            set
            {
                if (this.textDrawer != null)
                {
                    this.textDrawer.Text = value;

                    float maxWidth = this.Width <= 0 ? float.PositiveInfinity : this.Width;

                    var size = this.textDrawer.MeasureText(value, maxWidth);

                    //Set initial sizes
                    if (this.Width == 0) this.Width = size.X;
                    if (this.Height == 0) this.Height = size.Y;

                    if (!this.AdjustAreaWithText)
                    {
                        return;
                    }

                    //Grow area
                    this.Width = size.X;
                    this.Height = size.Y;
                }
            }
        }
        /// <summary>
        /// Gets or sets the text color
        /// </summary>
        public Color4 TextColor
        {
            get
            {
                return textDrawer?.TextColor ?? new Color4(0, 0, 0, 0);
            }
            set
            {
                if (textDrawer != null)
                {
                    textDrawer.TextColor = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the shadow color
        /// </summary>
        public Color4 TextShadowColor
        {
            get
            {
                return textDrawer?.ShadowColor ?? new Color4(0, 0, 0, 0);
            }
            set
            {
                if (textDrawer != null)
                {
                    textDrawer.ShadowColor = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the text horizontal align
        /// </summary>
        public TextAlign HorizontalAlign
        {
            get
            {
                return this.textDrawer?.HorizontalAlign ?? TextAlign.Left;
            }
            set
            {
                if (this.textDrawer != null)
                {
                    this.textDrawer.HorizontalAlign = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the text vertical align
        /// </summary>
        public VerticalAlign VerticalAlign
        {
            get
            {
                return this.textDrawer?.VerticalAlign ?? VerticalAlign.Top;
            }
            set
            {
                if (this.textDrawer != null)
                {
                    this.textDrawer.VerticalAlign = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets whether the area must grow or shrinks with the text value
        /// </summary>
        public bool AdjustAreaWithText { get; set; } = true;

        /// <summary>
        /// Gest or sets the left margin
        /// </summary>
        public float MarginLeft { get; set; }
        /// <summary>
        /// Gest or sets the top margin
        /// </summary>
        public float MarginTop { get; set; }
        /// <summary>
        /// Gest or sets the right margin
        /// </summary>
        public float MarginRight { get; set; }
        /// <summary>
        /// Gest or sets the bottom margin
        /// </summary>
        public float MarginBottom { get; set; }
        /// <inheritdoc/>
        public override float Alpha
        {
            get
            {
                return base.Alpha;
            }
            set
            {
                base.Alpha = value;

                this.textDrawer.Alpha = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UITextArea(Scene scene, UITextAreaDescription description) : base(scene, description)
        {
            this.MarginLeft = description.MarginLeft;
            this.MarginTop = description.MarginTop;
            this.MarginRight = description.MarginRight;
            this.MarginBottom = description.MarginBottom;
            this.AdjustAreaWithText = description.Width == 0 && description.Height == 0;

            if (description.Font != null)
            {
                description.Font.Name = description.Font.Name ?? $"{description.Name}.TextArea";

                this.textDrawer = new TextDrawer(scene, description.Font)
                {
                    Parent = this,
                };

                this.Text = description.Text;
            }
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.textDrawer?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!this.Active)
            {
                return;
            }

            this.textDrawer.Update(context);
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            base.Draw(context);

            if (!this.Visible)
            {
                return;
            }

            this.textDrawer?.Draw(context);
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            base.Resize();

            this.textDrawer?.Resize();
        }

        /// <inheritdoc/>
        public override RectangleF GetRenderArea()
        {
            float width = AbsoluteWidth == 0 ? this.Game.Form.RenderWidth : AbsoluteWidth;
            float height = AbsoluteHeight == 0 ? this.Game.Form.RenderHeight : AbsoluteHeight;

            return new RectangleF(
                AbsoluteLeft + MarginLeft,
                AbsoluteTop + MarginTop,
                width - (MarginLeft + MarginRight),
                height - (MarginTop + MarginBottom));
        }

        /// <summary>
        /// Sets the global margin
        /// </summary>
        /// <param name="margin">Margin value</param>
        public void SetMargin(float margin)
        {
            MarginLeft = MarginRight = MarginTop = MarginBottom = margin;
        }
        /// <summary>
        /// Sets the horizontal margin
        /// </summary>
        /// <param name="margin">Margin value</param>
        public void SetMarginHorizontal(float margin)
        {
            MarginLeft = MarginRight = margin;
        }
        /// <summary>
        /// Sets the vertical margin
        /// </summary>
        /// <param name="margin">Margin value</param>
        public void SetMarginVertical(float margin)
        {
            MarginTop = MarginBottom = margin;
        }
    }

    /// <summary>
    /// Sprite button extensions
    /// </summary>
    public static class UITextAreaExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UITextArea> AddComponentUITextArea(this Scene scene, UITextAreaDescription description, int order = 0)
        {
            UITextArea component = null;

            await Task.Run(() =>
            {
                component = new UITextArea(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }

    /// <summary>
    /// Text align
    /// </summary>
    public enum TextAlign
    {
        /// <summary>
        /// Align left
        /// </summary>
        Left,
        /// <summary>
        /// Align center
        /// </summary>
        Center,
        /// <summary>
        /// Align right
        /// </summary>
        Right,
    }

    /// <summary>
    /// Vertical align
    /// </summary>
    public enum VerticalAlign
    {
        /// <summary>
        /// Align top
        /// </summary>
        Top,
        /// <summary>
        /// Align middle
        /// </summary>
        Middle,
        /// <summary>
        /// Align bottom
        /// </summary>
        Bottom,
    }
}
