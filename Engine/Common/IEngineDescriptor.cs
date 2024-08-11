
namespace Engine.Common
{
    /// <summary>
    /// Engine descriptor interface
    /// </summary>
    public interface IEngineDescriptor
    {
        /// <summary>
        /// Dynamic buffer
        /// </summary>
        bool Dynamic { get; }
        /// <summary>
        /// Vertex buffer index in the buffer manager list
        /// </summary>
        int BufferIndex { get; set; }
        /// <summary>
        /// Gets whether the current buffer is dirty
        /// </summary>
        /// <remarks>A buffer is dirty when needs reallocation or if it's not allocated at all</remarks>
        bool Dirty { get; }
        /// <summary>
        /// Allocated size into graphics device
        /// </summary>
        int AllocatedSize { get; }
        /// <summary>
        /// Gets whether the internal buffer needs reallocation
        /// </summary>
        bool ReallocationNeeded { get; }
        /// <summary>
        /// Gets whether the internal buffer is currently allocated in the graphic device
        /// </summary>
        bool Allocated { get; }
        /// <summary>
        /// Number of allocations
        /// </summary>
        int Allocations { get; }

        /// <summary>
        /// Gets the buffer format stride
        /// </summary>
        /// <returns>Returns the buffer format stride in bytes</returns>
        int GetStride();
        /// <summary>
        /// Gets the complete data size in bytes
        /// </summary>
        int SizeInBytes();

        /// <summary>
        /// Removes a buffer descriptor from the internal list
        /// </summary>
        /// <param name="descriptor">Buffer descriptor to remove</param>
        void RemoveDescriptor(BufferDescriptor descriptor);

        /// <summary>
        /// Updates the allocated buffer size
        /// </summary>
        void Allocate();
        /// <summary>
        /// Creates the graphics buffer
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Buffer name</param>
        /// <returns>Returns a engine buffer</returns>
        EngineBuffer CreateBuffer(Graphics graphics, string name);
    }
}
