using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Add index buffer descriptor request
    /// </summary>
    class BufferDescriptorAddIndicesRequest(string id, bool dynamic, IEnumerable<uint> data) : IBufferDescriptorRequest
    {
        /// <summary>
        /// New descriptor instance
        /// </summary>
        private readonly BufferDescriptor descriptor = new();
        /// <summary>
        /// Data to assign
        /// </summary>
        private readonly IEnumerable<uint> data = data ?? [];

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

            BufferManagerIndices indicesDesc;

            Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {typeof(uint)} [{Id}]");

            int slot = bufferManager.FindIndexBufferDescription(Dynamic);
            if (slot < 0)
            {
                indicesDesc = new(Dynamic);
                slot = bufferManager.AddIndexBufferDescription(indicesDesc);
            }
            else
            {
                indicesDesc = bufferManager.GetIndexBufferDescription(slot) as BufferManagerIndices;
                indicesDesc.ReallocationNeeded = true;
            }

            indicesDesc.AddDescriptor(descriptor, Id, slot, data);

            Processed = ProcessedStages.Processed;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => {Action} {Processed}; {descriptor}";
        }
    }
}
