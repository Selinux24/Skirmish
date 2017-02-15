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
        /// Buffer manager
        /// </summary>
        private BufferManager bufferManager = new BufferManager();
        /// <summary>
        /// Vertex buffer offset
        /// </summary>
        private int vertexBufferOffset = -1;
        /// <summary>
        /// Vertex buffer slot
        /// </summary>
        private int vertexBufferSlot = -1;
        /// <summary>
        /// Vertex count
        /// </summary>
        private int vertexCount = 0;
        /// <summary>
        /// Index buffer offset
        /// </summary>
        private int indexBufferOffset = -1;
        /// <summary>
        /// Index buffer slot
        /// </summary>
        private int indexBufferSlot = -1;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;
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
        /// <param name="description">Sprite texture description</param>
        public SpriteTexture(Game game, SpriteTextureDescription description)
            : base(game, description)
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

            VertexPositionTexture[] vertices = VertexPositionTexture.Generate(cv, cuv);

            this.bufferManager.Add(0, vertices, false, 0, out this.vertexBufferOffset, out this.vertexBufferSlot);
            this.bufferManager.Add(0, ci, false, out this.indexBufferOffset, out this.indexBufferSlot);

            this.vertexCount = vertices.Length;
            this.indexCount = ci.Length;

            this.Channels = description.Channel;

            this.InitializeContext(description.Left, description.Top, description.Width, description.Height);

            this.bufferManager.CreateBuffers(game.Graphics, this.Name);
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.bufferManager);
        }
        /// <summary>
        /// Initialize minimap context
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        private void InitializeContext(int left, int top, int width, int height)
        {
            this.Manipulator.SetPosition(left, top);
            this.Manipulator.Update(new GameTime(), this.Game.Form.RelativeCenter, width, height);

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
            this.bufferManager.SetVertexBuffers(this.Game.Graphics);
            this.bufferManager.SetIndexBuffer(this.Game.Graphics, this.indexBufferSlot);

            if (context.DrawerMode != DrawerModesEnum.ShadowMap)
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.indexCount / 3;
            }

            var technique = DrawerPool.EffectDefaultSprite.GetTechnique(VertexTypes.PositionTexture, false, DrawingStages.Drawing, context.DrawerMode, this.Channels);

            this.bufferManager.SetInputAssembler(this.Game.Graphics, technique, VertexTypes.PositionTexture, false, PrimitiveTopology.TriangleList);

            DrawerPool.EffectDefaultSprite.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);
            DrawerPool.EffectDefaultSprite.UpdatePerObject(Color.White, this.Texture, 0);

            for (int p = 0; p < technique.Description.PassCount; p++)
            {
                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.DeviceContext.DrawIndexed(this.indexCount, this.indexBufferOffset, this.vertexBufferOffset);

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
