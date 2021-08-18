using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Text area
    /// </summary>
    public class UITextArea : UIControl, IScrollable
    {
        /// <summary>
        /// Button text drawer
        /// </summary>
        private readonly TextDrawer textDrawer = null;
        /// <summary>
        /// Vertical scroll bar
        /// </summary>
        private readonly UIScrollBar sbVertical = null;
        /// <summary>
        /// Horizontal scroll bar
        /// </summary>
        private readonly UIScrollBar sbHorizontal = null;
        /// <summary>
        /// Grow control with text value
        /// </summary>
        private bool growControlWithText = false;
        /// <summary>
        /// Vertical scroll offset (in pixels)
        /// </summary>
        private float verticalScrollOffset = 0;
        /// <summary>
        /// Vertical scroll position (from 0 to 1)
        /// </summary>
        private float verticalScrollPosition = 0;
        /// <summary>
        /// Horizontal scroll offset (in pixels)
        /// </summary>
        private float horizontalScrollOffset = 0;
        /// <summary>
        /// Horizontal scroll position (from 0 to 1)
        /// </summary>
        private float horizontalScrollPosition = 0;

        /// <summary>
        /// Gets or sets the control text
        /// </summary>
        public string Text
        {
            get
            {
                return textDrawer.Text;
            }
            set
            {
                if (!string.Equals(value, textDrawer.Text))
                {
                    textDrawer.Text = value;

                    GrowControl();
                }
            }
        }
        /// <summary>
        /// Gets the parsed text value
        /// </summary>
        public string ParsedText
        {
            get
            {
                return textDrawer.ParsedText;
            }
        }
        /// <summary>
        /// Gets or sets the text color
        /// </summary>
        public Color4 TextForeColor
        {
            get
            {
                return textDrawer.ForeColor;
            }
            set
            {
                textDrawer.ForeColor = value;
            }
        }
        /// <summary>
        /// Gets or sets the shadow color
        /// </summary>
        public Color4 TextShadowColor
        {
            get
            {
                return textDrawer.ShadowColor;
            }
            set
            {
                textDrawer.ShadowColor = value;
            }
        }
        /// <summary>
        /// Shadow position delta
        /// </summary>
        public Vector2 TextShadowDelta
        {
            get
            {
                return textDrawer.ShadowDelta;
            }
            set
            {
                textDrawer.ShadowDelta = value;
            }
        }
        /// <summary>
        /// Gets or sets the text horizontal align
        /// </summary>
        public HorizontalTextAlign TextHorizontalAlign
        {
            get
            {
                return textDrawer.HorizontalAlign;
            }
            set
            {
                textDrawer.HorizontalAlign = value;
            }
        }
        /// <summary>
        /// Gets or sets the text vertical align
        /// </summary>
        public VerticalTextAlign TextVerticalAlign
        {
            get
            {
                return textDrawer.VerticalAlign;
            }
            set
            {
                textDrawer.VerticalAlign = value;
            }
        }
        /// <summary>
        /// Returns the text line height
        /// </summary>
        public float TextLineHeight
        {
            get
            {
                return textDrawer.GetLineHeight();
            }
        }
        /// <summary>
        /// Gets or sets whether the area must grow or shrinks with the text value
        /// </summary>
        public bool GrowControlWithText
        {
            get
            {
                return growControlWithText;
            }
            set
            {
                if (growControlWithText != value)
                {
                    growControlWithText = value;

                    UpdateInternals = true;
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

                textDrawer.Alpha = value;
            }
        }
        /// <inheritdoc/>
        public ScrollModes Scroll { get; set; }
        /// <inheritdoc/>
        public float ScrollbarSize { get; set; }
        /// <inheritdoc/>
        public float VerticalScrollOffset
        {
            get
            {
                return verticalScrollOffset;
            }
            set
            {
                verticalScrollOffset = MathUtil.Clamp(value, 0f, this.GetMaximumVerticalOffset());
                verticalScrollPosition = this.ConvertVerticalOffsetToPosition(verticalScrollOffset);
            }
        }
        /// <inheritdoc/>
        public float HorizontalScrollOffset
        {
            get
            {
                return horizontalScrollOffset;
            }
            set
            {
                horizontalScrollOffset = MathUtil.Clamp(value, 0f, this.GetMaximumHorizontalOffset());
                horizontalScrollPosition = this.ConvertHorizontalOffsetToPosition(horizontalScrollOffset);
            }
        }
        /// <inheritdoc/>
        public float VerticalScrollPosition
        {
            get
            {
                return verticalScrollPosition;
            }
            set
            {
                verticalScrollPosition = MathUtil.Clamp(value, 0f, 1f);
                verticalScrollOffset = this.ConvertVerticalPositionToOffset(verticalScrollPosition);
            }
        }
        /// <inheritdoc/>
        public float HorizontalScrollPosition
        {
            get
            {
                return horizontalScrollPosition;
            }
            set
            {
                horizontalScrollPosition = MathUtil.Clamp(value, 0f, 1f);
                horizontalScrollOffset = this.ConvertHorizontalPositionToOffset(horizontalScrollPosition);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UITextArea(string id, string name, Scene scene, UITextAreaDescription description) :
            base(id, name, scene, description)
        {
            growControlWithText = description.GrowControlWithText;
            Scroll = description.Scroll;
            ScrollbarSize = description.ScrollbarSize;

            textDrawer = new TextDrawer(
                $"{id}.TextDrawer",
                $"{name}.TextDrawer",
                scene,
                this,
                description.Font)
            {
                Text = description.Text,
                ForeColor = description.TextForeColor,
                ShadowColor = description.TextShadowColor,
                ShadowDelta = description.TextShadowDelta,
                HorizontalAlign = description.TextHorizontalAlign,
                VerticalAlign = description.TextVerticalAlign,
            };

            if (Scroll.HasFlag(ScrollModes.Vertical))
            {
                sbVertical = AddScroll(id, name, scene, ScrollModes.Vertical);
            }

            if (Scroll.HasFlag(ScrollModes.Horizontal))
            {
                sbHorizontal = AddScroll(id, name, scene, ScrollModes.Horizontal);
            }

            GrowControl();
        }
        /// <summary>
        /// Adds a scroll controller to the control
        /// </summary>
        /// <param name="scroll">Scroll mode</param>
        private UIScrollBar AddScroll(string id, string name, Scene scene, ScrollModes scroll)
        {
            var sb = new UIScrollBar($"{id}.Scroll.{scroll}", $"{name}.BackPanel", scene, UIScrollBarDescription.Default(scroll))
            {
                EventsEnabled = true
            };

            sb.MouseOver += ScrollBarMouseOver;

            AddChild(sb, false);

            return sb;
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                textDrawer.Dispose();
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

            textDrawer.Update(context);

            float size = ScrollbarSize;
            var renderArea = GetRenderArea(false);

            if (sbVertical != null)
            {
                sbVertical.Left = renderArea.Width - size;
                sbVertical.Top = 0;
                sbVertical.Width = size;
                sbVertical.Height = renderArea.Height - size;
                sbVertical.MarkerPosition = VerticalScrollPosition;
            }

            if (sbHorizontal != null)
            {
                sbHorizontal.Left = 0;
                sbHorizontal.Top = renderArea.Height - size;
                sbHorizontal.Width = renderArea.Width - size;
                sbHorizontal.Height = size;
                sbHorizontal.MarkerPosition = HorizontalScrollPosition;
            }
        }

        /// <inheritdoc/>
        protected override void UpdateInternalState()
        {
            base.UpdateInternalState();

            textDrawer.UpdateInternals();
        }
        /// <inheritdoc/>
        public override Vector2? GetTransformationPivot()
        {
            //The internal text drawer is always attached to the text area parent

            if (Parent?.IsRoot == true)
            {
                //If the text area parent is the root, use the text area itself as pivot control
                return base.GetTransformationPivot();
            }

            //Otherwise, use the parent as pivot, if any
            return Parent?.GetTransformationPivot() ?? base.GetTransformationPivot();
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            base.Draw(context);

            if (!Visible)
            {
                return;
            }

            textDrawer.Draw(context);
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            base.Resize();

            textDrawer.Resize();
        }

        /// <inheritdoc/>
        public override RectangleF GetRenderArea(bool applyPadding)
        {
            var absRect = base.GetRenderArea(false);

            //If adjust area with text is enabled, or the drawing area is zero, set area from current top-left position to screen right-bottom position
            if ((GrowControlWithText && !HasParent) || absRect.Width == 0) absRect.Width = Game.Form.RenderWidth - absRect.Left;
            if ((GrowControlWithText && !HasParent) || absRect.Height == 0) absRect.Height = Game.Form.RenderHeight - absRect.Top;

            return applyPadding ? Padding.Apply(absRect) : absRect;
        }
        /// <inheritdoc/>
        public override RectangleF GetControlArea()
        {
            var size = textDrawer.TextSize;

            return new RectangleF
            {
                X = Left,
                Y = Top,
                Width = size.X,
                Height = size.Y,
            };
        }

        /// <inheritdoc/>
        public void ScrollUp(float amount)
        {
            VerticalScrollOffset -= amount * Game.GameTime.ElapsedSeconds;
            VerticalScrollOffset = Math.Max(0, VerticalScrollOffset);
        }
        /// <inheritdoc/>
        public void ScrollDown(float amount)
        {
            float maxOffset = this.GetMaximumVerticalOffset();

            VerticalScrollOffset += amount * Game.GameTime.ElapsedSeconds;
            VerticalScrollOffset = Math.Min(maxOffset, VerticalScrollOffset);
        }
        /// <inheritdoc/>
        public void ScrollLeft(float amount)
        {
            float maxOffset = this.GetMaximumHorizontalOffset();

            HorizontalScrollOffset += amount * Game.GameTime.ElapsedSeconds;
            HorizontalScrollOffset = Math.Min(maxOffset, HorizontalScrollOffset);
        }
        /// <inheritdoc/>
        public void ScrollRight(float amount)
        {
            HorizontalScrollOffset -= amount * Game.GameTime.ElapsedSeconds;
            HorizontalScrollOffset = Math.Max(0, HorizontalScrollOffset);
        }
        /// <summary>
        /// Scroll bar mouse over events
        /// </summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">Event arguments</param>
        private void ScrollBarMouseOver(UIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            if (sender == sbVertical)
            {
                VerticalScrollPosition = (e.PointerPosition.Y - sbVertical.AbsoluteTop) / sbVertical.Height;
            }

            if (sender == sbHorizontal)
            {
                HorizontalScrollPosition = (e.PointerPosition.X - sbHorizontal.AbsoluteLeft) / sbHorizontal.Width;
            }
        }

        /// <summary>
        /// Grows the control using the current text
        /// </summary>
        private void GrowControl()
        {
            var size = textDrawer.MeasureText(Text, TextHorizontalAlign, TextVerticalAlign);
            var minHeight = textDrawer.GetLineHeight();

            //Set sizes if grow control with text or sizes not setted
            if (GrowControlWithText || Width == 0) Width = size.X;
            if (GrowControlWithText || Height == 0) Height = size.Y == 0 ? minHeight : size.Y;
        }
    }

    /// <summary>
    /// UITextArea extensions
    /// </summary>
    public static class UITextAreaExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UITextArea> AddComponentUITextArea(this Scene scene, string id, string name, UITextAreaDescription description, int layer = Scene.LayerUI)
        {
            UITextArea component = null;

            await Task.Run(() =>
            {
                component = new UITextArea(id, name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
