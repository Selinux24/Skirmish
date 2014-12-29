using System;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine.Common
{
    using Engine.Helpers;

    /// <summary>
    /// Mesh
    /// </summary>
    public class Mesh : IDisposable
    {
        /// <summary>
        /// Vertex buffer
        /// </summary>
        protected Buffer VertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        protected VertexBufferBinding[] VertexBufferBinding = new VertexBufferBinding[0];
        /// <summary>
        /// Vertices cache
        /// </summary>
        protected IVertexData[] Vertices = null;
        /// <summary>
        /// Index buffer
        /// </summary>
        protected Buffer IndexBuffer;
        /// <summary>
        /// Indices cache
        /// </summary>
        protected uint[] Indices = null;

        /// <summary>
        /// Material name
        /// </summary>
        public string Material { get; private set; }
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertextType { get; private set; }
        /// <summary>
        /// Has skin
        /// </summary>
        public bool Skinned { get; private set; }
        /// <summary>
        /// Has textures
        /// </summary>
        public bool Textured { get; private set; }
        /// <summary>
        /// Topology
        /// </summary>
        public PrimitiveTopology Topology { get; private set; }
        /// <summary>
        /// Stride
        /// </summary>
        public int VertexBufferStride { get; protected set; }
        /// <summary>
        /// Vertex count
        /// </summary>
        public int VertexCount { get; protected set; }
        /// <summary>
        /// Index count
        /// </summary>
        public int IndexCount { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="material">Material name</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="topology">Topology</param>
        public Mesh(string material, PrimitiveTopology topology, IVertexData[] vertices, uint[] indices)
        {
            this.Material = material;
            this.Topology = topology;
            this.Vertices = vertices;
            this.Indices = indices;
            this.VertextType = vertices[0].VertexType;
            this.Textured = VertexData.IsTextured(vertices[0].VertexType);
            this.Skinned = VertexData.IsSkinned(vertices[0].VertexType);
        }
        /// <summary>
        /// Initializes the mesh graphics content
        /// </summary>
        /// <param name="device">Device</param>
        public virtual void Initialize(Device device)
        {
            if (this.Vertices != null && this.Vertices.Length > 0)
            {
                this.VertexBuffer = VertexData.CreateVertexBuffer(device, this.Vertices);
                this.VertexBufferStride = this.Vertices[0].Stride;
                this.VertexCount = this.Vertices.Length;

                this.AddVertexBufferBinding(new VertexBufferBinding(this.VertexBuffer, this.VertexBufferStride, 0));
            }

            if (this.Indices != null && this.Indices.Length > 0)
            {
                this.IndexBuffer = device.CreateIndexBufferImmutable((uint[])this.Indices);
                this.IndexCount = this.Indices.Length;
            }
        }
        /// <summary>
        /// Adds binding to precached buffer bindings for input assembler
        /// </summary>
        /// <param name="binding">Binding</param>
        public virtual void AddVertexBufferBinding(VertexBufferBinding binding)
        {
            Array.Resize(ref this.VertexBufferBinding, this.VertexBufferBinding.Length + 1);

            this.VertexBufferBinding[this.VertexBufferBinding.Length - 1] = binding;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            if (this.VertexBuffer != null)
            {
                this.VertexBuffer.Dispose();
                this.VertexBuffer = null;
            }

            if (this.IndexBuffer != null)
            {
                this.IndexBuffer.Dispose();
                this.IndexBuffer = null;
            }
        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="count">vertex or index count</param>
        public virtual void Draw(GameTime gameTime, DeviceContext deviceContext, int count = 0)
        {
            if (this.IndexBuffer != null)
            {
                deviceContext.DrawIndexed(count == 0 ? this.IndexCount : count, 0, 0);
            }
            else
            {
                deviceContext.Draw(count == 0 ? this.VertexCount : count, 0);
            }

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Sets input layout to assembler
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="inputLayout">Layout</param>
        public virtual void SetInputAssembler(DeviceContext deviceContext, InputLayout inputLayout)
        {
            deviceContext.InputAssembler.InputLayout = inputLayout;
            deviceContext.InputAssembler.PrimitiveTopology = this.Topology;
            deviceContext.InputAssembler.SetVertexBuffers(0, this.VertexBufferBinding);
            deviceContext.InputAssembler.SetIndexBuffer(this.IndexBuffer, Format.R32_UInt, 0);
        }
        /// <summary>
        /// Writes vertex data
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="data">Vertex data</param>
        public virtual void WriteVertexData(DeviceContext deviceContext, IVertexData[] data)
        {
            this.Vertices = data;

            if (this.VertexBuffer != null && this.Vertices != null && this.Vertices.Length > 0)
            {
                VertexData.WriteVertexBuffer(deviceContext, this.VertexBuffer, this.Vertices);
            }
        }
        /// <summary>
        /// Writes index data
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="data">Index data</param>
        public virtual void WriteIndexData(DeviceContext deviceContext, uint[] data)
        {
            this.Indices = data;

            if (this.IndexBuffer != null && this.Indices != null && this.Indices.Length > 0)
            {
                deviceContext.WriteBuffer(this.IndexBuffer, this.Indices);
            }
        }
    }
}
