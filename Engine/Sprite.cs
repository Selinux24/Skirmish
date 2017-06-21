using SharpDX;
using SharpDX.Direct3D;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Sprite drawer
    /// </summary>
    public class Sprite : Drawable, IScreenFitted, ITransformable2D
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
        /// Use textures flag
        /// </summary>
        public bool Textured { get; private set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Description</param>
        public Sprite(Game game, BufferManager bufferManager, SpriteDescription description)
            : base(game, bufferManager, description)
        {
            this.Textured = description.Textures != null && description.Textures.Length > 0;

            this.InitializeBuffers(description.Name, this.Textured);

            if (this.Textured)
            {
                this.InitializeTexture(description.ContentPath, description.Textures);
            }

            this.renderWidth = game.Form.RenderWidth.NextPair();
            this.renderHeight = game.Form.RenderHeight.NextPair();
            this.sourceWidth = description.Width <= 0 ? this.renderWidth : description.Width.NextPair();
            this.sourceHeight = description.Height <= 0 ? this.renderHeight : description.Height.NextPair();
            this.viewProjection = Sprite.CreateViewOrthoProjection(this.renderWidth, this.renderHeight);

            this.Width = this.sourceWidth;
            this.Height = this.sourceHeight;
            this.FitScreen = description.FitScreen;
            this.TextureIndex = 0;
            this.Color = description.Color;

            this.Manipulator = new Manipulator2D();
        }
        /// <summary>
        /// Internal resources disposition
        /// </summary>
        public override void Dispose()
        {

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
            if (this.indexBuffer.Count > 0)
            {
                this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);

                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;
                }

                var effect = DrawerPool.EffectDefaultSprite;
                var technique = effect.GetTechnique(this.Textured ? VertexTypes.PositionTexture : VertexTypes.PositionColor, false, DrawingStages.Drawing, context.DrawerMode);

                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.TriangleList);

                #region Per frame update

                effect.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);

                #endregion

                #region Per object update

                effect.UpdatePerObject(this.Color, this.spriteTexture, this.TextureIndex);

                #endregion

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawIndexed(this.indexBuffer.Count, this.indexBuffer.Offset, this.vertexBuffer.Offset);

                    Counters.DrawCallsPerFrame++;
                }
            }
        }
        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="textured">Use a textured buffer</param>
        private void InitializeBuffers(string name, bool textured)
        {
            Vector3[] vData;
            Vector2[] uvs;
            uint[] iData;
            GeometryUtil.CreateSprite(Vector2.Zero, 1, 1, 0, 0, out vData, out uvs, out iData);
            if (textured)
            {
                var vertices = VertexPositionTexture.Generate(vData, uvs);
                this.vertexBuffer = this.BufferManager.Add(name, vertices, false, 0);
            }
            else
            {
                var vertices = VertexPositionColor.Generate(vData, Color4.White);
                this.vertexBuffer = this.BufferManager.Add(name, vertices, false, 0);
            }

            this.indexBuffer = this.BufferManager.Add(name, iData, false);
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        private void InitializeTexture(string contentPath, string[] textures)
        {
            var image = ImageContent.Array(contentPath, textures);
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
