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
        public ProcessedStages Processed { get; set; } = ProcessedStages.Requested;

        /// <summary>
        /// Updates the buffer
        /// </summary>
        /// <param name="request">Buffer request</param>
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
        /// <summary>
        /// Updates the buffer descriptor
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
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
