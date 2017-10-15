using SharpDX;
using SharpDX.DXGI;

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
        private EngineRenderTargetView renderTarget;
        /// <summary>
        /// Minimap texture
        /// </summary>
        private EngineTexture renderTexture;
        /// <summary>
        /// Context to draw
        /// </summary>
        private DrawContext drawContext;
        /// <summary>
        /// Minimap rendered area
        /// </summary>
        private BoundingBox minimapArea;

        /// <summary>
        /// Reference to the objects that we render in the minimap
        /// </summary>
        public SceneObject[] Drawables;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Minimap description</param>
        public Minimap(Scene scene, MinimapDescription description)
            : base(scene, description)
        {
            this.Drawables = description.Drawables;

            this.minimapArea = description.MinimapArea;

            this.minimapBox = new SpriteTexture(scene, new SpriteTextureDescription()
            {
                Top = description.Top,
                Left = description.Left,
                Width = description.Width,
                Height = description.Height,
            });

            this.viewport = new Viewport(0, 0, description.Width, description.Height);

            this.Graphics.CreateRenderTargetTexture(
                Format.R8G8B8A8_UNorm,
                description.Width, description.Height,
                out this.renderTarget,
                out this.renderTexture);

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

            float aspect = this.minimapBox.Height / this.minimapBox.Width;
            float near = 0.1f;

            Vector3 eyePos = new Vector3(0, y + near, 0);
            Vector3 target = Vector3.Zero;
            Vector3 dir = Vector3.Normalize(target - eyePos);

            Matrix view = Matrix.LookAtLH(
                eyePos,
                target,
                Vector3.UnitZ);

            Matrix proj = Matrix.OrthoLH(
                x / aspect,
                z,
                near,
                y + near);

            this.drawContext = new DrawContext()
            {
                DrawerMode = DrawerModesEnum.Forward,
                World = Matrix.Identity,
                ViewProjection = view * proj,
                EyePosition = eyePos,
                EyeTarget = target,
                Lights = SceneLights.Default,
            };
        }
        /// <summary>
        /// Update objects
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {

        }
        /// <summary>
        /// Draw objects
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.Drawables != null && this.Drawables.Length > 0)
            {
                this.drawContext.GameTime = context.GameTime;

                this.Game.Graphics.SetViewport(this.viewport);
                this.Game.Graphics.SetRenderTargets(this.renderTarget, true, Color.Black, null, false, false);

                for (int i = 0; i < this.Drawables.Length; i++)
                {
                    this.Drawables[i].Get<IDrawable>()?.Draw(this.drawContext);
                }

                this.Game.Graphics.SetDefaultViewport();
                this.Game.Graphics.SetDefaultRenderTarget(false);
            }

            this.minimapBox.Texture = this.renderTexture;
            this.minimapBox.Draw(context);
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
}
