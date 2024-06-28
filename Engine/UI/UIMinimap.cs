using SharpDX;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Minimap
    /// </summary>
    /// <remarks>
    /// Contructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class UIMinimap(Scene scene, string id, string name) : Drawable<UIMinimapDescription>(scene, id, name), IScreenFitted
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
        /// Minimap camera
        /// </summary>
        private Camera minimapCamera;
        /// <summary>
        /// Minimap lights
        /// </summary>
        private SceneLights minimapLights;
        /// <summary>
        /// Drawables
        /// </summary>
        private readonly List<IDrawable> drawables = [];

        /// <summary>
        /// Back color
        /// </summary>
        public Color BackColor { get; set; }

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
        public override async Task ReadAssets(UIMinimapDescription description)
        {
            await base.ReadAssets(description);

            BackColor = Description.BackColor;

            viewport = new Viewport(0, 0, Description.Width, Description.Height);

            minimapBox = await CreateRenderer();

            var rt = Game.Graphics.CreateRenderTargetTexture(
                $"RenderTexture_{Name}",
                Format.R8G8B8A8_UNorm,
                Description.Width, Description.Height, false);

            renderTarget = rt.RenderTarget;
            renderTexture = rt.ShaderResource;

            minimapCamera = Camera.CreateOrtho(Description.MinimapArea, 0.1f, minimapBox.Width, minimapBox.Height);

            AddDrawables(Description.Drawables ?? Enumerable.Empty<IDrawable>());

            minimapLights = SceneLights.CreateDefault(Scene);

            SetMapArea(Description.MinimapArea);
        }

        public void SetMapArea(BoundingBox minimapArea)
        {
            minimapCamera.SetOrtho(minimapArea, 0.1f, minimapBox.Width, minimapBox.Height);
        }

        public void AddDrawable(IDrawable drawable)
        {
            if (drawable == null)
            {
                return;
            }

            if (!drawables.Contains(drawable))
            {
                drawables.Add(drawable);
            }
        }

        public void AddDrawables(IEnumerable<IDrawable> drawableList)
        {
            if (drawableList?.Any() != true)
            {
                return;
            }

            drawableList.ToList().ForEach(AddDrawable);
        }

        /// <summary>
        /// Creates texture renderer
        /// </summary>
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

            if (drawables.Count == 0)
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

            foreach (var item in drawables)
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
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
        {
            minimapLights.Cull(volume, Scene.Camera.Position, Scene.GameEnvironment.LODDistanceLow);

            return base.Cull(cullIndex, volume, out distance);
        }
    }
}
