using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Render to texture control
    /// </summary>
    public class UITextureRenderer : UIControl
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private readonly BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private readonly BufferDescriptor indexBuffer = null;
        /// <summary>
        /// View * projection for 2D projection
        /// </summary>
        private Matrix viewProjection;
        /// <summary>
        /// Drawing channels
        /// </summary>
        private UITextureRendererChannels channels = UITextureRendererChannels.None;

        /// <summary>
        /// Texture
        /// </summary>
        public EngineShaderResourceView Texture { get; set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public int TextureIndex { get; set; }
        /// <summary>
        /// Drawing channels
        /// </summary>
        public UITextureRendererChannels Channels
        {
            get
            {
                return this.channels;
            }
            set
            {
                if (this.channels != value)
                {
                    this.channels = value;
                }
            }
        }
        /// <summary>
        /// Gets whether the internal buffers were ready or not
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                return this.vertexBuffer?.Ready == true && this.indexBuffer?.Ready == true;
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sprite texture description</param>
        public UITextureRenderer(Scene scene, UITextureRendererDescription description)
            : base(scene, description)
        {
            var sprite = GeometryUtil.CreateSprite(Vector2.Zero, 1, 1);

            var vertices = VertexPositionTexture.Generate(sprite.Vertices, sprite.Uvs);
            var indices = sprite.Indices;

            this.vertexBuffer = this.BufferManager.AddVertexData(description.Name, false, vertices);
            this.indexBuffer = this.BufferManager.AddIndexData(description.Name, false, indices);

            this.Channels = description.Channel;

            this.viewProjection = CreateViewOrthoProjection(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);

            // Adjust to screen coordinates
            float x = description.Left - this.Game.Form.RelativeCenter.X;
            float y = description.Top - this.Game.Form.RelativeCenter.Y;

            this.Manipulator.SetPosition(x, y);
            this.Manipulator.SetScale(description.Width, description.Height);
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                this.BufferManager?.RemoveVertexData(this.vertexBuffer);
                this.BufferManager?.RemoveIndexData(this.indexBuffer);
            }
        }

        /// <summary>
        /// Draw objects
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (!BuffersReady)
            {
                return;
            }

            var mode = context.DrawerMode;
            var draw = (mode.HasFlag(DrawerModes.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModes.TransparentOnly) && this.Description.AlphaEnabled);

            if (draw && this.indexBuffer.Count > 0)
            {
                var effect = DrawerPool.EffectDefaultSprite;
                var technique = effect.GetTechnique(
                    VertexTypes.PositionTexture,
                    this.Channels);

                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;

                this.BufferManager.SetIndexBuffer(this.indexBuffer);
                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer, Topology.TriangleList);

                effect.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);
                effect.UpdatePerObject(Color4.White, this.Texture, this.TextureIndex);

                var graphics = this.Game.Graphics;

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    graphics.DrawIndexed(
                        this.indexBuffer.Count,
                        this.indexBuffer.BufferOffset,
                        this.vertexBuffer.BufferOffset);
                }
            }
        }

        /// <summary>
        /// Screen resize
        /// </summary>
        public override void Resize()
        {
            base.Resize();

            this.viewProjection = CreateViewOrthoProjection(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);

            // Adjust to screen coordinates
            float x = this.Left - this.Game.Form.RelativeCenter.X;
            float y = this.Top - this.Game.Form.RelativeCenter.Y;

            this.Manipulator.SetPosition(x, y);
            this.Manipulator.SetScale(this.Width, this.Height);
        }
    }

    /// <summary>
    /// Sprite texture extensions
    /// </summary>
    public static class UITextureRendererExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UITextureRenderer> AddComponentUITextureRenderer(this Scene scene, UITextureRendererDescription description, int order = 0)
        {
            UITextureRenderer component = null;

            await Task.Run(() =>
            {
                component = new UITextureRenderer(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
