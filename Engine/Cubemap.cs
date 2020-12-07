using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Cube-map drawer
    /// </summary>
    public class Cubemap : Drawable
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
        /// Cube map texture
        /// </summary>
        private EngineShaderResourceView cubeMapTexture = null;

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
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Cubemap(string name, Scene scene, CubemapDescription description)
            : base(name, scene, description)
        {
            Manipulator = new Manipulator3D();

            InitializeBuffers(name, description.Geometry, description.ReverseFaces);
            InitializeTexture(description.Texture, description.Faces);
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

            var effect = DrawerPool.EffectDefaultCubemap;
            var technique = DrawerPool.EffectDefaultCubemap.ForwardCubemap;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(local, context.ViewProjection);
            effect.UpdatePerObject(cubeMapTexture);

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
        /// Set the instance texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void SetTexture(EngineShaderResourceView texture)
        {
            cubeMapTexture = texture;
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
            else throw new ArgumentException("Bad geometry enum type");

            var vertices = VertexPosition.Generate(geom.Vertices);
            var indices = reverse ? GeometryUtil.ChangeCoordinate(geom.Indices) : geom.Indices;

            vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(name, false, indices);
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="faces">Texture faces</param>
        protected void InitializeTexture(string texture, Rectangle[] faces = null)
        {
            var image = ImageContent.Cubic(texture, faces);
            cubeMapTexture = Game.ResourceManager.RequestResource(image);
        }
    }
}
