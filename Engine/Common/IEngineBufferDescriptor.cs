
namespace Engine.Common
{
    /// <summary>
    /// Engine buffer descriptor interface
    /// </summary>
    public interface IEngineBufferDescriptor : IEngineDescriptor
    {
        /// <summary>
        /// Copies the descriptor
        /// </summary>
        IEngineBufferDescriptor Copy();
    }
}
