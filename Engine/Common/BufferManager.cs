using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Common
{
    using SharpDX.DXGI;

    /// <summary>
    /// Buffer manager
    /// </summary>
    public class BufferManager : IDisposable
    {
        private const string NoIdString = "no-id";
        private const string DynamicString = "dynamic";
        private const string StaticString = "static";

        /// <summary>
        /// Imput asembler key
        /// </summary>
        struct InputAssemblerKey
        {
            /// <summary>
            /// Shader
            /// </summary>
            public IEngineShader Shader;
            /// <summary>
            /// Vertices
            /// </summary>
            public BufferManagerVertices Vertices;
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
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="instancingData">Instancing data</param>
        /// <returns>Returns the new buffer</returns>
        private static async Task<EngineBuffer> CreateInstancingBuffer<T>(Graphics graphics, string name, bool dynamic, IEnumerable<T> instancingData)
            where T : struct, IInstacingData
        {
            if (instancingData?.Any() != true)
            {
                return null;
            }

            return await Task.Run(() => graphics.CreateVertexBuffer(name, instancingData, dynamic));
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
        private readonly List<EngineBuffer> vertexBuffers = [];
        /// <summary>
        /// Vertex buffer bindings
        /// </summary>
        private readonly List<EngineVertexBufferBinding> vertexBufferBindings = [];
        /// <summary>
        /// Index buffer
        /// </summary>
        private readonly List<EngineBuffer> indexBuffers = [];
        /// <summary>
        /// Vertex buffer descriptors
        /// </summary>
        private readonly List<BufferManagerVertices> vertexBufferDescriptors = [];
        /// <summary>
        /// Instancing buffer descriptors
        /// </summary>
        private readonly List<BufferManagerInstances<VertexInstancingData>> instancingBufferDescriptors = [];
        /// <summary>
        /// Index buffer descriptors
        /// </summary>
        private readonly List<BufferManagerIndices> indexBufferDescriptors = [];
        /// <summary>
        /// Input layouts by vertex shaders
        /// </summary>
        private readonly ConcurrentDictionary<InputAssemblerKey, EngineInputLayout> vertexShadersInputLayouts = [];
        /// <summary>
        /// Allocating buffers flag
        /// </summary>
        private bool allocating = false;
        /// <summary>
        /// Descriptor request list
        /// </summary>
        private ConcurrentBag<IBufferDescriptorRequest> requestedDescriptors = [];

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
        /// <inheritdoc/>
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
                    Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Creating reserved buffer descriptors");

                    await CreateReservedBuffers();

                    Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Reserved buffer descriptors created");

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

                Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Processing descriptor requests: {requestTotal}");

                float requestCurrent = await DoProcessRequest(id, progress, 0, requestTotal, toAssign);

                Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Descriptor requests processed: {requestCurrent}");

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

                Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Reallocating {total - requestTotal} buffers: Vtx[{vertexList.Length}], Idx[{indexList.Length}], Ins[{instancingList.Length}]");

                float current = await ReallocateInstances(id, progress, requestCurrent, total, instancingList);

                current = await ReallocateVertexData(id, progress, current, total, vertexList);

                current = await ReallocateIndexData(id, progress, current, total, indexList);

                Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Buffers reallocated: {current - requestCurrent}");
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Loading Group {id ?? NoIdString} => Error creating buffers: {ex.Message}", ex);

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

                    string name = $"Reserved buffer.{bufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";

                    //Empty buffer
                    vertexBuffers.Add(null);
                    vertexBufferBindings.Add(new EngineVertexBufferBinding());

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
                Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Processing {request}");

                await request.ProcessAsync(this);

                Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Processed {request}");

                progress?.Report(new LoadResourceProgress { Id = id, Progress = ++current / total });
            }

            //Replaces the bag
            requestedDescriptors = new ConcurrentBag<IBufferDescriptorRequest>(requestedDescriptors.Where(r => r?.Processed != ProcessedStages.Processed));

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
                    string name = $"IndexBuffer.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = await CreateIndexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                    //Reserve current buffer
                    var oldBuffer = indexBuffers[descriptor.BufferIndex];
                    //Replace buffer
                    indexBuffers[descriptor.BufferIndex] = buffer;
                    //Dispose buffer
                    oldBuffer?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Reallocated {name}. Size {descriptor.Data.Count()}");
                }
                else
                {
                    int bufferIndex = indexBuffers.Count;

                    //Recreate the buffer
                    string name = $"IndexBuffer.{bufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = await CreateIndexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                    indexBuffers.Add(buffer);

                    descriptor.BufferIndex = bufferIndex;

                    Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Created {name}. Size {descriptor.Data.Count()}");
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
                    string name = $"VertexBuffer.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = await CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);
                    var binding = new EngineVertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    vertexBuffers[descriptor.BufferIndex] = buffer;
                    vertexBufferBindings[descriptor.BufferBindingIndex] = binding;

                    //Dispose old buffer
                    oldBuffer?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Reallocated {name} and binding. Size {descriptor.Data.Count()}");
                }
                else
                {
                    int bufferIndex = vertexBuffers.Count;
                    int bindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = $"VertexBuffer.{bufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = await CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);
                    var binding = new EngineVertexBufferBinding(buffer, descriptor.GetStride(), 0);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(binding);

                    descriptor.AddInputs(bufferIndex);

                    descriptor.BufferIndex = bufferIndex;
                    descriptor.BufferBindingIndex = bindingIndex;

                    Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Created {name} and binding. Size {descriptor.Data.Count()}");
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
        /// Reallocates the instance data
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="current">Current progress value</param>
        /// <param name="total">Total progress value</param>
        /// <param name="dirtyList">Dirty buffer list</param>
        private async Task<float> ReallocateInstances(string id, IProgress<LoadResourceProgress> progress, float current, float total, IEnumerable<BufferManagerInstances<VertexInstancingData>> dirtyList)
        {
            foreach (var descriptor in dirtyList)
            {
                if (descriptor.Allocated)
                {
                    //Reserve current buffer
                    var oldBuffer = vertexBuffers[descriptor.BufferIndex];

                    //Recreate the buffer and binding
                    string name = $"InstancingBuffer.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    VertexInstancingData[] data = new VertexInstancingData[descriptor.Instances];
                    var buffer = await CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);
                    var binding = new EngineVertexBufferBinding(buffer, data[0].GetStride(), 0);

                    vertexBuffers[descriptor.BufferIndex] = buffer;
                    vertexBufferBindings[descriptor.BufferBindingIndex] = binding;

                    //Dispose old buffer
                    oldBuffer?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Reallocated {name}. Size {descriptor.Instances}");
                }
                else
                {
                    int bufferIndex = vertexBuffers.Count;
                    int bindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = $"InstancingBuffer.{bufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    VertexInstancingData[] data = new VertexInstancingData[descriptor.Instances];
                    var buffer = await CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);
                    var binding = new EngineVertexBufferBinding(buffer, data[0].GetStride(), 0);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(binding);

                    descriptor.BufferIndex = bufferIndex;
                    descriptor.BufferBindingIndex = bindingIndex;

                    Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => Created {name} and binding. Size {descriptor.Instances}");
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

            Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => AddIndexData {request}.");

            requestedDescriptors.Add(request);

            return request.Descriptor;
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

            Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => AddVertexData {request}.");

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

            Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => AddInstancingData {request}.");

            requestedDescriptors.Add(request);

            return request.Descriptor;
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
        public int AddInstancingBufferDescription(BufferManagerInstances<VertexInstancingData> description)
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
        public BufferManagerInstances<VertexInstancingData> GetInstancingBufferDescription(int index)
        {
            return instancingBufferDescriptors[index];
        }

        /// <summary>
        /// Sets index buffers to device context
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        public bool SetIndexBuffer(IEngineDeviceContext dc, BufferDescriptor descriptor)
        {
            if (dc == null)
            {
                return false;
            }

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
        /// Sets vertex buffers to device context
        /// </summary>
        /// <param name="dc">Device context</param>
        public bool SetVertexBuffers(IEngineDeviceContext dc)
        {
            if (dc == null)
            {
                return false;
            }

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

            dc.IASetVertexBuffers(0, [.. vertexBufferBindings]);

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
        public bool SetInputAssembler(IEngineDeviceContext dc, IEngineShader vertexShader, BufferDescriptor descriptor, Topology topology, bool instanced)
        {
            if (dc == null)
            {
                return false;
            }

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

            EngineInputLayout layout;
            if (!vertexShadersInputLayouts.ContainsKey(key))
            {
                // The vertex shader defines the input vertex data type
                var signature = vertexShader.GetShaderBytecode();
                var inputLayout = game.Graphics.CreateInputLayout(descriptor.Id, signature, vertexBufferDescriptor, instanced);

                layout = vertexShadersInputLayouts.AddOrUpdate(key, inputLayout, (k, v) => v);
            }
            else if (!vertexShadersInputLayouts.TryGetValue(key, out layout))
            {
                return false;
            }

            dc.IAInputLayout = layout;
            dc.IAPrimitiveTopology = topology;
            return true;
        }

        /// <summary>
        /// Writes index data into buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Data to write</param>
        public bool WriteIndexBuffer(IEngineDeviceContext dc, BufferDescriptor descriptor, IEnumerable<uint> data)
        {
            return WriteBuffer(
                dc, descriptor, data,
                indexBufferDescriptors.OfType<IEngineBufferDescriptor>().ToArray(),
                indexBuffers);
        }
        /// <summary>
        /// Writes vertex data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Data to write</param>
        public bool WriteVertexBuffer<T>(IEngineDeviceContext dc, BufferDescriptor descriptor, IEnumerable<T> data)
            where T : struct
        {
            return WriteBuffer(
                dc, descriptor, data,
                vertexBufferDescriptors.OfType<IEngineBufferDescriptor>().ToArray(),
                vertexBuffers);
        }
        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Instancig data</param>
        public bool WriteInstancingData<T>(IEngineDeviceContext dc, BufferDescriptor descriptor, IEnumerable<T> data)
            where T : struct
        {
            return WriteBuffer(
                dc, descriptor, data,
                instancingBufferDescriptors.OfType<IEngineBufferDescriptor>().ToArray(),
                vertexBuffers);
        }
        /// <summary>
        /// Writes data in buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="dc">Device context</param>
        /// <param name="descriptor">Buffer descriptor</param>
        /// <param name="data">Data to write</param>
        /// <param name="bufferDescriptors">Buffer descriptors</param>
        /// <param name="buffers">Buffer list</param>
        private bool WriteBuffer<T>(IEngineDeviceContext dc, BufferDescriptor descriptor, IEnumerable<T> data, IEnumerable<IEngineBufferDescriptor> bufferDescriptors, IEnumerable<EngineBuffer> buffers)
            where T : struct
        {
            if (dc == null)
            {
                return false;
            }

            if (descriptor?.Ready != true)
            {
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            if (!Initilialized)
            {
                Logger.WriteWarning(this, $"Attempt to write data in buffer description {descriptor.BufferDescriptionIndex} with no initialized manager");
                return false;
            }

            var bufferDescriptor = bufferDescriptors.ElementAt(descriptor.BufferDescriptionIndex);
            if (bufferDescriptor.Dirty)
            {
                Logger.WriteWarning(this, $"Attempt to write data in buffer description {descriptor.BufferDescriptionIndex} with no allocated buffer");
                return false;
            }

            var buffer = buffers.ElementAt(bufferDescriptor.BufferIndex);

            return dc.WriteDiscardBuffer(buffer, descriptor.BufferOffset, data);
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
