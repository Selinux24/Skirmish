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
            public BufferManagerVertices Vertices;
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
        private readonly List<BufferManagerIndices> indexBufferDescriptors = [];
        /// <summary>
        /// Index buffer
        /// </summary>
        private readonly List<EngineBuffer> indexBuffers = [];

        /// <summary>
        /// Vertex buffer descriptors
        /// </summary>
        private readonly List<BufferManagerVertices> vertexBufferDescriptors = [];
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
        private readonly List<BufferManagerInstances<VertexInstancingData>> instancingBufferDescriptors = [];

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
                vertexBufferDescriptors.Add(new(VertexTypes.Unknown, true));
            }

            for (int i = 0; i < reservedSlots; i++)
            {
                var descriptor = vertexBufferDescriptors[i];

                int bufferIndex = vertexBuffers.Count;
                int bindingIndex = vertexBufferBindings.Count;

                string name = $"Reserved buffer.{bufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";

                //Empty buffer
                vertexBuffers.Add(null);
                vertexBufferBindings.Add(new());

                descriptor.ClearInputs();

                descriptor.BufferIndex = bufferIndex;
                descriptor.BufferBindingIndex = bindingIndex;
                descriptor.Allocate();

                Logger.WriteTrace(this, $"Created {name} and binding. Size {descriptor.Data.Count()}");
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
        /// Creates an index buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="indices">Indices</param>
        /// <returns>Returns new buffer</returns>
        private static EngineBuffer CreateIndexBuffer(Graphics graphics, string name, bool dynamic, IEnumerable<uint> indices)
        {
            if (indices?.Any() != true)
            {
                return null;
            }

            return graphics.CreateIndexBuffer(name, indices, dynamic);
        }
        /// <summary>
        /// Creates a vertex buffer from IVertexData
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="vertices">Vertices</param>
        /// <returns>Returns new buffer</returns>
        private static EngineBuffer CreateVertexBuffer(Graphics graphics, string name, bool dynamic, IEnumerable<IVertexData> vertices)
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
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        /// <param name="instancingData">Instancing data</param>
        /// <returns>Returns the new buffer</returns>
        private static EngineBuffer CreateInstancingBuffer<T>(Graphics graphics, string name, bool dynamic, IEnumerable<T> instancingData)
            where T : struct, IInstacingData
        {
            if (instancingData?.Any() != true)
            {
                return null;
            }

            return graphics.CreateVertexBuffer(name, instancingData, dynamic);
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
                var data = new VertexInstancingData[descriptor.Instances];

                if (descriptor.Allocated)
                {
                    //Reserve current buffer
                    var oldBufferToDispose = vertexBuffers[descriptor.BufferIndex];

                    //Recreate the buffer and binding
                    string name = $"InstancingBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);

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
                    string name = $"InstancingBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = CreateInstancingBuffer(game.Graphics, name, descriptor.Dynamic, data);

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
                    string name = $"VertexBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                    vertexBuffers[descriptor.BufferIndex] = buffer;
                    vertexBufferBindings[descriptor.BufferBindingIndex] = new(buffer, descriptor.GetStride(), 0);

                    Logger.WriteTrace(this, $"Loading Group {grId} => Reallocated {name} and binding. Size {descriptor.Data.Count()}");

                    oldBufferToDispose?.Dispose();
                }
                else
                {
                    descriptor.BufferIndex = vertexBuffers.Count;
                    descriptor.BufferBindingIndex = vertexBufferBindings.Count;

                    //Create the buffer and binding
                    string name = $"VertexBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = CreateVertexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                    vertexBuffers.Add(buffer);
                    vertexBufferBindings.Add(new(buffer, descriptor.GetStride(), 0));

                    descriptor.AddInputs(descriptor.BufferIndex);

                    Logger.WriteTrace(this, $"Loading Group {grId} => Created {name} and binding. Size {descriptor.Data.Count()}");
                }

                if (descriptor.InstancingDescriptor != null)
                {
                    int instancingBufferIndex = instancingBufferDescriptors[descriptor.InstancingDescriptor.BufferDescriptionIndex].BufferIndex;

                    descriptor.SetInstancingInputs(instancingBufferIndex);
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
                    string name = $"IndexBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = CreateIndexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                    indexBuffers[descriptor.BufferIndex] = buffer;

                    oldBufferToDispose?.Dispose();

                    Logger.WriteTrace(this, $"Loading Group {grId} => Reallocated {name}. Size {descriptor.Data.Count()}");
                }
                else
                {
                    descriptor.BufferIndex = indexBuffers.Count;

                    //Recreate the buffer
                    string name = $"IndexBuffer_v{descriptor.Allocations}.{descriptor.BufferIndex}.{(descriptor.Dynamic ? DynamicString : StaticString)}";
                    var buffer = CreateIndexBuffer(game.Graphics, name, descriptor.Dynamic, descriptor.Data);

                    indexBuffers.Add(buffer);

                    Logger.WriteTrace(this, $"Loading Group {grId} => Created {name}. Size {descriptor.Data.Count()}");
                }

                //Updates the allocated buffer size
                descriptor.Allocate();
            }
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
                var newDescriptor = (BufferManagerInstances<VertexInstancingData>)instancingBufferDescriptors[i].Copy();

                bm.instancingBufferDescriptors.Add(newDescriptor);

                //Create the buffer and binding
                var data = new VertexInstancingData[newDescriptor.Instances];
                string name = $"InstancingBuffer_v{newDescriptor.Allocations}.{newDescriptor.BufferIndex}.{(newDescriptor.Dynamic ? DynamicString : StaticString)}";
                var buffer = CreateInstancingBuffer(game.Graphics, name, newDescriptor.Dynamic, data);

                tmpVBufferList[newDescriptor.BufferIndex] = buffer;
                tmpVBufferBindingsList[newDescriptor.BufferBindingIndex] = new(buffer, newDescriptor.GetStride(), 0);

                newDescriptor.Allocate();
            }

            for (int i = 0; i < vertexBufferDescriptors.Count; i++)
            {
                var newDescriptor = (BufferManagerVertices)vertexBufferDescriptors[i].Copy();

                bm.vertexBufferDescriptors.Add(newDescriptor);

                //Create the buffer and binding
                string name;
                if (i < reservedSlots)
                {
                    name = $"Reserved buffer.{newDescriptor.BufferIndex}.{(newDescriptor.Dynamic ? DynamicString : StaticString)}";
                }
                else
                {
                    name = $"VertexBuffer_v{newDescriptor.Allocations}.{newDescriptor.BufferIndex}.{(newDescriptor.Dynamic ? DynamicString : StaticString)}";
                }
                var buffer = CreateVertexBuffer(game.Graphics, name, newDescriptor.Dynamic, newDescriptor.Data);

                tmpVBufferList[newDescriptor.BufferIndex] = buffer;
                tmpVBufferBindingsList[newDescriptor.BufferBindingIndex] = new(buffer, newDescriptor.GetStride(), 0);

                newDescriptor.Allocate();
            }

            for (int i = 0; i < indexBufferDescriptors.Count; i++)
            {
                var newDescriptor = (BufferManagerIndices)indexBufferDescriptors[i].Copy();

                bm.indexBufferDescriptors.Add(newDescriptor);

                //Recreate the buffer
                string name = $"IndexBuffer_v{newDescriptor.Allocations}.{newDescriptor.BufferIndex}.{(newDescriptor.Dynamic ? DynamicString : StaticString)}";
                var buffer = CreateIndexBuffer(game.Graphics, name, newDescriptor.Dynamic, newDescriptor.Data);

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
        public BufferManagerVertices GetVertexBufferDescriptor(int index)
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
        public BufferManagerIndices GetIndexBufferDescriptor(int index)
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
        public IEngineBufferDescriptor[] GetInstancingBufferDescriptors()
        {
            return instancingBufferDescriptors.OfType<IEngineBufferDescriptor>().ToArray();
        }
        /// <summary>
        /// Gets vertex buffer descriptors
        /// </summary>
        public IEngineBufferDescriptor[] GetVertexBufferDescriptors()
        {
            return vertexBufferDescriptors.OfType<IEngineBufferDescriptor>().ToArray();
        }
        /// <summary>
        /// Gets index buffer descriptors
        /// </summary>
        public IEngineBufferDescriptor[] GetIndexBufferDescriptors()
        {
            return indexBufferDescriptors.OfType<IEngineBufferDescriptor>().ToArray();
        }

        /// <summary>
        /// Gets or creates an input layout
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="vertexBufferDescriptor">Buffer description</param>
        /// <param name="instanced">Instanced flag</param>
        /// <param name="layout">Returns the input layout</param>
        public bool GetOrCreateInputLayout(string name, IEngineShader vertexShader, BufferManagerVertices vertexBufferDescriptor, bool instanced, out EngineInputLayout layout)
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
                var inputLayout = game.Graphics.CreateInputLayout(name, signature, vertexBufferDescriptor, instanced);

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
    }
}
