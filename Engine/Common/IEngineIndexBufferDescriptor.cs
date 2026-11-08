
namespace Engine.Common
{
    /// <summary>
    /// Engine index buffer descriptor interface
    /// </summary>
    public interface IEngineIndexBufferDescriptor : IEngineDescriptor
    {
        /// <summary>
        /// Copies the descriptor
        /// </summary>
        IEngineIndexBufferDescriptor Copy();
    }
}
