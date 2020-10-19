using SharpDX;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Sprite drawer
    /// </summary>
    public class Sprite : UIControl
    {
        /// <summary>
        /// View * projection matrix
        /// </summary>
        private Matrix viewProjection;

        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// Sprite texture
        /// </summary>
        private EngineShaderResourceView spriteTexture = null;

        /// <summary>
        /// Gets or sets the texture index to render
        /// </summary>
        public int TextureIndex { get; set; }
        /// <summary>
        /// Use textures flag
        /// </summary>
        public bool Textured { get; private set; }
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
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Sprite(string name, Scene scene, SpriteDescription description)
            : base(name, scene, description)
        {
            Textured = description.Textures?.Any() == true;
            TextureIndex = description.TextureIndex;

            InitializeBuffers(name, Textured, description.UVMap);

            if (Textured)
            {
                InitializeTexture(description.ContentPath, description.Textures);
            }

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

        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="textured">Use a textured buffer</param>
        private void InitializeBuffers(string name, bool textured, Vector4? uvMap)
        {
            GeometryDescriptor geom;
            if (textured)
            {
                if (uvMap.HasValue)
                {
                    geom = GeometryUtil.CreateUnitSprite(uvMap.Value);
                }
                else
                {
                    geom = GeometryUtil.CreateUnitSprite();
                }

                var vertices = VertexPositionTexture.Generate(geom.Vertices, geom.Uvs);
                vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            }
            else
            {
                geom = GeometryUtil.CreateUnitSprite();

                var vertices = VertexPositionColor.Generate(geom.Vertices, Color4.White);
                vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            }

            indexBuffer = BufferManager.AddIndexData(name, false, geom.Indices);
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        private void InitializeTexture(string contentPath, string[] textures)
        {
            var image = ImageContent.Array(contentPath, textures);
            spriteTexture = Game.ResourceManager.RequestResource(image);
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
                Textured ? VertexTypes.PositionTexture : VertexTypes.PositionColor,
                UITextureRendererChannels.All);

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(Manipulator.LocalTransform, viewProjection);

            var color = Color4.AdjustSaturation(BaseColor * TintColor, 1f);
            color.Alpha *= Alpha;

            effect.UpdatePerObject(color, spriteTexture, TextureIndex);

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
    /// Sprite extensions
    /// </summary>
    public static class SpriteExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Sprite> AddComponentSprite(this Scene scene, string name, SpriteDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Sprite component = null;

            await Task.Run(() =>
            {
                component = new Sprite(name, scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
