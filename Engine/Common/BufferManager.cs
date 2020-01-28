using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            private readonly List<BufferDescriptor> descriptors = new List<BufferDescriptor>();

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
            /// Gets the size of the data to allocate
            /// </summary>
            public int ToAllocateSize
            {
                get
                {
                    return this.data?.Count ?? 0;
                }
            }
            /// <summary>
            /// Gets wether the internal buffer needs reallocation
            /// </summary>
            public bool ReallocationNeeded { get; set; } = false;
            /// <summary>
            /// Gets wether the internal buffer is currently allocated in the graphic device
            /// </summary>
            public bool Allocated { get; set; } = false;
            /// <summary>
            /// Gets wether the instancing buffer is currently allocated in the graphic device
            /// </summary>
            public bool AllocatedInstancing { get; set; } = false;
            /// <summary>
            /// Gets wether the current buffer is dirty
            /// </summary>
            /// <remarks>A buffer is dirty when needs reallocation or if it's not allocated at all</remarks>
            public bool Dirty
            {
                get
                {
                    return !Allocated || ReallocationNeeded;
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
            /// Clears the internal input list
            /// </summary>
            public void ClearInputs()
            {
                this.input.Clear();
                this.AllocatedSize = 0;
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
            /// Crears the instancing inputs from the input elements
            /// </summary>
            public void ClearInstancingInputs()
            {
                this.input.RemoveAll(i => i.Classification == InputClassification.PerInstanceData);
            }

            /// <summary>
            /// Adds a buffer descritor to the internal descriptos list
            /// </summary>
            /// <param name="descriptor">Descriptor</param>
            /// <param name="id">Id</param>
            /// <param name="slot">Buffer slot</param>
            /// <param name="vertices">Vertex list</param>
            /// <param name="instances">Instance count</param>
            public void AddDescriptor(BufferDescriptor descriptor, string id, int slot, IEnumerable<IVertexData> vertices, int instances)
            {
                Monitor.Enter(this.data);
                //Store current data index as descriptor offset
                int offset = this.data.Count;
                //Add items to data list
                this.data.AddRange(vertices);
                Monitor.Exit(this.data);

                //Increment the instance count
                this.Instances += instances;

                //Add the new descriptor to main descriptor list
                descriptor.Id = id;
                descriptor.Slot = slot;
                descriptor.Offset = offset;
                descriptor.Count = vertices.Count();

                Monitor.Enter(this.descriptors);
                this.descriptors.Add(descriptor);
                Monitor.Exit(this.descriptors);
            }
            /// <summary>
            /// Removes a buffer descriptor from the internal list
            /// </summary>
            /// <param name="descriptor">Buffer descriptor to remove</param>
            /// <param name="instances">Instance count</param>
            public void RemoveDescriptor(BufferDescriptor descriptor, int instances)
            {
                if (descriptor.Count > 0)
                {
                    //If descriptor has items, remove from buffer descriptors
                    Monitor.Enter(this.data);
                    this.data.RemoveRange(descriptor.Offset, descriptor.Count);
                    Monitor.Exit(this.data);
                }

                Monitor.Enter(this.descriptors);
                //Remove descriptor
                this.descriptors.Remove(descriptor);

                if (this.descriptors.Any())
                {
                    //Reallocate descriptor offsets
                    this.descriptors[0].Offset = 0;
                    for (int i = 1; i < this.descriptors.Count; i++)
                    {
                        var prev = this.descriptors[i - 1];

                        this.descriptors[i].Offset = prev.Offset + prev.Count;
                    }
                }
                Monitor.Exit(this.descriptors);

                this.Instances -= instances;
            }

            /// <summary>
            /// Gets the text representation of the instance
            /// </summary>
            /// <returns>Returns a description of the instance</returns>
            public override string ToString()
            {
                return $"[{Type}][{Dynamic}] Instances: {Instances} AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
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
            /// Index data
            /// </summary>
            public IEnumerable<uint> Data { get { return data.ToArray(); } }
            /// <summary>
            /// Allocated size into graphics device
            /// </summary>
            public int AllocatedSize { get; set; } = 0;
            /// <summary>
            /// Gets the size of the data to allocate
            /// </summary>
            public int ToAllocateSize
            {
                get
                {
                    return this.data?.Count ?? 0;
                }
            }
            /// <summary>
            /// Gets wether the internal buffer needs reallocation
            /// </summary>
            public bool ReallocationNeeded { get; set; } = false;
            /// <summary>
            /// Gets wether the internal buffer is currently allocated in the graphic device
            /// </summary>
            public bool Allocated { get; set; } = false;
            /// <summary>
            /// Gets wether the current buffer is dirty
            /// </summary>
            /// <remarks>A buffer is dirty when needs reallocation or if it's not allocated at all</remarks>
            public bool Dirty
            {
                get
                {
                    return !Allocated || ReallocationNeeded;
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
            /// <param name="descriptor">Descriptor</param>
            /// <param name="id">Id</param>
            /// <param name="slot">Buffer slot</param>
            /// <param name="indices">Index list</param>
            /// <returns>Returns the new registerd descriptor</returns>
            public void AddDescriptor(BufferDescriptor descriptor, string id, int slot, IEnumerable<uint> indices)
            {
                Monitor.Enter(this.data);
                //Store current data index as descriptor offset
                int offset = this.data.Count;
                //Add items to data list
                this.data.AddRange(indices);
                Monitor.Exit(this.data);

                //Create and add the new descriptor to main descriptor list
                descriptor.Id = id;
                descriptor.Slot = slot;
                descriptor.Offset = offset;
                descriptor.Count = indices.Count();

                Monitor.Enter(this.descriptors);
                this.descriptors.Add(descriptor);
                Monitor.Exit(this.descriptors);
            }
            /// <summary>
            /// Removes a buffer descriptor from the internal list
            /// </summary>
            /// <param name="descriptor">Buffer descriptor to remove</param>
            public void RemoveDescriptor(BufferDescriptor descriptor)
            {
                //Find descriptor
                var index = this.descriptors.IndexOf(descriptor);
                if (index < 0)
                {
                    return;
                }

                if (descriptor.Count > 0)
                {
                    Monitor.Enter(this.data);
                    //If descriptor has items, remove from buffer descriptors
                    this.data.RemoveRange(descriptor.Offset, descriptor.Count);
                    Monitor.Exit(this.data);
                }

                Monitor.Enter(this.descriptors);
                //Remove from descriptors list
                this.descriptors.RemoveAt(index);

                if (this.descriptors.Any())
                {
                    //Reallocate descriptor offsets
                    this.descriptors[0].Offset = 0;
                    for (int i = 1; i < this.descriptors.Count; i++)
                    {
                        var prev = this.descriptors[i - 1];

                        this.descriptors[i].Offset = prev.Offset + prev.Count;
                    }
                }
                Monitor.Exit(this.descriptors);
            }

            /// <summary>
            /// Gets the text representation of the instance
            /// </summary>
            /// <returns>Returns a description of the instance</returns>
            public override string ToString()
            {
                return $"[{Dynamic}] AllocatedSize: {AllocatedSize} ToAllocateSize: {ToAllocateSize} Dirty: {Dirty}";
            }
        }

        /// <summary>
        /// Vertex buffer descriptor request
        /// </summary>
        class BufferDescriptorRequestVertices : IBufferDescriptorRequest
        {
            /// <summary>
            /// Requester Id
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// Data to assign
            /// </summary>
            public IEnumerable<IVertexData> Data { get; set; }
            /// <summary>
            /// Gets or sets wheter the destination buffer must be dynamic or not
            /// </summary>
            public bool Dynamic { get; set; }
            /// <summary>
            /// Gets or sets de number of instances of this geometry
            /// </summary>
            public int Instances { get; set; }
            /// <summary>
            /// Descriptor
            /// </summary>
            public BufferDescriptor Descriptor { get; set; } = new BufferDescriptor();
            /// <summary>
            /// Request action
            /// </summary>
            public BufferDescriptorRequestActions Action { get; set; } = BufferDescriptorRequestActions.None;
            /// <summary>
            /// Gets wheter the descriptor is processed into the buffer manager or not
            /// </summary>
            public bool Processed { get; private set; } = false;

            /// <summary>
            /// Updates the specified buffer manager
            /// </summary>
            /// <param name="bufferManager">Buffer manager</param>
            public void Update(BufferManager bufferManager)
            {
                if (Action == BufferDescriptorRequestActions.Add)
                {
                    AssignToBuffers(bufferManager);
                }
                else if (Action == BufferDescriptorRequestActions.Remove)
                {
                    RemoveFromBuffers(bufferManager);
                }

                Processed = true;
            }
            /// <summary>
            /// Assign the descriptor to the specified buffer manager
            /// </summary>
            /// <param name="bufferManager">Buffer manager</param>
            private void AssignToBuffers(BufferManager bufferManager)
            {
                if (Data?.Any() != true)
                {
                    return;
                }

                VertexBufferDescription descriptor;

                VertexTypes vType = Data.First().VertexType;

                Console.WriteLine($"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {vType} [{Id}]");

                var slot = bufferManager.vertexData.FindIndex(k => k.Type == vType && k.Dynamic == Dynamic);
                if (slot < 0)
                {
                    slot = bufferManager.vertexData.Count;

                    descriptor = new VertexBufferDescription(vType, Dynamic);

                    bufferManager.vertexData.Add(descriptor);
                }
                else
                {
                    descriptor = bufferManager.vertexData[slot];
                    descriptor.ReallocationNeeded = true;

                    bufferManager.vertexBufferAllocationNeeded = true;
                }

                descriptor.AddDescriptor(Descriptor, Id, slot, Data, Instances);

                if (descriptor.AllocatedSize != descriptor.ToAllocateSize)
                {
                    bufferManager.vertexBufferAllocationNeeded = true;
                }
            }
            /// <summary>
            /// Remove the descriptor from de internal buffers of the specified buffer manager
            /// </summary>
            /// <param name="bufferManager">Buffer manager</param>
            private void RemoveFromBuffers(BufferManager bufferManager)
            {
                if (Descriptor.Slot >= 0)
                {
                    var data = bufferManager.vertexData[Descriptor.Slot];

                    Console.WriteLine($"Remove BufferDescriptor {(data.Dynamic ? "dynamic" : "static")} {data.Type} [{Descriptor.Id}]");

                    data.RemoveDescriptor(Descriptor, Instances);
                    data.ReallocationNeeded = true;
                }
            }
        }
        /// <summary>
        /// Index buffer descriptor request
        /// </summary>
        class BufferDescriptorRequestIndices : IBufferDescriptorRequest
        {
            /// <summary>
            /// Requester Id
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// Data to assign
            /// </summary>
            public IEnumerable<uint> Data { get; set; }
            /// <summary>
            /// Gets or sets wheter the destination buffer must be dynamic or not
            /// </summary>
            public bool Dynamic { get; set; }
            /// <summary>
            /// Descriptor
            /// </summary>
            public BufferDescriptor Descriptor { get; set; } = new BufferDescriptor();
            /// <summary>
            /// Request action
            /// </summary>
            public BufferDescriptorRequestActions Action { get; set; } = BufferDescriptorRequestActions.None;
            /// <summary>
            /// Gets wheter the descriptor is processed into the buffer manager or not
            /// </summary>
            public bool Processed { get; private set; } = false;

            /// <summary>
            /// Updates the specified buffer manager
            /// </summary>
            /// <param name="bufferManager">Buffer manager</param>
            public void Update(BufferManager bufferManager)
            {
                if (Action == BufferDescriptorRequestActions.Add)
                {
                    AssignToBuffers(bufferManager);
                }
                else if (Action == BufferDescriptorRequestActions.Remove)
                {
                    RemoveFromBuffers(bufferManager);
                }

                Processed = true;
            }
            /// <summary>
            /// Assign the descriptor to the specified buffer manager
            /// </summary>
            /// <param name="bufferManager">Buffer manager</param>
            private void AssignToBuffers(BufferManager bufferManager)
            {
                if (Data?.Any() != true)
                {
                    return;
                }

                IndexBufferDescription descriptor;

                Console.WriteLine($"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {typeof(uint)} [{Id}]");

                var keyIndex = bufferManager.indexData.FindIndex(k => k.Dynamic == Dynamic);
                if (keyIndex < 0)
                {
                    keyIndex = bufferManager.indexData.Count;

                    descriptor = new IndexBufferDescription(Dynamic);

                    bufferManager.indexData.Add(descriptor);
                }
                else
                {
                    descriptor = bufferManager.indexData[keyIndex];
                    descriptor.ReallocationNeeded = true;

                    bufferManager.indexBufferAllocationNeeded = true;
                }

                descriptor.AddDescriptor(Descriptor, Id, keyIndex, Data);

                if (descriptor.AllocatedSize != descriptor.ToAllocateSize)
                {
                    bufferManager.indexBufferAllocationNeeded = true;
                }
            }
            /// <summary>
            /// Remove the descriptor from de internal buffers of the specified buffer manager
            /// </summary>
            /// <param name="bufferManager">Buffer manager</param>
            private void RemoveFromBuffers(BufferManager bufferManager)
            {
                if (Descriptor?.Slot >= 0)
                {
                    var data = bufferManager.indexData[Descriptor.Slot];

                    Console.WriteLine($"Remove BufferDescriptor {(data.Dynamic ? "dynamic" : "static")} {typeof(uint)} [{Descriptor.Id}]");

                    data.RemoveDescriptor(Descriptor);
                    data.ReallocationNeeded = true;
                }
            }
        }

        /// <summary>
        /// Creates a vertex buffer from IVertexData
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <returns>Returns new buffer</returns>
        private static Buffer CreateVertexBuffer(Graphics graphics, string name, IEnumerable<IVertexData> vertices, bool dynamic)
        {
            if (vertices?.Any() != true)
            {
                return null;
            }

            return graphics.CreateVertexBuffer(name, vertices, dynamic);
        }
        /// <summary>
        /// Creates an instancing buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Instancing data</param>
        /// <returns>Returns the new buffer</returns>
        private static Buffer CreateInstancingBuffer(Graphics graphics, string name, IEnumerable<VertexInstancingData> data)
        {
            if (data?.Any() != true)
            {
                return null;
            }

            return graphics.CreateVertexBuffer(name, data, true);
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
            if (indices?.Any() != true)
            {
                return null;
            }

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
        /// Allocating buffers flag
        /// </summary>
        private bool allocating = false;
        /// <summary>
        /// Descriptor request list
        /// </summary>
        private readonly List<IBufferDescriptorRequest> requestedDescriptors = new List<IBufferDescriptorRequest>();

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
        /// Gets whether the manager is initialized or not
        /// </summary>
        public bool Initilialized { get; set; } = false;
        /// <summary>
        /// Gets whether any internal buffer descriptor is dirty
        /// </summary>
        /// <remarks>If not initialized, returns always false</remarks>
        public bool IsDirty
        {
            get
            {
                if (!Initilialized)
                {
                    return false;
                }

                return this.requestedDescriptors.Any();
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
                allocating = true;
                vertexBufferAllocationNeeded = false;
                indexBufferAllocationNeeded = false;

                requestedDescriptors.Clear();

                vertexData.Clear();
                indexData.Clear();

                vertexBufferBindings.Clear();

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
            BufferDescriptorRequestVertices request = new BufferDescriptorRequestVertices
            {
                Id = id,
                Data = data,
                Dynamic = dynamic,
                Instances = instances,
                Action = BufferDescriptorRequestActions.Add,
            };

            requestedDescriptors.Add(request);

            return request.Descriptor;
        }
        /// <summary>
        /// Adds indices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">Index list</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        public BufferDescriptor Add(string id, IEnumerable<uint> data, bool dynamic)
        {
            BufferDescriptorRequestIndices request = new BufferDescriptorRequestIndices
            {
                Id = id,
                Data = data,
                Dynamic = dynamic,
                Action = BufferDescriptorRequestActions.Add,
            };

            requestedDescriptors.Add(request);

            return request.Descriptor;
        }

        /// <summary>
        /// Removes vertex data from buffer manager
        /// </summary>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="instances">Instance count</param>
        public void RemoveVertexData(BufferDescriptor descriptor, int instances)
        {
            BufferDescriptorRequestVertices request = new BufferDescriptorRequestVertices
            {
                Id = descriptor.Id,
                Descriptor = descriptor,
                Instances = instances,
                Action = BufferDescriptorRequestActions.Remove,
            };

            requestedDescriptors.Add(request);
        }
        /// <summary>
        /// Removes index data from buffer manager
        /// </summary>
        /// <param name="descriptor">Buffer descriptor</param>
        public void RemoveIndexData(BufferDescriptor descriptor)
        {
            BufferDescriptorRequestIndices request = new BufferDescriptorRequestIndices
            {
                Id = descriptor.Id,
                Descriptor = descriptor,
                Action = BufferDescriptorRequestActions.Remove,
            };

            requestedDescriptors.Add(request);
        }

        /// <summary>
        /// Creates and populates vertex, instancing and index buffers
        /// </summary>
        public void CreateBuffers()
        {
            if (allocating)
            {
                return;
            }

            allocating = true;

            if (!Initilialized)
            {
                Console.WriteLine($"Creating buffers");

                CreateVertexBuffers();
                CreateInstancingBuffers();
                CreateIndexBuffers();

                Console.WriteLine($"Buffers created");

                Initilialized = true;
            }
            else
            {
                Console.WriteLine($"Processing descriptor requests");

                DoProcessRequest();

                Console.WriteLine($"Descriptor requests processed");

                Console.WriteLine($"Reallocating buffers");

                DoReallocation();

                Console.WriteLine($"Buffers reallocated");
            }

            allocating = false;
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
                    //Empty buffer
                    vertexBuffers.Add(null);
                    vertexBufferBindings.Add(new VertexBufferBinding());

                    descriptor.ClearInputs();

                    descriptor.AllocatedSize = descriptor.Data.Count();
                    descriptor.Allocated = true;
                    descriptor.ReallocationNeeded = false;
                }
                else
                {
                    int slot = vertexBuffers.Count;
                    string name = $"VertexBuffer.{slot}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = CreateVertexBuffer(game.Graphics, name, descriptor.Data, descriptor.Dynamic);
                    var binding = new VertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(binding);

                    descriptor.AddInputs(slot);

                    descriptor.AllocatedSize = descriptor.Data.Count();
                    descriptor.Allocated = true;
                    descriptor.ReallocationNeeded = false;

                    Console.WriteLine($"Created {name} and binding. Size {descriptor.Data.Count()}");
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

            //Create the buffers from empry instancing data
            var data = new VertexInstancingData[instances];
            var buffer = CreateInstancingBuffer(game.Graphics, "InstancingBuffer", data);
            var binding = new VertexBufferBinding(buffer, data[0].GetStride(), 0);

            //Track offsets in the internal collections
            this.instancingBufferOffset = this.vertexBuffers.Count;
            this.instancingBufferBindingOffset = this.vertexBufferBindings.Count;

            //Add buffer and binding to the internal collections
            this.vertexBuffers.Add(buffer);
            this.vertexBufferBindings.Add(binding);

            //Set inputs to descriptors
            foreach (var descriptor in this.vertexData.Where(d => d.Instances > 0))
            {
                descriptor.AddInstancingInputs(VertexInstancingData.Input(instancingBufferOffset));
                descriptor.AllocatedInstancing = true;
            }

            Console.WriteLine($"Created InstancingBuffer and binding. Size {data.Count()}");
        }
        /// <summary>
        /// Creates index buffers
        /// </summary>
        private void CreateIndexBuffers()
        {
            foreach (var descriptor in this.indexData)
            {
                string name = $"IndexBuffer.{(descriptor.Dynamic ? "dynamic" : "static")}";
                var buffer = CreateIndexBuffer(this.game.Graphics, name, descriptor.Data, descriptor.Dynamic);

                indexBuffers.Add(buffer);

                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                Console.WriteLine($"Created {name}. Size {descriptor.Data.Count()}");
            }

            this.indexBufferAllocationNeeded = false;
        }

        /// <summary>
        /// Do descriptor request processing
        /// </summary>
        private void DoProcessRequest()
        {
            //Copy request collection
            var toAssign = this.requestedDescriptors.ToArray();

            foreach (var request in toAssign.Where(r => !r.Processed))
            {
                request.Update(this);
            }

            Monitor.Enter(this.requestedDescriptors);
            this.requestedDescriptors.RemoveAll(r => r.Processed);
            Monitor.Exit(this.requestedDescriptors);
        }

        /// <summary>
        /// Does the reallocation of the internal buffers in the device
        /// </summary>
        private void DoReallocation()
        {
            if (vertexBufferAllocationNeeded)
            {
                bool reallocateInstances = ReallocateVertexData();

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
        private bool ReallocateVertexData()
        {
            bool reallocateInstances = false;

            for (int i = 0; i < this.vertexData.Count; i++)
            {
                var descriptor = this.vertexData[i];

                if (!descriptor.Dirty)
                {
                    continue;
                }

                //Dispose current buffer
                if (descriptor.Allocated)
                {
                    this.vertexBuffers[i]?.Dispose();

                    //Recreate the buffer and binding
                    string name = $"VertexBuffer.{i}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = CreateVertexBuffer(game.Graphics, name, descriptor.Data, descriptor.Dynamic);
                    var binding = new VertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    this.vertexBuffers[i] = buffer;
                    this.vertexBufferBindings[i] = binding;

                    Console.WriteLine($"Reallocated {name} and binding. Size {descriptor.Data.Count()}");
                }
                else
                {
                    int slot = vertexBuffers.Count;

                    //Create the buffer and binding
                    string name = $"VertexBuffer.{slot}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = CreateVertexBuffer(game.Graphics, name, descriptor.Data, descriptor.Dynamic);
                    var binding = new VertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    this.vertexBuffers.Add(buffer);
                    this.vertexBufferBindings.Add(binding);

                    descriptor.AddInputs(slot);

                    Console.WriteLine($"Created {name} and binding. Size {descriptor.Data.Count()}");
                }

                //Updates the allocated buffer size
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                reallocateInstances = true;
            }

            return reallocateInstances;
        }
        /// <summary>
        /// Reallocates the instance data
        /// </summary>
        private void ReallocateInstances()
        {
            if (instancingBufferOffset >= 0)
            {
                //Dispose current buffer and clear references
                this.vertexBuffers[instancingBufferOffset]?.Dispose();
                this.vertexBuffers[instancingBufferOffset] = null;
                this.vertexBufferBindings[instancingBufferBindingOffset] = new VertexBufferBinding();
            }

            //Clear inputs to descriptors
            foreach (var descriptor in this.vertexData)
            {
                descriptor.ClearInstancingInputs();
                descriptor.AllocatedInstancing = false;
            }

            int instances = this.TotalInstances;
            if (instances <= 0)
            {
                return;
            }

            //Recreate the buffer and binding
            var data = new VertexInstancingData[instances];
            var buffer = CreateInstancingBuffer(game.Graphics, "InstancingBuffer", data);
            var binding = new VertexBufferBinding(buffer, data[0].GetStride(), 0);

            if (instancingBufferOffset >= 0)
            {
                //Reasign to the internal collections
                this.vertexBuffers[instancingBufferOffset] = buffer;
                this.vertexBufferBindings[instancingBufferBindingOffset] = binding;

                Console.WriteLine($"Reallocated InstancingBuffer. Size {data.Count()}");
            }
            else
            {
                //Track offsets in the internal collections
                this.instancingBufferOffset = this.vertexBuffers.Count;
                this.instancingBufferBindingOffset = this.vertexBufferBindings.Count;

                //Add buffer and binding to the internal collections
                this.vertexBuffers.Add(buffer);
                this.vertexBufferBindings.Add(binding);

                Console.WriteLine($"Created InstancingBuffer. Size {data.Count()}");
            }

            //Set inputs to descriptors
            foreach (var descriptor in this.vertexData)
            {
                if (descriptor.Instances > 0)
                {
                    descriptor.AddInstancingInputs(VertexInstancingData.Input(instancingBufferOffset));
                }
                else
                {
                    descriptor.ClearInstancingInputs();
                }

                descriptor.AllocatedInstancing = true;
            }
        }
        /// <summary>
        /// Reallocates the index data
        /// </summary>
        private void ReallocateIndexData()
        {
            for (int i = 0; i < this.indexData.Count; i++)
            {
                var descriptor = this.indexData[i];

                if (!descriptor.Dirty)
                {
                    continue;
                }

                //Recreate the buffer
                string name = $"IndexBuffer.{(descriptor.Dynamic ? "dynamic" : "static")}";
                var buffer = CreateIndexBuffer(game.Graphics, name, descriptor.Data, descriptor.Dynamic);

                if (descriptor.Allocated)
                {
                    //Dispose current buffer
                    this.indexBuffers[i]?.Dispose();
                    this.indexBuffers[i] = buffer;

                    Console.WriteLine($"Reallocated {name}. Size {descriptor.Data.Count()}");
                }
                else
                {
                    this.indexBuffers.Add(buffer);

                    Console.WriteLine($"Created {name}. Size {descriptor.Data.Count()}");
                }

                //Updates the allocated buffer size
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;
            }
        }

        /// <summary>
        /// Sets vertex buffers to device context
        /// </summary>
        public bool SetVertexBuffers()
        {
            if (!Initilialized)
            {
                Console.WriteLine("Attempt to set vertex buffers to Input Assembler with no initialized manager");
                return false;
            }

            if (this.vertexData.Any(d => d.Dirty))
            {
                return false;
            }

            this.game.Graphics.IASetVertexBuffers(0, this.vertexBufferBindings.ToArray());

            return true;
        }
        /// <summary>
        /// Sets index buffers to device context
        /// </summary>
        /// <param name="slot">Slot</param>
        public bool SetIndexBuffer(int slot)
        {
            if (!Initilialized)
            {
                Console.WriteLine("Attempt to set index buffers to Input Assembler with no initialized manager");
                return false;
            }

            if (slot < 0)
            {
                return false;
            }

            var descriptor = this.indexData[slot];
            if (descriptor.Dirty)
            {
                Console.WriteLine($"Attempt to set index buffer in slot {slot} to Input Assembler with no allocated buffer");
                return false;
            }

            this.game.Graphics.IASetIndexBuffer(this.indexBuffers[slot], Format.R32_UInt, 0);
            return true;
        }
        /// <summary>
        /// Sets input layout to device context
        /// </summary>
        /// <param name="technique">Technique</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="topology">Topology</param>
        public bool SetInputAssembler(EngineEffectTechnique technique, int slot, Topology topology)
        {
            if (!Initilialized)
            {
                Console.WriteLine("Attempt to set technique to Input Assembler with no initialized manager");
                return false;
            }

            if (slot < 0)
            {
                return false;
            }

            var descriptor = this.vertexData[slot];
            if (descriptor.Dirty)
            {
                Console.WriteLine($"Attempt to set technique in slot {slot} to Input Assembler with no allocated buffer");
                return false;
            }

            //The technique defines the vertex type
            if (!inputLayouts.ContainsKey(technique))
            {
                var signature = technique.GetSignature();

                this.inputLayouts.Add(
                    technique,
                    this.game.Graphics.CreateInputLayout(signature, descriptor.Input.ToArray()));
            }

            this.game.Graphics.IAInputLayout = inputLayouts[technique];
            this.game.Graphics.IAPrimitiveTopology = (PrimitiveTopology)topology;
            return true;
        }

        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="data">Instancig data</param>
        public bool WriteInstancingData(IEnumerable<VertexInstancingData> data)
        {
            if (!Initilialized)
            {
                Console.WriteLine("Attempt to write instancing data with no initialized manager");
                return false;
            }

            if (instancingBufferOffset < 0)
            {
                Console.WriteLine("Attempt to write instancing data with no allocated buffer");
                return false;
            }

            if (data?.Any() == true)
            {
                var instancingBuffer = this.vertexBuffers[instancingBufferOffset];
                if (instancingBuffer != null)
                {
                    this.game.Graphics.WriteDiscardBuffer(instancingBuffer, data);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="vertexBufferSlot">Slot</param>
        /// <param name="vertexBufferOffset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer<T>(int vertexBufferSlot, int vertexBufferOffset, IEnumerable<T> data) where T : struct, IVertexData
        {
            if (!Initilialized)
            {
                Console.WriteLine($"Attempt to write vertex data in slot {vertexBufferSlot} with no initialized manager");
                return;
            }

            var descriptor = this.vertexData[vertexBufferSlot];
            if (descriptor.Dirty)
            {
                Console.WriteLine($"Attempt to write vertex data in slot {vertexBufferSlot} with no allocated buffer");
                return;
            }

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
            if (!Initilialized)
            {
                Console.WriteLine($"Attempt to write index data in slot {indexBufferSlot} with no initialized manager");
                return;
            }

            var descriptor = this.indexData[indexBufferSlot];
            if (descriptor.Dirty)
            {
                Console.WriteLine($"Attempt to write index data in slot {indexBufferSlot} with no allocated buffer");
                return;
            }

            if (data?.Any() == true)
            {
                var buffer = this.indexBuffers[indexBufferSlot];

                this.game.Graphics.WriteNoOverwriteBuffer(buffer, indexBufferOffset, data);
            }
        }
    }
}
