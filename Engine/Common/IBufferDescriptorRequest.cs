
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
        string Id { get; set; }
        /// <summary>
        /// Gets wheter the destination buffer is dynamic or not
        /// </summary>
        bool Dynamic { get; set; }
        /// <summary>
        /// Request action
        /// </summary>
        BufferDescriptorRequestActions Action { get; set; }
        /// <summary>
        /// Gets wheter the descriptor is processed into the buffer manager or not
        /// </summary>
        ProcessedStages Processed { get; set; }

        /// <summary>
        /// Updates the buffer descriptor
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        void Process(BufferManager bufferManager);
    }
}
