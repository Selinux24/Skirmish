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
        /// Vertex buffer description
        /// </summary>
        class VertexBufferDescription
        {
            /// <summary>
            /// Name
            /// </summary>
            public string Name = null;
            /// <summary>
            /// Vertex type
            /// </summary>
            public readonly VertexTypes Type;
            /// <summary>
            /// Dynamic buffer
            /// </summary>
            public readonly bool Dynamic;
            /// <summary>
            /// Vertex data
            /// </summary>
            public List<IVertexData> Data = new List<IVertexData>();
            /// <summary>
            /// Instances
            /// </summary>
            public int Instances = 0;
            /// <summary>
            /// Input elements
            /// </summary>
            public List<InputElement> Input = new List<InputElement>();

            /// <summary>
            /// Constructor
            /// </summary>
            public VertexBufferDescription(VertexTypes type, bool dynamic)
            {
                this.Type = type;
                this.Dynamic = dynamic;
            }
        }

        /// <summary>
        /// Index buffer description
        /// </summary>
        class IndexBufferDescription
        {
            /// <summary>
            /// Name
            /// </summary>
            public string Name = null;
            /// <summary>
            /// Dynamic
            /// </summary>
            public readonly bool Dynamic = false;
            /// <summary>
            /// Index data
            /// </summary>
            public List<uint> Data = new List<uint>();

            /// <summary>
            /// Constructor
            /// </summary>
            public IndexBufferDescription(bool dynamic)
            {
                this.Dynamic = dynamic;
            }
        }

        /// <summary>
        /// Creates the vertex buffers
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="vKeys">Vertices</param>
        /// <param name="vertexBuffers">Vertex buffer collection</param>
        /// <param name="vertexBufferBindings">Vertex buffer bindings</param>
        private static void CreateVertexBuffers(Graphics graphics, List<VertexBufferDescription> vKeys, List<Buffer> vertexBuffers, List<VertexBufferBinding> vertexBufferBindings)
        {
            for (int i = 0; i < vKeys.Count; i++)
            {
                var data = vKeys[i].Data.ToArray();
                int slot = vertexBuffers.Count;

                vertexBuffers.Add(graphics.CreateVertexBuffer(vKeys[i].Name, data, vKeys[i].Dynamic));
                vertexBufferBindings.Add(new VertexBufferBinding(vertexBuffers[slot], data[0].GetStride(), 0));

                vKeys[i].Input.AddRange(data[0].GetInput(slot));
            }
        }
        /// <summary>
        /// Creates the instancing buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="vKeys">Vertices</param>
        /// <param name="instances">Total instance count</param>
        /// <param name="vertexBuffers">Vertex buffer collection</param>
        /// <param name="vertexBufferBindings">Vertex buffer bindings</param>
        private static void CreateInstancingBuffers(Graphics graphics, List<VertexBufferDescription> vKeys, int instances, List<Buffer> vertexBuffers, List<VertexBufferBinding> vertexBufferBindings)
        {
            int instancingBufferOffset = vertexBuffers.Count;

            var instancingData = new VertexInstancingData[instances];
            vertexBuffers.Add(graphics.CreateVertexBufferWrite(null, instancingData));
            vertexBufferBindings.Add(new VertexBufferBinding(vertexBuffers[instancingBufferOffset], instancingData[0].GetStride(), 0));

            foreach (var item in vKeys)
            {
                if (item.Instances > 0)
                {
                    item.Input.AddRange(VertexInstancingData.Input(instancingBufferOffset));
                }
            }
        }
        /// <summary>
        /// Creates index buffers
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="iKeys">Indices</param>
        /// <param name="indexBuffers">Index buffer list</param>
        private static void CreateIndexBuffers(Graphics graphics, List<IndexBufferDescription> iKeys, List<Buffer> indexBuffers)
        {
            for (int i = 0; i < iKeys.Count; i++)
            {
                indexBuffers.Add(graphics.CreateIndexBuffer(iKeys[i].Name, iKeys[i].Data.ToArray(), iKeys[i].Dynamic));
            }
        }

        /// <summary>
        /// Game instance
        /// </summary>
        private Game game = null;
        /// <summary>
        /// Vertex keys
        /// </summary>
        private List<VertexBufferDescription> vertexData = new List<VertexBufferDescription>();
        /// <summary>
        /// Index keys
        /// </summary>
        private List<IndexBufferDescription> indexData = new List<IndexBufferDescription>();
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

                foreach (var item in this.vertexData)
                {
                    count += item.Instances;
                }

                return count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public BufferManager(Game game)
        {
            this.game = game;
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

            Helper.Dispose(this.inputLayouts);
            this.inputLayouts = null;
        }

        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <typeparam name="T">Type of vertex</typeparam>
        /// <param name="id">Id</param>
        /// <param name="vertexData">Vertex list</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="instances">Add instancing space</param>
        public BufferDescriptor Add<T>(string id, T[] vertexData, bool dynamic, int instances) where T : struct, IVertexData
        {
            List<IVertexData> verts = new List<IVertexData>();
            vertexData.ForEach(v => verts.Add(v));

            return this.Add(id, verts.ToArray(), dynamic, instances);
        }
        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="vertexData">Vertex list</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="instances">Add instancing space</param>
        public BufferDescriptor Add(string id, IVertexData[] vertexData, bool dynamic, int instances)
        {
            int offset = -1;
            int slot = -1;

            if (vertexData != null && vertexData.Length > 0)
            {
                VertexTypes vType = vertexData[0].VertexType;

                var keyIndex = this.vertexData.FindIndex(k => k.Type == vType && k.Dynamic == dynamic && (k.Instances > 0 == instances > 0));
                if (keyIndex < 0)
                {
                    keyIndex = this.vertexData.Count;

                    this.vertexData.Add(new VertexBufferDescription(vType, dynamic) { Name = id });
                }

                var key = this.vertexData[keyIndex];

                offset = key.Data.Count;

                key.Data.AddRange(vertexData);
                key.Instances += instances;

                slot = keyIndex;
            }

            return new BufferDescriptor(slot, offset, vertexData.Length);
        }
        /// <summary>
        /// Adds indices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="indexData">Index list</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        public BufferDescriptor Add(string id, uint[] indexData, bool dynamic)
        {
            int offset = -1;
            int slot = -1;

            if (indexData != null && indexData.Length > 0)
            {
                var keyIndex = this.indexData.FindIndex(k => k.Dynamic == dynamic);
                if (keyIndex < 0)
                {
                    keyIndex = this.indexData.Count;

                    this.indexData.Add(new IndexBufferDescription(dynamic) { Name = id });
                }

                var key = this.indexData[keyIndex];

                offset = key.Data.Count;
                key.Data.AddRange(indexData);

                slot = keyIndex;
            }

            return new BufferDescriptor(slot, offset, indexData.Length);
        }

        /// <summary>
        /// Creates and populates the buffer
        /// </summary>
        public void CreateBuffers()
        {
            int instances = this.TotalInstances;

            var vertexBuffers = new List<Buffer>();
            var vertexBufferBindings = new List<VertexBufferBinding>();
            var indexBuffers = new List<Buffer>();

            CreateVertexBuffers(this.game.Graphics, this.vertexData, vertexBuffers, vertexBufferBindings);
            if (instances > 0)
            {
                CreateInstancingBuffers(this.game.Graphics, this.vertexData, instances, vertexBuffers, vertexBufferBindings);
            }
            CreateIndexBuffers(this.game.Graphics, this.indexData, indexBuffers);

            this.VertexBuffers = vertexBuffers.ToArray();
            this.VertexBufferBindings = vertexBufferBindings.ToArray();
            this.IndexBuffers = indexBuffers.ToArray();
        }

        /// <summary>
        /// Sets vertex buffers to device context
        /// </summary>
        public void SetVertexBuffers()
        {
            this.game.Graphics.IASetVertexBuffers(0, this.VertexBufferBindings);
        }
        /// <summary>
        /// Sets index buffers to device context
        /// </summary>
        /// <param name="slot">Slot</param>
        public void SetIndexBuffer(int slot)
        {
            if (slot >= 0)
            {
                this.game.Graphics.IASetIndexBuffer(this.IndexBuffers[slot], Format.R32_UInt, 0);
            }
        }
        /// <summary>
        /// Sets input layout to device context
        /// </summary>
        /// <param name="technique">Technique</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="topology">Topology</param>
        public void SetInputAssembler(EffectTechnique technique, int slot, PrimitiveTopology topology)
        {
            //The technique defines the vertex type
            if (!inputLayouts.ContainsKey(technique))
            {
                var desc = technique.GetPassByIndex(0).Description;
                var key = this.vertexData[slot];

                this.inputLayouts.Add(
                    technique,
                    new InputLayout(this.game.Graphics.Device, desc.Signature, key.Input.ToArray()));
            }

            this.game.Graphics.IAInputLayout = inputLayouts[technique];
            this.game.Graphics.IAPrimitiveTopology = topology;
        }

        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="data">Instancig data</param>
        public void WriteInstancingData(VertexInstancingData[] data)
        {
            if (data != null && data.Length > 0)
            {
                var instancingBuffer = this.VertexBuffers[this.VertexBuffers.Length - 1];

                this.game.Graphics.DeviceContext.WriteDiscardBuffer(instancingBuffer, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="vertexBufferSlot">Slot</param>
        /// <param name="vertexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer<T>(int vertexBufferSlot, int vertexBufferOffset, T[] data) where T : struct, IVertexData
        {
            if (data != null && data.Length > 0)
            {
                var buffer = this.VertexBuffers[vertexBufferSlot];

                this.game.Graphics.DeviceContext.WriteNoOverwriteBuffer(buffer, vertexBufferOffset, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="vertexBufferSlot">Slot</param>
        /// <param name="vertexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer(int vertexBufferSlot, int vertexBufferOffset, IVertexData[] data)
        {
            if (data != null && data.Length > 0)
            {
                var buffer = this.VertexBuffers[vertexBufferSlot];

                this.game.Graphics.DeviceContext.WriteNoOverwriteBuffer(buffer, vertexBufferOffset, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="indexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer(int indexBufferSlot, int indexBufferOffset, uint[] data)
        {
            if (data != null && data.Length > 0)
            {
                var buffer = this.IndexBuffers[indexBufferSlot];

                this.game.Graphics.DeviceContext.WriteNoOverwriteBuffer(buffer, indexBufferOffset, data);
            }
        }
    }
}
