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
                return this.vertexBuffer?.Ready == true && this.indexBuffer?.Ready == true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Sprite(Scene scene, SpriteDescription description)
            : base(scene, description)
        {
            this.Textured = description.Textures?.Any() == true;
            this.TextureIndex = description.TextureIndex;

            this.InitializeBuffers(description.Name, this.Textured, description.UVMap);

            if (this.Textured)
            {
                this.InitializeTexture(description.ContentPath, description.Textures);
            }

            this.viewProjection = CreateViewOrthoProjection(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
        }
        /// <summary>
        /// Internal resources disposition
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
                    geom = GeometryUtil.CreateSprite(Vector2.Zero, 1, 1, 0, 0, uvMap.Value);
                }
                else
                {
                    geom = GeometryUtil.CreateSprite(Vector2.Zero, 1, 1, 0, 0);
                }

                var vertices = VertexPositionTexture.Generate(geom.Vertices, geom.Uvs);
                this.vertexBuffer = this.BufferManager.AddVertexData(name, false, vertices);
            }
            else
            {
                geom = GeometryUtil.CreateSprite(Vector2.Zero, 1, 1, 0, 0);

                var vertices = VertexPositionColor.Generate(geom.Vertices, Color4.White);
                this.vertexBuffer = this.BufferManager.AddVertexData(name, false, vertices);
            }

            this.indexBuffer = this.BufferManager.AddIndexData(name, false, geom.Indices);
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        private void InitializeTexture(string contentPath, string[] textures)
        {
            var image = ImageContent.Array(contentPath, textures);
            this.spriteTexture = this.Game.ResourceManager.RequestResource(image);
        }

        /// <summary>
        /// Draw
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
            var draw =
                (mode.HasFlag(DrawerModes.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModes.TransparentOnly) && this.Description.AlphaEnabled);

            if (draw && this.indexBuffer.Count > 0)
            {
                var effect = DrawerPool.EffectDefaultSprite;
                var technique = effect.GetTechnique(
                    this.Textured ? VertexTypes.PositionTexture : VertexTypes.PositionColor,
                    UITextureRendererChannels.All);

                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;

                this.BufferManager.SetIndexBuffer(this.indexBuffer);
                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer, Topology.TriangleList);

                effect.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);
                effect.UpdatePerObject(this.Color, this.spriteTexture, this.TextureIndex);

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
        /// Resizes internal components
        /// </summary>
        public override void Resize()
        {
            base.Resize();

            this.viewProjection = CreateViewOrthoProjection(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
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
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Sprite> AddComponentSprite(this Scene scene, SpriteDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Sprite component = null;

            await Task.Run(() =>
            {
                component = new Sprite(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
