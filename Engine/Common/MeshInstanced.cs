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
        /// Maximum number of instances
        /// </summary>
        protected int MaxInstaces = 0;

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
            this.MaxInstaces = maxInstances;
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

            if (this.MaxInstaces > 0)
            {
                VertexInstancingData[] instancingData = new VertexInstancingData[this.MaxInstaces];

                this.InstancingBuffer = device.CreateVertexBufferWrite(instancingData);
                this.InstanceCount = this.MaxInstaces;
                this.InstancingBufferStride = instancingData[0].Stride;

                this.AddVertexBufferBinding(new VertexBufferBinding(this.InstancingBuffer, this.InstancingBufferStride, 0));
            }
        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="deviceContext">Immediate context</param>
        public override void Draw(GameTime gameTime, DeviceContext deviceContext)
        {
            if (this.Indexed)
            {
                deviceContext.DrawIndexedInstanced(
                    this.IndexCount,
                    this.InstanceCount,
                    0, 0, 0);
            }
            else
            {
                deviceContext.DrawInstanced(
                    this.VertexCount,
                    this.InstanceCount,
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
            if (this.InstancingBuffer != null && data != null && data.Length > 0)
            {
                deviceContext.WriteBuffer(this.InstancingBuffer, data);
            }
        }
    }
}
