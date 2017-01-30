using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Sprite drawer
    /// </summary>
    public class Sprite : Drawable, IScreenFitted
    {
        /// <summary>
        /// Creates view and orthoprojection from specified size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns view * orthoprojection matrix</returns>
        public static Matrix CreateViewOrthoProjection(int width, int height)
        {
            Matrix view;
            Matrix projection;
            CreateViewOrthoProjection(width, height, out view, out projection);

            return view * projection;
        }
        /// <summary>
        /// Creates view and orthoprojection from specified size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Ortho projection matrix</param>
        public static void CreateViewOrthoProjection(int width, int height, out Matrix view, out Matrix projection)
        {
            Vector3 pos = new Vector3(0, 0, -1);

            view = Matrix.LookAtLH(
                pos,
                pos + Vector3.ForwardLH,
                Vector3.Up);

            projection = Matrix.OrthoLH(
                width,
                height,
                0f, 100f);
        }

        /// <summary>
        /// Source render width
        /// </summary>
        private readonly int renderWidth;
        /// <summary>
        /// Source render height
        /// </summary>
        private readonly int renderHeight;
        /// <summary>
        /// Source width
        /// </summary>
        private readonly int sourceWidth;
        /// <summary>
        /// Source height
        /// </summary>
        private readonly int sourceHeight;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        private Matrix viewProjection;

        /// <summary>
        /// Index buffer
        /// </summary>
        private Buffer indexBuffer = null;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;
        /// <summary>
        /// Vertex buffer
        /// </summary>
        private Buffer vertexBuffer = null;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding[] vertexBufferBinding = null;
        /// <summary>
        /// Sprite texture
        /// </summary>
        private ShaderResourceView spriteTexture = null;

        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public int Left
        {
            get
            {
                return (int)this.Manipulator.Position.X;
            }
            set
            {
                this.Manipulator.SetPosition(new Vector2(value, this.Manipulator.Position.Y));
            }
        }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public int Top
        {
            get
            {
                return (int)this.Manipulator.Position.Y;
            }
            set
            {
                this.Manipulator.SetPosition(new Vector2(this.Manipulator.Position.X, value));
            }
        }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Relative center
        /// </summary>
        public Vector2 RelativeCenter
        {
            get
            {
                return (new Vector2(this.Width, this.Height)) * 0.5f;
            }
        }
        /// <summary>
        /// Sprite rectangle
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(
                    this.Left,
                    this.Top,
                    this.Width,
                    this.Height);
            }
        }
        /// <summary>
        /// Indicates whether the sprite has to maintain proportion with window size
        /// </summary>
        public bool FitScreen { get; set; }
        /// <summary>
        /// Gets or sets the texture index to render
        /// </summary>
        public int TextureIndex { get; set; }
        /// <summary>
        /// Base color
        /// </summary>
        public Color4 Color { get; set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; }
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
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Description</param>
        public Sprite(Game game, SpriteDescription description)
            : base(game, description)
        {
            this.InitializeBuffers();

            this.InitializeTexture(description.ContentPath, description.Textures);

            this.renderWidth = game.Form.RenderWidth.NextPair();
            this.renderHeight = game.Form.RenderHeight.NextPair();
            this.sourceWidth = description.Width <= 0 ? this.renderWidth : description.Width.NextPair();
            this.sourceHeight = description.Height <= 0 ? this.renderHeight : description.Height.NextPair();
            this.viewProjection = Sprite.CreateViewOrthoProjection(this.renderWidth, this.renderHeight);

            this.Width = this.sourceWidth;
            this.Height = this.sourceHeight;
            this.FitScreen = description.FitScreen;
            this.TextureIndex = 0;
            this.Color = Color4.White;

            this.Manipulator = new Manipulator2D();
        }
        /// <summary>
        /// Internal resources disposition
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.vertexBuffer);
            Helper.Dispose(this.indexBuffer);
            Helper.Dispose(this.spriteTexture);
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.Manipulator.Update(context.GameTime, this.Game.Form.RelativeCenter, this.Width, this.Height);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.indexCount > 0)
            {
                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexCount / 3;
                }

                var effect = DrawerPool.EffectDefaultSprite;
                var technique = effect.GetTechnique(VertexTypes.PositionTexture, false, DrawingStages.Drawing, context.DrawerMode);

                #region Per frame update

                effect.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);

                #endregion

                #region Per object update

                effect.UpdatePerObject(this.Color, this.spriteTexture, this.TextureIndex);

                #endregion

                //Sets vertex and index buffer
                this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                Counters.IAInputLayoutSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
                Counters.IAVertexBuffersSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, 0, 0);

                    Counters.DrawCallsPerFrame++;
                }
            }
        }
        /// <summary>
        /// Initialize buffers
        /// </summary>
        private void InitializeBuffers()
        {
            Vector3[] vData;
            Vector2[] uvs;
            uint[] iData;
            GeometryUtil.CreateSprite(Vector2.Zero, 1, 1, 0, 0, out vData, out uvs, out iData);

            VertexPositionTexture[] vertices = VertexPositionTexture.Generate(vData, uvs);

            this.vertexBuffer = this.Game.Graphics.Device.CreateVertexBufferImmutable(vertices);
            this.vertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.vertexBuffer, vertices[0].GetStride(), 0),
            };

            this.indexBuffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(iData);
            this.indexCount = iData.Length;
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        private void InitializeTexture(string contentPath, string[] textures)
        {
            ImageContent image = ImageContent.Array(contentPath, textures);
            this.spriteTexture = this.Game.ResourceManager.CreateResource(image);
        }
        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            int width = this.Game.Form.RenderWidth.NextPair();
            int height = this.Game.Form.RenderHeight.NextPair();

            this.viewProjection = Sprite.CreateViewOrthoProjection(width, height);

            if (this.FitScreen)
            {
                float w = width / (float)this.renderWidth;
                float h = height / (float)this.renderHeight;

                this.Width = ((int)(this.sourceWidth * w)).NextPair();
                this.Height = ((int)(this.sourceHeight * h)).NextPair();
            }
        }
    }
}
