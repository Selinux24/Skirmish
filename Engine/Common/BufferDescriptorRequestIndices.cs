using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Common
{
    /// <summary>
    /// Index buffer descriptor request
    /// </summary>
    class BufferDescriptorRequestIndices : IBufferDescriptorRequest
    {
        /// <inheritdoc/>
        public string Id { get; set; }
        /// <inheritdoc/>
        public bool Dynamic { get; set; }
        /// <inheritdoc/>
        public BufferDescriptorRequestActions Action { get; set; } = BufferDescriptorRequestActions.None;
        /// <inheritdoc/>
        public ProcessedStages Processed { get; set; } = ProcessedStages.Requested;
        /// <summary>
        /// Data to assign
        /// </summary>
        public IEnumerable<uint> Data { get; set; }
        /// <summary>
        /// Descriptor
        /// </summary>
        public BufferDescriptor Descriptor { get; set; } = new BufferDescriptor();

        /// <inheritdoc/>
        public void Process(BufferManager bufferManager)
        {
            Processed = ProcessedStages.InProcess;

            if (Action == BufferDescriptorRequestActions.Add)
            {
                Add(bufferManager);
            }
            else if (Action == BufferDescriptorRequestActions.Remove)
            {
                Remove(bufferManager);
            }

            Processed = ProcessedStages.Processed;
        }
        /// <inheritdoc/>
        public async Task ProcessAsync(BufferManager bufferManager)
        {
            await Task.Run(() => Process(bufferManager));
        }
        /// <summary>
        /// Assign the descriptor to the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void Add(BufferManager bufferManager)
        {
            if (Data?.Any() != true)
            {
                return;
            }

            BufferManagerIndices descriptor;

            Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {typeof(uint)} [{Id}]");

            int slot = bufferManager.FindIndexBufferDescription(Dynamic);
            if (slot < 0)
            {
                descriptor = new BufferManagerIndices(Dynamic);
                slot = bufferManager.AddIndexBufferDescription(descriptor);
            }
            else
            {
                descriptor = bufferManager.GetIndexBufferDescription(slot);
                descriptor.ReallocationNeeded = true;
            }

            descriptor.AddDescriptor(Descriptor, Id, slot, Data);
        }
        /// <summary>
        /// Remove the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void Remove(BufferManager bufferManager)
        {
            if (Descriptor?.Ready == true)
            {
                var descriptor = bufferManager.GetIndexBufferDescription(Descriptor.BufferDescriptionIndex);

                Logger.WriteTrace(this, $"Remove BufferDescriptor {(descriptor.Dynamic ? "dynamic" : "static")} {typeof(uint)} [{Descriptor.Id}]");

                descriptor.RemoveDescriptor(Descriptor);
                descriptor.ReallocationNeeded = true;
            }
        }
    }
}
