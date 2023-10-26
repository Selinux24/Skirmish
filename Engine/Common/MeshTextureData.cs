using System.Threading.Tasks;

namespace Engine.Common
{
    using Engine.Content;

    /// <summary>
    /// Mesh texture data
    /// </summary>
    public class MeshTextureData
    {
        /// <summary>
        /// Texture content
        /// </summary>
        public IImageContent Content { get; set; }
        /// <summary>
        /// Texture resource
        /// </summary>
        public EngineShaderResourceView Resource { get; set; }

        /// <summary>
        /// Create mesh texture from texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public static MeshTextureData FromContent(IImageContent texture)
        {
            return new MeshTextureData
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
            var view = await resourceManager.RequestResource(Content);
            if (view == null)
            {
                string errorMessage = $"Texture cannot be requested: {Content}";

                Logger.WriteError(nameof(DrawingData), errorMessage);

                throw new EngineException(errorMessage);
            }

            Resource = view;
        }
    }
}
