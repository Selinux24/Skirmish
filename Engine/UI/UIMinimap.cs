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
        /// Minimap rendered area
        /// </summary>
        private BoundingBox minimapArea;
        /// <summary>
        /// Minimap camera
        /// </summary>
        private Camera minimapCamera;
        /// <summary>
        /// Minimap lights
        /// </summary>
        private SceneLights minimapLights;

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

            minimapCamera = Camera.CreateOrtho(minimapArea.Size, 0.1f, minimapBox.Width, minimapBox.Height);
            minimapLights = SceneLights.CreateDefault(Scene);
        }
        private async Task<UITextureRenderer> CreateRenderer()
        {
            var desc = UITextureRendererDescription.Default(Description.Left, Description.Top, Description.Width, Description.Height);

            return await Scene.CreateComponent<UITextureRenderer, UITextureRendererDescription>(
                $"{Id}.TextureRenderer",
                $"{Name}.TextureRenderer",
                desc);
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

            var drawContext = context.Clone($"{Name ?? nameof(UIMinimap)}", DrawerModes.Forward | DrawerModes.OpaqueOnly);
            drawContext.Camera = minimapCamera;
            drawContext.Lights = minimapLights;

            var graphics = Game.Graphics;
            var dc = drawContext.DeviceContext;

            dc.SetViewport(viewport);

            dc.SetRenderTargets(
                renderTarget, true, BackColor,
                false);

            foreach (var item in Drawables)
            {
                item.Draw(drawContext);
            }

            dc.SetViewport(graphics.Viewport);
            dc.SetRenderTargets(graphics.DefaultRenderTarget, false, Color.Transparent);

            minimapBox.Texture = renderTexture;
            bool drawn = minimapBox.Draw(drawContext);

            return base.Draw(drawContext) || drawn;
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
            minimapLights.Cull(volume, Scene.Camera.Position, Scene.GameEnvironment.LODDistanceLow);

            return base.Cull(volume, out distance);
        }
    }
}
