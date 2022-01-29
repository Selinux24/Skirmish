using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Cube-map drawer
    /// </summary>
    public class Cubemap<T> : Drawable<T>, IHasGameState where T : CubemapDescription
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
        /// Local transform
        /// </summary>
        private Matrix local = Matrix.Identity;
        /// <summary>
        /// Texture
        /// </summary>
        private EngineShaderResourceView texture = null;
        /// <summary>
        /// Texture cubic
        /// </summary>
        private bool textureCubic;

        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; set; }
        /// <summary>
        /// Returns true if the buffers were ready
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                if (vertexBuffer?.Ready != true)
                {
                    return false;
                }

                if (indexBuffer?.Ready != true)
                {
                    return false;
                }

                if (indexBuffer.Count <= 0)
                {
                    return false;
                }

                return true;
            }
        }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public Cubemap(Scene scene, string id, string name)
            : base(scene, id, name)
        {

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
        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(T description)
        {
            await base.InitializeAssets(description);

            Manipulator = new Manipulator3D();
            textureCubic = Description.IsCubic;

            InitializeBuffers(Name, Description.Geometry, Description.ReverseFaces);

            if (textureCubic)
            {
                await InitializeTextureCubic(Description.CubicTexture, Description.Faces);
            }
            else
            {
                await InitializeTextureArray(Description.PlainTextures);
            }
        }
        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="geometry">Geometry to use</param>
        /// <param name="reverse">Reverse faces</param>
        protected void InitializeBuffers(string name, CubemapDescription.CubeMapGeometry geometry, bool reverse)
        {
            GeometryDescriptor geom;
            if (geometry == CubemapDescription.CubeMapGeometry.Box) geom = GeometryUtil.CreateBox(1, 10, 10);
            else if (geometry == CubemapDescription.CubeMapGeometry.Sphere) geom = GeometryUtil.CreateSphere(1, 10, 10);
            else if (geometry == CubemapDescription.CubeMapGeometry.Hemispheric) geom = GeometryUtil.CreateHemispheric(1, 10, 10);
            else throw new ArgumentException("Bad geometry enum type", nameof(geometry));

            if (textureCubic)
            {
                var vertices = VertexPosition.Generate(geom.Vertices);
                vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            }
            else
            {
                var vertices = VertexPositionTexture.Generate(geom.Vertices, geom.Uvs);
                vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            }

            var indices = reverse ? GeometryUtil.ChangeCoordinate(geom.Indices) : geom.Indices;
            indexBuffer = BufferManager.AddIndexData(name, false, indices);
        }
        /// <summary>
        /// Initialize cubic texture
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="faces">Texture faces</param>
        protected async Task InitializeTextureCubic(string textureFileName, Rectangle[] faces = null)
        {
            var image = new FileCubicImageContent(textureFileName, faces);

            texture = await Game.ResourceManager.RequestResource(image);
        }
        /// <summary>
        /// Initialize texture array
        /// </summary>
        /// <param name="textureFileNames">Texture file names</param>
        protected async Task InitializeTextureArray(IEnumerable<string> textureFileNames)
        {
            var image = new FileArrayImageContent(textureFileNames);

            texture = await Game.ResourceManager.RequestResource(image);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            Manipulator.Update(context.GameTime);

            local = Manipulator.LocalTransform;
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

            if (textureCubic)
            {
                DrawCubic(context);
            }
            else
            {
                DrawPlain(context);
            }
        }
        /// <summary>
        /// Draws the cubic texture
        /// </summary>
        /// <param name="context">Draw context</param>
        private void DrawCubic(DrawContext context)
        {
            var effect = DrawerPool.EffectDefaultCubemap;
            var technique = DrawerPool.EffectDefaultCubemap.ForwardCubemap;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(local, context.ViewProjection);
            effect.UpdatePerObject(texture);

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
        /// Draws the plain texture
        /// </summary>
        /// <param name="context">Draw context</param>
        private void DrawPlain(DrawContext context)
        {
            var effect = DrawerPool.EffectDefaultTexture;
            var technique = DrawerPool.EffectDefaultTexture.SimpleTexture;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(local, context.ViewProjection);
            effect.UpdatePerObject(TextureIndex, texture);

            var graphics = Game.Graphics;

            graphics.SetRasterizerCullNone();

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
        /// Set the instance texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void SetTexture(EngineShaderResourceView texture)
        {
            this.texture = texture;
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new CubemapState
            {
                Name = Name,
                Active = Active,
                Visible = Visible,
                Usage = Usage,
                Layer = Layer,
                OwnerId = Owner?.Name,

                Local = local,
                Manipulator = Manipulator.GetState(),
                TextureIndex = TextureIndex,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (!(state is CubemapState cubemapState))
            {
                return;
            }

            Name = cubemapState.Name;
            Active = cubemapState.Active;
            Visible = cubemapState.Visible;
            Usage = cubemapState.Usage;
            Layer = cubemapState.Layer;

            if (!string.IsNullOrEmpty(cubemapState.OwnerId))
            {
                Owner = Scene.GetComponents().FirstOrDefault(c => c.Id == cubemapState.OwnerId);
            }

            local = cubemapState.Local;
            Manipulator?.SetState(cubemapState.Manipulator);
            TextureIndex = cubemapState.TextureIndex;
        }
    }
}
