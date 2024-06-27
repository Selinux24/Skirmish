
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Mesh image interface
    /// </summary>
    public interface IMeshImage
    {
        /// <summary>
        /// Image resource
        /// </summary>
        EngineShaderResourceView Resource { get; set; }
    }
}
