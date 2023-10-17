using SharpDX;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Sprites;
    using Engine.Common;
    using Engine.Content;

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
        /// Effect
        /// </summary>
        private readonly BuiltInSpriteTexture spriteDrawer;

        /// <summary>
        /// Texture
        /// </summary>
        public EngineShaderResourceView Texture { get; set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;
        /// <summary>
        /// Color channel
        /// </summary>
        public ColorChannels Channel { get; set; }
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
            spriteDrawer = BuiltInShaders.GetDrawer<BuiltInSpriteTexture>();
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
        public override async Task ReadAssets(UITextureRendererDescription description)
        {
            await base.ReadAssets(description);

            var sprite = GeometryUtil.CreateUnitSprite();

            var vertices = VertexPositionTexture.Generate(sprite.Vertices, sprite.Uvs);
            var indices = sprite.Indices;

            vertexBuffer = BufferManager.AddVertexData(Name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(Name, false, indices);

            Texture = await InitializeTexture(Description.ContentPath, Description.Textures);
            TextureIndex = Description.TextureIndex;
            Channel = Description.Channel;
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
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (!BuffersReady)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            var dc = context.DeviceContext;

            spriteDrawer.UpdateSprite(dc, new BuiltInSpriteState
            {
                Local = Manipulator.LocalTransform,
                Color1 = Color4.White,
                Texture = Texture,
                TextureIndex = TextureIndex,
                Channel = Channel,
            });

            bool drawn = spriteDrawer.Draw(dc, BufferManager, new DrawOptions
            {
                IndexBuffer = indexBuffer,
                VertexBuffer = vertexBuffer,
                Topology = Topology.TriangleList,
            });

            return base.Draw(context) || drawn;
        }
    }
}
