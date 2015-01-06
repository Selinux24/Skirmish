using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Windows;

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
        /// Show mouse
        /// </summary>
        public bool ShowMouse { get; set; }
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
        /// Is form active
        /// </summary>
        public bool Active { get; private set; }

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
            this.ShowMouse = !fullScreen;

            if (fullScreen)
            {
                this.Size = new System.Drawing.Size(screenWidth, screenHeight);
            }
            else
            {
                this.ClientSize = new System.Drawing.Size(screenWidth, screenHeight);
            }

            this.Active = true;

            this.initialized = true;
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
                if (this.IsFullscreen)
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
        /// <summary>
        /// Mouse enter override
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            this.Active = this.ShowMouse ? this.Focused : (EngineForm.ActiveForm == this);
        }
        /// <summary>
        /// Mouse leave override
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            this.Active = this.ShowMouse ? this.Focused : false;
        }
    }
}
