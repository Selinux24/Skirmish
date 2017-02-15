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
            /// <summary>
            /// Instances
            /// </summary>
            public int Instances = 0;
        }

        /// <summary>
        /// Creates the vertex buffers
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="vList">Vertex list</param>
        /// <param name="dynamic">Create dynamic buffers</param>
        /// <param name="vertexBuffers">Vertex buffer collection</param>
        /// <param name="vertexBufferBindings">Vertex buffer bindings</param>
        private static void CreateVertexBuffers(Graphics graphics, string name, Dictionary<VertexTypes, VertexData> vList, bool dynamic, List<Buffer> vertexBuffers, List<VertexBufferBinding> vertexBufferBindings)
        {
            var vTypes = vList.Keys.ToArray();

            for (int i = 0; i < vTypes.Length; i++)
            {
                var vData = vList[vTypes[i]];
                var verts = vData.Vertices.ToArray();
                int slot = vertexBuffers.Count;

                vertexBuffers.Add(graphics.Device.CreateVertexBuffer(name, verts, dynamic));
                vertexBufferBindings.Add(new VertexBufferBinding(vertexBuffers[slot], verts[0].GetStride(), 0));

                vData.Input.AddRange(verts[0].GetInput(slot));
            }
        }
        /// <summary>
        /// Creates the instancing buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="vList1">Static list</param>
        /// <param name="vList2">Dynamic list</param>
        /// <param name="instances">Total instance count</param>
        /// <param name="vertexBuffers">Vertex buffer collection</param>
        /// <param name="vertexBufferBindings">Vertex buffer bindings</param>
        private static void CreateInstancingBuffers(Graphics graphics, string name, Dictionary<VertexTypes, VertexData> vList1, Dictionary<VertexTypes, VertexData> vList2, int instances, List<Buffer> vertexBuffers, List<VertexBufferBinding> vertexBufferBindings)
        {
            int instancingBufferOffset = vertexBuffers.Count;

            var instancingData = new VertexInstancingData[instances];
            vertexBuffers.Add(graphics.Device.CreateVertexBufferWrite(name, instancingData));
            vertexBufferBindings.Add(new VertexBufferBinding(vertexBuffers[instancingBufferOffset], instancingData[0].GetStride(), 0));

            foreach (var item in vList1.Values)
            {
                if (item.Instances > 0)
                {
                    item.Input.AddRange(VertexInstancingData.GetInput(instancingBufferOffset));
                }
            }

            foreach (var item in vList2.Values)
            {
                if (item.Instances > 0)
                {
                    item.Input.AddRange(VertexInstancingData.GetInput(instancingBufferOffset));
                }
            }
        }
        /// <summary>
        /// Creates index buffers
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="iList">Index list</param>
        /// <param name="dynamic">Create dynamic buffers</param>
        /// <param name="indexBuffers">Index buffer list</param>
        private static void CreateIndexBuffers(Graphics graphics, string name, List<uint> iList, bool dynamic, List<Buffer> indexBuffers)
        {
            if (iList.Count > 0)
            {
                int slot = indexBuffers.Count;

                indexBuffers.Add(graphics.Device.CreateIndexBuffer(name, iList.ToArray(), dynamic));
            }
        }

        /// <summary>
        /// Static vertex data dictionary by vertex type
        /// </summary>
        private Dictionary<VertexTypes, VertexData> staticVertexList = new Dictionary<VertexTypes, VertexData>();
        /// <summary>
        /// Dynamic vertex data dictionary by vertex type
        /// </summary>
        private Dictionary<VertexTypes, VertexData> dynamicVertexList = new Dictionary<VertexTypes, VertexData>();
        /// <summary>
        /// Static index data list
        /// </summary>
        private List<uint> staticIndexList = new List<uint>();
        /// <summary>
        /// Dynamic index data list
        /// </summary>
        private List<uint> dynamicIndexList = new List<uint>();
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
        protected Buffer[] IndexBuffers = null;
        /// <summary>
        /// Vertex buffer bindings
        /// </summary>
        protected VertexBufferBinding[] VertexBufferBindings = null;
        /// <summary>
        /// Total instances
        /// </summary>
        protected int TotalInstances
        {
            get
            {
                int count = 0;

                foreach (var item in this.staticVertexList.Values)
                {
                    count += item.Instances;
                }

                foreach (var item in this.dynamicVertexList.Values)
                {
                    count += item.Instances;
                }

                return count;
            }
        }

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

            Helper.Dispose(this.IndexBuffers);
            this.IndexBuffers = null;
        }

        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <typeparam name="T">Type of vertex</typeparam>
        /// <param name="id">Id</param>
        /// <param name="vertexData">Vertex list</param>
        /// <param name="offset">Returns the assigned offset in buffer</param>
        /// <param name="slot">Returns the assigned buffer slot</param>
        public void Add<T>(int id, T[] vertexData, bool dynamic, int instances, out int offset, out int slot) where T : struct, IVertexData
        {
            List<IVertexData> verts = new List<IVertexData>();
            Array.ForEach(vertexData, v => verts.Add(v));

            this.Add(id, verts.ToArray(), dynamic, instances, out offset, out slot);
        }
        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="vertexData">Vertex list</param>
        /// <param name="offset">Returns the assigned offset in buffer</param>
        /// <param name="slot">Returns the assigned buffer slot</param>
        public void Add(int id, IVertexData[] vertexData, bool dynamic, int instances, out int offset, out int slot)
        {
            offset = -1;
            slot = -1;

            if (vertexData != null && vertexData.Length > 0)
            {
                VertexTypes vType = vertexData[0].VertexType;

                var vList = dynamic ? this.dynamicVertexList : this.staticVertexList;

                if (!vList.ContainsKey(vType))
                {
                    vList.Add(vType, new VertexData());
                }

                var vData = vList[vType];
                offset = vData.Vertices.Count;
                vData.Vertices.AddRange(vertexData);
                vData.Instances = instances;

                var vTypes = vList.Keys.ToArray();
                slot = Array.IndexOf(vTypes, vType);
            }
        }
        /// <summary>
        /// Adds indices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="indexData">Index list</param>
        /// <param name="offset">Returns the assigned offset in buffer</param>
        public void Add(int id, uint[] indexData, bool dynamic, out int offset, out int slot)
        {
            offset = -1;
            slot = -1;

            if (indexData != null && indexData.Length > 0)
            {
                var iList = dynamic ? this.dynamicIndexList : this.staticIndexList;

                offset = iList.Count;
                iList.AddRange(indexData);

                slot = dynamic && this.staticIndexList.Count > 0 ? 1 : 0;
            }
        }

        /// <summary>
        /// Creates and populates the buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <param name="dynamic">Use dynamic buffers</param>
        /// <param name="instances">Instance count</param>
        public void CreateBuffers(Graphics graphics, string name)
        {
            int instances = this.TotalInstances;

            List<Buffer> vertexBuffers = new List<Buffer>();
            List<VertexBufferBinding> vertexBufferBindings = new List<VertexBufferBinding>();
            List<Buffer> indexBuffers = new List<Buffer>();

            CreateVertexBuffers(graphics, name, this.staticVertexList, false, vertexBuffers, vertexBufferBindings);
            CreateVertexBuffers(graphics, name, this.dynamicVertexList, true, vertexBuffers, vertexBufferBindings);
            if (instances > 0)
            {
                CreateInstancingBuffers(graphics, name, this.staticVertexList, this.dynamicVertexList, instances, vertexBuffers, vertexBufferBindings);
            }
            CreateIndexBuffers(graphics, name, this.staticIndexList, false, indexBuffers);
            CreateIndexBuffers(graphics, name, this.dynamicIndexList, true, indexBuffers);

            this.VertexBuffers = vertexBuffers.ToArray();
            this.VertexBufferBindings = vertexBufferBindings.ToArray();
            this.IndexBuffers = indexBuffers.ToArray();
        }

        /// <summary>
        /// Sets vertex buffers to device context
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public void SetVertexBuffers(Graphics graphics)
        {
            graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.VertexBufferBindings);
            Counters.IAVertexBuffersSets++;
        }
        /// <summary>
        /// Sets index buffers to device context
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="slot">Slot</param>
        public void SetIndexBuffer(Graphics graphics, int slot)
        {
            if (slot >= 0)
            {
                graphics.DeviceContext.InputAssembler.SetIndexBuffer(this.IndexBuffers[slot], Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;
            }
        }
        /// <summary>
        /// Sets input layout to device context
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="technique">Technique</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="topology">Topology</param>
        public void SetInputAssembler(Graphics graphics, EffectTechnique technique, VertexTypes vertexType, bool dynamic, PrimitiveTopology topology)
        {
            //The technique defines the vertex type
            if (!inputLayouts.ContainsKey(technique))
            {
                var desc = technique.GetPassByIndex(0).Description;
                var vList = dynamic ? this.dynamicVertexList : this.staticVertexList;

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
        public void WriteBuffer(Graphics graphics, int indexBufferSlot, int indexBufferOffset, uint[] data)
        {
            if (data != null && data.Length > 0)
            {
                var buffer = this.IndexBuffers[indexBufferSlot];

                graphics.DeviceContext.WriteNoOverwriteBuffer(buffer, indexBufferOffset, data);
            }
        }
    }
}
