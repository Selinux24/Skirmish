using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Text area
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class UITextArea(Scene scene, string id, string name) : UIControl<UITextAreaDescription>(scene, id, name), IScrollable
    {
        /// <summary>
        /// Button text drawer
        /// </summary>
        private TextDrawer textDrawer = null;
        /// <summary>
        /// Vertical scroll bar
        /// </summary>
        private UIScrollBar sbVertical = null;
        /// <summary>
        /// Horizontal scroll bar
        /// </summary>
        private UIScrollBar sbHorizontal = null;
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
        /// Current scrolls bar picked control
        /// </summary>
        private IUIControl currentPickedControl = null;
        /// <summary>
        /// Alpha component
        /// </summary>
        private float alpha;

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
                if (textDrawer == null)
                {
                    return;
                }

                if (string.Equals(value, textDrawer.Text))
                {
                    return;
                }

                textDrawer.Text = value;

                GrowControl();
            }
        }
        /// <summary>
        /// Gets the parsed text value
        /// </summary>
        public string ParsedText
        {
            get
            {
                return textDrawer?.ParsedText;
            }
        }
        /// <summary>
        /// Gets or sets the text color
        /// </summary>
        public Color4 TextForeColor
        {
            get
            {
                return textDrawer?.ForeColor ?? Color.Transparent;
            }
            set
            {
                if (textDrawer == null)
                {
                    return;
                }

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
                return textDrawer?.ShadowColor ?? Color.Transparent;
            }
            set
            {
                if (textDrawer == null)
                {
                    return;
                }

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
                return textDrawer?.ShadowDelta ?? Vector2.Zero;
            }
            set
            {
                if (textDrawer == null)
                {
                    return;
                }

                textDrawer.ShadowDelta = value;
            }
        }
        /// <summary>
        /// Gets or sets the text horizontal align
        /// </summary>
        public TextHorizontalAlign TextHorizontalAlign
        {
            get
            {
                return textDrawer?.HorizontalAlign ?? TextHorizontalAlign.Left;
            }
            set
            {
                if (textDrawer == null)
                {
                    return;
                }

                textDrawer.HorizontalAlign = value;
            }
        }
        /// <summary>
        /// Gets or sets the text vertical align
        /// </summary>
        public TextVerticalAlign TextVerticalAlign
        {
            get
            {
                return textDrawer?.VerticalAlign ?? TextVerticalAlign.Top;
            }
            set
            {
                if (textDrawer == null)
                {
                    return;
                }

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
                return textDrawer?.GetLineHeight() ?? 0;
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
                return alpha;
            }
            set
            {
                alpha = value;

                base.Alpha = alpha;

                if (textDrawer != null)
                {
                    textDrawer.Alpha = alpha;
                }
            }
        }
        /// <inheritdoc/>
        public ScrollModes Scroll { get; set; }
        /// <inheritdoc/>
        public float ScrollbarSize { get; set; }
        /// <inheritdoc/>
        public ScrollVerticalAlign ScrollVerticalAlign { get; set; }
        /// <inheritdoc/>
        public float ScrollVerticalOffset
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
        public float ScrollVerticalPosition
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
        public ScrollHorizontalAlign ScrollHorizontalAlign { get; set; }
        /// <inheritdoc/>
        public float ScrollHorizontalOffset
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
        public float ScrollHorizontalPosition
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                textDrawer?.Dispose();
                textDrawer = null;
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(UITextAreaDescription description)
        {
            await base.ReadAssets(description);

            growControlWithText = Description.GrowControlWithText;
            Scroll = Description.Scroll;
            ScrollbarSize = Description.ScrollbarSize;
            ScrollVerticalAlign = Description.ScrollVerticalAlign;
            ScrollHorizontalAlign = Description.ScrollHorizontalAlign;

            textDrawer = await CreateTextDrawer();
            textDrawer.Parent = this;

            if (Scroll.HasFlag(ScrollModes.Vertical))
            {
                sbVertical = await CreateScrollBar(ScrollModes.Vertical);
                AddChild(sbVertical);
            }

            if (Scroll.HasFlag(ScrollModes.Horizontal))
            {
                sbHorizontal = await CreateScrollBar(ScrollModes.Horizontal);
                AddChild(sbHorizontal);
            }

            GrowControl();
        }
        private async Task<TextDrawer> CreateTextDrawer()
        {
            var td = await Scene.CreateComponent<TextDrawer, TextDrawerDescription>(
                $"{Id}.TextDrawer",
                $"{Name}.TextDrawer",
                Description.Font,
                Description.MaxTextLength);

            td.Text = Description.Text;
            td.ForeColor = Description.TextForeColor;
            td.ShadowColor = Description.TextShadowColor;
            td.ShadowDelta = Description.TextShadowDelta;
            td.HorizontalAlign = Description.TextHorizontalAlign;
            td.VerticalAlign = Description.TextVerticalAlign;

            return td;
        }
        private async Task<UIScrollBar> CreateScrollBar(ScrollModes scrollMode)
        {
            var desc = UIScrollBarDescription.Default(Description.ScrollbarBaseColor, Description.ScrollbarMarkerColor, Description.ScrollbarMarkerSize, scrollMode);
            desc.EventsEnabled = true;

            var sb = await Scene.CreateComponent<UIScrollBar, UIScrollBarDescription>(
                $"{Id}.Scroll.{desc.ScrollMode}",
                $"{Name}.Scroll.{desc.ScrollMode}",
                desc);

            sb.MouseJustPressed += PbJustPressed;
            sb.MouseJustReleased += PbJustReleased;

            return sb;
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

            UpdateScrollBarsLayout();
            UpdateScrollBarsState();
        }
        /// <summary>
        /// Updates the scroll bars layout
        /// </summary>
        private void UpdateScrollBarsLayout()
        {
            if (sbVertical == null && sbHorizontal == null)
            {
                return;
            }

            var barSize = ScrollbarSize;
            var verticalAlign = sbVertical != null ? ScrollVerticalAlign : ScrollVerticalAlign.None;
            var horizontalAlign = sbHorizontal != null ? ScrollHorizontalAlign : ScrollHorizontalAlign.None;

            if (sbVertical != null)
            {
                var rect = this.GetVerticalLayout(barSize, verticalAlign, horizontalAlign);

                sbVertical.SetRectangle(rect);
                sbVertical.MarkerPosition = ScrollVerticalPosition;
            }

            if (sbHorizontal != null)
            {
                var rect = this.GetHorizontalLayout(barSize, verticalAlign, horizontalAlign);

                sbHorizontal.SetRectangle(rect);
                sbHorizontal.MarkerPosition = ScrollHorizontalPosition;
            }
        }
        /// <summary>
        /// Updates the scroll bars state
        /// </summary>
        private void UpdateScrollBarsState()
        {
            if (currentPickedControl == null)
            {
                return;
            }

            if (currentPickedControl == sbVertical)
            {
                ScrollVerticalPosition = (Game.Input.MousePosition.Y - sbVertical.AbsoluteTop) / sbVertical.Height;
            }

            if (currentPickedControl == sbHorizontal)
            {
                ScrollHorizontalPosition = (Game.Input.MousePosition.X - sbHorizontal.AbsoluteLeft) / sbHorizontal.Width;
            }
        }

        /// <inheritdoc/>
        protected override void UpdateInternalState()
        {
            base.UpdateInternalState();

            textDrawer?.UpdateInternals();
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
        public override bool Draw(DrawContext context)
        {
            base.Draw(context);

            if (!Visible)
            {
                return false;
            }

            return textDrawer?.Draw(context) ?? false;
        }

        /// <inheritdoc/>
        public override void Invalidate()
        {
            GrowControl();

            base.Invalidate();
        }

        /// <inheritdoc/>
        public override RectangleF GetRenderArea(bool applyPadding)
        {
            var absRect = base.GetRenderArea(false);

            //If adjust area with text is enabled, or the drawing area is zero, set area from current top-left position to screen right-bottom position
            if ((GrowControlWithText && !HasParent) || MathUtil.IsZero(absRect.Width)) absRect.Width = Game.Form.RenderWidth - absRect.Left;
            if ((GrowControlWithText && !HasParent) || MathUtil.IsZero(absRect.Height)) absRect.Height = Game.Form.RenderHeight - absRect.Top;

            return applyPadding ? Padding.Apply(absRect) : absRect;
        }
        /// <inheritdoc/>
        public override RectangleF GetControlArea()
        {
            var size = textDrawer?.TextSize ?? Vector2.Zero;

            return new RectangleF
            {
                X = Left,
                Y = Top,
                Width = size.X,
                Height = size.Y,
            };
        }

        /// <summary>
        /// Sets the text value, drawing the first key character occurence with the keyColor, and the other characters with the text color
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="keyChar">Key character</param>
        /// <param name="textColor">Text color</param>
        /// <param name="keyColor">Key color</param>
        public void SetTextWithKeyChar(string text, char keyChar, Color textColor, Color keyColor)
        {
            string res;
            int keyIndex = text.IndexOf(keyChar);
            if (keyIndex < 0)
            {
                res = $"{textColor}{text}";
            }
            else
            {
                string pre = text[..keyIndex];
                string key = text[keyIndex].ToString();
                string suf = text[(keyIndex + 1)..];
                pre = pre.Length > 0 ? $"{textColor}{pre}" : pre;
                key = key.Length > 0 ? $"{keyColor}{key}" : key;
                suf = suf.Length > 0 ? $"{textColor}{suf}" : suf;
                res = pre + key + suf;
            }

            Text = res;
        }

        /// <inheritdoc/>
        public void ScrollUp(float amount)
        {
            ScrollVerticalOffset -= amount * Game.GameTime.ElapsedSeconds;
            ScrollVerticalOffset = MathF.Max(0f, ScrollVerticalOffset);
        }
        /// <inheritdoc/>
        public void ScrollDown(float amount)
        {
            float maxOffset = this.GetMaximumVerticalOffset();

            ScrollVerticalOffset += amount * Game.GameTime.ElapsedSeconds;
            ScrollVerticalOffset = MathF.Min(maxOffset, ScrollVerticalOffset);
        }
        /// <inheritdoc/>
        public void ScrollLeft(float amount)
        {
            float maxOffset = this.GetMaximumHorizontalOffset();

            ScrollHorizontalOffset += amount * Game.GameTime.ElapsedSeconds;
            ScrollHorizontalOffset = MathF.Min(maxOffset, ScrollHorizontalOffset);
        }
        /// <inheritdoc/>
        public void ScrollRight(float amount)
        {
            ScrollHorizontalOffset -= amount * Game.GameTime.ElapsedSeconds;
            ScrollHorizontalOffset = MathF.Max(0f, ScrollHorizontalOffset);
        }
        /// <summary>
        /// Scroll bar mouse just pressed event
        /// </summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">Event arguments</param>
        private void PbJustPressed(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            currentPickedControl = sender;
        }
        /// <summary>
        /// Scroll bar mouse just released event
        /// </summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">Event arguments</param>
        private void PbJustReleased(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            currentPickedControl = null;
        }

        /// <summary>
        /// Grows the control using the current text
        /// </summary>
        private void GrowControl()
        {
            if (HasParent && FitWithParent)
            {
                var renderArea = GetRenderArea(true);
                Width = renderArea.Width;
                Height = renderArea.Height;

                return;
            }

            var size = textDrawer?.MeasureText(Text, TextHorizontalAlign, TextVerticalAlign) ?? Vector2.Zero;
            var minHeight = textDrawer?.GetLineHeight() ?? 0;

            //Set sizes if grow control with text or sizes not setted
            if (GrowControlWithText || MathUtil.IsZero(Width)) Width = size.X;
            if (GrowControlWithText || MathUtil.IsZero(Height)) Height = MathUtil.IsZero(size.Y) ? minHeight : size.Y;
        }
    }
}
