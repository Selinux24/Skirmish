using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Common
{
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;

    /// <summary>
    /// Buffer manager
    /// </summary>
    public class BufferManager : IDisposable
    {
        /// <summary>
        /// Imput asembler key
        /// </summary>
        struct InputAssemblerKey
        {
            /// <summary>
            /// Shader
            /// </summary>
            public IEngineVertexShader Shader;
            /// <summary>
            /// Vertices
            /// </summary>
            public BufferManagerVertices Vertices;
        }

        /// <summary>
        /// Creates a vertex buffer from IVertexData
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="vertices">Vertices</param>
        /// <returns>Returns new buffer</returns>
        private static async Task<EngineBuffer> CreateVertexBuffer(Graphics graphics, string name, bool dynamic, IEnumerable<IVertexData> vertices)
        {
            if (vertices?.Any() != true)
            {
                return null;
            }

            return await Task.Run(() => graphics.CreateVertexBuffer(name, vertices, dynamic));
        }
        /// <summary>
        /// Creates an instancing buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="instancingData">Instancing data</param>
        /// <returns>Returns the new buffer</returns>
        private static async Task<EngineBuffer> CreateInstancingBuffer(Graphics graphics, string name, bool dynamic, IEnumerable<VertexInstancingData> instancingData)
        {
            if (instancingData?.Any() != true)
            {
                return null;
            }

            return await Task.Run(() => graphics.CreateVertexBuffer(name, instancingData, dynamic));
        }
        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="indices">Indices</param>
        /// <returns>Returns new buffer</returns>
        private static async Task<EngineBuffer> CreateIndexBuffer(Graphics graphics, string name, bool dynamic, IEnumerable<uint> indices)
        {
            if (indices?.Any() != true)
            {
                return null;
            }

            return await Task.Run(() => graphics.CreateIndexBuffer(name, indices, dynamic));
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
        private readonly List<EngineBuffer> vertexBuffers = new();
        /// <summary>
        /// Vertex buffer bindings
        /// </summary>
        private readonly List<VertexBufferBinding> vertexBufferBindings = new();
        /// <summary>
        /// Index buffer
        /// </summary>
        private readonly List<EngineBuffer> indexBuffers = new();
        /// <summary>
        /// Vertex buffer descriptors
        /// </summary>
        private readonly List<BufferManagerVertices> vertexBufferDescriptors = new();
        /// <summary>
        /// Instancing buffer descriptors
        /// </summary>
        private readonly List<BufferManagerInstances> instancingBufferDescriptors = new();
        /// <summary>
        /// Index buffer descriptors
        /// </summary>
        private readonly List<BufferManagerIndices> indexBufferDescriptors = new();
        /// <summary>
        /// Input layouts by technique
        /// </summary>
        private readonly Dictionary<EngineEffectTechnique, InputLayout> techniqueInputLayouts = new();
        /// <summary>
        /// Input layouts by vertex shaders
        /// </summary>
        private readonly Dictionary<InputAssemblerKey, InputLayout> vertexShadersInputLayouts = new();
        /// <summary>
        /// Allocating buffers flag
        /// </summary>
        private bool allocating = false;
        /// <summary>
        /// Descriptor request list
        /// </summary>
        private ConcurrentBag<IBufferDescriptorRequest> requestedDescriptors = new();

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
                return PendingRequestCount > 0;
            }
        }
        /// <summary>
        /// Gets the pending request count
        /// </summary>
        public int PendingRequestCount
        {
            get
            {
                if (!Initilialized)
                {
                    return 0;
                }

                return requestedDescriptors.Count;
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
                vertexBufferDescriptors.Add(new BufferManagerVertices(VertexTypes.Unknown, true));
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

                requestedDescriptors.Clear();
                requestedDescriptors = null;

                vertexBufferDescriptors.Clear();
                indexBufferDescriptors.Clear();

                vertexBufferBindings.Clear();

                vertexBuffers.ForEach(b => b?.Dispose());
                vertexBuffers.Clear();

                indexBuffers.ForEach(b => b?.Dispose());
                indexBuffers.Clear();

                techniqueInputLayouts.Values.ToList().ForEach(il => il?.Dispose());
                techniqueInputLayouts.Clear();
            }
        }

        /// <summary>
        /// Creates and populates vertex, instancing and index buffers
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="callback">Callback</param>
        internal async Task CreateBuffersAsync(string id, IProgress<LoadResourceProgress> progress, Action callback)
        {
            await CreateBuffersAsync(id, progress);

            callback?.Invoke();
        }
        /// <summary>
        /// Creates and populates vertex, instancing and index buffers
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="callback">Callback</param>
        internal async Task CreateBuffersAsync(string id, IProgress<LoadResourceProgress> progress, Func<Task> callback)
        {
            await CreateBuffersAsync(id, progress);

            await callback?.Invoke();
        }
        /// <summary>
        /// Creates and populates vertex, instancing and index buffers
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        internal async Task CreateBuffersAsync(string id, IProgress<LoadResourceProgress> progress)
        {
            if (allocating)
            {
                return;
            }

            try
            {
                allocating = true;

                if (!Initilialized)
                {
                    Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Creating reserved buffer descriptors");

                    await CreateReservedBuffers();

                    Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Reserved buffer descriptors created");

                    Initilialized = true;
                }

                if (!HasPendingRequests)
                {
                    return;
                }

                //Copy request collection
                var toAssign = requestedDescriptors
                    .Where(r => r?.Processed == ProcessedStages.Requested)
                    .ToArray();

                float requestTotal = toAssign.Length;

                Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Processing descriptor requests: {requestTotal}");

                float requestCurrent = await DoProcessRequest(id, progress, 0, requestTotal, toAssign);

                Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Descriptor requests processed: {requestCurrent}");

                var instancingList = instancingBufferDescriptors
                    .Where(v => v.Dirty)
                    .ToArray();

                var vertexList = vertexBufferDescriptors
                    .Where(v => v.Dirty)
                    .ToArray();

                var indexList = indexBufferDescriptors
                    .Where(v => v.Dirty)
                    .ToArray();

                float total =
                    requestTotal +
                    instancingList.Length +
                    vertexList.Length +
                    indexList.Length;

                Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Reallocating {total - requestTotal} buffers: Vtx[{vertexList.Length}], Idx[{indexList.Length}], Ins[{instancingList.Length}]");

                float current = await ReallocateInstances(id, progress, requestCurrent, total, instancingList);

                current = await ReallocateVertexData(id, progress, current, total, vertexList);

                current = await ReallocateIndexData(id, progress, current, total, indexList);

                Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Buffers reallocated: {current - requestCurrent}");
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Loading Group {id ?? "no-id"} => Error creating buffers: {ex.Message}", ex);

                throw;
            }
            finally
            {
                allocating = false;
            }
        }
        /// <summary>
        /// Creates reserved buffers
        /// </summary>
        private async Task CreateReservedBuffers()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < reservedSlots; i++)
                {
                    var descriptor = vertexBufferDescriptors[i];

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

                    Logger.WriteTrace(this, $"Created {name} and binding. Size {descriptor.Data.Count()}");
                }
            });
        }
        /// <summary>
        /// Do descriptor request processing
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="current">Current progress value</param>
        /// <param name="total">Total progress value</param>
        /// <param name="toAssign">To assign buffer list</param>
        private async Task<float> DoProcessRequest(string id, IProgress<LoadResourceProgress> progress, float current, float total, IEnumerable<IBufferDescriptorRequest> toAssign)
        {
            foreach (var request in toAssign)
            {
                Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Processing {request}");

                await request.ProcessAsync(this);

                Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Processed {request}");

                progress?.Report(new LoadResourceProgress { Id = id, Progress = ++current / total });
            }

            //Replaces the bag
            requestedDescriptors = new ConcurrentBag<IBufferDescriptorRequest>(requestedDescriptors.Where(r => r?.Processed != ProcessedStages.Processed));

            return current;
        }
        /// <summary>
        /// Reallocates the instance data
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="current">Current progress value</param>
        /// <param name="total">Total progress value</param>
        /// <param name="dirtyList">Dirty buffer list</param>
        private async Task<float> ReallocateInstances(string id, IProgress<LoadResourceProgress> progress, float current, float total, IEnumerable<BufferManagerInstances> dirtyList)
        {
            foreach (var descriptor in dirtyList)
            {
                if (descriptor.Allocated)
                {
                    //Reserve current buffer
                    var oldBuffer = vertexBuffers[descriptor.BufferIndex];

                    //Recreate the buffer and binding
                    string name = $"InstancingBuffer.{descriptor.BufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    VertexInstancingData[] data = new VertexInstancingData[descriptor.Instances];
                    var buffer = await CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);
                    var binding = new VertexBufferBinding(buffer?.GetBuffer(), data[0].GetStride(), 0);

                    vertexBuffers[descriptor.BufferIndex] = buffer;
                    vertexBufferBindings[descriptor.BufferBindingIndex] = binding;

                    //Dispose old buffer
                    oldBuffer?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Reallocated {name}. Size {descriptor.Instances}");
                }
                else
                {
                    int bufferIndex = vertexBuffers.Count;
                    int bindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = $"InstancingBuffer.{bufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    VertexInstancingData[] data = new VertexInstancingData[descriptor.Instances];
                    var buffer = await CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);
                    var binding = new VertexBufferBinding(buffer?.GetBuffer(), data[0].GetStride(), 0);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(binding);

                    descriptor.BufferIndex = bufferIndex;
                    descriptor.BufferBindingIndex = bindingIndex;

                    Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Created {name} and binding. Size {descriptor.Instances}");
                }

                //Updates the allocated buffer size
                descriptor.AllocatedSize = descriptor.Instances;
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                progress?.Report(new LoadResourceProgress { Id = id, Progress = ++current / total });
            }

            return current;
        }
        /// <summary>
        /// Reallocates the vertex data
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="current">Current progress value</param>
        /// <param name="total">Total progress value</param>
        /// <param name="dirtyList">Dirty buffer list</param>
        private async Task<float> ReallocateVertexData(string id, IProgress<LoadResourceProgress> progress, float current, float total, IEnumerable<BufferManagerVertices> dirtyList)
        {
            foreach (var descriptor in dirtyList)
            {
                if (descriptor.Allocated)
                {
                    //Reserve current buffer
                    var oldBuffer = vertexBuffers[descriptor.BufferIndex];

                    //Recreate the buffer and binding
                    string name = $"VertexBuffer.{descriptor.BufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = await CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);
                    var binding = new VertexBufferBinding(buffer?.GetBuffer(), descriptor.GetStride(), 0);

                    vertexBuffers[descriptor.BufferIndex] = buffer;
                    vertexBufferBindings[descriptor.BufferBindingIndex] = binding;

                    //Dispose old buffer
                    oldBuffer?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Reallocated {name} and binding. Size {descriptor.Data.Count()}");
                }
                else
                {
                    int bufferIndex = vertexBuffers.Count;
                    int bindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = $"VertexBuffer.{bufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = await CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);
                    var binding = new VertexBufferBinding(buffer?.GetBuffer(), descriptor.GetStride(), 0);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(binding);

                    descriptor.AddInputs(bufferIndex);

                    descriptor.BufferIndex = bufferIndex;
                    descriptor.BufferBindingIndex = bindingIndex;

                    Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Created {name} and binding. Size {descriptor.Data.Count()}");
                }

                descriptor.ClearInstancingInputs();

                if (descriptor.InstancingDescriptor != null)
                {
                    var bufferIndex = instancingBufferDescriptors[descriptor.InstancingDescriptor.BufferDescriptionIndex].BufferIndex;
                    descriptor.AddInstancingInputs(bufferIndex);
                }

                //Updates the allocated buffer size
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                progress?.Report(new LoadResourceProgress { Id = id, Progress = ++current / total });
            }

            return current;
        }
        /// <summary>
        /// Reallocates the index data
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="current">Current progress value</param>
        /// <param name="total">Total progress value</param>
        /// <param name="dirtyList">Dirty buffer list</param>
        private async Task<float> ReallocateIndexData(string id, IProgress<LoadResourceProgress> progress, float current, float total, IEnumerable<BufferManagerIndices> dirtyList)
        {
            foreach (var descriptor in dirtyList)
            {
                if (descriptor.Allocated)
                {
                    //Recreate the buffer
                    string name = $"IndexBuffer.{descriptor.BufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = await CreateIndexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                    //Reserve current buffer
                    var oldBuffer = indexBuffers[descriptor.BufferIndex];
                    //Replace buffer
                    indexBuffers[descriptor.BufferIndex] = buffer;
                    //Dispose buffer
                    oldBuffer?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Reallocated {name}. Size {descriptor.Data.Count()}");
                }
                else
                {
                    int bufferIndex = indexBuffers.Count;

                    //Recreate the buffer
                    string name = $"IndexBuffer.{bufferIndex}.{(descriptor.Dynamic ? "dynamic" : "static")}";
                    var buffer = await CreateIndexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                    indexBuffers.Add(buffer);

                    descriptor.BufferIndex = bufferIndex;

                    Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Created {name}. Size {descriptor.Data.Count()}");
                }

                //Updates the allocated buffer size
                descriptor.AllocatedSize = descriptor.Data.Count();
                descriptor.Allocated = true;
                descriptor.ReallocationNeeded = false;

                progress?.Report(new LoadResourceProgress { Id = id, Progress = ++current / total });
            }

            return current;
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
            return AddVertexData(
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
            var request = new BufferDescriptorRequestVertices
            {
                Id = id,
                Data = data,
                Dynamic = dynamic,
                InstancingDescriptor = instancingBuffer,
                Action = BufferDescriptorRequestActions.Add,
            };

            Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => AddVertexData {request}.");

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
            var request = new BufferDescriptorRequestInstancing
            {
                Id = id,
                Dynamic = dynamic,
                Instances = instances,
                Action = BufferDescriptorRequestActions.Add,
            };

            Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => AddInstancingData {request}.");

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
            var request = new BufferDescriptorRequestIndices
            {
                Id = id,
                Data = data,
                Dynamic = dynamic,
                Action = BufferDescriptorRequestActions.Add,
            };

            Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => AddIndexData {request}.");

            requestedDescriptors.Add(request);

            return request.Descriptor;
        }

        /// <summary>
        /// Removes vertex data from buffer manager
        /// </summary>
        /// <param name="descriptor">Buffer descriptor</param>
        public void RemoveVertexData(BufferDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return;
            }

            var request = new BufferDescriptorRequestVertices
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
            if (descriptor == null)
            {
                return;
            }

            var request = new BufferDescriptorRequestInstancing
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
            if (descriptor == null)
            {
                return;
            }

            var request = new BufferDescriptorRequestIndices
            {
                Id = descriptor.Id,
                Descriptor = descriptor,
                Action = BufferDescriptorRequestActions.Remove,
            };

            requestedDescriptors.Add(request);
        }

        /// <summary>
        /// Adds a new vertex buffer description into the buffer manager
        /// </summary>
        /// <param name="description">Vertex buffer description</param>
        /// <returns>Returns the internal description index</returns>
        public int AddVertexBufferDescription(BufferManagerVertices description)
        {
            int index = vertexBufferDescriptors.Count;

            vertexBufferDescriptors.Add(description);

            return index;
        }
        /// <summary>
        /// Finds a vertex buffer description in the buffer manager
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="dynamic">Dynamic</param>
        /// <returns>Returns the internal description index</returns>
        public int FindVertexBufferDescription(VertexTypes vertexType, bool dynamic)
        {
            return vertexBufferDescriptors.FindIndex(k =>
                k.Type == vertexType &&
                k.Dynamic == dynamic);
        }
        /// <summary>
        /// Gets the vertex buffer description in the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the description</returns>
        public BufferManagerVertices GetVertexBufferDescription(int index)
        {
            return vertexBufferDescriptors[index];
        }
        /// <summary>
        /// Adds a new instancing buffer description into the buffer manager
        /// </summary>
        /// <param name="description">Instancing buffer description</param>
        /// <returns>Returns the internal description index</returns>
        public int AddInstancingBufferDescription(BufferManagerInstances description)
        {
            int index = instancingBufferDescriptors.Count;

            instancingBufferDescriptors.Add(description);

            return index;
        }
        /// <summary>
        /// Finds a instancing buffer description in the buffer manager
        /// </summary>
        /// <param name="dynamic">Dynamic</param>
        /// <returns>Returns the internal description index</returns>
        public int FindInstancingBufferDescription(bool dynamic)
        {
            return instancingBufferDescriptors.FindIndex(k => k.Dynamic == dynamic);
        }
        /// <summary>
        /// Gets the instancing buffer description in the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the description</returns>
        public BufferManagerInstances GetInstancingBufferDescription(int index)
        {
            return instancingBufferDescriptors[index];
        }
        /// <summary>
        /// Adds a new index buffer description into the buffer manager
        /// </summary>
        /// <param name="description">Index buffer description</param>
        /// <returns>Returns the internal description index</returns>
        public int AddIndexBufferDescription(BufferManagerIndices description)
        {
            int index = indexBufferDescriptors.Count;

            indexBufferDescriptors.Add(description);

            return index;
        }
        /// <summary>
        /// Finds a index buffer description in the buffer manager
        /// </summary>
        /// <param name="dynamic">Dynamic</param>
        /// <returns>Returns the internal description index</returns>
        public int FindIndexBufferDescription(bool dynamic)
        {
            return indexBufferDescriptors.FindIndex(k => k.Dynamic == dynamic);
        }
        /// <summary>
        /// Gets the index buffer description in the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the description</returns>
        public BufferManagerIndices GetIndexBufferDescription(int index)
        {
            return indexBufferDescriptors[index];
        }

        /// <summary>
        /// Sets vertex buffers to device context
        /// </summary>
        /// <param name="dc">Device context</param>
        public bool SetVertexBuffers(EngineDeviceContext dc)
        {
            if (!Initilialized)
            {
                Logger.WriteWarning(this, "Attempt to set vertex buffers to Input Assembler with no initialized manager");
                return false;
            }

            if (vertexBufferDescriptors.Exists(d => d.Dirty))
            {
                Logger.WriteWarning(this, "Attempt to set vertex buffers to Input Assembler with dirty descriptors");
                return false;
            }

            dc.IASetVertexBuffers(0, vertexBufferBindings.ToArray());

            return true;
        }
        /// <summary>
        /// Sets index buffers to device context
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        public bool SetIndexBuffer(EngineDeviceContext dc, BufferDescriptor descriptor)
        {
            if (descriptor == null)
            {
                dc.IASetIndexBuffer(null, Format.R32_UInt, 0);
                return true;
            }

            if (!descriptor.Ready)
            {
                return false;
            }

            if (!Initilialized)
            {
                Logger.WriteWarning(this, "Attempt to set index buffers to Input Assembler with no initialized manager");
                return false;
            }

            var indexBufferDescriptor = indexBufferDescriptors[descriptor.BufferDescriptionIndex];
            if (indexBufferDescriptor.Dirty)
            {
                Logger.WriteWarning(this, $"Attempt to set index buffer in buffer description {descriptor.BufferDescriptionIndex} to Input Assembler with no allocated buffer");
                return false;
            }

            dc.IASetIndexBuffer(indexBuffers[descriptor.BufferDescriptionIndex], Format.R32_UInt, 0);

            return true;
        }
        /// <summary>
        /// Sets input layout to device context
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="topology">Topology</param>
        /// <param name="instanced">Use instancig data</param>
        public bool SetInputAssembler(EngineDeviceContext dc, IEngineVertexShader vertexShader, BufferDescriptor descriptor, Topology topology, bool instanced)
        {
            if (descriptor == null)
            {
                return true;
            }

            if (!descriptor.Ready)
            {
                return false;
            }

            if (!Initilialized)
            {
                Logger.WriteWarning(this, "Attempt to set technique to Input Assembler with no initialized manager");
                return false;
            }

            var vertexBufferDescriptor = vertexBufferDescriptors[descriptor.BufferDescriptionIndex];
            if (vertexBufferDescriptor.Dirty)
            {
                Logger.WriteWarning(this, $"Attempt to set technique in buffer description {descriptor.BufferDescriptionIndex} to Input Assembler with no allocated buffer");
                return false;
            }

            var key = new InputAssemblerKey
            {
                Shader = vertexShader,
                Vertices = vertexBufferDescriptor,
            };

            if (!vertexShadersInputLayouts.ContainsKey(key))
            {
                // The vertex shader defines the input vertex data type
                var signature = vertexShader.GetShaderBytecode();
                var inputLayout = instanced ?
                    vertexBufferDescriptor.Input.ToArray() :
                    vertexBufferDescriptor.Input.Where(i => i.Classification == InputClassification.PerVertexData).ToArray();

                vertexShadersInputLayouts.Add(
                    key,
                    game.Graphics.CreateInputLayout(descriptor.Id, signature, inputLayout));
            }

            dc.IAInputLayout = vertexShadersInputLayouts[key];
            dc.IAPrimitiveTopology = topology;
            return true;
        }

        /// <summary>
        /// Writes vertex data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Data to write</param>
        public bool WriteVertexBuffer<T>(EngineDeviceContext dc, BufferDescriptor descriptor, IEnumerable<T> data)
            where T : struct
        {
            if (descriptor?.Ready != true)
            {
                return false;
            }

            if (!Initilialized)
            {
                Logger.WriteWarning(this, $"Attempt to write vertex data in buffer description {descriptor.BufferDescriptionIndex} with no initialized manager");
                return false;
            }

            var vertexBufferDescriptor = vertexBufferDescriptors[descriptor.BufferDescriptionIndex];
            if (vertexBufferDescriptor.Dirty)
            {
                Logger.WriteWarning(this, $"Attempt to write vertex data in buffer description {descriptor.BufferDescriptionIndex} with no allocated buffer");
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            var buffer = vertexBuffers[vertexBufferDescriptor.BufferIndex];

            return dc.WriteNoOverwriteBuffer(buffer, descriptor.BufferOffset, data);
        }
        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Instancig data</param>
        public bool WriteInstancingData<T>(EngineDeviceContext dc, BufferDescriptor descriptor, IEnumerable<T> data)
            where T : struct
        {
            if (descriptor?.Ready != true)
            {
                return false;
            }

            if (!Initilialized)
            {
                Logger.WriteWarning(this, "Attempt to write instancing data with no initialized manager");
                return false;
            }

            var instancingBufferDescriptor = instancingBufferDescriptors[descriptor.BufferDescriptionIndex];
            if (instancingBufferDescriptor.Dirty)
            {
                Logger.WriteWarning(this, $"Attempt to write instancing data in buffer description {descriptor.BufferDescriptionIndex} with no allocated buffer");
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            var instancingBuffer = vertexBuffers[instancingBufferDescriptor.BufferIndex];

            return dc.WriteDiscardBuffer(instancingBuffer, descriptor.BufferOffset, data);
        }
        /// <summary>
        /// Writes imdex data into buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Data to write</param>
        public bool WriteIndexBuffer(EngineDeviceContext dc, BufferDescriptor descriptor, IEnumerable<uint> data)
        {
            if (descriptor?.Ready != true)
            {
                return false;
            }

            if (!Initilialized)
            {
                Logger.WriteWarning(this, $"Attempt to write index data in buffer description {descriptor.BufferDescriptionIndex} with no initialized manager");
                return false;
            }

            var indexBufferDescriptor = indexBufferDescriptors[descriptor.BufferDescriptionIndex];
            if (indexBufferDescriptor.Dirty)
            {
                Logger.WriteWarning(this, $"Attempt to write index data in buffer description {descriptor.BufferDescriptionIndex} with no allocated buffer");
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            var buffer = indexBuffers[indexBufferDescriptor.BufferIndex];

            return dc.WriteNoOverwriteBuffer(buffer, descriptor.BufferOffset, data);
        }

        /// <summary>
        /// Create buffers manually
        /// </summary>
        public void CreateBuffers()
        {
            Task.Run(async () => await CreateBuffersAsync(string.Empty, null));
        }
    }
}
