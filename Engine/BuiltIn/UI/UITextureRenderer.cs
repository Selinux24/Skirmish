using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Sprites;
using Engine.BuiltIn.Format;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace Engine.BuiltIn.UI
{
    /// <summary>
    /// Render to texture control
    /// </summary>
    /// <remarks>
    /// Contructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class UITextureRenderer(Scene scene, string id, string name) : UIControl<UITextureRendererDescription>(scene, id, name)
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
        private readonly BuiltInSpriteTexture spriteDrawer = BuiltInShaders.GetDrawer<BuiltInSpriteTexture>();

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

            Texture = InitializeTexture(Description.ContentPath, Description.Textures);
            TextureIndex = Description.TextureIndex;
            Channel = Description.Channel;
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        private EngineShaderResourceView InitializeTexture(string contentPath, string[] textures)
        {
            if ((textures?.Length ?? 0) == 0)
            {
                return null;
            }

            var image = new FileArrayImageContent(contentPath, textures);
            return Game.ResourceManager.RequestResource(image);
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

            bool drawn = spriteDrawer.Draw(dc, new DrawOptions
            {
                IndexBuffer = indexBuffer,
                VertexBuffer = vertexBuffer,
                Topology = Topology.TriangleList,
            });

            return base.Draw(context) || drawn;
        }
    }
}
