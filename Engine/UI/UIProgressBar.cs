using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

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
                    FitParent = false,
                });

            this.right = new Sprite(
                scene,
                new SpriteDescription()
                {
                    Color = description.BaseColor,
                    Width = description.Width,
                    Height = description.Height,
                    FitParent = false,
                });

            if (description.TextDescription != null)
            {
                description.TextDescription.Name = description.TextDescription.Name ?? $"{description.Name}.TextProgressBar";

                this.textDrawer = new TextDrawer(scene, description.TextDescription)
                {
                    Parent = this
                };
                this.textDrawer.CenterParent();

                this.Text = description.Text;
            }
        }
        /// <summary>
        /// Releases used resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (left != null)
                {
                    left.Dispose();
                }
                if (right != null)
                {
                    right.Dispose();
                }
                if (textDrawer != null)
                {
                    textDrawer.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Updates state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            this.left.Width = (int)(LeftScale * Width);
            this.left.Left = this.Left;
            this.left.Top = this.Top;
            this.left.Update(context);

            this.right.Width = (int)(RightScale * Width);
            this.right.Left = this.Left + this.left.Width;
            this.right.Top = this.Top;
            this.right.Update(context);

            if (!string.IsNullOrEmpty(this.Text))
            {
                //Center text
                this.textDrawer?.CenterParent();
            }

            this.textDrawer?.Update(context);
        }

        /// <summary>
        /// Draws button
        /// </summary>
        /// <param name="context">Context</param>
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

            if (!string.IsNullOrEmpty(this.Text))
            {
                this.textDrawer?.Draw(context);
            }
        }

        /// <summary>
        /// Resize
        /// </summary>
        public override void Resize()
        {
            base.Resize();

            this.left?.Resize();
            this.right?.Resize();
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
