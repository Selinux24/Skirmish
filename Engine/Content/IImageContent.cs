﻿
namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Image content
    /// </summary>
    public interface IImageContent
    {
        const int retryCount = 10;

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the image count into the image content
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Creates a mesh image
        /// </summary>
        /// <param name="resourceManager">Resource manager</param>
        public IMeshImage CreateMeshImage(GameResourceManager resourceManager)
        {
            for (int i = 0; i < retryCount; i++)
            {
                var view = resourceManager.RequestResource(this);
                if (view != null)
                {
                    return new MeshImage() { Resource = view };
                }
            }

            string errorMessage = $"Texture cannot be requested: {this}";

            Logger.WriteError(nameof(DrawingData), errorMessage);

            throw new EngineException(errorMessage);
        }
        /// <summary>
        /// Generates the resource view
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="name">Name</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic texture</param>
        /// <returns>Returns the created resource view</returns>
        EngineShaderResourceView CreateResource(Game game, string name, bool mipAutogen = true, bool dynamic = false);
        /// <summary>
        /// Gets the image unique resource key
        /// </summary>
        /// <returns>Returns the resource key</returns>
        string GetResourceKey();
    }
}
