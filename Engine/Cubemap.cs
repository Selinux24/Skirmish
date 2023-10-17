using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Cubemap;
    using Engine.Common;
    using Engine.Content;

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
        /// Texture
        /// </summary>
        private EngineShaderResourceView texture = null;
        /// <summary>
        /// Texture cubic
        /// </summary>
        private bool textureCubic;

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

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(T description)
        {
            await base.ReadAssets(description);

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
            if (geometry == CubemapDescription.CubeMapGeometry.Box) geom = GeometryUtil.CreateBox(Topology.TriangleList, 1, 10, 10);
            else if (geometry == CubemapDescription.CubeMapGeometry.Sphere) geom = GeometryUtil.CreateSphere(Topology.TriangleList, 1, 10, 10);
            else if (geometry == CubemapDescription.CubeMapGeometry.Hemispheric) geom = GeometryUtil.CreateHemispheric(Topology.TriangleList, 1, 10, 10);
            else throw new ArgumentException("Bad geometry enumeration type", nameof(geometry));

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
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (!BuffersReady)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            if (textureCubic)
            {
                return DrawCubic(context.DeviceContext);
            }
            else
            {
                return DrawPlain(context.DeviceContext);
            }
        }
        /// <summary>
        /// Draws the cubic texture
        /// </summary>
        /// <param name="dc">Device context</param>
        private bool DrawCubic(IEngineDeviceContext dc)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInCubemap>();
            if (drawer == null)
            {
                return false;
            }

            drawer.Update(texture);

            return drawer.Draw(dc, BufferManager, new DrawOptions
            {
                IndexBuffer = indexBuffer,
                VertexBuffer = vertexBuffer,
                Topology = Topology.TriangleList,
            });
        }
        /// <summary>
        /// Draws the plain texture
        /// </summary>
        /// <param name="dc">Device context</param>
        private bool DrawPlain(IEngineDeviceContext dc)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInSkymap>();
            if (drawer == null)
            {
                return false;
            }

            drawer.Update(dc, texture, TextureIndex);

            dc.SetRasterizerState(Game.Graphics.GetRasterizerCullNone());

            return drawer.Draw(dc, BufferManager, new DrawOptions
            {
                IndexBuffer = indexBuffer,
                VertexBuffer = vertexBuffer,
                Topology = Topology.TriangleList,
            });
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

                TextureIndex = TextureIndex,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not CubemapState cubemapState)
            {
                return;
            }

            Name = cubemapState.Name;
            Active = cubemapState.Active;
            Visible = cubemapState.Visible;
            Usage = cubemapState.Usage;
            Layer = cubemapState.Layer;
            Owner = Scene.Components.ById(cubemapState.OwnerId);
            TextureIndex = cubemapState.TextureIndex;
        }
    }
}
