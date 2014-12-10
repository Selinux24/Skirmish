using System.Collections.Generic;
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
    using SharpDX;

    /// <summary>
    /// Instanced Mesh
    /// </summary>
    public class MeshInstanced : Mesh
    {
        /// <summary>
        /// Instancing data buffer
        /// </summary>
        private Buffer instancingBuffer = null;
        /// <summary>
        /// Instancing data cache
        /// </summary>
        private VertexInstancingData[] instancingData = null;

        /// <summary>
        /// Stride of instancing data
        /// </summary>
        public int InstancingBufferStride { get; set; }
        /// <summary>
        /// Offset of the instancing data
        /// </summary>
        public int InstancingBufferOffset { get; set; }
        /// <summary>
        /// Instances
        /// </summary>
        public int InstanceCount { get; protected set; }

        /// <summary>
        /// Creates a instanced Mesh
        /// </summary>
        /// <typeparam name="T">Vertex Type</typeparam>
        /// <typeparam name="Y">Instancing data Type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="material">Material name</param>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="maxInstances">Maximum number of instances</param>
        /// <returns>Returns the new instanced mesh</returns>
        public static MeshInstanced Create(Device device, string material, PrimitiveTopology topology, IVertex[] vertices, uint[] indices, BoundingBox bbox, BoundingSphere bsphere, int maxInstances)
        {
            VertexTypes vertexType = vertices[0].VertexType;
            VertexInstancingData[] instancingData = new VertexInstancingData[maxInstances];

            MeshInstanced mesh = new MeshInstanced(material, vertexType, topology);

            mesh.vertices = vertices;
            mesh.indices = indices;
            mesh.instancingData = instancingData;
            mesh.BoundingBox = bbox;
            mesh.BoundingSphere = bsphere;

            return mesh;
        }
        /// <summary>
        /// Merges two instanced meshses
        /// </summary>
        /// <typeparam name="T">Vertex Type</typeparam>
        /// <typeparam name="Y">Instancing data Type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="material">Material name</param>
        /// <param name="meshes">Mesh list to merge</param>
        /// <param name="maxInstances">Maximum number of instances</param>
        /// <returns>Returns the merged instanced mesh</returns>
        public static MeshInstanced Merge(Device device, string material, MeshInstanced[] meshes, int maxInstances)
        {
            List<IVertex> vertices = new List<IVertex>();
            List<uint> indices = new List<uint>();
            BoundingBox bbox = new BoundingBox();
            BoundingSphere bsphere = new BoundingSphere();

            uint indexOffset = 0;

            foreach (MeshInstanced mesh in meshes)
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

            return MeshInstanced.Create(
                device, 
                material, 
                meshes[0].Topology, 
                vertices.ToArray(), 
                indices.ToArray(), 
                bbox, 
                bsphere, 
                maxInstances);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="material">Materia name</param>
        /// <param name="vertexType">Vertex Type</param>
        /// <param name="topology">Topology</param>
        protected MeshInstanced(string material, VertexTypes vertexType, PrimitiveTopology topology)
            : base(material, vertexType, topology)
        {

        }
        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (this.instancingBuffer != null)
            {
                this.instancingBuffer.Dispose();
                this.instancingBuffer = null;
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
        /// Sets input layout to assembler
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="inputLayout">Layout</param>
        public override void SetInputAssembler(DeviceContext deviceContext, InputLayout inputLayout)
        {
            deviceContext.InputAssembler.InputLayout = inputLayout;

            deviceContext.InputAssembler.SetVertexBuffers(
                0,
                new VertexBufferBinding[]
                {
                    new VertexBufferBinding(this.VertexBuffer, this.VertexBufferStride, this.VertexBufferOffset),
                    new VertexBufferBinding(this.instancingBuffer, this.InstancingBufferStride, this.InstancingBufferOffset),
                });

            deviceContext.InputAssembler.PrimitiveTopology = this.Topology;

            deviceContext.InputAssembler.SetIndexBuffer(this.IndexBuffer, Format.R32_UInt, 0);
        }
        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="data">Instancig data</param>
        public virtual void WriteInstancingData(DeviceContext deviceContext, VertexInstancingData[] data)
        {
            this.instancingData = data;

            if (this.instancingBuffer != null && data != null && data.Length > 0)
            {
                deviceContext.WriteBuffer(this.instancingBuffer, data);
            }
        }
        /// <summary>
        /// Creates instancing buffer
        /// </summary>
        /// <typeparam name="Y">Instancing buffer data Type</typeparam>
        /// <param name="device">Device</param>
        public void CreateInstancingBuffer(Device device)
        {
            this.instancingBuffer = device.CreateVertexBufferWrite(this.instancingData);
            this.InstanceCount = instancingData.Length;
            this.InstancingBufferOffset = 0;
            this.InstancingBufferStride = VertexInstancingData.SizeInBytes;
        }
    }
}
