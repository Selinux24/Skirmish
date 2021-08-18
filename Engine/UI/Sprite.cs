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
        public int TextureIndex { get; set; }
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
        public int DrawDirection { get; set; }
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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Sprite(string id, string name, Scene scene, SpriteDescription description)
            : base(id, name, scene, description)
        {
            Color1 = description.Color1;
            Color2 = description.Color2;
            Color3 = description.Color3;
            Color4 = description.Color4;
            Percentage1 = description.Percentage1;
            Percentage2 = description.Percentage2;
            Percentage3 = description.Percentage3;
            DrawDirection = (int)description.DrawDirection;
            Textured = description.Textures?.Any() == true;
            TextureIndex = description.TextureIndex;

            InitializeBuffers(name, Textured, description.UVMap);

            if (Textured)
            {
                InitializeTexture(description.ContentPath, description.Textures);
            }

            viewProjection = Game.Form.GetOrthoProjectionMatrix();
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
        private void InitializeTexture(string contentPath, string[] textures)
        {
            var image = ImageContent.Array(contentPath, textures);
            spriteTexture = Game.ResourceManager.RequestResource(image);
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

            if (UsePercentage)
            {
                DrawPct();
            }
            else
            {
                Draw();
            }
        }
        /// <summary>
        /// Default sprite draw
        /// </summary>
        private void Draw()
        {
            var effect = DrawerPool.EffectDefaultSprite;
            var technique = effect.GetTechnique(
                Textured ? VertexTypes.PositionTexture : VertexTypes.PositionColor,
                ColorChannels.All);

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(
                GetTransform(),
                viewProjection,
                Game.Form.RenderRectangle.BottomRight);

            var color = Color4.AdjustSaturation(BaseColor * TintColor, 1f);
            color.Alpha *= Alpha;

            effect.UpdatePerObject(color, spriteTexture, TextureIndex);

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(
                    indexBuffer.Count,
                    indexBuffer.BufferOffset,
                    vertexBuffer.BufferOffset);
            }
        }
        /// <summary>
        /// Percentage sprite draw
        /// </summary>
        private void DrawPct()
        {
            var effect = DrawerPool.EffectDefaultSprite;
            var technique = effect.GetTechniquePct(
                Textured ? VertexTypes.PositionTexture : VertexTypes.PositionColor);

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(
                GetTransform(),
                viewProjection,
                Game.Form.RenderRectangle.BottomRight);

            var color = BaseColor;
            color.Alpha *= Alpha;
            var color2 = TintColor;
            color2.Alpha *= Alpha;

            var parameters = new SpriteEffectParameters(
                new[] { Color1, Color2, Color3, Color4 },
                new[] { Percentage1, Percentage2, Percentage3 },
                DrawDirection,
                GetRenderArea(true));

            effect.UpdatePerObjectPct(
                parameters,
                spriteTexture,
                TextureIndex);

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(
                    indexBuffer.Count,
                    indexBuffer.BufferOffset,
                    vertexBuffer.BufferOffset);
            }
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            base.Resize();

            viewProjection = Game.Form.GetOrthoProjectionMatrix();
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

    /// <summary>
    /// Sprite extensions
    /// </summary>
    public static class SpriteExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Sprite> AddComponentSprite(this Scene scene, string id, string name, SpriteDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerDefault)
        {
            Sprite component = null;

            await Task.Run(() =>
            {
                component = new Sprite(id, name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
