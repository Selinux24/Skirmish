using SharpDX;
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
        /// Progress sprite
        /// </summary>
        private readonly Sprite spriteProgress = null;
        /// <summary>
        /// Base sprite
        /// </summary>
        private readonly Sprite spriteBase = null;
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
                return this.spriteProgress.Color;
            }
            set
            {
                this.spriteProgress.Color = value;
            }
        }
        /// <summary>
        /// Gets or sets the back color
        /// </summary>
        public Color4 BaseColor
        {
            get
            {
                return this.spriteBase.Color;
            }
            set
            {
                this.spriteBase.Color = value;
            }
        }
        /// <summary>
        /// Gets or sets the text color
        /// </summary>
        public Color4 TextColor
        {
            get
            {
                return this.textDrawer?.TextColor ?? new Color4(0f, 0f, 0f, 0f);
            }
            set
            {
                if (textDrawer != null)
                {
                    this.textDrawer.TextColor = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the text shadow color
        /// </summary>
        public Color4 ShadowColor
        {
            get
            {
                return this.textDrawer?.ShadowColor ?? new Color4(0f, 0f, 0f, 0f);
            }
            set
            {
                if (textDrawer != null)
                {
                    this.textDrawer.ShadowColor = value;
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

            this.spriteBase = new Sprite(
                scene,
                new SpriteDescription()
                {
                    Name = $"{description.Name}.SpriteBase",
                    Color = description.BaseColor,
                    Width = description.Width,
                    Height = description.Height,
                    EventsEnabled = false,
                });

            this.AddChild(spriteBase, false);

            this.spriteProgress = new Sprite(
                scene,
                new SpriteDescription()
                {
                    Name = $"{description.Name}.SpriteProgress",
                    Color = description.ProgressColor,
                    Width = description.Width,
                    Height = description.Height,
                    EventsEnabled = false,
                });

            this.AddChild(spriteProgress, false);

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

            int width = (int)(LeftScale * Width);

            this.spriteProgress.Width = width;
            this.spriteProgress.Height = Height;
            this.spriteProgress.Left = 0;
            this.spriteProgress.Top = 0;

            this.spriteBase.Width = Width - width;
            this.spriteBase.Height = Height;
            this.spriteBase.Left = width;
            this.spriteBase.Top = 0;

            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                this.textDrawer?.CenterParent();
                this.textDrawer?.Update(context);
            }

            base.Update(context);
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (this.RightScale > 0f)
            {
                this.spriteBase.Draw(context);
            }

            if (this.LeftScale > 0f)
            {
                this.spriteProgress.Draw(context);
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
