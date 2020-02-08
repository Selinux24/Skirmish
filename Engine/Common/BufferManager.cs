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
        /// Creates a vertex buffer from IVertexData
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="vertices">Vertices</param>
        /// <returns>Returns new buffer</returns>
        private static Buffer CreateVertexBuffer(Graphics graphics, string name, bool dynamic, IEnumerable<IVertexData> vertices)
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
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="instancingData">Instancing data</param>
        /// <returns>Returns the new buffer</returns>
        private static Buffer CreateInstancingBuffer(Graphics graphics, string name, bool dynamic, IEnumerable<VertexInstancingData> instancingData)
        {
            if (instancingData?.Any() != true)
            {
                return null;
            }

            return graphics.CreateVertexBuffer(name, instancingData, dynamic);
        }
        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="indices">Indices</param>
        /// <returns>Returns new buffer</returns>
        private static Buffer CreateIndexBuffer(Graphics graphics, string name, bool dynamic, IEnumerable<uint> indices)
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
        /// Descriptor request list
        /// </summary>
        private readonly List<IBufferDescriptorRequest> requestedDescriptors = new List<IBufferDescriptorRequest>();
        /// <summary>
        /// Vertex buffers
        /// </summary>
        private readonly List<Buffer> vertexBuffers = new List<Buffer>();
        /// <summary>
        /// Vertex buffer bindings
        /// </summary>
        private readonly List<VertexBufferBinding> vertexBufferBindings = new List<VertexBufferBinding>();
        /// <summary>
        /// Index buffer
        /// </summary>
        private readonly List<Buffer> indexBuffers = new List<Buffer>();
        /// <summary>
        /// Vertex buffer descriptors
        /// </summary>
        private readonly List<VertexBufferDescription> vertexBufferDescriptors = new List<VertexBufferDescription>();
        /// <summary>
        /// Instancing buffer descriptors
        /// </summary>
        private readonly List<InstancingBufferDescription> instancingBufferDescriptors = new List<InstancingBufferDescription>();
        /// <summary>
        /// Index buffer descriptors
        /// </summary>
        private readonly List<IndexBufferDescription> indexBufferDescriptors = new List<IndexBufferDescription>();
        /// <summary>
        /// Input layouts by technique
        /// </summary>
        private readonly Dictionary<EngineEffectTechnique, InputLayout> inputLayouts = new Dictionary<EngineEffectTechnique, InputLayout>();
        /// <summary>
        /// Allocating buffers flag
        /// </summary>
        private bool allocating = false;
        /// <summary>
        /// Vertex buffer allocation needed
        /// </summary>
        private bool vertexBufferAllocationNeeded = false;
        /// <summary>
        /// Instancing buffer allocation needed
        /// </summary>
        private bool instancingBufferAllocationNeeded = false;
        /// <summary>
        /// Index buffer allocation needed
        /// </summary>
        private bool indexBufferAllocationNeeded = false;

        /// <summary>
        /// Gets whether the manager is initialized or not
        /// </summary>
        public bool Initilialized { get; set; } = false;
        /// <summary>
        /// Gets whether any internal buffer descriptor has pending requests
        /// </summary>
        /// <remarks>If not initialized, returns always false</remarks>
        public bool HasPendingRequests
        {
            get
            {
                if (!Initilialized)
                {
                    return false;
                }

                return this.requestedDescriptors.Count > 0;
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
                this.vertexBufferDescriptors.Add(new VertexBufferDescription(VertexTypes.Unknown, true));
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
                instancingBufferAllocationNeeded = false;
                indexBufferAllocationNeeded = false;

                requestedDescriptors.Clear();

                vertexBufferDescriptors.Clear();
                indexBufferDescriptors.Clear();

                vertexBufferBindings.Clear();

                vertexBuffers.ForEach(b => b?.Dispose());
                vertexBuffers.Clear();

                indexBuffers.ForEach(b => b?.Dispose());
                indexBuffers.Clear();

                inputLayouts.Values.ToList().ForEach(il => il?.Dispose());
                inputLayouts.Clear();
            }
        }

        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <typeparam name="T">Type of vertex</typeparam>
        /// <param name="id">Id</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="data">Vertex list</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        public BufferDescriptor AddVertexData<T>(string id, bool dynamic, IEnumerable<T> data, BufferDescriptor instancingBuffer = null) where T : struct, IVertexData
        {
            return this.AddVertexData(
                id,
                dynamic,
                data.OfType<IVertexData>(),
                instancingBuffer);
        }
        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="data">Vertex list</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        public BufferDescriptor AddVertexData(string id, bool dynamic, IEnumerable<IVertexData> data, BufferDescriptor instancingBuffer = null)
        {
            BufferDescriptorRequestVertices request = new BufferDescriptorRequestVertices
            {
                Id = id,
                Data = data,
                Dynamic = dynamic,
                InstancingDescriptor = instancingBuffer,
                Action = BufferDescriptorRequestActions.Add,
            };

            requestedDescriptors.Add(request);

            return request.VertexDescriptor;
        }
        /// <summary>
        /// Adds instances to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="instances">Number of instances</param>
        public BufferDescriptor AddInstancingData(string id, bool dynamic, int instances)
        {
            BufferDescriptorRequestInstancing request = new BufferDescriptorRequestInstancing
            {
                Id = id,
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
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="data">Index list</param>
        public BufferDescriptor AddIndexData(string id, bool dynamic, IEnumerable<uint> data)
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
        public void RemoveVertexData(BufferDescriptor descriptor)
        {
            BufferDescriptorRequestVertices request = new BufferDescriptorRequestVertices
            {
                Id = descriptor.Id,
                VertexDescriptor = descriptor,
                Action = BufferDescriptorRequestActions.Remove,
            };

            requestedDescriptors.Add(request);
        }
        /// <summary>
        /// Removes instancing data from buffer manager
        /// </summary>
        /// <param name="descriptor">Buffer descriptor</param>
        public void RemoveInstancingData(BufferDescriptor descriptor)
        {
            BufferDescriptorRequestInstancing request = new BufferDescriptorRequestInstancing
            {
                Id = descriptor.Id,
                Descriptor = descriptor,
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
        public void CreateBuffers(IProgress<float> progress)
        {
            if (allocating)
            {
                return;
            }

            allocating = true;

            if (!Initilialized)
            {
                Console.WriteLine($"Creating buffers");

                CreateReservedBuffers();
                CreateInstancingBuffers(progress);
                CreateVertexBuffers(progress);
                CreateIndexBuffers(progress);

                Console.WriteLine($"Buffers created");

                Initilialized = true;
            }

            if (HasPendingRequests)
            {
                Console.WriteLine($"Processing descriptor requests");

                DoProcessRequest(progress);

                Console.WriteLine($"Descriptor requests processed");

                Console.WriteLine($"Reallocating buffers");

                DoReallocation(progress);

                Console.WriteLine($"Buffers reallocated");
            }

            allocating = false;
        }

        /// <summary>
        /// Creates reserved buffers
        /// </summary>
        private void CreateReservedBuffers()
        {
            for (int i = 0; i < reservedSlots; i++)
            {
                var descriptor = this.vertexBufferDescriptors[i];

                int bufferIndex = vertexBuffers.Count;
                int bindingIndex = vertexBufferBindings.Count;

                string name = $"Reserved buffer.{bufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";

                //Empty buffer
                vertexBuffers.Add(null);
                vertexBufferBindings.Add(new VertexBufferBinding());

                descriptor.ClearInputs();

                descriptor.BufferIndex = bufferIndex;
                descriptor.BufferBindingIndex = bindingIndex;
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                Console.WriteLine($"Created {name} and binding. Size {descriptor.Data.Count()}");
            }
        }
        /// <summary>
        /// Creates the instancing buffers
        /// </summary>
        private void CreateInstancingBuffers(IProgress<float> progress)
        {
            float total = this.instancingBufferDescriptors.Count;
            float current = 0;

            foreach (var descriptor in this.instancingBufferDescriptors)
            {
                int bufferIndex = vertexBuffers.Count;
                int bindingIndex = vertexBufferBindings.Count;

                string name = $"InstancingBuffer.{bufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";

                VertexInstancingData[] data = new VertexInstancingData[descriptor.Instances];
                var buffer = CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);
                var binding = new VertexBufferBinding(buffer, data[0].GetStride(), 0);

                vertexBuffers.Add(buffer);
                vertexBufferBindings.Add(binding);

                descriptor.BufferIndex = bufferIndex;
                descriptor.BufferBindingIndex = bindingIndex;
                descriptor.AllocatedSize = descriptor.Instances;
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                Console.WriteLine($"Created {name} and binding. Size {descriptor.Instances}");

                progress?.Report(++current / total);
            }

            this.instancingBufferAllocationNeeded = false;
        }
        /// <summary>
        /// Creates the vertex buffers
        /// </summary>
        private void CreateVertexBuffers(IProgress<float> progress)
        {
            float total = this.vertexBufferDescriptors.Count;
            float current = 0;

            foreach (var descriptor in this.vertexBufferDescriptors)
            {
                if (!descriptor.Dirty)
                {
                    continue;
                }

                int bufferIndex = vertexBuffers.Count;
                int bindingIndex = vertexBufferBindings.Count;

                string name = $"VertexBuffer.{bufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";

                var buffer = CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);
                var binding = new VertexBufferBinding(buffer, descriptor.GetStride(), 0);

                vertexBuffers.Add(buffer);
                vertexBufferBindings.Add(binding);

                descriptor.AddInputs(bufferIndex);

                if (descriptor.InstancingDescriptor != null)
                {
                    var instancingSlot = this.instancingBufferDescriptors[descriptor.InstancingDescriptor.Slot].BufferIndex;
                    descriptor.AddInstancingInputs(instancingSlot);
                }

                descriptor.BufferIndex = bufferIndex;
                descriptor.BufferBindingIndex = bindingIndex;
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                Console.WriteLine($"Created {name} and binding. Size {descriptor.Data.Count()}");

                progress?.Report(++current / total);
            }

            this.vertexBufferAllocationNeeded = false;
        }
        /// <summary>
        /// Creates index buffers
        /// </summary>
        private void CreateIndexBuffers(IProgress<float> progress)
        {
            float total = this.indexBufferDescriptors.Count;
            float current = 0;

            foreach (var descriptor in this.indexBufferDescriptors)
            {
                int slot = indexBuffers.Count;
                string name = $"IndexBuffer.{slot}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                var buffer = CreateIndexBuffer(this.game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                indexBuffers.Add(buffer);

                descriptor.BufferIndex = indexBuffers.Count - 1;
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                Console.WriteLine($"Created {name}. Size {descriptor.Data.Count()}");

                progress?.Report(++current / total);
            }

            this.indexBufferAllocationNeeded = false;
        }

        /// <summary>
        /// Do descriptor request processing
        /// </summary>
        private void DoProcessRequest(IProgress<float> progress)
        {
            //Copy request collection
            var toAssign = this.requestedDescriptors.ToArray();
            toAssign = toAssign.Where(r => !r.Processed).ToArray();

            float total = toAssign.Length;
            float current = 0;

            foreach (var request in toAssign)
            {
                Update(request);

                progress?.Report(++current / total);
            }

            Monitor.Enter(this.requestedDescriptors);
            this.requestedDescriptors.RemoveAll(r => r.Processed);
            Monitor.Exit(this.requestedDescriptors);
        }

        /// <summary>
        /// Does the reallocation of the internal buffers in the device
        /// </summary>
        private void DoReallocation(IProgress<float> progress)
        {
            if (instancingBufferAllocationNeeded)
            {
                ReallocateInstances(progress);

                instancingBufferAllocationNeeded = false;
            }

            if (vertexBufferAllocationNeeded)
            {
                ReallocateVertexData(progress);

                vertexBufferAllocationNeeded = false;
            }

            if (indexBufferAllocationNeeded)
            {
                ReallocateIndexData(progress);

                indexBufferAllocationNeeded = false;
            }
        }
        /// <summary>
        /// Reallocates the instance data
        /// </summary>
        private void ReallocateInstances(IProgress<float> progress)
        {
            var dirtyList = this.instancingBufferDescriptors.Where(v => v.Dirty).ToArray();

            float total = dirtyList.Length;
            float current = 0;

            for (int i = 0; i < dirtyList.Length; i++)
            {
                var descriptor = dirtyList[i];

                if (descriptor.Allocated)
                {
                    //Dispose current buffer
                    this.vertexBuffers[descriptor.BufferIndex]?.Dispose();

                    //Recreate the buffer and binding
                    string name = $"InstancingBuffer.{i}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    VertexInstancingData[] data = new VertexInstancingData[descriptor.Instances];
                    var buffer = CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);
                    var binding = new VertexBufferBinding(buffer, data[0].GetStride(), 0);

                    this.vertexBuffers[descriptor.BufferIndex] = buffer;
                    this.vertexBufferBindings[descriptor.BufferBindingIndex] = binding;

                    Console.WriteLine($"Reallocated {name}. Size {descriptor.Instances}");
                }
                else
                {
                    int bufferIndex = vertexBuffers.Count;
                    int bindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = $"InstancingBuffer.{bufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    VertexInstancingData[] data = new VertexInstancingData[descriptor.Instances];
                    var buffer = CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);
                    var binding = new VertexBufferBinding(buffer, data[0].GetStride(), 0);

                    this.vertexBuffers.Add(buffer);
                    this.vertexBufferBindings.Add(binding);

                    descriptor.BufferIndex = bufferIndex;
                    descriptor.BufferBindingIndex = bindingIndex;

                    Console.WriteLine($"Created {name} and binding. Size {descriptor.Instances}");
                }

                //Updates the allocated buffer size
                descriptor.AllocatedSize = descriptor.Instances;
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                progress?.Report(++current / total);
            }
        }
        /// <summary>
        /// Reallocates the vertex data
        /// </summary>
        /// <param name="reallocateInstances">Returns wether instance reallocation is necessary</param>
        private void ReallocateVertexData(IProgress<float> progress)
        {
            var dirtyList = this.vertexBufferDescriptors.Where(v => v.Dirty).ToArray();

            float total = dirtyList.Length;
            float current = 0;

            for (int i = 0; i < dirtyList.Length; i++)
            {
                var descriptor = dirtyList[i];

                if (descriptor.Allocated)
                {
                    //Dispose current buffer
                    this.vertexBuffers[descriptor.BufferIndex]?.Dispose();

                    //Recreate the buffer and binding
                    string name = $"VertexBuffer.{descriptor.BufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);
                    var binding = new VertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    this.vertexBuffers[descriptor.BufferIndex] = buffer;
                    this.vertexBufferBindings[descriptor.BufferBindingIndex] = binding;

                    Console.WriteLine($"Reallocated {name} and binding. Size {descriptor.Data.Count()}");
                }
                else
                {
                    int bufferIndex = vertexBuffers.Count;
                    int bindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = $"VertexBuffer.{bufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);
                    var binding = new VertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    this.vertexBuffers.Add(buffer);
                    this.vertexBufferBindings.Add(binding);

                    descriptor.AddInputs(bufferIndex);

                    descriptor.BufferIndex = bufferIndex;
                    descriptor.BufferBindingIndex = bindingIndex;

                    Console.WriteLine($"Created {name} and binding. Size {descriptor.Data.Count()}");
                }

                descriptor.ClearInstancingInputs();

                if (descriptor.InstancingDescriptor != null)
                {
                    var instancingSlot = this.instancingBufferDescriptors[descriptor.InstancingDescriptor.Slot].BufferIndex;
                    descriptor.AddInstancingInputs(instancingSlot);
                }

                //Updates the allocated buffer size
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                progress?.Report(++current / total);
            }
        }
        /// <summary>
        /// Reallocates the index data
        /// </summary>
        private void ReallocateIndexData(IProgress<float> progress)
        {
            var dirtyList = this.indexBufferDescriptors.Where(v => v.Dirty).ToArray();

            float total = dirtyList.Length;
            float current = 0;

            for (int i = 0; i < dirtyList.Length; i++)
            {
                var descriptor = dirtyList[i];

                //Recreate the buffer
                string name = $"IndexBuffer.{i}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                var buffer = CreateIndexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                if (descriptor.Allocated)
                {
                    //Dispose current buffer
                    this.indexBuffers[descriptor.BufferIndex]?.Dispose();
                    this.indexBuffers[descriptor.BufferIndex] = buffer;

                    Console.WriteLine($"Reallocated {name}. Size {descriptor.Data.Count()}");
                }
                else
                {
                    this.indexBuffers.Add(buffer);

                    descriptor.BufferIndex = indexBuffers.Count - 1;

                    Console.WriteLine($"Created {name}. Size {descriptor.Data.Count()}");
                }

                //Updates the allocated buffer size
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                progress?.Report(++current / total);
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

            if (this.vertexBufferDescriptors.Any(d => d.Dirty))
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
            if (slot < 0)
            {
                return false;
            }

            if (!Initilialized)
            {
                Console.WriteLine("Attempt to set index buffers to Input Assembler with no initialized manager");
                return false;
            }

            var descriptor = this.indexBufferDescriptors[slot];
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
            if (slot < 0)
            {
                return false;
            }

            if (!Initilialized)
            {
                Console.WriteLine("Attempt to set technique to Input Assembler with no initialized manager");
                return false;
            }

            var descriptor = this.vertexBufferDescriptors[slot];
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
        public void WriteInstancingData(int slot, int offset, IEnumerable<VertexInstancingData> data)
        {
            if (slot < 0)
            {
                return;
            }

            if (offset < 0)
            {
                return;
            }

            if (!Initilialized)
            {
                Console.WriteLine("Attempt to write instancing data with no initialized manager");
                return;
            }

            var descriptor = this.instancingBufferDescriptors[slot];
            if (descriptor.Dirty)
            {
                Console.WriteLine($"Attempt to write instancing data in slot {slot} with no allocated buffer");
                return;
            }

            if (data?.Any() == true)
            {
                var instancingBuffer = this.vertexBuffers[descriptor.BufferIndex];
                if (instancingBuffer != null)
                {
                    this.game.Graphics.WriteDiscardBuffer(instancingBuffer, offset, data);
                }
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="offset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer<T>(int slot, int offset, IEnumerable<T> data) where T : struct, IVertexData
        {
            if (slot < 0)
            {
                return;
            }

            if (offset < 0)
            {
                return;
            }

            if (!Initilialized)
            {
                Console.WriteLine($"Attempt to write vertex data in slot {slot} with no initialized manager");
                return;
            }

            var descriptor = this.vertexBufferDescriptors[slot];
            if (descriptor.Dirty)
            {
                Console.WriteLine($"Attempt to write vertex data in slot {slot} with no allocated buffer");
                return;
            }

            if (data?.Any() == true)
            {
                var buffer = this.vertexBuffers[descriptor.BufferIndex];

                this.game.Graphics.WriteNoOverwriteBuffer(buffer, offset, data);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="data">Data to write</param>
        public void WriteBuffer(int slot, int offset, IEnumerable<uint> data)
        {
            if (slot < 0)
            {
                return;
            }

            if (offset < 0)
            {
                return;
            }

            if (!Initilialized)
            {
                Console.WriteLine($"Attempt to write index data in slot {slot} with no initialized manager");
                return;
            }

            var descriptor = this.indexBufferDescriptors[slot];
            if (descriptor.Dirty)
            {
                Console.WriteLine($"Attempt to write index data in slot {slot} with no allocated buffer");
                return;
            }

            if (data?.Any() == true)
            {
                var buffer = this.indexBuffers[descriptor.BufferIndex];

                this.game.Graphics.WriteNoOverwriteBuffer(buffer, offset, data);
            }
        }

        /// <summary>
        /// Updates the buffer
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void Update(IBufferDescriptorRequest request)
        {
            if (request is BufferDescriptorRequestVertices vRequest)
            {
                UpdateVertexBuffer(vRequest);
            }
            else if (request is BufferDescriptorRequestInstancing ivRequest)
            {
                UpdateInstancingVertexBuffer(ivRequest);
            }
            else if (request is BufferDescriptorRequestIndices iRequest)
            {
                UpdateIndexBuffer(iRequest);
            }
        }

        /// <summary>
        /// Updates the buffer
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void UpdateVertexBuffer(BufferDescriptorRequestVertices request)
        {
            if (request.Action == BufferDescriptorRequestActions.Add)
            {
                AssignToVertexBuffers(request);
            }
            else if (request.Action == BufferDescriptorRequestActions.Remove)
            {
                RemoveFromVertexBuffers(request);
            }

            request.Processed = true;
        }
        /// <summary>
        /// Assign the descriptor to the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void AssignToVertexBuffers(BufferDescriptorRequestVertices request)
        {
            if (request.Data?.Any() != true)
            {
                return;
            }

            VertexBufferDescription descriptor;

            VertexTypes vType = request.Data.First().VertexType;
            bool instanced = request.InstancingDescriptor != null;

            Console.WriteLine($"Add BufferDescriptor {(request.Dynamic ? "dynamic" : "static")} {vType} Instanced {instanced} [{request.Id}]");

            var slot = vertexBufferDescriptors.FindIndex(k =>
                k.Type == vType &&
                k.Dynamic == request.Dynamic);
            if (slot < 0)
            {
                slot = vertexBufferDescriptors.Count;

                descriptor = new VertexBufferDescription(vType, request.Dynamic);

                vertexBufferDescriptors.Add(descriptor);
            }
            else
            {
                descriptor = vertexBufferDescriptors[slot];
                descriptor.ReallocationNeeded = true;

                vertexBufferAllocationNeeded = true;
            }

            if (request.InstancingDescriptor != null)
            {
                descriptor.InstancingDescriptor = request.InstancingDescriptor;
            }

            descriptor.AddDescriptor(request.VertexDescriptor, request.Id, slot, request.Data);

            if (descriptor.AllocatedSize != descriptor.ToAllocateSize)
            {
                vertexBufferAllocationNeeded = true;
            }
        }
        /// <summary>
        /// Remove the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void RemoveFromVertexBuffers(BufferDescriptorRequestVertices request)
        {
            if (request.VertexDescriptor.Slot >= 0)
            {
                var descriptor = vertexBufferDescriptors[request.VertexDescriptor.Slot];

                Console.WriteLine($"Remove BufferDescriptor {(descriptor.Dynamic ? "dynamic" : "static")} {descriptor.Type} [{request.VertexDescriptor.Id}]");

                descriptor.RemoveDescriptor(request.VertexDescriptor);
                descriptor.ReallocationNeeded = true;
            }
        }

        /// <summary>
        /// Updates the buffer
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void UpdateInstancingVertexBuffer(BufferDescriptorRequestInstancing request)
        {
            if (request.Action == BufferDescriptorRequestActions.Add)
            {
                AssignToInstancingVertexBuffers(request);
            }
            else if (request.Action == BufferDescriptorRequestActions.Remove)
            {
                RemoveFromInstancingVertexBuffers(request);
            }

            request.Processed = true;
        }
        /// <summary>
        /// Assign the descriptor to the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void AssignToInstancingVertexBuffers(BufferDescriptorRequestInstancing request)
        {
            InstancingBufferDescription descriptor;

            Console.WriteLine($"Add BufferDescriptor {(request.Dynamic ? "dynamic" : "static")} {typeof(IInstacingData)} [{request.Id}]");

            var slot = instancingBufferDescriptors.FindIndex(k => k.Dynamic == request.Dynamic);
            if (slot < 0)
            {
                slot = instancingBufferDescriptors.Count;

                descriptor = new InstancingBufferDescription(request.Dynamic);

                instancingBufferDescriptors.Add(descriptor);
            }
            else
            {
                descriptor = instancingBufferDescriptors[slot];
                descriptor.ReallocationNeeded = true;

                instancingBufferAllocationNeeded = true;
            }

            descriptor.AddDescriptor(request.Descriptor, request.Id, slot, request.Instances);

            if (descriptor.AllocatedSize != descriptor.ToAllocateSize)
            {
                instancingBufferAllocationNeeded = true;
            }
        }
        /// <summary>
        /// Remove the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void RemoveFromInstancingVertexBuffers(BufferDescriptorRequestInstancing request)
        {
            if (request.Descriptor.Slot >= 0)
            {
                var data = instancingBufferDescriptors[request.Descriptor.Slot];

                Console.WriteLine($"Remove BufferDescriptor {(data.Dynamic ? "dynamic" : "static")} {typeof(VertexInstancingData)} [{request.Descriptor.Id}]");

                data.RemoveDescriptor(request.Descriptor, request.Instances);
                data.ReallocationNeeded = true;
            }
        }

        /// <summary>
        /// Updates the buffer
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void UpdateIndexBuffer(BufferDescriptorRequestIndices request)
        {
            if (request.Action == BufferDescriptorRequestActions.Add)
            {
                AssignToIndexBuffers(request);
            }
            else if (request.Action == BufferDescriptorRequestActions.Remove)
            {
                RemoveFromIndexBuffers(request);
            }

            request.Processed = true;
        }
        /// <summary>
        /// Assign the descriptor to the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void AssignToIndexBuffers(BufferDescriptorRequestIndices request)
        {
            if (request.Data?.Any() != true)
            {
                return;
            }

            IndexBufferDescription descriptor;

            Console.WriteLine($"Add BufferDescriptor {(request.Dynamic ? "dynamic" : "static")} {typeof(uint)} [{request.Id}]");

            var keyIndex = indexBufferDescriptors.FindIndex(k => k.Dynamic == request.Dynamic);
            if (keyIndex < 0)
            {
                keyIndex = indexBufferDescriptors.Count;

                descriptor = new IndexBufferDescription(request.Dynamic);

                indexBufferDescriptors.Add(descriptor);
            }
            else
            {
                descriptor = indexBufferDescriptors[keyIndex];
                descriptor.ReallocationNeeded = true;

                indexBufferAllocationNeeded = true;
            }

            descriptor.AddDescriptor(request.Descriptor, request.Id, keyIndex, request.Data);

            if (descriptor.AllocatedSize != descriptor.ToAllocateSize)
            {
                indexBufferAllocationNeeded = true;
            }
        }
        /// <summary>
        /// Remove the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void RemoveFromIndexBuffers(BufferDescriptorRequestIndices request)
        {
            if (request.Descriptor?.Slot >= 0)
            {
                var data = indexBufferDescriptors[request.Descriptor.Slot];

                Console.WriteLine($"Remove BufferDescriptor {(data.Dynamic ? "dynamic" : "static")} {typeof(uint)} [{request.Descriptor.Id}]");

                data.RemoveDescriptor(request.Descriptor);
                data.ReallocationNeeded = true;
            }
        }
    }
}
