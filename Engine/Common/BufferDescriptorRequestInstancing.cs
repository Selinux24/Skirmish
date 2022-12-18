using System.Threading.Tasks;

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
            BufferManagerInstances descriptor;

            Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {typeof(IInstacingData)} [{Id}]");

            var slot = bufferManager.FindInstancingBufferDescription(Dynamic);
            if (slot < 0)
            {
                descriptor = new BufferManagerInstances(Dynamic);
                slot = bufferManager.AddInstancingBufferDescription(descriptor);
            }
            else
            {
                descriptor = bufferManager.GetInstancingBufferDescription(slot);
                descriptor.ReallocationNeeded = true;
            }

            descriptor.AddDescriptor(Descriptor, Id, slot, Instances);
        }
        /// <summary>
        /// Remove the descriptor from de internal buffers of the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void Remove(BufferManager bufferManager)
        {
            if (Descriptor?.Ready == true)
            {
                var descriptor = bufferManager.GetInstancingBufferDescription(Descriptor.BufferDescriptionIndex);

                Logger.WriteTrace(this, $"Remove BufferDescriptor {(descriptor.Dynamic ? "dynamic" : "static")} {typeof(VertexInstancingData)} [{Descriptor.Id}]");

                descriptor.RemoveDescriptor(Descriptor, Instances);
                descriptor.ReallocationNeeded = true;
            }
        }
    }
}
