using System;
using System.Drawing;
using SharpDX.Windows;

namespace Engine
{
    public class EngineForm : RenderForm
    {
        private bool initialized = false;

        public bool ShowMouse { get; set; }
        public int RenderWidth { get; private set; }
        public int RenderHeight { get; private set; }
        public Point RelativeCenter { get; private set; }
        public Point AbsoluteCenter { get; private set; }
        public bool Active { get; private set; }

        public EngineForm(string name, int screenWidth, int screenHeight, bool fullScreen)
            : base(name)
        {
            this.IsFullscreen = fullScreen;
            this.AllowUserResizing = !fullScreen;
            this.ShowMouse = !fullScreen;

            if (fullScreen)
            {
                this.Size = new Size(screenWidth, screenHeight);
            }
            else
            {
                this.ClientSize = new Size(screenWidth, screenHeight);
            }

            this.Active = true;

            this.initialized = true;
        }

        protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e)
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
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            this.Active = this.ShowMouse ? this.Focused : (EngineForm.ActiveForm == this);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            this.Active = this.ShowMouse ? this.Focused : false;
        }
    }
}
