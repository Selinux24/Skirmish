using SharpDX;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Render to texture control
    /// </summary>
    public sealed class UITextureRenderer : UIControl<UITextureRendererDescription>
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// View * projection for 2D projection
        /// </summary>
        private Matrix viewProjection;
        /// <summary>
        /// Effect
        /// </summary>
        private readonly EffectDefaultSprite drawEffect;

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
        public ColorChannels Channels { get; set; }
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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public UITextureRenderer(Scene scene, string id, string name)
            : base(scene, id, name)
        {
            drawEffect = DrawerPool.GetEffect<EffectDefaultSprite>();
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
        public override async Task InitializeAssets(UITextureRendererDescription description)
        {
            await base.InitializeAssets(description);

            var sprite = GeometryUtil.CreateUnitSprite();

            var vertices = VertexPositionTexture.Generate(sprite.Vertices, sprite.Uvs);
            var indices = sprite.Indices;

            vertexBuffer = BufferManager.AddVertexData(Name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(Name, false, indices);

            Texture = await InitializeTexture(Description.ContentPath, Description.Textures);
            TextureIndex = Description.TextureIndex;
            Channels = Description.Channel;

            viewProjection = Game.Form.GetOrthoProjectionMatrix();
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        private async Task<EngineShaderResourceView> InitializeTexture(string contentPath, string[] textures)
        {
            if (textures?.Any() != true)
            {
                return null;
            }

            var image = new FileArrayImageContent(contentPath, textures);
            return await Game.ResourceManager.RequestResource(image);
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

            var technique = drawEffect.GetTechnique(
                VertexTypes.PositionTexture,
                Channels);

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            drawEffect.UpdatePerFrame(
                Manipulator.LocalTransform,
                viewProjection,
                Game.Form.RenderRectangle.BottomRight);

            drawEffect.UpdatePerObject(
                Color4.White,
                Texture,
                TextureIndex);

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(
                    indexBuffer.Count,
                    indexBuffer.BufferOffset,
                    vertexBuffer.BufferOffset);
            }

            base.Draw(context);
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            base.Resize();

            viewProjection = Game.Form.GetOrthoProjectionMatrix();
        }
    }
}
