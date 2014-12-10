using System;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    public class Drawable : IDisposable
    {
        public Game Game { get; private set; }
        public virtual Scene Scene { get; set; }
        public DeviceContext DeviceContext { get { return this.Game.Graphics.DeviceContext; } }
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
        public virtual void Update(GameTime gameTime)
        {

        }
        public virtual void Draw(GameTime gameTime)
        {

        }
        public virtual void Dispose()
        {

        }
        public override string ToString()
        {
            return string.Format("Type: {0}; Name: {1}; Order: {2}", this.GetType(), this.Name, this.Order);
        }
    }
}
