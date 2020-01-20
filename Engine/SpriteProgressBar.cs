using SharpDX;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sprite progress bar
    /// </summary>
    public class SpriteProgressBar : Drawable, IScreenFitted
    {
        /// <summary>
        /// Left sprite
        /// </summary>
        private Sprite left = null;
        /// <summary>
        /// Right sprite
        /// </summary>
        private Sprite right = null;
        /// <summary>
        /// Button text drawer
        /// </summary>
        private TextDrawer textDrawer = null;

        /// <summary>
        /// Left scale
        /// </summary>
        protected float LeftScale { get { return this.ProgressValue; } }
        /// <summary>
        /// Right scale
        /// </summary>
        protected float RightScale { get { return 1f - this.ProgressValue; } }

        /// <summary>
        /// Gets or sets the progress valur
        /// </summary>
        public float ProgressValue { get; set; }
        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// Button width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Button height
        /// </summary>
        public int Height { get; set; }
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
        /// Gets bounding rectangle of button
        /// </summary>
        /// <remarks>Bounding rectangle without text</remarks>
        public Rectangle Rectangle { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Button description</param>
        public SpriteProgressBar(Scene scene, SpriteProgressBarDescription description)
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
                    FitScreen = false,
                });

            this.right = new Sprite(
                scene,
                new SpriteDescription()
                {
                    Color = description.BaseColor,
                    Width = description.Width,
                    Height = description.Height,
                    FitScreen = false,
                });

            if (description.TextDescription != null)
            {
                this.textDrawer = new TextDrawer(
                    scene,
                    description.TextDescription);
            }

            this.Left = description.Left;
            this.Top = description.Top;
            this.Width = description.Width;
            this.Height = description.Height;
            this.Text = description.Text;
            this.Rectangle = new Rectangle(description.Left, description.Top, description.Width, description.Height);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SpriteProgressBar()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
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
                    left = null;
                }
                if (right != null)
                {
                    right.Dispose();
                    right = null;
                }
                if (textDrawer != null)
                {
                    textDrawer.Dispose();
                    textDrawer = null;
                }
            }
        }
        /// <summary>
        /// Updates state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.left.Manipulator.SetScale(LeftScale, 1f);
            this.right.Manipulator.SetScale(RightScale, 1f);

            this.left.Left = this.Left;
            this.left.Top = this.Top;

            this.right.Left = this.Left + (int)(this.left.Width * LeftScale);
            this.right.Top = this.Top;

            if (!string.IsNullOrEmpty(this.Text))
            {
                //Center text
                float leftmove = ((float)this.Width * 0.5f) - ((float)this.textDrawer.Width * 0.5f);
                float topmove = ((float)this.Height * 0.5f) - ((float)this.textDrawer.Height * 0.5f);

                this.textDrawer.Left = this.Left + (int)leftmove;
                this.textDrawer.Top = this.Top + (int)topmove;

                this.textDrawer.Update(context);
            }

            this.left.Update(context);
            this.right.Update(context);
        }
        /// <summary>
        /// Draws button
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
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
                this.textDrawer.Draw(context);
            }
        }
        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            if (this.left != null) this.left.Resize();
            if (this.right != null) this.right.Resize();
            if (this.textDrawer != null) this.textDrawer.Resize();
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
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<SpriteProgressBar> AddComponentSpriteProgressBar(this Scene scene, SpriteProgressBarDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            SpriteProgressBar component = null;

            await Task.Run(() =>
            {
                component = new SpriteProgressBar(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
