
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
        /// Gets the input element collection
        /// </summary>
        /// <returns></returns>
        EngineInputElement[] GetInput();

        /// <summary>
        /// Copies the descriptor
        /// </summary>
        IEngineInstancingBufferDescriptor Copy();
    }
}
