using SharpDX;
using SharpDX.Direct3D11;

namespace Engine
{
    /// <summary>
    /// Render scene
    /// </summary>
    public abstract class Scene
    {
        /// <summary>
        /// Default relative content path
        /// </summary>
        public const string DefaultContentPath = "Resources";

        /// <summary>
        /// Scene world matrix
        /// </summary>
        private Matrix world = Matrix.Identity;
        /// <summary>
        /// Scene inverse world matrix
        /// </summary>
        private Matrix worldInverse = Matrix.Identity;

        /// <summary>
        /// Game class
        /// </summary>
        protected Game Game { get; private set; }
        /// <summary>
        /// Graphics Device
        /// </summary>
        protected Device Device
        {
            get
            {
                return this.Game.Graphics.Device;
            }
        }
        /// <summary>
        /// Graphics Context
        /// </summary>
        protected DeviceContext DeviceContext
        {
            get
            {
                return this.Game.Graphics.DeviceContext;
            }
        }

        /// <summary>
        /// Indicates whether the current scene is active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Processing order
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Indicates whether the current scene uses the Z buffer if available
        /// </summary>
        public bool UseZBuffer { get; set; }
        /// <summary>
        /// Content path for scene resources
        /// </summary>
        /// <remarks>Default folder defined by constant DefaultContentPath</remarks>
        public string ContentPath { get; set; }
        /// <summary>
        /// Scene world matrix
        /// </summary>
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
        /// <summary>
        /// Scene inverse world matrix
        /// </summary>
        public Matrix WorldInverse
        {
            get
            {
                return this.worldInverse;
            }
        }
        /// <summary>
        /// Perspective projection matrix
        /// </summary>
        public Matrix ViewProjectionPerspective { get; private set; }
        /// <summary>
        /// Orthogonal projection matrix
        /// </summary>
        public Matrix ViewProjectionOrthogonal { get; private set; }
        /// <summary>
        /// Scene camera
        /// </summary>
        public Camera Camera { get; private set; }
        /// <summary>
        /// Scene lights
        /// </summary>
        public SceneLight Lights { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game)
        {
            this.Game = game;

            this.UseZBuffer = true;
            this.ContentPath = DefaultContentPath;
            this.World = Matrix.Identity;
            this.Lights = new SceneLight();
        }

        /// <summary>
        /// Initialize scene objects
        /// </summary>
        public virtual void Initialize()
        {
            this.Camera = Camera.CreateFree(
                new Vector3(0.0f, 0.0f, -10.0f),
                Vector3.Zero);

            this.Camera.SetLens(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight);
        }
        /// <summary>
        /// Update scene objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            this.Camera.Update();

            this.ViewProjectionPerspective = this.Camera.PerspectiveView * this.Camera.PerspectiveProjection;
            this.ViewProjectionOrthogonal = this.Camera.OrthoView * this.Camera.OrthoProjection;
        }
        /// <summary>
        /// Draw scene objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Draw(GameTime gameTime)
        {

        }
        /// <summary>
        /// Dispose scene objects
        /// </summary>
        public virtual void Dispose()
        {
            if (this.Camera != null)
            {
                this.Camera.Dispose();
                this.Camera = null;
            }
        }
        /// <summary>
        /// Handle window resize
        /// </summary>
        public virtual void HandleWindowResize()
        {
            this.Camera.SetLens(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight);
        }
    }
}
