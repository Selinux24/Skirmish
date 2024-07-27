
namespace Engine.Common
{
    /// <summary>
    /// Instace buffer descriptor request
    /// </summary>
    class BufferDescriptorRequestInstancing<T> : IBufferDescriptorRequest where T : struct, IInstacingData
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
        /// Gets or sets de number of instances of this geometry
        /// </summary>
        public int Instances { get; set; }
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
        /// <summary>
        /// Assign the descriptor to the buffer manager
        /// </summary>
        /// <param name="request">Buffer request</param>
        private void Add(BufferManager bufferManager)
        {
            BufferManagerInstances<T> descriptor;

            Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {typeof(T)} [{Id}]");

            var slot = bufferManager.FindInstancingBufferDescription(Dynamic);
            if (slot < 0)
            {
                descriptor = new BufferManagerInstances<T>(Dynamic);
                slot = bufferManager.AddInstancingBufferDescription(descriptor);
            }
            else
            {
                descriptor = bufferManager.GetInstancingBufferDescription<T>(slot);
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
                var descriptor = bufferManager.GetInstancingBufferDescription<T>(Descriptor.BufferDescriptionIndex);

                Logger.WriteTrace(this, $"Remove BufferDescriptor {(descriptor.Dynamic ? "dynamic" : "static")} {typeof(T)} [{Descriptor.Id}]");

                descriptor.RemoveDescriptor(Descriptor, Instances);
                descriptor.ReallocationNeeded = true;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => {Action} {Processed}; {Descriptor}";
        }
    }
}
