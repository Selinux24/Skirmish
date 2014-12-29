using SharpDX.Direct3D;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine.Common
{
    using Engine.Helpers;

    /// <summary>
    /// Instanced Mesh
    /// </summary>
    public class MeshInstanced : Mesh
    {
        /// <summary>
        /// Instancing data buffer
        /// </summary>
        protected Buffer InstancingBuffer = null;
        /// <summary>
        /// Instancing data cache
        /// </summary>
        protected VertexInstancingData[] InstancingData = null;

        /// <summary>
        /// Stride of instancing data
        /// </summary>
        public int InstancingBufferStride { get; set; }
        /// <summary>
        /// Instances
        /// </summary>
        public int InstanceCount { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="material">Materia name</param>
        /// <param name="vertexType">Vertex Type</param>
        /// <param name="topology">Topology</param>
        public MeshInstanced(string material, PrimitiveTopology topology, IVertexData[] vertices, uint[] indices, int maxInstances)
            : base(material, topology, vertices, indices)
        {
            this.InstancingData = new VertexInstancingData[maxInstances];
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (this.InstancingBuffer != null)
            {
                this.InstancingBuffer.Dispose();
                this.InstancingBuffer = null;
            }
        }
        /// <summary>
        /// Initializes the mesh graphics content
        /// </summary>
        /// <param name="device">Device</param>
        public override void Initialize(Device device)
        {
            base.Initialize(device);

            if (this.InstancingData != null && this.InstancingData.Length > 0)
            {
                this.InstancingBuffer = device.CreateVertexBufferWrite(this.InstancingData);
                this.InstanceCount = InstancingData.Length;
                this.InstancingBufferStride = InstancingData[0].Stride;

                this.AddVertexBufferBinding(new VertexBufferBinding(this.InstancingBuffer, this.InstancingBufferStride, 0));
            }
        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="count">Vertex or index count</param>
        public override void Draw(GameTime gameTime, DeviceContext deviceContext, int count = 0)
        {
            if (this.IndexBuffer != null)
            {
                deviceContext.DrawIndexedInstanced(
                    count == 0 ? this.IndexCount : count,
                    this.InstanceCount,
                    0, 0, 0);
            }
            else
            {
                deviceContext.DrawInstanced(
                    count == 0 ? this.VertexCount : count,
                    this.InstanceCount,
                    0, 0);
            }

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="instanceCount">Instances to draw</param>
        /// <param name="count">Vertex or index count</param>
        public virtual void Draw(GameTime gameTime, DeviceContext deviceContext, int instanceCount, int count = 0)
        {
            if (this.IndexBuffer != null)
            {
                deviceContext.DrawIndexedInstanced(
                    count == 0 ? this.IndexCount : count,
                    instanceCount == 0 ? this.InstanceCount : instanceCount,
                    0, 0, 0);
            }
            else
            {
                deviceContext.DrawInstanced(
                    count == 0 ? this.VertexCount : count,
                    instanceCount == 0 ? this.InstanceCount : instanceCount,
                    0, 0);
            }

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="data">Instancig data</param>
        public virtual void WriteInstancingData(DeviceContext deviceContext, VertexInstancingData[] data)
        {
            this.InstancingData = data;

            if (this.InstancingBuffer != null && this.InstancingData != null && this.InstancingData.Length > 0)
            {
                deviceContext.WriteBuffer(this.InstancingBuffer, this.InstancingData);
            }
        }
    }
}
