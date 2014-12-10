using System.IO;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine
{
    public abstract class Scene
    {
        private Matrix world = Matrix.Identity;
        private Matrix worldInverse = Matrix.Identity;

        protected Game Game { get; private set; }
        protected Device Device
        {
            get
            {
                return this.Game.Graphics.Device;
            }
        }
        protected DeviceContext DeviceContext
        {
            get
            {
                return this.Game.Graphics.DeviceContext;
            }
        }
        public bool Active { get; set; }
        public int Order { get; set; }
        public bool UseZBuffer { get; set; }
        public string ContentPath { get; set; }
        public Matrix World
        {
            get
            {
                return this.world;
            }
            private set
            {
                this.world = value;
                this.worldInverse = Matrix.Invert(value);
            }
        }
        public Matrix WorldInverse
        {
            get
            {
                return this.worldInverse;
            }
        }
        public Matrix ViewProjectionPerspective { get; private set; }
        public Matrix ViewProjectionOrthogonal { get; private set; }
        public Camera Camera { get; private set; }
        public SceneLight Lights { get; private set; }

        public Scene(Game game)
        {
            this.Game = game;

            this.UseZBuffer = true;
            this.ContentPath = "Resources";
            this.World = Matrix.Identity;
            this.Lights = new SceneLight();
        }
        public virtual void Initialize()
        {
            this.Camera = Camera.CreateFree(
                new Vector3(0.0f, 0.0f, -10.0f),
                Vector3.Zero);

            this.Camera.SetLens(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight);
        }
        public virtual void Update(GameTime gameTime)
        {
            this.Camera.Update();

            this.ViewProjectionPerspective = this.Camera.PerspectiveView * this.Camera.PerspectiveProjection;
            this.ViewProjectionOrthogonal = this.Camera.OrthoView * this.Camera.OrthoProjection;
        }
        public virtual void Draw(GameTime gameTime)
        {

        }
        public virtual void Dispose()
        {
            if (this.Camera != null)
            {
                this.Camera.Dispose();
                this.Camera = null;
            }
        }
    }
}
