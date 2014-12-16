using System;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    public abstract class Drawable : IDisposable
    {
        public virtual Game Game { get; private set; }
        public virtual Scene Scene { get; protected set; }
        public virtual Device Device { get { return this.Game.Graphics.Device; } }
        public virtual DeviceContext DeviceContext { get { return this.Game.Graphics.DeviceContext; } }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public bool Active { get; set; }
        public int Order { get; set; }

        public Drawable(Game game, Scene scene)
        {
            this.Game = game;
            this.Scene = scene;
            this.Active = true;
            this.Visible = true;
            this.Order = 0;
        }
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
        public abstract void Dispose();
        public abstract void HandleResizing();
        public override string ToString()
        {
            return string.Format("Type: {0}; Name: {1}; Order: {2}", this.GetType(), this.Name, this.Order);
        }
    }
}
