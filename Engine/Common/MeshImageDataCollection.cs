
namespace Engine.Common
{
    /// <summary>
    /// Mesh image data collection
    /// </summary>
    public class MeshImageDataCollection : DrawingDataCollection<MeshImageData>
    {
        /// <summary>
        /// Gets the resource of the specified image
        /// </summary>
        /// <param name="name">Image name</param>
        public EngineShaderResourceView GetImage(string name)
        {
            return GetValue(name)?.Texture?.Resource;
        }
    }
}
