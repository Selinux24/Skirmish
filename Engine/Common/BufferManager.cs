using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Buffer manager
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="game">Game</param>
    /// <param name="reservedSlots">Reserved slots</param>
    public partial class BufferManager(Game game, int reservedSlots = 1) : IDisposable
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
            public IEngineVertexBufferDescriptor Vertices;

            /// <inheritdoc/>
            public override readonly string ToString()
            {
                return $"{Shader} - {Vertices}";
            }
        }

        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game = game;
        /// <summary>
        /// Reserved slots
        /// </summary>
        private readonly int reservedSlots = reservedSlots;

        /// <summary>
        /// Index buffer descriptors
        /// </summary>
        private readonly List<IEngineIndexBufferDescriptor> indexBufferDescriptors = [];
        /// <summary>
        /// Index buffer
        /// </summary>
        private readonly List<EngineBuffer> indexBuffers = [];

        /// <summary>
        /// Vertex buffer descriptors
        /// </summary>
        private readonly List<IEngineVertexBufferDescriptor> vertexBufferDescriptors = [];
        /// <summary>
        /// Vertex buffers
        /// </summary>
        private readonly List<EngineBuffer> vertexBuffers = [];
        /// <summary>
        /// Vertex buffer bindings
        /// </summary>
        private readonly List<EngineVertexBufferBinding> vertexBufferBindings = [];

        /// <summary>
        /// Instancing buffer descriptors
        /// </summary>
        private readonly List<IEngineInstancingBufferDescriptor> instancingBufferDescriptors = [];

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
        public void CreateBuffers(string id)
        {
            if (allocating)
            {
                return;
            }

            allocating = true;

            string grId = id ?? NoIdString;

            try
            {
                if (!Initilialized)
                {
                    CreateReservedBuffers(grId);

                    Initilialized = true;
                }

                if (!HasPendingRequests)
                {
                    return;
                }

                DoProcessRequest(grId);

                ReallocateInstances(grId);
                ReallocateVertexData(grId);
                ReallocateIndexData(grId);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Loading Group {grId} => Error creating buffers: {ex.Message}", ex);

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
        /// <param name="grId">Load group id</param>
        private void CreateReservedBuffers(string grId)
        {
            if (reservedSlots == 0)
            {
                return;
            }

            Logger.WriteTrace(this, $"Loading Group {grId} => Creating {reservedSlots} reserved buffer descriptors");

            for (int i = 0; i < reservedSlots; i++)
            {
                BufferManagerVertices<ReservedDataFormat> descriptor = new(true)
                {
                    BufferIndex = vertexBuffers.Count,
                    BufferBindingIndex = vertexBufferBindings.Count
                };

                //Empty buffer
                vertexBuffers.Add(null);
                vertexBufferBindings.Add(new());

                descriptor.ClearInputs();
                descriptor.Allocate();

                string name = GetVertexBufferName(descriptor, true);
                Logger.WriteTrace(this, $"Created {name} and binding. Size {descriptor.SizeInBytes()}");

                vertexBufferDescriptors.Add(descriptor);
            }

            Logger.WriteTrace(this, $"Loading Group {grId} => Reserved buffer descriptors created");
        }
        /// <summary>
        /// Do descriptor request processing
        /// </summary>
        /// <param name="grId">Load group id</param>
        private void DoProcessRequest(string grId)
        {
            //Copy request collection
            var toAssign = requestedDescriptors
                .Where(r => r?.Processed == ProcessedStages.Requested)
                .ToArray();

            if (toAssign.Length == 0)
            {
                return;
            }

            Logger.WriteTrace(this, $"Loading Group {grId} => Processing descriptor requests: {toAssign.Length}");

            foreach (var request in toAssign)
            {
                Logger.WriteTrace(this, $"Loading Group {grId} => Processing {request}");

                request.Process(this);

                Logger.WriteTrace(this, $"Loading Group {grId} => Processed {request}");
            }

            //Replaces the bag
            requestedDescriptors = new(requestedDescriptors.Where(r => r?.Processed != ProcessedStages.Processed));
        }

        /// <summary>
        /// Reallocates the instance data
        /// </summary>
        /// <param name="grId">Load group id</param>
        private void ReallocateInstances(string grId)
        {
            var dirtyList = instancingBufferDescriptors
                .Where(v => v.Dirty)
                .ToArray();

            if (dirtyList.Length == 0)
            {
                return;
            }

            Logger.WriteTrace(this, $"Loading Group {grId} => Ins[{dirtyList.Length}]");

            foreach (var descriptor in dirtyList)
            {
                if (descriptor.Allocated)
                {
                    //Reserve current buffer
                    var oldBufferToDispose = vertexBuffers[descriptor.BufferIndex];

                    //Recreate the buffer and binding
                    string name = GetInstancingBufferName(descriptor);
                    var buffer = descriptor.CreateBuffer(game.Graphics, name);

                    vertexBuffers[descriptor.BufferIndex] = buffer;
                    vertexBufferBindings[descriptor.BufferBindingIndex] = new(buffer, descriptor.GetStride(), 0);

                    oldBufferToDispose?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {grId} => Reallocated {name}. Size {descriptor.Instances}");
                }
                else
                {
                    descriptor.BufferIndex = vertexBuffers.Count;
                    descriptor.BufferBindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = GetInstancingBufferName(descriptor);
                    var buffer = descriptor.CreateBuffer(game.Graphics, name);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(new(buffer, descriptor.GetStride(), 0));

                    Logger.WriteTrace(this, $"Loading Group {grId} => Created {name} and binding. Size {descriptor.Instances}");
                }

                //Updates the allocated buffer size
                descriptor.Allocate();
            }
        }
        /// <summary>
        /// Reallocates the vertex data
        /// </summary>
        /// <param name="grId">Load group id</param>
        private void ReallocateVertexData(string grId)
        {
            var dirtyList = vertexBufferDescriptors
                .Where(v => v.Dirty)
                .ToArray();

            if (dirtyList.Length == 0)
            {
                return;
            }

            Logger.WriteTrace(this, $"Loading Group {grId} => Vtx[{dirtyList.Length}]");

            foreach (var descriptor in dirtyList)
            {
                if (descriptor.Allocated)
                {
                    //Reserve current buffer
                    var oldBufferToDispose = vertexBuffers[descriptor.BufferIndex];

                    //Recreate the buffer and binding
                    string name = GetVertexBufferName(descriptor, false);
                    var buffer = descriptor.CreateBuffer(game.Graphics, name);

                    vertexBuffers[descriptor.BufferIndex] = buffer;
                    vertexBufferBindings[descriptor.BufferBindingIndex] = new(buffer, descriptor.GetStride(), 0);

                    Logger.WriteTrace(this, $"Loading Group {grId} => Reallocated {name} and binding. Size {descriptor.SizeInBytes()}");

                    oldBufferToDispose?.Dispose();
                }
                else
                {
                    descriptor.BufferIndex = vertexBuffers.Count;
                    descriptor.BufferBindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = GetVertexBufferName(descriptor, false);
                    var buffer = descriptor.CreateBuffer(game.Graphics, name);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(new(buffer, descriptor.GetStride(), 0));

                    descriptor.AddInputs();

                    Logger.WriteTrace(this, $"Loading Group {grId} => Created {name} and binding. Size {descriptor.SizeInBytes()}");
                }

                if (descriptor.InstancingDescriptor != null)
                {
                    var instancingBuffer = instancingBufferDescriptors[descriptor.InstancingDescriptor.BufferDescriptionIndex];

                    descriptor.SetInstancingInputs(instancingBuffer.GetInput());
                }

                //Updates the allocated buffer size
                descriptor.Allocate();
            }
        }
        /// <summary>
        /// Reallocates the index data
        /// </summary>
        /// <param name="grId">Load group id</param>
        private void ReallocateIndexData(string grId)
        {
            var dirtyList = indexBufferDescriptors
                .Where(v => v.Dirty)
                .ToArray();

            if (dirtyList.Length == 0)
            {
                return;
            }

            Logger.WriteTrace(this, $"Loading Group {grId} => Idx[{dirtyList.Length}]");

            foreach (var descriptor in dirtyList)
            {
                if (descriptor.Allocated)
                {
                    //Reserve current buffer
                    var oldBufferToDispose = indexBuffers[descriptor.BufferIndex];

                    //Recreate the buffer
                    string name = GetIndexBufferName(descriptor);
                    var buffer = descriptor.CreateBuffer(game.Graphics, name);

                    indexBuffers[descriptor.BufferIndex] = buffer;

                    oldBufferToDispose?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {grId} => Reallocated {name}. Size {descriptor.SizeInBytes()}");
                }
                else
                {
                    descriptor.BufferIndex = indexBuffers.Count;

                    //Recreate the buffer
                    string name = GetIndexBufferName(descriptor);
                    var buffer = descriptor.CreateBuffer(game.Graphics, name);

                    indexBuffers.Add(buffer);

                    Logger.WriteTrace(this, $"Loading Group {grId} => Created {name}. Size {descriptor.SizeInBytes()}");
                }

                //Updates the allocated buffer size
                descriptor.Allocate();
            }
        }

        /// <summary>
        /// Gets a buffer name for the specified descriptor
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        private static string GetInstancingBufferName(IEngineDescriptor descriptor)
        {
            return $"InstancingBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
        }
        /// <summary>
        /// Gets a buffer name for the specified descriptor
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        /// <param name="reserved">Is reserved</param>
        private static string GetVertexBufferName(IEngineVertexBufferDescriptor descriptor, bool reserved)
        {
            if (reserved)
            {
                return $"Reserved buffer.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
            }
            else
            {
                return $"VertexBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
            }
        }
        /// <summary>
        /// Gets a buffer name for the specified descriptor
        /// </summary>
        /// <param name="descriptor">Descriptor</param>
        private static string GetIndexBufferName(IEngineIndexBufferDescriptor descriptor)
        {
            return $"IndexBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
        }

        /// <summary>
        /// Copies the buffer manager to another instance
        /// </summary>
        public BufferManager Copy()
        {
            BufferManager bm = new(game, reservedSlots)
            {
                allocating = false,
                Initilialized = false,
            };

            var tmpVBufferList = new EngineBuffer[vertexBuffers.Count];
            var tmpVBufferBindingsList = new EngineVertexBufferBinding[vertexBufferBindings.Count];
            var tmpIBufferList = new EngineBuffer[indexBuffers.Count];

            for (int i = 0; i < instancingBufferDescriptors.Count; i++)
            {
                var newDescriptor = instancingBufferDescriptors[i].Copy();

                bm.instancingBufferDescriptors.Add(newDescriptor);

                //Create the buffer and binding
                string name = GetInstancingBufferName(newDescriptor);
                var buffer = newDescriptor.CreateBuffer(game.Graphics, name);

                tmpVBufferList[newDescriptor.BufferIndex] = buffer;
                tmpVBufferBindingsList[newDescriptor.BufferBindingIndex] = new(buffer, newDescriptor.GetStride(), 0);

                newDescriptor.Allocate();
            }

            for (int i = 0; i < vertexBufferDescriptors.Count; i++)
            {
                var newDescriptor = vertexBufferDescriptors[i].Copy();

                bm.vertexBufferDescriptors.Add(newDescriptor);

                //Create the buffer and binding
                string name = GetVertexBufferName(newDescriptor, i < reservedSlots);
                var buffer = newDescriptor.CreateBuffer(game.Graphics, name);

                tmpVBufferList[newDescriptor.BufferIndex] = buffer;
                tmpVBufferBindingsList[newDescriptor.BufferBindingIndex] = new(buffer, newDescriptor.GetStride(), 0);

                newDescriptor.Allocate();
            }

            for (int i = 0; i < indexBufferDescriptors.Count; i++)
            {
                var newDescriptor = (BufferManagerIndices)indexBufferDescriptors[i].Copy();

                bm.indexBufferDescriptors.Add(newDescriptor);

                //Recreate the buffer
                string name = GetIndexBufferName(newDescriptor);
                var buffer = newDescriptor.CreateBuffer(game.Graphics, name);

                tmpIBufferList[newDescriptor.BufferIndex] = buffer;

                newDescriptor.Allocate();
            }

            bm.vertexBuffers.AddRange(tmpVBufferList);
            bm.vertexBufferBindings.AddRange(tmpVBufferBindingsList);
            bm.indexBuffers.AddRange(tmpIBufferList);

            bm.Initilialized = true;

            return bm;
        }

        /// <summary>
        /// Gets the vertex buffer binding array
        /// </summary>
        public EngineVertexBufferBinding[] GetVertexBufferBindings()
        {
            return [.. vertexBufferBindings];
        }

        /// <summary>
        /// Gets whether the buffer manager has dirty descriptors
        /// </summary>
        public bool HasVertexBufferDescriptorsDirty()
        {
            return vertexBufferDescriptors.Exists(d => d.Dirty);
        }
        /// <summary>
        /// Gets the vertex buffer descriptor by index
        /// </summary>
        /// <param name="index">Index</param>
        public IEngineVertexBufferDescriptor GetVertexBufferDescriptor(int index)
        {
            return vertexBufferDescriptors[index];
        }
        /// <summary>
        /// Gets the vertex buffer by index
        /// </summary>
        /// <param name="index">Index</param>
        public EngineBuffer GetVertexBuffer(int index)
        {
            return vertexBuffers[index];
        }
        /// <summary>
        /// Gets the vertex buffer list
        /// </summary>
        public EngineBuffer[] GetVertexBuffers()
        {
            return [.. vertexBuffers];
        }

        /// <summary>
        /// Gets the index buffer descriptor by index
        /// </summary>
        /// <param name="index">Index</param>
        public IEngineIndexBufferDescriptor GetIndexBufferDescriptor(int index)
        {
            return indexBufferDescriptors[index];
        }
        /// <summary>
        /// Gets the index buffer by index
        /// </summary>
        /// <param name="index">Index</param>
        public EngineBuffer GetIndexBuffer(int index)
        {
            return indexBuffers[index];
        }
        /// <summary>
        /// Gets the index buffer list
        /// </summary>
        public EngineBuffer[] GetIndexBuffers()
        {
            return [.. indexBuffers];
        }

        /// <summary>
        /// Gets instancing buffer descriptors
        /// </summary>
        public IEngineInstancingBufferDescriptor[] GetInstancingBufferDescriptors()
        {
            return instancingBufferDescriptors.OfType<IEngineInstancingBufferDescriptor>().ToArray();
        }
        /// <summary>
        /// Gets vertex buffer descriptors
        /// </summary>
        public IEngineVertexBufferDescriptor[] GetVertexBufferDescriptors()
        {
            return vertexBufferDescriptors.OfType<IEngineVertexBufferDescriptor>().ToArray();
        }
        /// <summary>
        /// Gets index buffer descriptors
        /// </summary>
        public IEngineIndexBufferDescriptor[] GetIndexBufferDescriptors()
        {
            return indexBufferDescriptors.OfType<IEngineIndexBufferDescriptor>().ToArray();
        }

        /// <summary>
        /// Gets or creates an input layout
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="vertexBufferDescriptor">Buffer description</param>
        /// <param name="instanced">Instanced flag</param>
        /// <param name="layout">Returns the input layout</param>
        public bool GetOrCreateInputLayout(string name, IEngineShader vertexShader, IEngineVertexBufferDescriptor vertexBufferDescriptor, bool instanced, out EngineInputLayout layout)
        {
            var key = new InputAssemblerKey
            {
                Shader = vertexShader,
                Vertices = vertexBufferDescriptor,
            };

            if (!vertexShadersInputLayouts.ContainsKey(key))
            {
                // The vertex shader defines the input vertex data type
                var signature = vertexShader.GetShaderBytecode();
                var inputElements = vertexBufferDescriptor.GetInput(instanced);
                var inputLayout = game.Graphics.CreateInputLayout(name, signature, inputElements);

                layout = vertexShadersInputLayouts.AddOrUpdate(key, inputLayout, (k, v) => v);
            }
            else if (!vertexShadersInputLayouts.TryGetValue(key, out layout))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds instances to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="instances">Number of instances</param>
        public BufferDescriptor AddInstancingData<T>(string id, bool dynamic, int instances)
            where T : struct, IInstacingData
        {
            var request = new BufferDescriptorAddInstancingRequest<T>(id, dynamic, instances);

            Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => AddInstancingData {request}.");

            requestedDescriptors.Add(request);

            return request.GetDescriptor();
        }
        /// <summary>
        /// Adds vertices to manager
        /// </summary>
        /// <typeparam name="T">Type of vertex</typeparam>
        /// <param name="id">Id</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="data">Vertex list</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        public BufferDescriptor AddVertexData<T>(string id, bool dynamic, IEnumerable<T> data, BufferDescriptor instancingBuffer = null)
            where T : struct, IVertexData
        {
            var request = new BufferDescriptorAddVerticesRequest<T>(id, dynamic, data, instancingBuffer);

            Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => AddVertexData {request}.");

            requestedDescriptors.Add(request);

            return request.GetDescriptor();
        }
        /// <summary>
        /// Adds indices to manager
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="dynamic">Add to dynamic buffers</param>
        /// <param name="data">Index list</param>
        public BufferDescriptor AddIndexData(string id, bool dynamic, IEnumerable<uint> data)
        {
            var request = new BufferDescriptorAddIndicesRequest(id, dynamic, data);

            Logger.WriteTrace(this, $"Loading Group {id ?? NoIdString} => AddIndexData {request}.");

            requestedDescriptors.Add(request);

            return request.GetDescriptor();
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

            var request = new BufferDescriptorRemoveInstancingRequest(descriptor);

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

            var request = new BufferDescriptorRemoveVerticesRequest(descriptor);

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

            var request = new BufferDescriptorRemoveIndicesRequest(descriptor);

            requestedDescriptors.Add(request);
        }

        /// <summary>
        /// Adds a new instancing buffer description into the buffer manager
        /// </summary>
        /// <param name="description">Instancing buffer description</param>
        /// <returns>Returns the internal description index</returns>
        public int AddInstancingBufferDescription<T>(BufferManagerInstances<T> description) where T : struct, IInstacingData
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
        public IEngineInstancingBufferDescriptor GetInstancingBufferDescription(int index)
        {
            return instancingBufferDescriptors[index];
        }

        /// <summary>
        /// Adds a new vertex buffer description into the buffer manager
        /// </summary>
        /// <param name="description">Vertex buffer description</param>
        /// <returns>Returns the internal description index</returns>
        public int AddVertexBufferDescription(IEngineVertexBufferDescriptor description)
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
        public int FindVertexBufferDescription<T>(bool dynamic) where T : struct, IVertexData
        {
            return vertexBufferDescriptors.FindIndex(k =>
                k.OfType<T>() &&
                k.Dynamic == dynamic);
        }
        /// <summary>
        /// Gets the vertex buffer description in the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the description</returns>
        public IEngineVertexBufferDescriptor GetVertexBufferDescription(int index)
        {
            return vertexBufferDescriptors[index];
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
        public IEngineIndexBufferDescriptor GetIndexBufferDescription(int index)
        {
            return indexBufferDescriptors[index];
        }
    }
}
