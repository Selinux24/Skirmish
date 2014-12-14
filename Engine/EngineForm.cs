using System;
using System.Drawing;
using SharpDX.Windows;

namespace Engine
{
    public class EngineForm : RenderForm
    {
        private bool initialized = false;

        public bool VerticalSync { get; set; }
        public bool FullScreen { get; set; }
        public bool ShowMouse { get; set; }
        public int RenderWidth { get; private set; }
        public int RenderHeight { get; private set; }
        public Point ScreenCenter { get; private set; }
        public bool Active { get; private set; }

        public EngineForm(string name, int screenWidth, int screenHeight, bool vSync, bool fullScreen, bool showMouse = false)
            : base(name)
        {
            this.Size = new Size(screenWidth, screenHeight);
            this.VerticalSync = vSync;
            this.FullScreen = fullScreen;
            this.ShowMouse = showMouse;

            this.initialized = true;

            this.UpdateProperties();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            this.UpdateProperties();
        }
        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

            this.UpdateProperties();
        }
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            this.UpdateProperties();
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            this.Active = EngineForm.ActiveForm == this;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            this.Active = false;
        }

        private void UpdateProperties()
        {
            if (this.initialized)
            {
                if (this.FullScreen)
                {
                    this.RenderWidth = this.Width;
                    this.RenderHeight = this.Height;
                    this.ScreenCenter = new Point(this.Location.X + (this.Width / 2), this.Location.Y + (this.Height / 2));
                }
                else
                {
                    this.RenderWidth = this.ClientSize.Width;
                    this.RenderHeight = this.ClientSize.Height;
                    this.ScreenCenter = new Point(this.Location.X + (this.ClientSize.Width / 2), this.Location.Y + (this.ClientSize.Height / 2));
                }
            }
        }
    }
}
