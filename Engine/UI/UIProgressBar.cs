using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;
    using SharpDX;

    /// <summary>
    /// Sprite progress bar
    /// </summary>
    public class UIProgressBar : UIControl
    {
        /// <summary>
        /// Left sprite
        /// </summary>
        private readonly Sprite left = null;
        /// <summary>
        /// Right sprite
        /// </summary>
        private readonly Sprite right = null;
        /// <summary>
        /// Button text drawer
        /// </summary>
        private readonly TextDrawer textDrawer = null;

        /// <summary>
        /// Left scale
        /// </summary>
        protected float LeftScale { get { return this.ProgressValue; } }
        /// <summary>
        /// Right scale
        /// </summary>
        protected float RightScale { get { return 1f - this.ProgressValue; } }

        /// <summary>
        /// Gets or sets the progress value
        /// </summary>
        public float ProgressValue { get; set; }
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

                if (this.textDrawer != null)
                {
                    this.textDrawer.Alpha = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the progress color
        /// </summary>
        public Color4 ProgressColor
        {
            get
            {
                return this.left.Color;
            }
            set
            {
                this.left.Color = value;
            }
        }
        /// <summary>
        /// Gets or sets the back color
        /// </summary>
        public Color4 BackgroundColor
        {
            get
            {
                return this.right.Color;
            }
            set
            {
                this.right.Color = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Button description</param>
        public UIProgressBar(Scene scene, UIProgressBarDescription description)
            : base(scene, description)
        {
            this.ProgressValue = 0;

            this.left = new Sprite(
                scene,
                new SpriteDescription()
                {
                    Color = description.ProgressColor,
                    Width = description.Width,
                    Height = description.Height,
                    EventsEnabled = false,
                });

            this.right = new Sprite(
                scene,
                new SpriteDescription()
                {
                    Color = description.BaseColor,
                    Width = description.Width,
                    Height = description.Height,
                    EventsEnabled = false,
                });

            this.AddChild(left, false);
            this.AddChild(right, false);

            if (description.Font != null)
            {
                description.Font.Name = description.Font.Name ?? $"{description.Name}.TextProgressBar";

                this.textDrawer = new TextDrawer(scene, description.Font)
                {
                    Parent = this,
                };
                this.textDrawer.CenterParent();

                this.Text = description.Text;
            }
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                textDrawer?.Dispose();

                base.Dispose(disposing);
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            this.left.Width = (int)(LeftScale * Width);
            this.left.Left = 0;
            this.left.Top = 0;
            this.left.Update(context);

            this.right.Width = Width - this.left.Width;
            this.right.Left = this.left.Width;
            this.right.Top = 0;
            this.right.Update(context);

            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                this.textDrawer?.CenterParent();
                this.textDrawer?.Update(context);
            }
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (this.LeftScale > 0f)
            {
                this.left.Draw(context);
            }

            if (this.RightScale > 0f)
            {
                this.right.Draw(context);
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
    }

    /// <summary>
    /// Progress bar extensions
    /// </summary>
    public static class SpriteProgressBarExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIProgressBar> AddComponentUIProgressBar(this Scene scene, UIProgressBarDescription description, int order = 0)
        {
            UIProgressBar component = null;

            await Task.Run(() =>
            {
                component = new UIProgressBar(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
