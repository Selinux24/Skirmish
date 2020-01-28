using SharpDX;
using System.Threading.Tasks;

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
            CreateViewOrthoProjection(width, height, out Matrix view, out Matrix projection);

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
        private EngineShaderResourceView spriteTexture = null;

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
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Sprite(Scene scene, SpriteDescription description)
            : base(scene, description)
        {
            this.Textured = description.Textures != null && description.Textures.Length > 0;

            this.InitializeBuffers(description.Name, this.Textured, description.UVMap);

            if (this.Textured)
            {
                this.InitializeTexture(description.ContentPath, description.Textures);
            }

            this.renderWidth = this.Game.Form.RenderWidth.NextPair();
            this.renderHeight = this.Game.Form.RenderHeight.NextPair();
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
        /// Destructor
        /// </summary>
        ~Sprite()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Internal resources disposition
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                this.BufferManager?.RemoveVertexData(this.vertexBuffer, 0);
                this.BufferManager?.RemoveIndexData(this.indexBuffer);
            }
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
            var mode = context.DrawerMode;
            var draw =
                (mode.HasFlag(DrawerModes.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModes.TransparentOnly) && this.Description.AlphaEnabled);

            if (draw && this.indexBuffer.Count > 0)
            {
                var effect = DrawerPool.EffectDefaultSprite;
                var technique = effect.GetTechnique(
                    this.Textured ? VertexTypes.PositionTexture : VertexTypes.PositionColor,
                    SpriteTextureChannels.All);

                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;

                this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);
                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, Topology.TriangleList);

                effect.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);
                effect.UpdatePerObject(this.Color, this.spriteTexture, this.TextureIndex);

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
                this.vertexBuffer = this.BufferManager.Add(name, vertices, false, 0);
            }
            else
            {
                geom = GeometryUtil.CreateSprite(Vector2.Zero, 1, 1, 0, 0);

                var vertices = VertexPositionColor.Generate(geom.Vertices, Color4.White);
                this.vertexBuffer = this.BufferManager.Add(name, vertices, false, 0);
            }

            this.indexBuffer = this.BufferManager.Add(name, geom.Indices, false);
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
