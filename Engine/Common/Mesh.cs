using System;
using System.Collections;
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
        /// Vertex buffer
        /// </summary>
        protected Buffer VertexBuffer;
        /// <summary>
        /// Index buffer
        /// </summary>
        protected Buffer IndexBuffer;
        /// <summary>
        /// Vertices cache
        /// </summary>
        protected ICollection vertices = null;
        /// <summary>
        /// Indices cache
        /// </summary>
        protected ICollection indices = null;

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
        /// Offset
        /// </summary>
        public int VertexBufferOffset { get; protected set; }
        /// <summary>
        /// Vertex count
        /// </summary>
        public int VertexCount { get; protected set; }
        /// <summary>
        /// Index count
        /// </summary>
        public int IndexCount { get; protected set; }
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; protected set; }
        /// <summary>
        /// Bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; protected set; }

        /// <summary>
        /// Creates a Mesh
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="material">Material name</param>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <returns>Returns a new mesh</returns>
        public static Mesh Create(Device device, string material, PrimitiveTopology topology, IVertex[] vertices, uint[] indices, BoundingBox bbox, BoundingSphere bsphere)
        {
            VertexTypes vertexType = vertices[0].VertexType;

            Mesh mesh = new Mesh(material, vertexType, topology);

            mesh.vertices = vertices;
            mesh.indices = indices;
            mesh.BoundingBox = bbox;
            mesh.BoundingSphere = bsphere;

            return mesh;
        }
        /// <summary>
        /// Merges a list of meshes
        /// </summary>
        /// <typeparam name="T">Vertex type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="material">Material name</param>
        /// <param name="meshes">Mesh list</param>
        /// <returns>Returns a new mesh</returns>
        public static Mesh Merge(Device device, string material, Mesh[] meshes)
        {
            List<IVertex> vertices = new List<IVertex>();
            List<uint> indices = new List<uint>();
            BoundingBox bbox = new BoundingBox();
            BoundingSphere bsphere = new BoundingSphere();

            uint indexOffset = 0;

            foreach (Mesh mesh in meshes)
            {
                if (mesh.vertices != null && mesh.vertices.Count > 0)
                {
                    foreach (IVertex v in mesh.vertices)
                    {
                        vertices.Add(v);
                    }
                }

                if (mesh.indices != null && mesh.indices.Count > 0)
                {
                    foreach (uint i in mesh.indices)
                    {
                        indices.Add(indexOffset + i);
                    }
                }

                bbox = BoundingBox.Merge(bbox, mesh.BoundingBox);
                bsphere = BoundingSphere.Merge(bsphere, mesh.BoundingSphere);

                indexOffset = (uint)vertices.Count;

                mesh.Dispose();
            }

            return Mesh.Create(
                device, 
                material, 
                meshes[0].Topology, 
                vertices.ToArray(), 
                indices.ToArray(),
                bbox,
                bsphere);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="material">Material name</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="topology">Topology</param>
        protected Mesh(string material, VertexTypes vertexType, PrimitiveTopology topology)
        {
            this.Material = material;
            this.VertextType = vertexType;
            this.Topology = topology;
            this.Textured = Vertex.IsTextured(vertexType);
            this.Skinned = Vertex.IsSkinned(vertexType);
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

            deviceContext.InputAssembler.SetVertexBuffers(
                0,
                new VertexBufferBinding[]
                {
                    new VertexBufferBinding(this.VertexBuffer, this.VertexBufferStride, this.VertexBufferOffset),
                });

            deviceContext.InputAssembler.PrimitiveTopology = this.Topology;

            deviceContext.InputAssembler.SetIndexBuffer(this.IndexBuffer, Format.R32_UInt, 0);
        }
        /// <summary>
        /// Creates vertex buffer
        /// </summary>
        /// <param name="device">Device</param>
        public void CreateVertexBuffer(Device device)
        {
            Buffer buffer;
            int stride;
            Vertex.CreateVertexBuffer(
                device,
                this.VertextType,
                this.vertices,
                out buffer,
                out stride);

            this.VertexBuffer = buffer;
            this.VertexBufferStride = stride;
            this.VertexBufferOffset = 0;
            this.VertexCount = this.vertices.Count;
        }
        /// <summary>
        /// Creates index buffer
        /// </summary>
        /// <param name="device">Device</param>
        public void CreateIndexBuffer(Device device)
        {
            if (this.indices != null && this.indices.Count > 0)
            {
                this.IndexBuffer = device.CreateIndexBufferImmutable((uint[])this.indices);
                this.IndexCount = this.indices.Count;
            }
        }


        public BoundingSphere ComputeBoundingSphere(Matrix transform)
        {
            //TODO: Apply transform to all vertices and recompute bsphere
            return this.BoundingSphere;
        }

        public BoundingBox ComputeBoundingBox(Matrix transform)
        {
            //TODO: Apply transform to all vertices and recompute bbox
            return this.BoundingBox;
        }
    }
}
