using System.Collections.Generic;
using System.Linq;

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
        public bool Processed { get; set; } = false;

        /// <summary>
        /// Updates the buffer
        /// </summary>
        /// <param name="request">Buffer request</param>
        public void Process(BufferManager bufferManager)
        {
            if (this.Action == BufferDescriptorRequestActions.Add)
            {
                Add(bufferManager);
            }
            else if (this.Action == BufferDescriptorRequestActions.Remove)
            {
                Remove(bufferManager);
            }

            this.Processed = true;
        }
        /// <summary>
        /// Assign the descriptor to the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void Add(BufferManager bufferManager)
        {
            if (this.Data?.Any() != true)
            {
                return;
            }

            BufferManagerIndices descriptor;

            Logger.WriteTrace($"Add BufferDescriptor {(this.Dynamic ? "dynamic" : "static")} {typeof(uint)} [{this.Id}]");

            int slot = bufferManager.FindIndexBufferDescription(this.Dynamic);
            if (slot < 0)
            {
                descriptor = new BufferManagerIndices(this.Dynamic);
                slot = bufferManager.AddIndexBufferDescription(descriptor);
            }
            else
            {
                descriptor = bufferManager.GetIndexBufferDescription(slot);
                descriptor.ReallocationNeeded = true;
            }

            descriptor.AddDescriptor(this.Descriptor, this.Id, slot, this.Data);
        }
        /// <summary>
        /// Remove the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void Remove(BufferManager bufferManager)
        {
            if (this.Descriptor?.Ready == true)
            {
                var descriptor = bufferManager.GetIndexBufferDescription(this.Descriptor.BufferDescriptionIndex);

                Logger.WriteTrace($"Remove BufferDescriptor {(descriptor.Dynamic ? "dynamic" : "static")} {typeof(uint)} [{this.Descriptor.Id}]");

                descriptor.RemoveDescriptor(this.Descriptor);
                descriptor.ReallocationNeeded = true;
            }
        }
    }
}
