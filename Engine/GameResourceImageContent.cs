using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Image content resource request
    /// </summary>
    public class GameResourceImageContent : IGameResourceRequest
    {
        /// <summary>
        /// Engine resource view
        /// </summary>
        public EngineShaderResourceView ResourceView { get; private set; }
        /// <summary>
        /// Image content
        /// </summary>
        public IImageContent ImageContent { get; set; }
        /// <summary>
        /// Mip autogen
        /// </summary>
        public bool MipAutogen { get; set; }
        /// <summary>
        /// Dynamic resource
        /// </summary>
        public bool Dynamic { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Mip auto generation</param>
        /// <param name="dynamic">Dynamic resource</param>
        public GameResourceImageContent(IImageContent imageContent, bool mipAutogen = true, bool dynamic = false)
        {
            ImageContent = imageContent ?? throw new ArgumentNullException(nameof(imageContent), "A image content must be specified.");
            MipAutogen = mipAutogen;
            Dynamic = dynamic;

            ResourceView = new EngineShaderResourceView(imageContent.GetResourceKey());
        }

        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="game">Game</param>
        public void Create(Game game)
        {
            var resource = ImageContent.CreateResource(game, MipAutogen, Dynamic).GetResource();

            ResourceView.SetResource(resource);
        }
    }
}
