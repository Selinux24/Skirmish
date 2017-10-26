using SharpDX;
using SharpDX.Direct3D;
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
        public override void Dispose()
        {

        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.Manipulator.Update(context.GameTime);

            this.local = this.Manipulator.LocalTransform * context.World;
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.indexBuffer.Count > 0)
            {
                var graphics = this.Game.Graphics;

                this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);

                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;
                }

                var effect = DrawerPool.EffectDefaultCubemap;
                var technique = effect.GetTechnique(VertexTypes.Position, false, DrawingStages.Drawing, context.DrawerMode);

                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.TriangleList);

                #region Per frame update

                effect.UpdatePerFrame(this.local, context.ViewProjection);

                #endregion

                #region Per object update

                effect.UpdatePerObject(this.cubeMapTexture);

                #endregion

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
        protected virtual void InitializeBuffers(string name, CubemapDescription.CubeMapGeometryEnum geometry, bool reverse)
        {
            Vector3[] vData;
            uint[] iData;
            if (geometry == CubemapDescription.CubeMapGeometryEnum.Box) GeometryUtil.CreateBox(1, 10, 10, out vData, out iData);
            else if (geometry == CubemapDescription.CubeMapGeometryEnum.Sphere) GeometryUtil.CreateSphere(1, 10, 10, out vData, out iData);
            else throw new ArgumentException("Bad geometry enum type");

            var vertices = new List<VertexPosition>();
            foreach (var v in vData)
            {
                vertices.Add(new VertexPosition() { Position = v });
            }

            if (reverse) iData = GeometryUtil.ChangeCoordinate(iData);

            this.vertexBuffer = this.BufferManager.Add(name, vertices.ToArray(), false, 0);
            this.indexBuffer = this.BufferManager.Add(name, iData, false);
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        protected virtual void InitializeTexture(string contentPath, params string[] textures)
        {
            var image = ImageContent.Cubic(contentPath, textures[0]);
            this.cubeMapTexture = this.Game.ResourceManager.CreateResource(image);
        }
    }
}
