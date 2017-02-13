using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using InputElement = SharpDX.Direct3D11.InputElement;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
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
        /// Position list cache
        /// </summary>
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;

        /// <summary>
        /// Vertices cache
        /// </summary>
        public IVertexData[] Vertices = null;
        /// <summary>
        /// Indexed model
        /// </summary>
        public bool Indexed = false;
        /// <summary>
        /// Indices cache
        /// </summary>
        public uint[] Indices = null;

        public int BufferSlot = -1;
        public int VertexBufferOffset = -1;
        public int IndexBufferOffset = -1;
        public int InstancingBufferOffset = 2;

        private Dictionary<EffectTechnique, InputLayout> dict = new Dictionary<EffectTechnique, InputLayout>();

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
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
        public int VertexBufferStride { get; internal set; }
        /// <summary>
        /// Vertex count
        /// </summary>
        public int VertexCount { get; internal set; }
        /// <summary>
        /// Index count
        /// </summary>
        public int IndexCount { get; internal set; }
        /// <summary>
        /// Is instanced
        /// </summary>
        public bool Instanced { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Mesh name</param>
        /// <param name="material">Material name</param>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="instanced">Instanced</param>
        public Mesh(string name, string material, PrimitiveTopology topology, IVertexData[] vertices, uint[] indices, bool instanced)
        {
            this.Name = name;
            this.Material = material;
            this.Topology = topology;
            this.Vertices = vertices;
            this.Indexed = (indices != null && indices.Length > 0);
            this.Indices = indices;
            this.VertextType = vertices[0].VertexType;
            this.Textured = VertexData.IsTextured(vertices[0].VertexType);
            this.Skinned = VertexData.IsSkinned(vertices[0].VertexType);
            this.Instanced = instanced;
        }
        /// <summary>
        /// Initializes the mesh graphics content
        /// </summary>
        /// <param name="device">Device</param>
        public virtual void Initialize(Device device)
        {

        }
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {

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
                    deviceContext.DrawIndexed(this.IndexCount, this.IndexBufferOffset, 0);
                }
            }
            else
            {
                if (this.VertexCount > 0)
                {
                    deviceContext.Draw(this.VertexCount, this.VertexBufferOffset);
                }
            }
        }
        /// <summary>
        /// Draw mesh geometry
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        /// <param name="count">Instances to draw</param>
        public virtual void Draw(DeviceContext deviceContext, int startInstanceLocation, int count)
        {
            if (count > 0)
            {
                if (this.Indexed)
                {
                    if (this.IndexCount > 0)
                    {
                        deviceContext.DrawIndexedInstanced(
                            this.IndexCount,
                            count,
                            this.IndexBufferOffset, 0, startInstanceLocation);
                    }
                }
                else
                {
                    if (this.VertexCount > 0)
                    {
                        deviceContext.DrawInstanced(
                            this.VertexCount,
                            count,
                            this.VertexBufferOffset, startInstanceLocation);
                    }
                }
            }
        }
        /// <summary>
        /// Sets input layout to assembler
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="inputLayout">Layout</param>
        public virtual void SetInputAssembler(Graphics graphics, EffectTechnique technique)
        {
            if (!dict.ContainsKey(technique))
            {
                List<InputElement> input = new List<InputElement>(this.Vertices[0].GetInput(this.BufferSlot));

                if (this.Instanced)
                {
                    input.AddRange(VertexInstancingData.GetInput(this.InstancingBufferOffset));
                }

                var desc = technique.GetPassByIndex(0).Description;

                dict.Add(technique, new InputLayout(graphics.Device, desc.Signature, input.ToArray()));
            }

            graphics.DeviceContext.InputAssembler.InputLayout = dict[technique];
            Counters.IAInputLayoutSets++;
            graphics.DeviceContext.InputAssembler.PrimitiveTopology = this.Topology;
            Counters.IAPrimitiveTopologySets++;
        }

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints(bool refresh = false)
        {
            if (refresh || this.positionCache == null)
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
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints(Matrix[] boneTransforms, bool refresh = false)
        {
            if (refresh || this.positionCache == null)
            {
                Vector3[] res = new Vector3[this.Vertices.Length];

                for (int i = 0; i < this.Vertices.Length; i++)
                {
                    res[i] = VertexData.ApplyWeight(this.Vertices[i], boneTransforms);
                }

                this.positionCache = res;
            }

            return this.positionCache;
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles(bool refresh = false)
        {
            if (refresh || this.triangleCache == null)
            {
                Vector3[] positions = this.GetPoints(refresh);
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
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="boneTransforms">Bone transforms</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles(Matrix[] boneTransforms, bool refresh = false)
        {
            if (refresh || this.triangleCache == null)
            {
                Vector3[] positions = this.GetPoints(boneTransforms, refresh);

                if (this.Indices != null && this.Indices.Length > 0)
                {
                    this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions, this.Indices);
                }
                else
                {
                    this.triangleCache = Triangle.ComputeTriangleList(this.Topology, positions);
                }
            }

            return this.triangleCache;
        }
    }
}
