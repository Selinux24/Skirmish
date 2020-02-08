using SharpDX;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Minimap
    /// </summary>
    public class SpriteTexture : Drawable, IScreenFitted, ITransformable2D
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
        private SpriteTextureChannels channels = SpriteTextureChannels.None;

        /// <summary>
        /// Sprite initial width
        /// </summary>
        public float Width { get; private set; }
        /// <summary>
        /// Sprite initial height
        /// </summary>
        public float Height { get; private set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; }
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
        public SpriteTextureChannels Channels
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
        /// Contructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sprite texture description</param>
        public SpriteTexture(Scene scene, SpriteTextureDescription description)
            : base(scene, description)
        {
            var sprite = GeometryUtil.CreateSprite(Vector2.Zero, 1, 1);

            var vertices = VertexPositionTexture.Generate(sprite.Vertices, sprite.Uvs);
            var indices = sprite.Indices;

            this.vertexBuffer = this.BufferManager.AddVertexData(description.Name, false, vertices);
            this.indexBuffer = this.BufferManager.AddIndexData(description.Name, false, indices);

            this.Channels = description.Channel;

            this.Manipulator = new Manipulator2D();
            this.Manipulator.SetPosition(description.Left, description.Top);
            this.Manipulator.Update(new GameTime(), this.Game.Form.RelativeCenter, description.Width, description.Height);

            Sprite.CreateViewOrthoProjection(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight,
                out Matrix view,
                out Matrix proj);

            this.viewProjection = view * proj;

            this.Width = description.Width;
            this.Height = description.Height;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SpriteTexture()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
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

                this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);
                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, Topology.TriangleList);

                effect.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);
                effect.UpdatePerObject(Color.White, this.Texture, this.TextureIndex);

                var graphics = this.Game.Graphics;

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    graphics.DrawIndexed(
                        this.indexBuffer.Count,
                        this.indexBuffer.Offset,
                        this.vertexBuffer.Offset);
                }
            }
        }
        /// <summary>
        /// Screen resize
        /// </summary>
        public virtual void Resize()
        {
            this.viewProjection = Sprite.CreateViewOrthoProjection(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
        }
        /// <summary>
        /// Object resize
        /// </summary>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        public virtual void ResizeSprite(float width, float height)
        {
            this.Manipulator.Update(new GameTime(), this.Game.Form.RelativeCenter, width, height);
        }
    }

    /// <summary>
    /// Sprite texture extensions
    /// </summary>
    public static class SpriteTextureExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<SpriteTexture> AddComponentSpriteTexture(this Scene scene, SpriteTextureDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            SpriteTexture component = null;

            await Task.Run(() =>
            {
                component = new SpriteTexture(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
