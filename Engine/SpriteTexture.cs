using SharpDX;
using SharpDX.Direct3D;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Minimap
    /// </summary>
    public class SpriteTexture : Drawable, IScreenFitted
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
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator = new Manipulator2D();
        /// <summary>
        /// Texture
        /// </summary>
        public ShaderResourceView Texture;
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
        /// Maximum number of instances
        /// </summary>
        public override int MaxInstances
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Sprite texture description</param>
        public SpriteTexture(Game game, BufferManager bufferManager, SpriteTextureDescription description)
            : base(game, bufferManager, description)
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

            this.vertexBuffer = this.BufferManager.Add(this.Name, vertices, false, 0);
            this.indexBuffer = this.BufferManager.Add(this.Name, ci, false);

            this.Channels = description.Channel;

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
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public override void Dispose()
        {

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
            this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);

            if (context.DrawerMode != DrawerModesEnum.ShadowMap)
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;
            }

            var technique = DrawerPool.EffectDefaultSprite.GetTechnique(VertexTypes.PositionTexture, false, DrawingStages.Drawing, context.DrawerMode, this.Channels);

            this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.TriangleList);

            DrawerPool.EffectDefaultSprite.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);
            DrawerPool.EffectDefaultSprite.UpdatePerObject(Color.White, this.Texture, 0);

            for (int p = 0; p < technique.Description.PassCount; p++)
            {
                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.DeviceContext.DrawIndexed(this.indexBuffer.Count, this.indexBuffer.Offset, this.vertexBuffer.Offset);

                Counters.DrawCallsPerFrame++;
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
