using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;

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
            /// Data list
            /// </summary>
            private readonly List<IVertexData> data = new List<IVertexData>();
            /// <summary>
            /// Input element list
            /// </summary>
            private readonly List<InputElement> input = new List<InputElement>();
            /// <summary>
            /// Descriptor list
            /// </summary>
            public readonly List<BufferDescriptor> descriptors = new List<BufferDescriptor>();

            /// <summary>
            /// Vertex type
            /// </summary>
            public readonly VertexTypes Type;
            /// <summary>
            /// Dynamic buffer
            /// </summary>
            public readonly bool Dynamic;
            /// <summary>
            /// Name
            /// </summary>
            public string Name { get; set; } = null;
            /// <summary>
            /// Vertex data
            /// </summary>
            public IEnumerable<IVertexData> Data { get { return data.ToArray(); } }
            /// <summary>
            /// Instances
            /// </summary>
            public int Instances { get; set; } = 0;
            /// <summary>
            /// Input elements
            /// </summary>
            public IEnumerable<InputElement> Input { get { return input.ToArray(); } }
            /// <summary>
            /// Allocated size into graphics device
            /// </summary>
            public int AllocatedSize { get; set; } = 0;
            /// <summary>
            /// Gets wether the internal buffer needs reallocation
            /// </summary>
            public bool ReallocationNeeded
            {
                get
                {
                    return AllocatedSize != this.data.Count;
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public VertexBufferDescription(VertexTypes type, bool dynamic)
            {
                this.Type = type;
                this.Dynamic = dynamic;
            }

            /// <summary>
            /// Gets the buffer format stride
            /// </summary>
            /// <returns>Returns the buffer format stride in bytes</returns>
            public int GetStride()
            {
                return this.data.FirstOrDefault()?.GetStride() ?? 0;
            }
            /// <summary>
            /// Adds the input element to the internal input list, of the specified slot
            /// </summary>
            /// <param name="slot">Buffer descriptor slot</param>
            public void AddInputs(int slot)
            {
                //Get the input element list from the vertex data
                var inputs = this.data.First().GetInput(slot);

                //Adds the input list
                this.input.AddRange(inputs);
                //Updates the allocated size
                this.AllocatedSize = this.data.Count;
            }
            /// <summary>
            /// Adds the specified instancing input elements to the internal list
            /// </summary>
            /// <param name="inputs">Input element list</param>
            public void AddInstancingInputs(IEnumerable<InputElement> inputs)
            {
                this.input.AddRange(inputs);
            }
            /// <summary>
            /// Clears the internal input list
            /// </summary>
            public void ClearInputs()
            {
                this.input.Clear();
                this.AllocatedSize = 0;
            }

            /// <summary>
            /// Adds a buffer descritor to the internal descriptos list
            /// </summary>
            /// <param name="slot">Buffer slot</param>
            /// <param name="vertices">Vertex list</param>
            /// <param name="instances">Instance count</param>
            /// <returns>Returns the new registerd descriptor</returns>
            public BufferDescriptor AddDescriptor(int slot, IEnumerable<IVertexData> vertices, int instances)
            {
                //Store current data index as descriptor offset
                int offset = this.data.Count;

                //Add items to data list
                this.data.AddRange(vertices);
                //Increment the instance count
                this.Instances += instances;

                //Create and add the new descriptor to main descriptor list
                var descriptor = new BufferDescriptor(slot, offset, vertices.Count());
                this.descriptors.Add(descriptor);
                return descriptor;
            }
            /// <summary>
            /// Removes a buffer descriptor from the internal list
            /// </summary>
            /// <param name="descriptor">Buffer descriptor to remove</param>
            public void RemoveDescriptor(BufferDescriptor descriptor)
            {
                //Find descriptor
                var index = this.descriptors.IndexOf(descriptor);
                if (index >= 0)
                {
                    if (descriptor.Count > 0)
                    {
                        //If descriptor has items, remove from buffer descriptors
                        this.data.RemoveRange(descriptor.Offset, descriptor.Count);

                        //Reallocate the next descriptor offsets
                        for (int i = index + 1; i < this.descriptors.Count; i++)
                        {
                            this.descriptors[i].Offset -= descriptor.Count;
                        }
                    }

                    //Remove from descriptors list
                    this.descriptors.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Index buffer description
        /// </summary>
        class IndexBufferDescription
        {
            /// <summary>
            /// Data list
            /// </summary>
            private readonly List<uint> data = new List<uint>();
            /// <summary>
            /// Descriptor list
            /// </summary>
            public readonly List<BufferDescriptor> descriptors = new List<BufferDescriptor>();

            /// <summary>
            /// Dynamic
            /// </summary>
            public readonly bool Dynamic;
            /// <summary>
            /// Name
            /// </summary>
            public string Name { get; set; } = null;
            /// <summary>
            /// Index data
            /// </summary>
            public IEnumerable<uint> Data { get { return data.ToArray(); } }
            /// <summary>
            /// Allocated size into graphics device
            /// </summary>
            public int AllocatedSize { get; set; } = 0;
            /// <summary>
            /// Gets wether the internal buffer needs reallocation
            /// </summary>
            public bool ReallocationNeeded
            {
                get
                {
                    return AllocatedSize != data.Count;
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public IndexBufferDescription(bool dynamic)
            {
                this.Dynamic = dynamic;
            }

            /// <summary>
            /// Adds a buffer descritor to the internal descriptos list
            /// </summary>
            /// <param name="slot">Buffer slot</param>
            /// <param name="indices">Index list</param>
            /// <returns>Returns the new registerd descriptor</returns>
            public BufferDescriptor AddDescriptor(int slot, IEnumerable<uint> indices)
            {
                //Store current data index as descriptor offset
                int offset = this.data.Count;

                //Add items to data list
                this.data.AddRange(indices);

                //Create and add the new descriptor to main descriptor list
                var descriptor = new BufferDescriptor(slot, offset, indices.Count());
                this.descriptors.Add(descriptor);
                return descriptor;
            }
            /// <summary>
            /// Removes a buffer descriptor from the internal list
            /// </summary>
            /// <param name="descriptor">Buffer descriptor to remove</param>
            public void RemoveDescriptor(BufferDescriptor descriptor)
            {
                //Find descriptor
                var index = this.descriptors.IndexOf(descriptor);
                if (index >= 0)
                {
                    if (descriptor.Count > 0)
                    {
                        //If descriptor has items, remove from buffer descriptors
                        this.data.RemoveRange(descriptor.Offset, descriptor.Count);

                        //Reallocate the next descriptor offsets
                        for (int i = index + 1; i < this.descriptors.Count; i++)
                        {
                            this.descriptors[i].Offset -= descriptor.Count;
                        }
                    }

                    //Remove from descriptors list
                    this.descriptors.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Creates a vertex buffer from IVertexData
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="description">Vertex buffer description</param>
        /// <returns>Returns new buffer</returns>
        private static Buffer CreateVertexBuffer(Graphics graphics, VertexBufferDescription description)
        {
            string name = description.Name;
            var vertices = description.Data;
            bool dynamic = description.Dynamic;

            if (vertices?.Any() != true)
            {
                return null;
            }

            var vertexType = vertices.First().VertexType;

            switch (vertexType)
            {
                case VertexTypes.Billboard:
                    return graphics.CreateVertexBuffer<VertexBillboard>(name, vertices, dynamic);
                case VertexTypes.CPUParticle:
                    return graphics.CreateVertexBuffer<VertexCpuParticle>(name, vertices, dynamic);
                case VertexTypes.GPUParticle:
                    return graphics.CreateVertexBuffer<VertexGpuParticle>(name, vertices, dynamic);
                case VertexTypes.Terrain:
                    return graphics.CreateVertexBuffer<VertexTerrain>(name, vertices, dynamic);
                case VertexTypes.Position:
                    return graphics.CreateVertexBuffer<VertexPosition>(name, vertices, dynamic);
                case VertexTypes.PositionColor:
                    return graphics.CreateVertexBuffer<VertexPositionColor>(name, vertices, dynamic);
                case VertexTypes.PositionTexture:
                    return graphics.CreateVertexBuffer<VertexPositionTexture>(name, vertices, dynamic);
                case VertexTypes.PositionNormalColor:
                    return graphics.CreateVertexBuffer<VertexPositionNormalColor>(name, vertices, dynamic);
                case VertexTypes.PositionNormalTexture:
                    return graphics.CreateVertexBuffer<VertexPositionNormalTexture>(name, vertices, dynamic);
                case VertexTypes.PositionNormalTextureTangent:
                    return graphics.CreateVertexBuffer<VertexPositionNormalTextureTangent>(name, vertices, dynamic);
                case VertexTypes.PositionSkinned:
                    return graphics.CreateVertexBuffer<VertexSkinnedPosition>(name, vertices, dynamic);
                case VertexTypes.PositionColorSkinned:
                    return graphics.CreateVertexBuffer<VertexSkinnedPositionColor>(name, vertices, dynamic);
                case VertexTypes.PositionTextureSkinned:
                    return graphics.CreateVertexBuffer<VertexSkinnedPositionTexture>(name, vertices, dynamic);
                case VertexTypes.PositionNormalColorSkinned:
                    return graphics.CreateVertexBuffer<VertexSkinnedPositionNormalColor>(name, vertices, dynamic);
                case VertexTypes.PositionNormalTextureSkinned:
                    return graphics.CreateVertexBuffer<VertexSkinnedPositionNormalTexture>(name, vertices, dynamic);
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return graphics.CreateVertexBuffer<VertexSkinnedPositionNormalTextureTangent>(name, vertices, dynamic);
                case VertexTypes.Unknown:
                default:
                    throw new EngineException(string.Format("Unknown vertex type: {0}", vertexType));
            }
        }
        /// <summary>
        /// Creates an instancing buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="data">Instancing data</param>
        /// <returns>Returns the new buffer</returns>
        private static Buffer CreateInstancingBuffer(Graphics graphics, IEnumerable<VertexInstancingData> data)
        {
            return graphics.CreateVertexBuffer(null, data, true);
        }
        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <param name="indices">Indices</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <returns>Returns new buffer</returns>
        private static Buffer CreateIndexBuffer(Graphics graphics, string name, IEnumerable<uint> indices, bool dynamic)
        {
            return graphics.CreateIndexBuffer(name, indices, dynamic);
        }

        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game = null;
        /// <summary>
        /// Reserved slots
        /// </summary>
        private readonly int reservedSlots = 0;
        /// <summary>
        /// Vertex buffers
        /// </summary>
        private readonly List<Buffer> vertexBuffers = new List<Buffer>();
        /// <summary>
        /// Index buffer
        /// </summary>
        private readonly List<Buffer> indexBuffers = new List<Buffer>();
        /// <summary>
        /// Vertex buffer bindings
        /// </summary>
        private readonly List<VertexBufferBinding> vertexBufferBindings = new List<VertexBufferBinding>();
        /// <summary>
        /// Vertex keys
        /// </summary>
        private readonly List<VertexBufferDescription> vertexData = new List<VertexBufferDescription>();
        /// <summary>
        /// Index keys
        /// </summary>
        private readonly List<IndexBufferDescription> indexData = new List<IndexBufferDescription>();
        /// <summary>
        /// Input layouts by technique
        /// </summary>
        private readonly Dictionary<EngineEffectTechnique, InputLayout> inputLayouts = new Dictionary<EngineEffectTechnique, InputLayout>();
        /// <summary>
        /// Vertex buffer allocation needed
        /// </summary>
        private bool vertexBufferAllocationNeeded = false;
        /// <summary>
        /// Index buffer allocation needed
        /// </summary>
        private bool indexBufferAllocationNeeded = false;
        /// <summary>
        /// Instancing buffer offset
        /// </summary>
        private int instancingBufferOffset = -1;
        /// <summary>
        /// Instancing buffer binding offset
        /// </summary>
        private int instancingBufferBindingOffset = -1;

        /// <summary>
        /// Total instances
        /// </summary>
        protected int TotalInstances
        {
            get
            {
                return vertexData.Sum(i => i.Instances);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="reservedSlots">Reserved slots</param>
        public BufferManager(Game game, int reservedSlots = 1)
        {
            this.game = game;
            this.reservedSlots = reservedSlots;

            for (int i = 0; i < reservedSlots; i++)
            {
                this.vertexData.Add(new VertexBufferDescription(VertexTypes.Unknown, true));
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BufferManager()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < this.vertexBuffers.Count; i++)
                {
                    this.vertexBuffers[i]?.Dispose();
                }
                this.vertexBuffers.Clear();

                for (int i = 0; i < this.indexBuffers.Count; i++)
                {
                    this.indexBuffers[i]?.Dispose();
                }
                this.indexBuffers.Clear();

                foreach (var item in this.inputLayouts)
                {
                    item.Value?.Dispose();
                }
                this.inputLayouts.Clear();
            }
        }

        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <typeparam name="T">Type of vertex</typeparam>
        /// <param name="id">Id</param>
        /// <param name="data">Vertex list</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="instances">Add instancing space</param>
        public BufferDescriptor Add<T>(string id, IEnumerable<T> data, bool dynamic, int instances) where T : struct, IVertexData
        {
            var verts = data.OfType<IVertexData>();

            return this.Add(id, verts, dynamic, instances);
        }
        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">Vertex list</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="instances">Add instancing space</param>
        public BufferDescriptor Add(string id, IEnumerable<IVertexData> data, bool dynamic, int instances)
        {
            if (data?.Any() != true)
            {
                return null;
            }

            VertexTypes vType = data.First().VertexType;

            var keyIndex = this.vertexData.FindIndex(k => k.Type == vType && k.Dynamic == dynamic && (k.Instances > 0 == instances > 0));
            if (keyIndex < 0)
            {
                keyIndex = this.vertexData.Count;

                this.vertexData.Add(new VertexBufferDescription(vType, dynamic) { Name = id });
            }

            var key = this.vertexData[keyIndex];

            var descriptor = key.AddDescriptor(keyIndex, data, instances);

            if (key.ReallocationNeeded)
            {
                vertexBufferAllocationNeeded = true;
            }

            return descriptor;
        }
        /// <summary>
        /// Adds indices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">Index list</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        public BufferDescriptor Add(string id, IEnumerable<uint> data, bool dynamic)
        {
            if (data?.Any() != true)
            {
                return null;
            }

            var keyIndex = this.indexData.FindIndex(k => k.Dynamic == dynamic);
            if (keyIndex < 0)
            {
                keyIndex = this.indexData.Count;

                this.indexData.Add(new IndexBufferDescription(dynamic) { Name = id });
            }

            var key = this.indexData[keyIndex];

            var descriptor = key.AddDescriptor(keyIndex, data);

            if (key.ReallocationNeeded)
            {
                indexBufferAllocationNeeded = true;
            }

            return descriptor;
        }

        /// <summary>
        /// Removes vertex data from buffer manager
        /// </summary>
        /// <param name="descriptor">Buffer descriptor</param>
        public void RemoveVertexData(BufferDescriptor descriptor)
        {
            if (descriptor != null && descriptor.Slot >= 0)
            {
                this.vertexData[descriptor.Slot].RemoveDescriptor(descriptor);
            }
        }
        /// <summary>
        /// Removes index data from buffer manager
        /// </summary>
        /// <param name="descriptor">Buffer descriptor</param>
        public void RemoveIndexData(BufferDescriptor descriptor)
        {
            if (descriptor != null && descriptor.Slot >= 0)
            {
                this.indexData[descriptor.Slot].RemoveDescriptor(descriptor);
            }
        }

        /// <summary>
        /// Creates and populates vertex, instancing and index buffers
        /// </summary>
        public void CreateBuffers()
        {
            CreateVertexBuffers();
            CreateInstancingBuffers();
            CreateIndexBuffers();
        }

        /// <summary>
        /// Creates the vertex buffers
        /// </summary>
        private void CreateVertexBuffers()
        {
            int index = 0;
            foreach (var descriptor in this.vertexData)
            {
                if (index < reservedSlots)
                {
                    vertexBuffers.Add(null);
                    vertexBufferBindings.Add(new VertexBufferBinding());

                    descriptor.ClearInputs();
                }
                else
                {
                    int slot = vertexBuffers.Count;
                    var buffer = CreateVertexBuffer(this.game.Graphics, descriptor);
                    var binding = new VertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(binding);

                    descriptor.AddInputs(slot);
                }

                index++;
            }

            this.vertexBufferAllocationNeeded = false;
        }
        /// <summary>
        /// Creates the instancing buffer
        /// </summary>
        private void CreateInstancingBuffers()
        {
            int instances = this.TotalInstances;
            if (instances <= 0)
            {
                return;
            }

            this.instancingBufferOffset = vertexBuffers.Count;
            this.instancingBufferBindingOffset = vertexBufferBindings.Count;

            var instancingData = new VertexInstancingData[instances];

            var buffer = CreateInstancingBuffer(this.game.Graphics, instancingData);
            var binding = new VertexBufferBinding(buffer, instancingData[0].GetStride(), 0);

            this.vertexBuffers.Add(buffer);
            this.vertexBufferBindings.Add(binding);

            foreach (var item in this.vertexData)
            {
                if (item.Instances > 0)
                {
                    item.AddInstancingInputs(VertexInstancingData.Input(instancingBufferOffset));
                }
            }
        }
        /// <summary>
        /// Creates index buffers
        /// </summary>
        private void CreateIndexBuffers()
        {
            foreach (var descriptor in this.indexData)
            {
                var name = descriptor.Name;
                var data = descriptor.Data;
                var dynamic = descriptor.Dynamic;

                var buffer = CreateIndexBuffer(this.game.Graphics, name, data.ToArray(), dynamic);

                indexBuffers.Add(buffer);
                descriptor.AllocatedSize = data.Count();
            }

            this.indexBufferAllocationNeeded = false;
        }

        /// <summary>
        /// Updates the buffers if reallocation needed
        /// </summary>
        public void UpdateBuffers()
        {
            if (vertexBufferAllocationNeeded)
            {
                ReallocateVertexData(out bool reallocateInstances);

                vertexBufferAllocationNeeded = false;

                if (reallocateInstances)
                {
                    ReallocateInstances();
                }
            }

            if (indexBufferAllocationNeeded)
            {
                ReallocateIndexData();

                indexBufferAllocationNeeded = false;
            }
        }
        /// <summary>
        /// Reallocates the vertex data
        /// </summary>
        /// <param name="reallocateInstances">Returns wether instance reallocation is necessary</param>
        private void ReallocateVertexData(out bool reallocateInstances)
        {
            reallocateInstances = false;

            for (int i = 0; i < this.vertexData.Count; i++)
            {
                var descriptor = this.vertexData[i];

                if (descriptor.ReallocationNeeded)
                {
                    //Dipose current buffer
                    var mb = this.vertexBuffers[i];
                    mb.Dispose();

                    //Recreate the buffer and binding
                    var buffer = CreateVertexBuffer(game.Graphics, descriptor);
                    var binding = new VertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    this.vertexBuffers[i] = buffer;
                    this.vertexBufferBindings[i] = binding;

                    //Updates the allocated buffer size
                    descriptor.AllocatedSize = descriptor.Data.Count();

                    if (descriptor.Instances > 0)
                    {
                        //If current descriptor has instances, instancing buffers must be reallocated too
                        reallocateInstances = true;
                    }
                }
            }
        }
        /// <summary>
        /// Reallocates the instance data
        /// </summary>
        private void ReallocateInstances()
        {
            int instances = this.TotalInstances;
            if (instances <= 0)
            {
                return;
            }

            //Dipose current buffer
            var instancingBuffer = this.vertexBuffers[this.vertexBuffers.Count - 1];
            instancingBuffer.Dispose();

            //Recreate the buffer and binding
            var data = new VertexInstancingData[instances];
            var buffer = CreateInstancingBuffer(game.Graphics, data);
            var binding = new VertexBufferBinding(buffer, data[0].GetStride(), 0);

            this.vertexBuffers[this.instancingBufferOffset] = buffer;
            this.vertexBufferBindings[this.instancingBufferBindingOffset] = binding;
        }
        /// <summary>
        /// Reallocates the index data
        /// </summary>
        private void ReallocateIndexData()
        {
            for (int i = 0; i < this.indexData.Count; i++)
            {
                var descriptor = this.indexData[i];

                if (descriptor.ReallocationNeeded)
                {
                    //Dipose current buffer
                    var ib = this.indexBuffers[i];
                    ib.Dispose();

                    //Recreate the buffer
                    var buffer = CreateIndexBuffer(game.Graphics, descriptor.Name, descriptor.Data.ToArray(), descriptor.Dynamic);

                    this.indexBuffers[i] = buffer;

                    //Updates the allocated buffer size
                    descriptor.AllocatedSize = descriptor.Data.Count();
                }
            }
        }

        /// <summary>
        /// Sets vertex buffers to device context
        /// </summary>
        public void SetVertexBuffers()
        {
            this.game.Graphics.IASetVertexBuffers(0, this.vertexBufferBindings.ToArray());
        }
        /// <summary>
        /// Sets index buffers to device context
        /// </summary>
        /// <param name="slot">Slot</param>
        public void SetIndexBuffer(int slot)
        {
            if (slot >= 0)
            {
                this.game.Graphics.IASetIndexBuffer(this.indexBuffers[slot], Format.R32_UInt, 0);
            }
        }
        /// <summary>
        /// Sets input layout to device context
        /// </summary>
        /// <param name="technique">Technique</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="topology">Topology</param>
        public void SetInputAssembler(EngineEffectTechnique technique, int slot, Topology topology)
        {
            //The technique defines the vertex type
            if (!inputLayouts.ContainsKey(technique))
            {
                var key = this.vertexData[slot];
                var signature = technique.GetSignature();

                this.inputLayouts.Add(
                    technique,
                    this.game.Graphics.CreateInputLayout(signature, key.Input.ToArray()));
            }

            this.game.Graphics.IAInputLayout = inputLayouts[technique];
            this.game.Graphics.IAPrimitiveTopology = (PrimitiveTopology)topology;
        }

        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="data">Instancig data</param>
        public void WriteInstancingData(IEnumerable<VertexInstancingData> data)
        {
            if (data?.Any() == true)
            {
                var instancingBuffer = this.vertexBuffers[this.vertexBuffers.Count - 1];

                this.game.Graphics.WriteDiscardBuffer(instancingBuffer, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="vertexBufferSlot">Slot</param>
        /// <param name="vertexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer<T>(int vertexBufferSlot, int vertexBufferOffset, IEnumerable<T> data) where T : struct, IVertexData
        {
            if (data?.Any() == true)
            {
                var buffer = this.vertexBuffers[vertexBufferSlot];

                this.game.Graphics.WriteNoOverwriteBuffer(buffer, vertexBufferOffset, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="indexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer(int indexBufferSlot, int indexBufferOffset, IEnumerable<uint> data)
        {
            if (data?.Any() == true)
            {
                var buffer = this.indexBuffers[indexBufferSlot];

                this.game.Graphics.WriteNoOverwriteBuffer(buffer, indexBufferOffset, data);
            }
        }
    }
}
