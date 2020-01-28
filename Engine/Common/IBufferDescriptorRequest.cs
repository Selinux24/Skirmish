
namespace Engine.Common
{
    /// <summary>
    /// Buffer decriptor request interface
    /// </summary>
    interface IBufferDescriptorRequest
    {
        /// <summary>
        /// Requester Id
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Descriptor
        /// </summary>
        BufferDescriptor Descriptor { get; }
        /// <summary>
        /// Request action
        /// </summary>
        BufferDescriptorRequestActions Action { get; }
        /// <summary>
        /// Gets wheter the descriptor is processed into the buffer manager or not
        /// </summary>
        bool Processed { get; }

        /// <summary>
        /// Updates the specified buffer manager
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        void Update(BufferManager bufferManager);
    }
}
