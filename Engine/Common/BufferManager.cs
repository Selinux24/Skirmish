using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using InputElement = SharpDX.Direct3D11.InputElement;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine.Common
{
    using Engine.Helpers;

    /// <summary>
    /// Buffer manager
    /// </summary>
    public class BufferManager : IDisposable
    {
        /// <summary>
        /// Vertex data
        /// </summary>
        class VertexData
        {
            /// <summary>
            /// Vertices
            /// </summary>
            public List<IVertexData> Vertices = new List<IVertexData>();
            /// <summary>
            /// Input elements
            /// </summary>
            public List<InputElement> Input = new List<InputElement>();
        }

        /// <summary>
        /// Vertex data dictionary by vertex type
        /// </summary>
        private Dictionary<VertexTypes, VertexData> vList = new Dictionary<VertexTypes, VertexData>();
        /// <summary>
        /// Index data list
        /// </summary>
        private List<uint> iList = new List<uint>();
        /// <summary>
        /// Input layouts by technique
        /// </summary>
        private Dictionary<EffectTechnique, InputLayout> inputLayouts = new Dictionary<EffectTechnique, InputLayout>();

        /// <summary>
        /// Vertex buffers
        /// </summary>
        protected Buffer[] VertexBuffers = null;
        /// <summary>
        /// Index buffer
        /// </summary>
        protected Buffer IndexBuffer = null;
        /// <summary>
        /// Vertex buffer bindings
        /// </summary>
        protected VertexBufferBinding[] VertexBufferBindings = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public BufferManager()
        {

        }
        /// <summary>
        /// Free resources from memory
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.VertexBuffers);
            this.VertexBuffers = null;

            Helper.Dispose(this.IndexBuffer);
            this.IndexBuffer = null;
        }

        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <typeparam name="T">Type of vertex</typeparam>
        /// <param name="id">Id</param>
        /// <param name="vertexData">Vertex list</param>
        /// <param name="offset">Returns the assigned offset in buffer</param>
        /// <param name="slot">Returns the assigned buffer slot</param>
        public void Add<T>(int id, T[] vertexData, out int offset, out int slot) where T : struct, IVertexData
        {
            List<IVertexData> verts = new List<IVertexData>();
            Array.ForEach(vertexData, v => verts.Add(v));

            Add(id, verts.ToArray(), out offset, out slot);
        }
        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="vertexData">Vertex list</param>
        /// <param name="offset">Returns the assigned offset in buffer</param>
        /// <param name="slot">Returns the assigned buffer slot</param>
        public void Add(int id, IVertexData[] vertexData, out int offset, out int slot)
        {
            offset = -1;
            slot = -1;

            if (vertexData != null && vertexData.Length > 0)
            {
                VertexTypes vType = vertexData[0].VertexType;

                if (!vList.ContainsKey(vType))
                {
                    vList.Add(vType, new VertexData());
                }

                var vTypes = vList.Keys.ToArray();
                slot = Array.IndexOf(vTypes, vType);

                var vData = vList[vType];
                offset = vData.Vertices.Count;
                vData.Vertices.AddRange(vertexData);
            }
        }
        /// <summary>
        /// Adds indices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="indexData">Index list</param>
        /// <param name="offset">Returns the assigned offset in buffer</param>
        public void Add(int id, uint[] indexData, out int offset)
        {
            offset = -1;

            if (indexData != null && indexData.Length > 0)
            {
                offset = iList.Count;
                iList.AddRange(indexData);
            }
        }

        /// <summary>
        /// Creates and populates the buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="dynamic">Use dynamic buffers</param>
        /// <param name="instances">Instance count</param>
        public void CreateBuffers(Graphics graphics, string name, bool dynamic, int instances)
        {
            this.VertexBuffers = new Buffer[vList.Keys.Count + (instances > 0 ? 1 : 0)];
            this.VertexBufferBindings = new VertexBufferBinding[vList.Keys.Count + (instances > 0 ? 1 : 0)];

            var vTypes = vList.Keys.ToArray();

            for (int i = 0; i < vTypes.Length; i++)
            {
                var vData = vList[vTypes[i]];
                var verts = vData.Vertices.ToArray();

                this.VertexBuffers[i] = graphics.Device.CreateVertexBuffer(name, verts, dynamic);
                this.VertexBufferBindings[i] = new VertexBufferBinding(this.VertexBuffers[i], verts[0].GetStride(), 0);

                vData.Input.AddRange(verts[0].GetInput(i));
            }

            if (instances > 0)
            {
                int instancingBufferOffset = this.VertexBuffers.Length - 1;

                var instancingData = new VertexInstancingData[instances];
                this.VertexBuffers[instancingBufferOffset] = graphics.Device.CreateVertexBufferWrite(name, instancingData);
                this.VertexBufferBindings[instancingBufferOffset] = new VertexBufferBinding(this.VertexBuffers[instancingBufferOffset], instancingData[0].GetStride(), 0);

                foreach (var item in vList.Values)
                {
                    item.Input.AddRange(VertexInstancingData.GetInput(instancingBufferOffset));
                }
            }

            if (iList.Count > 0)
            {
                this.IndexBuffer = graphics.Device.CreateIndexBuffer(name, iList.ToArray(), dynamic);
            }
        }

        /// <summary>
        /// Sets buffers to device context
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public void SetBuffers(Graphics graphics)
        {
            graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.VertexBufferBindings);
            Counters.IAVertexBuffersSets++;
            graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.IndexBuffer, Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;
        }
        /// <summary>
        /// Sets input layout to device context
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="technique">Technique</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="topology">Topology</param>
        public void SetInputAssembler(Graphics graphics, EffectTechnique technique, VertexTypes vertexType, PrimitiveTopology topology)
        {
            //The technique defines the vertex type
            if (!inputLayouts.ContainsKey(technique))
            {
                var desc = technique.GetPassByIndex(0).Description;

                this.inputLayouts.Add(
                    technique, 
                    new InputLayout(graphics.Device, desc.Signature, vList[vertexType].Input.ToArray()));
            }

            graphics.DeviceContext.InputAssembler.InputLayout = inputLayouts[technique];
            Counters.IAInputLayoutSets++;
            graphics.DeviceContext.InputAssembler.PrimitiveTopology = topology;
            Counters.IAPrimitiveTopologySets++;
        }

        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="data">Instancig data</param>
        public void WriteInstancingData(Graphics graphics, VertexInstancingData[] data)
        {
            if (data != null && data.Length > 0)
            {
                var instancingBuffer = this.VertexBuffers[this.VertexBuffers.Length - 1];

                graphics.DeviceContext.WriteDiscardBuffer(instancingBuffer, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="vertexBufferSlot">Slot</param>
        /// <param name="vertexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer<T>(Graphics graphics, int vertexBufferSlot, int vertexBufferOffset, T[] data) where T : struct, IVertexData
        {
            if (data != null && data.Length > 0)
            {
                var buffer = this.VertexBuffers[vertexBufferSlot];

                graphics.DeviceContext.WriteNoOverwriteBuffer(buffer, vertexBufferOffset, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="vertexBufferSlot">Slot</param>
        /// <param name="vertexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer(Graphics graphics, int vertexBufferSlot, int vertexBufferOffset, IVertexData[] data)
        {
            if (data != null && data.Length > 0)
            {
                var buffer = this.VertexBuffers[vertexBufferSlot];

                graphics.DeviceContext.WriteNoOverwriteBuffer(buffer, vertexBufferOffset, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="indexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer(Graphics graphics, int indexBufferOffset, uint[] data)
        {
            if (data != null && data.Length > 0)
            {
                var buffer = this.IndexBuffer;

                graphics.DeviceContext.WriteNoOverwriteBuffer(buffer, indexBufferOffset, data);
            }
        }
    }
}
