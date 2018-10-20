using SharpDX;
using SharpDX.Windows;
using System.Windows.Forms;

namespace Engine
{
    using Engine.Properties;

    /// <summary>
    /// Engine render form
    /// </summary>
    public class EngineForm : RenderForm
    {
        /// <summary>
        /// Intialization internal flag
        /// </summary>
        private readonly bool initialized = false;

        /// <summary>
        /// Render width
        /// </summary>
        public int RenderWidth { get; private set; }
        /// <summary>
        /// Render height
        /// </summary>
        public int RenderHeight { get; private set; }
        /// <summary>
        /// Relative center
        /// </summary>
        public Point RelativeCenter { get; private set; }
        /// <summary>
        /// Absolute center
        /// </summary>
        public Point AbsoluteCenter { get; private set; }
        /// <summary>
        /// Gets or sets a value indicationg whether the current engine form is in fullscreen
        /// </summary>
        public new bool IsFullscreen
        {
            get
            {
                return base.IsFullscreen;
            }
            set
            {
                base.IsFullscreen = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Form name</param>
        /// <param name="screenWidth">Width</param>
        /// <param name="screenHeight">Height</param>
        /// <param name="fullScreen">Full screen</param>
        public EngineForm(string name, int screenWidth, int screenHeight, bool fullScreen)
            : base(name)
        {
            base.IsFullscreen = fullScreen;
            this.AllowUserResizing = !fullScreen;

            this.Size = new System.Drawing.Size(screenWidth, screenHeight);

            this.UpdateSizes(fullScreen);

            this.InitializeComponent();

            this.initialized = true;
        }
        /// <summary>
        /// Initialize component
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Icon = Resources.engine;
            this.Name = "EngineForm";
            this.Text = "Engine Form";

            this.ResumeLayout(false);
        }
        /// <summary>
        /// Invalidation override
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);

            if (this.initialized)
            {
                this.UpdateSizes(this.IsFullscreen);
            }
        }
        /// <summary>
        /// Update form sizes
        /// </summary>
        /// <param name="fullScreen">Indicates whether the form is windowed or full screen</param>
        private void UpdateSizes(bool fullScreen)
        {
            if (fullScreen)
            {
                this.RenderWidth = this.Size.Width;
                this.RenderHeight = this.Size.Height;
            }
            else
            {
                this.RenderWidth = this.ClientSize.Width;
                this.RenderHeight = this.ClientSize.Height;
            }

            this.RelativeCenter = new Point(this.RenderWidth / 2, this.RenderHeight / 2);
            this.AbsoluteCenter = new Point(this.Location.X + this.RelativeCenter.X, this.Location.Y + this.RelativeCenter.Y);
        }
    }
}
