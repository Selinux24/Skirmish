using SharpDX;
using RenderTargetView = SharpDX.Direct3D11.RenderTargetView;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Minimap
    /// </summary>
    public class Minimap : Drawable, IScreenFitted
    {
        /// <summary>
        /// Viewport to match the minimap texture size
        /// </summary>
        private readonly Viewport viewport;
        /// <summary>
        /// Surface to draw
        /// </summary>
        private SpriteTexture minimapBox;
        /// <summary>
        /// Minimap render target
        /// </summary>
        private RenderTargetView renderTarget;
        /// <summary>
        /// Minimap texture
        /// </summary>
        private ShaderResourceView renderTexture;
        /// <summary>
        /// Context to draw
        /// </summary>
        private Context drawContext;
        /// <summary>
        /// Minimap rendered area
        /// </summary>
        private BoundingBox minimapArea;

        /// <summary>
        /// Reference to the objects that we render in the minimap
        /// </summary>
        public Drawable[] Drawables;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Minimap description</param>
        public Minimap(Game game, MinimapDescription description)
            : base(game)
        {
            this.Drawables = description.Drawables;

            this.minimapArea = description.MinimapArea;

            this.minimapBox = new SpriteTexture(game, new SpriteTextureDescription()
            {
                Top = description.Top,
                Left = description.Left,
                Width = description.Width,
                Height = description.Height,
            });

            this.viewport = new Viewport(0, 0, description.Width, description.Height);

            using (var texture = this.Device.CreateRenderTargetTexture(description.Width, description.Height))
            {
                this.renderTarget = new RenderTargetView(this.Device, texture);
                this.renderTexture = new ShaderResourceView(this.Device, texture);
            }

            this.InitializeContext();
        }
        /// <summary>
        /// Initialize terrain context
        /// </summary>
        private void InitializeContext()
        {
            float x = this.minimapArea.Maximum.X - this.minimapArea.Minimum.X;
            float y = this.minimapArea.Maximum.Y - this.minimapArea.Minimum.Y;
            float z = this.minimapArea.Maximum.Z - this.minimapArea.Minimum.Z;

            float near = 0.1f;

            Vector3 eyePos = new Vector3(0, y + near, 0);
            Vector3 target = Vector3.Zero;
            Vector3 dir = Vector3.Normalize(target - eyePos);

            Matrix view = Matrix.LookAtLH(
                eyePos,
                target,
                Vector3.UnitZ);

            Matrix proj = Matrix.OrthoLH(
                x,
                z,
                near,
                y + near);

            this.drawContext = new Context()
            {
                DrawerMode = DrawerModesEnum.Forward,
                World = Matrix.Identity,
                ViewProjection = view * proj,
                EyePosition = eyePos,
                Lights = SceneLights.Default,
            };
        }
        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Update(GameTime gameTime, Context context)
        {

        }
        /// <summary>
        /// Draw objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Draw(GameTime gameTime, Context context)
        {
            if (this.Drawables != null && this.Drawables.Length > 0)
            {
                this.Game.Graphics.SetViewport(this.viewport);
                this.Game.Graphics.SetRenderTarget(this.renderTarget, true, Color.Silver, null, false);

                for (int i = 0; i < this.Drawables.Length; i++)
                {
                    this.Drawables[i].Draw(gameTime, this.drawContext);
                }

                this.Game.Graphics.SetDefaultViewport();
                this.Game.Graphics.SetDefaultRenderTarget(false);
            }

            this.minimapBox.Texture = this.renderTexture;
            this.minimapBox.Draw(gameTime, context);
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public override void Dispose()
        {
            if (this.renderTarget != null)
            {
                this.renderTarget.Dispose();
                this.renderTarget = null;
            }

            if (this.renderTexture != null)
            {
                this.renderTexture.Dispose();
                this.renderTexture = null;
            }

            if (this.minimapBox != null)
            {
                this.minimapBox.Dispose();
                this.minimapBox = null;
            }
        }
        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            this.minimapBox.Resize();
        }
    }

    /// <summary>
    /// Minimap description
    /// </summary>
    public class MinimapDescription
    {
        /// <summary>
        /// Top position
        /// </summary>
        public int Top;
        /// <summary>
        /// Left position
        /// </summary>
        public int Left;
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Terrain to draw
        /// </summary>
        public Drawable[] Drawables;
        /// <summary>
        /// Minimap render area
        /// </summary>
        public BoundingBox MinimapArea;
    }
}
