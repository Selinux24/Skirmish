
namespace Engine.Common
{
    /// <summary>
    /// Engine instancing buffer descriptor interface
    /// </summary>
    public interface IEngineInstancingBufferDescriptor : IEngineDescriptor
    {
        /// <summary>
        /// Instances
        /// </summary>
        int Instances { get; set; }
        /// <summary>
        /// Vertex buffer binding index in the manager list
        /// </summary>
        int BufferBindingIndex { get; set; }

        /// <summary>
        /// Gets the buffer format stride
        /// </summary>
        /// <returns>Returns the buffer format stride in bytes</returns>
        int GetStride();

        /// <summary>
        /// Copies the descriptor
        /// </summary>
        IEngineInstancingBufferDescriptor Copy();
    }
}
