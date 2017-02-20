using SharpDX;
using SharpDX.Direct3D;
using System;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

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
        /// Local transform
        /// </summary>
        private Matrix local = Matrix.Identity;
        /// <summary>
        /// Vertex buffer offset
        /// </summary>
        private int vertexBuferOffset;
        /// <summary>
        /// Vertex buffer offset
        /// </summary>
        private int vertexBufferSlot;
        /// <summary>
        /// Vertex count
        /// </summary>
        private int vertexCount;
        /// <summary>
        /// Index buffer offset
        /// </summary>
        private int indexBufferOffset;
        /// <summary>
        /// Index buffer slot
        /// </summary>
        private int indexBufferSlot;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;
        /// <summary>
        /// Cube map texture
        /// </summary>
        private ShaderResourceView cubeMapTexture = null;

        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; set; }
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
        /// <param name="game">Game class</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="content">Content</param>
        /// <param name="description">Description</param>
        public Cubemap(Game game, BufferManager bufferManager, CubemapDescription description)
            : base(game, bufferManager, description)
        {
            this.Manipulator = new Manipulator3D();

            this.InitializeBuffers(description.Geometry, description.ReverseFaces);
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
            if (this.indexCount > 0)
            {
                this.BufferManager.SetIndexBuffer(this.indexBufferSlot);

                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexCount / 3;
                }

                var effect = DrawerPool.EffectDefaultCubemap;
                var technique = effect.GetTechnique(VertexTypes.Position, false, DrawingStages.Drawing, context.DrawerMode);

                this.BufferManager.SetInputAssembler(technique, this.vertexBufferSlot, PrimitiveTopology.TriangleList);

                #region Per frame update

                effect.UpdatePerFrame(this.local, context.ViewProjection);

                #endregion

                #region Per object update

                effect.UpdatePerObject(this.cubeMapTexture);

                #endregion

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, this.indexBufferOffset, this.vertexBuferOffset);

                    Counters.DrawCallsPerFrame++;
                }
            }
        }

        /// <summary>
        /// Set the instance texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void SetTexture(ShaderResourceView texture)
        {
            this.cubeMapTexture = texture;
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="geometry">Geometry to use</param>
        /// <param name="reverse">Reverse faces</param>
        protected virtual void InitializeBuffers(CubemapDescription.CubeMapGeometryEnum geometry, bool reverse)
        {
            Vector3[] vData;
            uint[] iData;
            if (geometry == CubemapDescription.CubeMapGeometryEnum.Box) GeometryUtil.CreateBox(1, 10, 10, out vData, out iData);
            else if (geometry == CubemapDescription.CubeMapGeometryEnum.Sphere) GeometryUtil.CreateSphere(1, 10, 10, out vData, out iData);
            else throw new ArgumentException("Bad geometry enum type");

            VertexPosition[] vertices = new VertexPosition[vData.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new VertexPosition() { Position = vData[i] };
            }

            if (reverse) iData = GeometryUtil.ChangeCoordinate(iData);

            this.BufferManager.Add(this.Name, vertices, false, 0, out this.vertexBuferOffset, out this.vertexBufferSlot);
            this.BufferManager.Add(this.Name, iData, false, out this.indexBufferOffset, out this.indexBufferSlot);

            this.vertexCount = vertices.Length;
            this.indexCount = iData.Length;
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="textures">Texture names</param>
        protected virtual void InitializeTexture(string contentPath, params string[] textures)
        {
            var image = ImageContent.Array(contentPath, textures);
            this.cubeMapTexture = this.Game.ResourceManager.CreateResource(image);
        }
    }
}
