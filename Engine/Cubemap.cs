using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Cube-map drawer
    /// </summary>
    public class Cubemap : Drawable
    {
        /// <summary>
        /// Index buffer
        /// </summary>
        protected Buffer indexBuffer = null;
        /// <summary>
        /// Index count
        /// </summary>
        protected int indexCount = 0;
        /// <summary>
        /// Vertex buffer
        /// </summary>
        protected Buffer vertexBuffer = null;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        protected VertexBufferBinding[] vertexBufferBinding = null;
        /// <summary>
        /// Cube map texture
        /// </summary>
        protected ShaderResourceView cubeMapTexture = null;

        /// <summary>
        /// Local transform
        /// </summary>
        private Matrix local = Matrix.Identity;

        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="description">Description</param>
        public Cubemap(Game game, CubemapDescription description)
            : base(game, description)
        {
            this.Manipulator = new Manipulator3D();

            this.InitializeBuffers();
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.vertexBuffer);
            Helper.Dispose(this.indexBuffer);
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
            EffectCubemap effect = DrawerPool.EffectCubemap;
            EffectTechnique technique = null;
            if (context.DrawerMode == DrawerModesEnum.Forward) { technique = effect.ForwardCubemap; }
            else if (context.DrawerMode == DrawerModesEnum.Deferred) { technique = effect.DeferredCubemap; }

            if (technique != null)
            {
                #region Per frame update

                effect.UpdatePerFrame(
                    this.local,
                    context.ViewProjection,
                    context.EyePosition,
                    context.Lights);

                #endregion

                #region Per object update

                effect.UpdatePerObject(this.cubeMapTexture);

                #endregion

                //Sets vertex and index buffer
                this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
                Counters.IAInputLayoutSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
                Counters.IAVertexBuffersSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;
                this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, 0, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                    Counters.TrianglesPerFrame += this.indexCount / 3;
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
        protected virtual void InitializeBuffers()
        {
            Vector3[] vData;
            uint[] iData;
            GeometryUtil.CreateSphere(1, 10, 10, out vData, out iData);

            VertexPosition[] vertices = VertexPosition.Generate(vData);

            this.vertexBuffer = this.Game.Graphics.Device.CreateVertexBufferImmutable(vertices);
            this.vertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.vertexBuffer, vertices[0].Stride, 0),
            };

            this.indexBuffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(iData);
            this.indexCount = iData.Length;
        }
    }
}
