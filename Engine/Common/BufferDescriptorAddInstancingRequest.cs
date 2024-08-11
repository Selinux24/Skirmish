namespace Engine.Common
{
    /// <summary>
    /// Instace buffer descriptor request
    /// </summary>
    class BufferDescriptorAddInstancingRequest<T>(string id, bool dynamic, int instances) : IBufferDescriptorRequest
        where T : struct, IInstacingData
    {
        /// <summary>
        /// New descriptor instance
        /// </summary>
        private readonly BufferDescriptor descriptor = new();
        /// <summary>
        /// Gets or sets de number of instances of this geometry
        /// </summary>
        private readonly int instances = instances;

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

            if (instances == 0)
            {
                Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} discarded no data [{Id}]");

                Processed = ProcessedStages.Processed;
                return;
            }

            BufferManagerInstances<T> instancesDesc;

            Logger.WriteTrace(this, $"Add BufferDescriptor {(Dynamic ? "dynamic" : "static")} {typeof(T)} [{Id}]");

            var slot = bufferManager.FindInstancingBufferDescription(Dynamic);
            if (slot < 0)
            {
                instancesDesc = new(Dynamic);
                slot = bufferManager.AddInstancingBufferDescription(instancesDesc);
            }
            else
            {
                instancesDesc = bufferManager.GetInstancingBufferDescription(slot) as BufferManagerInstances<T>;
                instancesDesc.ReallocationNeeded = true;
            }

            instancesDesc.AddDescriptor(descriptor, Id, slot, instances);

            Processed = ProcessedStages.Processed;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => {Action} {Processed}; {descriptor}";
        }
    }
}
