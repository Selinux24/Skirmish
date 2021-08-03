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
    public class UIMinimap : Drawable, IScreenFitted
    {
        /// <summary>
        /// Viewport to match the minimap texture size
        /// </summary>
        private readonly Viewport viewport;
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
        private readonly BoundingBox minimapArea;

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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Minimap description</param>
        public UIMinimap(string id, string name, Scene scene, UIMinimapDescription description)
            : base(id, name, scene, description)
        {
            Drawables = description.Drawables;
            BackColor = description.BackColor;

            minimapArea = description.MinimapArea;

            minimapBox = new UITextureRenderer(
                $"{id}.TextureRenderer",
                $"{name}.TextureRenderer",
                scene,
                UITextureRendererDescription.Default(description.Left, description.Top, description.Width, description.Height));

            viewport = new Viewport(0, 0, description.Width, description.Height);

            Game.Graphics.CreateRenderTargetTexture(
                Format.R8G8B8A8_UNorm,
                description.Width, description.Height, false,
                out renderTarget,
                out renderTexture);

            InitializeContext();
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

            Vector3 eyePos = new Vector3(0, y + near, 0);
            Vector3 target = Vector3.Zero;

            Matrix view = Matrix.LookAtLH(
                eyePos,
                target,
                Vector3.UnitZ);

            Matrix proj = Matrix.OrthoLH(
                x / aspect,
                z,
                near,
                y + near);

            drawContext = new DrawContext()
            {
                DrawerMode = DrawerModes.Forward | DrawerModes.OpaqueOnly,
                ViewProjection = view * proj,
                EyePosition = eyePos,
                EyeTarget = target,
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
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (Drawables?.Any() != true)
            {
                return;
            }

            drawContext.GameTime = context.GameTime;

            var graphics = Game.Graphics;

            graphics.SetViewport(viewport);

            graphics.SetRenderTargets(
                renderTarget, true, BackColor,
                null, false, false,
                false);

            foreach (var item in Drawables)
            {
                item.Draw(drawContext);
            }

            graphics.SetDefaultViewport();
            graphics.SetDefaultRenderTarget(false, Color4.Black, false, false);

            minimapBox.Texture = renderTexture;
            minimapBox.Draw(context);
        }

        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            minimapBox.Resize();
        }

        /// <inheritdoc/>
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            drawContext.Lights.Cull(volume, drawContext.EyePosition, Scene.GameEnvironment.LODDistanceLow);

            return base.Cull(volume, out distance);
        }
    }

    /// <summary>
    /// Minimap extensions
    /// </summary>
    public static class UIMinimapExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIMinimap> AddComponentUIMinimap(this Scene scene, string id, string name, UIMinimapDescription description, int layer = Scene.LayerUI)
        {
            UIMinimap component = null;

            await Task.Run(() =>
            {
                component = new UIMinimap(id, name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
