
namespace Engine.Common
{
    /// <summary>
    /// Remove vertex buffer descriptor request
    /// </summary>
    class BufferDescriptorRemoveVerticesRequest(BufferDescriptor descriptor) : IBufferDescriptorRequest
    {
        /// <summary>
        /// Descriptor to remove
        /// </summary>
        private readonly BufferDescriptor descriptor = descriptor;

        /// <inheritdoc/>
        public string Id { get; } = descriptor.Id;
        /// <inheritdoc/>
        public bool Dynamic { get; } = false;
        /// <inheritdoc/>
        public BufferDescriptorRequestActions Action { get; } = BufferDescriptorRequestActions.Remove;
        /// <inheritdoc/>
        public ProcessedStages Processed { get; private set; } = ProcessedStages.Requested;

        /// <inheritdoc/>
        public void Process(BufferManager bufferManager)
        {
            Processed = ProcessedStages.InProcess;

            if (descriptor?.Ready == true)
            {
                var desc = bufferManager.GetVertexBufferDescription(descriptor.BufferDescriptionIndex);

                Logger.WriteTrace(this, $"Remove BufferDescriptor {(desc.Dynamic ? "dynamic" : "static")} [{descriptor.Id}]");

                desc.RemoveDescriptor(descriptor);
            }

            Processed = ProcessedStages.Processed;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => {Action} {Processed}; {descriptor}";
        }
    }
}
