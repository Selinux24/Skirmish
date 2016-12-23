using SharpDX;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Sky dom
    /// </summary>
    /// <remarks>
    /// It's a cubemap that fits his position with the eye camera position
    /// </remarks>
    public class Skydom : Cubemap
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="content">Content</param>
        /// <param name="description">Skydom description</param>
        public Skydom(Game game, SkydomDescription description)
            : base(game, description)
        {
            this.Cull = false;
        }

        /// <summary>
        /// Updates object state
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            this.Manipulator.SetPosition(context.EyePosition);

            base.Update(context);
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        protected override void InitializeBuffers()
        {
            Vector3[] vData;
            uint[] iData;
            GeometryUtil.CreateSphere(1, 10, 10, out vData, out iData);

            VertexPosition[] vertices = VertexPosition.Generate(vData);

            var indices = Helper.ChangeCoordinate(iData);

            this.vertexBuffer = this.Game.Graphics.Device.CreateVertexBufferImmutable(vertices);
            this.vertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.vertexBuffer, vertices[0].Stride, 0),
            };

            this.indexBuffer = this.Game.Graphics.Device.CreateIndexBufferImmutable(indices);
            this.indexCount = indices.Length;
        }
    }
}
