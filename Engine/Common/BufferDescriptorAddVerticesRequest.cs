using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Add vertex buffer descriptor request
    /// </summary>
    class BufferDescriptorAddVerticesRequest<T>(string id, bool dynamic, IEnumerable<T> data, BufferDescriptor instancingBuffer = null) : IBufferDescriptorRequest
        where T : struct, IVertexData
    {
        /// <summary>
        /// New descriptor instance
        /// </summary>
        private readonly BufferDescriptor descriptor = new();
        /// <summary>
        /// Data to assign
        /// </summary>
        private readonly IEnumerable<T> data = data ?? [];
        /// <summary>
        /// Instancing buffer descriptor
        /// </summary>
        private readonly BufferDescriptor instancingDescriptor = instancingBuffer;

        /// <inheritdoc/>
        public string Id { get; } = id;
        /// <inheritdoc/>
        public bool Dynamic { get; } = dynamic;
        /// <inheritdoc/>
        public BufferDescriptorRequestActions Action { get; } = BufferDescriptorRequestActions.Add;
        /// <inheritdoc/>
        public ProcessedStages Processed { get; private set; } = ProcessedStages.Requested;

        /// <summary>
        /// Gets the new descriptor
        /// </summary>
        public BufferDescriptor GetDescriptor()
        {
            return descriptor;
        }

        /// <inheritdoc/>
        public void Process(BufferManager bufferManager)
        {
            Processed = ProcessedStages.InProcess;

            if (!data.Any())
            {
                Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} discarded no data [{Id}]");

                Processed = ProcessedStages.Processed;
                return;
            }

            BufferManagerVertices<T> verticesDesc;

            bool instanced = instancingDescriptor != null;

            Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {typeof(T)} Instanced {instanced} [{Id}]");

            int slot = bufferManager.FindVertexBufferDescription<T>(Dynamic);
            if (slot < 0)
            {
                verticesDesc = new BufferManagerVertices<T>(Dynamic);
                slot = bufferManager.AddVertexBufferDescription(verticesDesc);
            }
            else
            {
                verticesDesc = bufferManager.GetVertexBufferDescription(slot) as BufferManagerVertices<T>;
                verticesDesc.ReallocationNeeded = true;
            }

            if (instanced)
            {
                //Additive only
                verticesDesc.InstancingDescriptor = instancingDescriptor;
            }

            verticesDesc.AddDescriptor(descriptor, Id, slot, data);

            Processed = ProcessedStages.Processed;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => {Action} {Processed}; {descriptor}";
        }
    }
}
