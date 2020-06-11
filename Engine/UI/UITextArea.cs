using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;
    using System;

    public class UITextArea : UIControl
    {
        /// <summary>
        /// Button text drawer
        /// </summary>
        public readonly TextDrawer textDrawer = null;

        /// <inheritdoc/>
        public override float Width
        {
            get
            {
                return Math.Max(base.Width, this.textDrawer?.Width ?? 0);
            }
            set
            {
                base.Width = value;
            }
        }
        /// <inheritdoc/>
        public override float Height
        {
            get
            {
                return Math.Max(base.Height, this.textDrawer?.Height ?? 0);
            }
            set
            {
                base.Height = value;
            }
        }
        /// <summary>
        /// Gets or sets the button text
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
                }
            }
        }
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

            if (description.TextDescription != null)
            {
                description.TextDescription.Name = description.TextDescription.Name ?? $"{description.Name}.TextArea";

                this.textDrawer = new TextDrawer(scene, description.TextDescription)
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

            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                this.textDrawer?.Update(context);
            }
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            base.Draw(context);

            if (!this.Visible)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                this.textDrawer?.Draw(context);
            }
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
            return new RectangleF(
                AbsoluteLeft + MarginLeft,
                AbsoluteTop + MarginTop,
                AbsoluteWidth - (MarginLeft + MarginRight),
                AbsoluteHeight - (MarginTop + MarginBottom));
        }

        /// <inheritdoc/>
        public override void CenterHorizontally(CenterTargets target)
        {
            base.CenterHorizontally(target);

            this.textDrawer.CenterHorizontally((TextCenteringTargets)target);
        }
        /// <inheritdoc/>
        public override void CenterVertically(CenterTargets target)
        {
            base.CenterVertically(target);

            this.textDrawer.CenterVertically((TextCenteringTargets)target);
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
}
