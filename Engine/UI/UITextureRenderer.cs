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
        /// Texture
        /// </summary>
        public EngineShaderResourceView Texture { get; set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public int TextureIndex { get; set; } = 0;
        /// <summary>
        /// Drawing channels
        /// </summary>
        public UITextureRendererChannels Channels { get; set; } = UITextureRendererChannels.None;
        /// <summary>
        /// Gets whether the internal buffers were ready or not
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                return vertexBuffer?.Ready == true && indexBuffer?.Ready == true && indexBuffer.Count > 0;
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

            vertexBuffer = BufferManager.AddVertexData(description.Name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(description.Name, false, indices);

            Channels = description.Channel;

            viewProjection = Game.Form.GetOrthoProjectionMatrix();
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                BufferManager?.RemoveVertexData(vertexBuffer);
                BufferManager?.RemoveIndexData(indexBuffer);
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
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

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return;
            }

            var effect = DrawerPool.EffectDefaultSprite;
            var technique = effect.GetTechnique(
                VertexTypes.PositionTexture,
                Channels);

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(Manipulator.LocalTransform, viewProjection);
            effect.UpdatePerObject(Color4.White, Texture, TextureIndex);

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(
                    indexBuffer.Count,
                    indexBuffer.BufferOffset,
                    vertexBuffer.BufferOffset);
            }
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            base.Resize();

            viewProjection = Game.Form.GetOrthoProjectionMatrix();
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
