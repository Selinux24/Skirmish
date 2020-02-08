using SharpDX;
using System;
using System.Collections.Generic;

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
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Cubemap(Scene scene, CubemapDescription description)
            : base(scene, description)
        {
            this.Manipulator = new Manipulator3D();

            this.InitializeBuffers(description.Name, description.Geometry, description.ReverseFaces);
            this.InitializeTexture(description.ContentPath, description.Texture);
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                this.BufferManager?.RemoveVertexData(this.vertexBuffer);
                this.BufferManager?.RemoveIndexData(this.indexBuffer);
            }
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.Manipulator.Update(context.GameTime);

            this.local = this.Manipulator.LocalTransform;
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
                var effect = DrawerPool.EffectDefaultCubemap;
                var technique = DrawerPool.EffectDefaultCubemap.ForwardCubemap;

                if (!mode.HasFlag(DrawerModes.ShadowMap))
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;
                }

                this.BufferManager.SetIndexBuffer(this.indexBuffer);
                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer, Topology.TriangleList);

                effect.UpdatePerFrame(this.local, context.ViewProjection);
                effect.UpdatePerObject(this.cubeMapTexture);

                var graphics = this.Game.Graphics;

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    graphics.DrawIndexed(
                        this.indexBuffer.Count,
                        this.indexBuffer.BufferOffset,
                        this.vertexBuffer.BufferOffset);
                }
            }
        }

        /// <summary>
        /// Set the instance texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void SetTexture(EngineShaderResourceView texture)
        {
            this.cubeMapTexture = texture;
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

            this.vertexBuffer = this.BufferManager.AddVertexData(name, false, vertices);
            this.indexBuffer = this.BufferManager.AddIndexData(name, false, indices);
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        protected void InitializeTexture(string contentPath, params string[] textures)
        {
            var image = ImageContent.Cubic(contentPath, textures[0]);
            this.cubeMapTexture = this.Game.ResourceManager.RequestResource(image);
        }
    }
}
