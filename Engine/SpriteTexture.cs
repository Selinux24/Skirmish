using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Minimap
    /// </summary>
    public class SpriteTexture : Drawable, IScreenFitted
    {
        /// <summary>
        /// Minimap vertex buffer
        /// </summary>
        private Buffer vertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding vertexBufferBinding;
        /// <summary>
        /// Minimap index buffer
        /// </summary>
        private Buffer indexBuffer;
        /// <summary>
        /// Drawer
        /// </summary>
        private EffectDefaultSprite effect;
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

            this.vertexBuffer = this.Device.CreateVertexBufferImmutable(vertices);
            this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, vertices[0].GetStride(), 0);
            this.indexBuffer = this.Device.CreateIndexBufferImmutable(ci);

            this.effect = DrawerPool.EffectDefaultSprite;
            this.Channels = description.Channel;

            this.InitializeContext(description.Left, description.Top, description.Width, description.Height);
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
            if (context.DrawerMode != DrawerModesEnum.ShadowMap)
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += 2;
            }

            var technique = effect.GetTechnique(VertexTypes.PositionTexture, false, DrawingStages.Drawing, context.DrawerMode, this.Channels);

            this.DeviceContext.InputAssembler.InputLayout = this.effect.GetInputLayout(technique);
            Counters.IAInputLayoutSets++;
            this.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Counters.IAPrimitiveTopologySets++;
            this.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            Counters.IAVertexBuffersSets++;
            this.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;

            this.effect.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);
            this.effect.UpdatePerObject(Color.White, this.Texture, 0);

            for (int p = 0; p < technique.Description.PassCount; p++)
            {
                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.DeviceContext.DrawIndexed(6, 0, 0);

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
        /// <summary>
        /// Dispose objects
        /// </summary>
        public override void Dispose()
        {
            if (this.vertexBuffer != null)
            {
                this.vertexBuffer.Dispose();
                this.vertexBuffer = null;
            }

            if (this.indexBuffer != null)
            {
                this.indexBuffer.Dispose();
                this.indexBuffer = null;
            }
        }
    }
}
