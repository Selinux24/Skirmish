using SharpDX;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Sprites;
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Sprite drawer
    /// </summary>
    public sealed class Sprite : UIControl<SpriteDescription>
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
        /// Sprite texture
        /// </summary>
        private EngineShaderResourceView spriteTexture = null;
        /// <summary>
        /// Color drawer
        /// </summary>
        private readonly BuiltInSpriteColor spriteColorDrawer;
        /// <summary>
        /// Texture drawer
        /// </summary>
        private readonly BuiltInSpriteTexture spriteTextureDrawer;

        /// <summary>
        /// First color
        /// </summary>
        public Color4 Color1 { get; set; }
        /// <summary>
        /// Second color
        /// </summary>
        public Color4 Color2 { get; set; }
        /// <summary>
        /// Third color
        /// </summary>
        public Color4 Color3 { get; set; }
        /// <summary>
        /// Fourth color
        /// </summary>
        public Color4 Color4 { get; set; }
        /// <summary>
        /// Gets or sets the texture index to render
        /// </summary>
        public uint TextureIndex { get; set; }
        /// <summary>
        /// Use textures flag
        /// </summary>
        public bool Textured { get; private set; }
        /// <summary>
        /// First percentage
        /// </summary>
        public float Percentage1 { get; set; }
        /// <summary>
        /// Second percentage
        /// </summary>
        public float Percentage2 { get; set; }
        /// <summary>
        /// Third percentage
        /// </summary>
        public float Percentage3 { get; set; }
        /// <summary>
        /// Draw direction
        /// </summary>
        public uint DrawDirection { get; set; }
        /// <summary>
        /// Use percentage drawing
        /// </summary>
        public bool UsePercentage
        {
            get
            {
                return Percentage1 > 0f || Percentage2 > 0f || Percentage3 > 0f;
            }
        }
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
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public Sprite(Scene scene, string id, string name)
            : base(scene, id, name)
        {
            spriteColorDrawer = BuiltInShaders.GetDrawer<BuiltInSpriteColor>();
            spriteTextureDrawer = BuiltInShaders.GetDrawer<BuiltInSpriteTexture>();
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

        /// <inheritdoc/>
        public override async Task InitializeAssets(SpriteDescription description)
        {
            await base.InitializeAssets(description);

            Color1 = Description.Color1;
            Color2 = Description.Color2;
            Color3 = Description.Color3;
            Color4 = Description.Color4;
            Percentage1 = Description.Percentage1;
            Percentage2 = Description.Percentage2;
            Percentage3 = Description.Percentage3;
            DrawDirection = (uint)Description.DrawDirection;
            Textured = Description.Textures?.Any() == true;
            TextureIndex = Description.TextureIndex;

            InitializeBuffers(Name, Textured, Description.UVMap);

            if (Textured)
            {
                await InitializeTexture(Description.ContentPath, Description.Textures);
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
        private async Task InitializeTexture(string contentPath, string[] textures)
        {
            var image = new FileArrayImageContent(contentPath, textures);
            spriteTexture = await Game.ResourceManager.RequestResource(image);
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

            Draw();

            base.Draw(context);
        }
        /// <summary>
        /// Default sprite draw
        /// </summary>
        private void Draw()
        {
            BuiltInSpriteState state;

            if (UsePercentage)
            {
                state = new BuiltInSpriteState
                {
                    Local = GetTransform(),
                    Color1 = Color1,
                    Color2 = Color2,
                    Color3 = Color3,
                    Color4 = Color4,
                    UsePercentage = true,
                    Percentage1 = Percentage1,
                    Percentage2 = Percentage2,
                    Percentage3 = Percentage3,
                    Direction = DrawDirection,
                    Texture = spriteTexture,
                    TextureIndex = TextureIndex,
                    RenderArea = GetRenderArea(true),
                };
            }
            else
            {
                var color = Color4.AdjustSaturation(BaseColor * TintColor, 1f);
                color.Alpha *= Alpha;

                state = new BuiltInSpriteState
                {
                    Local = GetTransform(),
                    Color1 = color,
                    Texture = spriteTexture,
                    TextureIndex = TextureIndex,
                };
            }

            var drawOptions = new DrawOptions
            {
                IndexBuffer = indexBuffer,
                VertexBuffer = vertexBuffer,
                Topology = Topology.TriangleList,
            };

            if (Textured)
            {
                spriteTextureDrawer.UpdateSprite(state);
                spriteTextureDrawer.Draw(BufferManager, drawOptions);
            }
            else
            {
                spriteColorDrawer.UpdateSprite(state);
                spriteColorDrawer.Draw(BufferManager, drawOptions);
            }

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;
        }

        /// <summary>
        /// Sets the percentage values
        /// </summary>
        /// <param name="percent1">First percentage</param>
        /// <param name="percent2">Second percentage</param>
        /// <param name="percent3">Third percentage</param>
        public void SetPercentage(float percent1, float percent2 = 1.0f, float percent3 = 1.0f)
        {
            Percentage1 = percent1;
            Percentage2 = percent2;
            Percentage3 = percent3;
        }
    }
}
