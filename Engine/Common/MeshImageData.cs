using System.Threading.Tasks;

namespace Engine.Common
{
    using Engine.Content;

    /// <summary>
    /// Mesh image data
    /// </summary>
    public class MeshImageData
    {
        /// <summary>
        /// Image content
        /// </summary>
        public IImageContent Content { get; set; }
        /// <summary>
        /// Mesh image
        /// </summary>
        public IMeshImage Texture { get; set; }

        /// <summary>
        /// Create mesh texture from texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public static MeshImageData FromContent(IImageContent texture)
        {
            return new MeshImageData
            {
                Content = texture,
            };
        }

        /// <summary>
        /// Requests the image resource
        /// </summary>
        /// <param name="resourceManager">Resource manager</param>
        public async Task RequestResource(GameResourceManager resourceManager)
        {
            Texture = await Content.CreateMeshImage(resourceManager);
        }
    }
}
