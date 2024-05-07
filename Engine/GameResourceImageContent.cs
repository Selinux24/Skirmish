using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Image content resource request
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="imageContent">Image content</param>
    /// <param name="mipAutogen">Mip auto generation</param>
    /// <param name="dynamic">Dynamic resource</param>
    public class GameResourceImageContent(IImageContent imageContent, bool mipAutogen = true, bool dynamic = false) : IGameResourceRequest
    {
        /// <inheritdoc/>
        public string Name { get; private set; } = imageContent.Name;
        /// <inheritdoc/>
        public EngineShaderResourceView ResourceView { get; private set; } = new(imageContent.GetResourceKey());
        /// <summary>
        /// Image content
        /// </summary>
        public IImageContent ImageContent { get; set; } = imageContent ?? throw new ArgumentNullException(nameof(imageContent), "A image content must be specified.");
        /// <summary>
        /// Mip autogen
        /// </summary>
        public bool MipAutogen { get; set; } = mipAutogen;
        /// <summary>
        /// Dynamic resource
        /// </summary>
        public bool Dynamic { get; set; } = dynamic;

        /// <inheritdoc/>
        public void Create(Game game)
        {
            var srv = ImageContent.CreateResource(game, Name, MipAutogen, Dynamic).GetResource();
            ResourceView.SetResource(srv);
        }
    }
}
