using System;
using System.Collections.Generic;
using SharpDX;
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
        /// Dynamic or inmutable buffers
        /// </summary>
        private bool dynamicBuffers = false;
        /// <summary>
        /// Position list cache
        /// </summary>
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;

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
        /// Indexed model
        /// </summary>
        protected bool Indexed = false;
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
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        public Mesh(string material, PrimitiveTopology topology, IVertexData[] vertices, uint[] indices, bool dynamic = false)
        {
            this.dynamicBuffers = dynamic;

            this.Material = material;
            this.Topology = topology;
            this.Vertices = vertices;
            this.Indexed = (indices != null && indices.Length > 0);
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
                this.VertexBuffer = VertexData.CreateVertexBuffer(device, this.Vertices, this.dynamicBuffers);
                this.VertexBufferStride = this.Vertices[0].Stride;
                this.VertexCount = this.Vertices.Length;

                this.AddVertexBufferBinding(new VertexBufferBinding(this.VertexBuffer, this.VertexBufferStride, 0));
            }

            if (this.Indices != null && this.Indices.Length > 0)
            {
                if (this.dynamicBuffers)
                {
                    this.IndexBuffer = device.CreateIndexBufferWrite((uint[])this.Indices);
                }
                else
                {
                    this.IndexBuffer = device.CreateIndexBufferImmutable((uint[])this.Indices);
                }
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
        /// <param name="deviceContext">Immediate context</param>
        public virtual void Draw(DeviceContext deviceContext)
        {
            if (this.Indexed)
            {
                if (this.IndexCount > 0)
                {
                    deviceContext.DrawIndexed(this.IndexCount, 0, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                }
            }
            else
            {
                if (this.VertexCount > 0)
                {
                    deviceContext.Draw(this.VertexCount, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                }
            }
        }
        /// <summary>
        /// Sets input layout to assembler
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="inputLayout">Layout</param>
        public virtual void SetInputAssembler(DeviceContext deviceContext, InputLayout inputLayout)
        {
            deviceContext.InputAssembler.InputLayout = inputLayout;
            Counters.IAInputLayoutSets++;
            deviceContext.InputAssembler.PrimitiveTopology = this.Topology;
            Counters.IAPrimitiveTopologySets++;
            deviceContext.InputAssembler.SetVertexBuffers(0, this.VertexBufferBinding);
            Counters.IAVertexBuffersSets++;
            deviceContext.InputAssembler.SetIndexBuffer(this.IndexBuffer, Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;
        }
        /// <summary>
        /// Writes vertex data
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="data">Vertex data</param>
        public virtual void WriteVertexData(DeviceContext deviceContext, IVertexData[] data)
        {
            if (this.dynamicBuffers)
            {
                this.Vertices = data;

                if (this.Vertices != null && this.Vertices.Length > 0)
                {
                    this.VertexCount = this.Vertices.Length;

                    if (this.VertexBuffer != null)
                    {
                        VertexData.WriteVertexBuffer(deviceContext, this.VertexBuffer, this.Vertices);
                    }
                }
                else
                {
                    this.VertexCount = 0;
                }
            }
            else
            {
                throw new Exception("Attemp to write in inmutable buffers");
            }
        }
        /// <summary>
        /// Writes index data
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="data">Index data</param>
        public virtual void WriteIndexData(DeviceContext deviceContext, uint[] data)
        {
            if (this.dynamicBuffers)
            {
                this.Indices = data;

                if (this.Indices != null && this.Indices.Length > 0)
                {
                    this.IndexCount = this.Indices.Length;

                    if (this.IndexBuffer != null)
                    {
                        deviceContext.WriteBuffer(this.IndexBuffer, this.Indices);
                    }
                }
                else
                {
                    this.IndexCount = 0;
                }
            }
            else
            {
                throw new Exception("Attemp to write in inmutable buffers");
            }
        }

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints()
        {
            if (this.positionCache == null)
            {
                List<Vector3> positionList = new List<Vector3>();

                if (this.Vertices != null && this.Vertices.Length > 0)
                {
                    if (this.Vertices[0].HasChannel(VertexDataChannels.Position))
                    {
                        Array.ForEach(this.Vertices, v =>
                        {
                            positionList.Add(v.GetChannelValue<Vector3>(VertexDataChannels.Position));
                        });
                    }
                }

                this.positionCache = positionList.ToArray();
            }

            return this.positionCache;
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles()
        {
            if (this.triangleCache == null)
            {
                Vector3[] positions = this.GetPoints();
                if (positions != null && positions.Length > 0)
                {
                    if (this.Indices != null && this.Indices.Length > 0)
                    {
                        this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions, this.Indices);
                    }
                    else
                    {
                        this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions);
                    }
                }
            }

            return this.triangleCache;
        }
    }
}
