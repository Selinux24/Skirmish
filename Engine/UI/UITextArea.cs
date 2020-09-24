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
        /// Line height
        /// </summary>
        private readonly float lineHeight = 0;

        /// <summary>
        /// Gets or sets the control text
        /// </summary>
        public string Text
        {
            get
            {
                return textDrawer?.Text;
            }
            set
            {
                if (textDrawer != null && !string.Equals(value, textDrawer.Text))
                {
                    textDrawer.Text = value;

                    GrowControl();
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
                if (textDrawer != null && textDrawer.TextColor != value)
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
                if (textDrawer != null && textDrawer.ShadowColor != value)
                {
                    textDrawer.ShadowColor = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the text horizontal align
        /// </summary>
        public HorizontalTextAlign HorizontalAlign
        {
            get
            {
                return textDrawer?.HorizontalAlign ?? HorizontalTextAlign.Left;
            }
            set
            {
                if (textDrawer != null && textDrawer.HorizontalAlign != value)
                {
                    textDrawer.HorizontalAlign = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the text vertical align
        /// </summary>
        public VerticalTextAlign VerticalAlign
        {
            get
            {
                return textDrawer?.VerticalAlign ?? VerticalTextAlign.Top;
            }
            set
            {
                if (textDrawer != null && textDrawer.VerticalAlign != value)
                {
                    textDrawer.VerticalAlign = value;
                }
            }
        }
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

                if (textDrawer != null && textDrawer.Alpha != value)
                {
                    textDrawer.Alpha = value;
                }
            }
        }

        /// <summary>
        /// Padding
        /// </summary>
        public Padding Padding { get; set; }
        /// <summary>
        /// Gets or sets whether the area must grow or shrinks with the text value
        /// </summary>
        public bool AdjustAreaWithText { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UITextArea(Scene scene, UITextAreaDescription description) : base(scene, description)
        {
            Padding = description.Padding;
            AdjustAreaWithText = description.AdjustAreaWithText;

            if (description.Font != null)
            {
                description.Font.Name = description.Font.Name ?? $"{description.Name}.TextArea";

                textDrawer = new TextDrawer(scene, description.Font)
                {
                    Parent = this,
                };

                Text = description.Text;

                lineHeight = textDrawer.GetLineHeight();

                GrowControl();
            }
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                textDrawer?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            textDrawer?.Update(context);
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            base.Draw(context);

            if (!Visible)
            {
                return;
            }

            textDrawer?.Draw(context);
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            base.Resize();

            textDrawer?.Resize();
        }

        /// <inheritdoc/>
        /// <remarks>Applies margin configuration if any</remarks>
        public override RectangleF GetRenderArea()
        {
            float left;
            float top;
            float width;
            float height;

            if (AdjustAreaWithText && !HasParent)
            {
                left = Left;
                top = Top;
                width = Game.Form.RenderWidth - left;
                height = Game.Form.RenderHeight - top;
            }
            else
            {
                left = AbsoluteLeft;
                top = AbsoluteTop;
                width = AbsoluteWidth == 0 ? Game.Form.RenderWidth - AbsoluteLeft : AbsoluteWidth;
                height = AbsoluteHeight == 0 ? Game.Form.RenderHeight - AbsoluteTop : AbsoluteHeight;
            }

            return new RectangleF(
                left + Padding.Left,
                top + Padding.Top,
                width - (Padding.Left + Padding.Right),
                height - (Padding.Top + Padding.Bottom));
        }

        /// <summary>
        /// Grows the control using the current text
        /// </summary>
        private void GrowControl()
        {
            var size = textDrawer.MeasureText(Text, GetRenderArea(), HorizontalAlign, VerticalAlign);

            //Set initial sizes
            if (Width == 0) Width = size.X;
            if (Height == 0) Height = size.Y == 0 ? lineHeight : size.Y;

            if (!AdjustAreaWithText)
            {
                return;
            }

            //Grow area
            Width = size.X;
            Height = size.Y == 0 ? lineHeight : size.Y;
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
}
