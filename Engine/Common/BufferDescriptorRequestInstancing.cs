using System;

namespace Engine.Common
{
    class BufferDescriptorRequestInstancing : IBufferDescriptorRequest
    {
        /// <summary>
        /// Requester Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets wheter the destination buffer must be dynamic or not
        /// </summary>
        public bool Dynamic { get; set; }
        /// <summary>
        /// Gets or sets de number of instances of this geometry
        /// </summary>
        public int Instances { get; set; }
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
            InstancingBufferDescription descriptor;

            Console.WriteLine($"Add BufferDescriptor {(this.Dynamic ? "dynamic" : "static")} {typeof(IInstacingData)} [{this.Id}]");

            var slot = bufferManager.FindInstancingBufferDescription(this.Dynamic);
            if (slot < 0)
            {
                descriptor = new InstancingBufferDescription(this.Dynamic);
                slot = bufferManager.AddInstancingBufferDescription(descriptor);
            }
            else
            {
                descriptor = bufferManager.GetInstancingBufferDescription(slot);
                descriptor.ReallocationNeeded = true;
            }

            descriptor.AddDescriptor(this.Descriptor, this.Id, slot, this.Instances);
        }
        /// <summary>
        /// Remove the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void Remove(BufferManager bufferManager)
        {
            if (this.Descriptor?.Ready == true)
            {
                var descriptor = bufferManager.GetInstancingBufferDescription(this.Descriptor.BufferDescriptionIndex);

                Console.WriteLine($"Remove BufferDescriptor {(descriptor.Dynamic ? "dynamic" : "static")} {typeof(VertexInstancingData)} [{this.Descriptor.Id}]");

                descriptor.RemoveDescriptor(this.Descriptor, this.Instances);
                descriptor.ReallocationNeeded = true;
            }
        }
    }
}
