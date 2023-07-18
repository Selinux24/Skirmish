using SharpDX;
using SharpDX.DXGI;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Minimap
    /// </summary>
    public sealed class UIMinimap : Drawable<UIMinimapDescription>, IScreenFitted
    {
        /// <summary>
        /// Viewport to match the minimap texture size
        /// </summary>
        private Viewport viewport;
        /// <summary>
        /// Surface to draw
        /// </summary>
        private UITextureRenderer minimapBox;
        /// <summary>
        /// Minimap render target
        /// </summary>
        private EngineRenderTargetView renderTarget;
        /// <summary>
        /// Minimap texture
        /// </summary>
        private EngineShaderResourceView renderTexture;
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
        public IDrawable[] Drawables { get; set; }
        /// <summary>
        /// Back color
        /// </summary>
        public Color BackColor { get; set; }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public UIMinimap(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~UIMinimap()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                renderTarget?.Dispose();
                renderTarget = null;

                renderTexture?.Dispose();
                renderTexture = null;

                minimapBox?.Dispose();
                minimapBox = null;
            }
        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(UIMinimapDescription description)
        {
            await base.InitializeAssets(description);

            Drawables = Description.Drawables;
            BackColor = Description.BackColor;

            minimapArea = Description.MinimapArea;

            viewport = new Viewport(0, 0, Description.Width, Description.Height);

            minimapBox = await CreateRenderer();

            var rt = Game.Graphics.CreateRenderTargetTexture(
                $"RenderTexture_{Name}",
                Format.R8G8B8A8_UNorm,
                Description.Width, Description.Height, false);

            renderTarget = rt.RenderTarget;
            renderTexture = rt.ShaderResource;

            InitializeContext();
        }
        private async Task<UITextureRenderer> CreateRenderer()
        {
            var desc = UITextureRendererDescription.Default(Description.Left, Description.Top, Description.Width, Description.Height);

            return await Scene.CreateComponent<UITextureRenderer, UITextureRendererDescription>(
                $"{Id}.TextureRenderer",
                $"{Name}.TextureRenderer",
                desc);
        }

        /// <summary>
        /// Initialize terrain context
        /// </summary>
        private void InitializeContext()
        {
            float x = minimapArea.Maximum.X - minimapArea.Minimum.X;
            float y = minimapArea.Maximum.Y - minimapArea.Minimum.Y;
            float z = minimapArea.Maximum.Z - minimapArea.Minimum.Z;

            float aspect = minimapBox.Height / minimapBox.Width;
            float near = 0.1f;

            var eyePosition = new Vector3(0, y + near, 0);
            var eyeDirection = Vector3.Zero;

            var view = Matrix.LookAtLH(
                eyePosition,
                eyeDirection,
                Vector3.UnitZ);

            var proj = Matrix.OrthoLH(
                x / aspect,
                z,
                near,
                y + near);

            drawContext = new DrawContext()
            {
                DrawerMode = DrawerModes.Forward | DrawerModes.OpaqueOnly,
                ViewProjection = view * proj,
                EyePosition = eyePosition,
                EyeDirection = eyeDirection,
                Lights = SceneLights.CreateDefault(Scene),
            };
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            minimapBox?.Update(context);
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (Drawables?.Any() != true)
            {
                return false;
            }

            drawContext.GameTime = context.GameTime;

            var graphics = Game.Graphics;

            graphics.SetViewport(viewport);

            graphics.SetRenderTargets(
                renderTarget, true, BackColor,
                false);

            foreach (var item in Drawables)
            {
                item.Draw(drawContext);
            }

            graphics.SetDefaultViewport();
            graphics.SetDefaultRenderTarget(false, Color.Transparent);

            minimapBox.Texture = renderTexture;
            bool drawn = minimapBox.Draw(context);

            return drawn || base.Draw(context);
        }

        /// <summary>
        /// Resize
        /// </summary>
        public void Resize()
        {
            minimapBox.Resize();
        }

        /// <inheritdoc/>
        public override bool Cull(ICullingVolume volume, out float distance)
        {
            drawContext.Lights.Cull(volume, drawContext.EyePosition, Scene.GameEnvironment.LODDistanceLow);

            return base.Cull(volume, out distance);
        }
    }
}
