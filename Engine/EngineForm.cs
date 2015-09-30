using Engine.Properties;
using SharpDX;
using SharpDX.Windows;
using System.Windows.Forms;

namespace Engine
{
    /// <summary>
    /// Engine render form
    /// </summary>
    public class EngineForm : RenderForm
    {
        /// <summary>
        /// Intialization internal flag
        /// </summary>
        private bool initialized = false;

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
        /// Constructor
        /// </summary>
        /// <param name="name">Form name</param>
        /// <param name="screenWidth">Width</param>
        /// <param name="screenHeight">Height</param>
        /// <param name="fullScreen">Full screen</param>
        public EngineForm(string name, int screenWidth, int screenHeight, bool fullScreen)
            : base(name)
        {
            this.IsFullscreen = fullScreen;
            this.AllowUserResizing = !fullScreen;

            if (fullScreen)
            {
                this.Size = new System.Drawing.Size(screenWidth, screenHeight);
            }
            else
            {
                this.ClientSize = new System.Drawing.Size(screenWidth, screenHeight);
            }

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
