using SharpDX;

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
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// View * projection for 2D projection
        /// </summary>
        private Matrix viewProjection = Matrix.Identity;
        /// <summary>
        /// Drawing channels
        /// </summary>
        private SpriteTextureChannelsEnum channels = SpriteTextureChannelsEnum.None;

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
        public SpriteTextureChannelsEnum Channels
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
            Vector3[] cv;
            Vector2[] cuv;
            uint[] ci;
            GeometryUtil.CreateSprite(
                Vector2.Zero,
                1, 1,
                0, 0,
                out cv,
                out cuv,
                out ci);

            var vertices = VertexPositionTexture.Generate(cv, cuv);

            this.vertexBuffer = this.BufferManager.Add(description.Name, vertices, false, 0);
            this.indexBuffer = this.BufferManager.Add(description.Name, ci, false);

            this.Channels = description.Channel;

            this.Manipulator = new Manipulator2D();
            this.Manipulator.SetPosition(description.Left, description.Top);
            this.Manipulator.Update(new GameTime(), this.Game.Form.RelativeCenter, description.Width, description.Height);

            Matrix view;
            Matrix proj;
            Sprite.CreateViewOrthoProjection(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight,
                out view,
                out proj);

            this.viewProjection = view * proj;

            this.Width = description.Width;
            this.Height = description.Height;
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.BufferManager != null)
                {
                    //Remove data from buffer manager
                    this.BufferManager.RemoveVertexData(this.vertexBuffer);
                    this.BufferManager.RemoveIndexData(this.indexBuffer);
                }
            }
        }
        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {

        }
        /// <summary>
        /// Draw objects
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;

            if ((mode.HasFlag(DrawerModesEnum.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModesEnum.TransparentOnly) && this.Description.AlphaEnabled))
            {
                if (this.indexBuffer.Count > 0)
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
}
