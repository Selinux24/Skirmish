using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Image content resource request
    /// </summary>
    public class GameResourceImageContent : IGameResourceRequest
    {
        /// <summary>
        /// Engine resource view
        /// </summary>
        public EngineShaderResourceView ResourceView { get; private set; } = new EngineShaderResourceView();
        /// <summary>
        /// Image content
        /// </summary>
        public ImageContent ImageContent { get; set; }
        /// <summary>
        /// Mip autogen
        /// </summary>
        public bool MipAutogen { get; set; }

        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="game">Game</param>
        public void Create(Game game)
        {
            var srv = CreateResource(game, ImageContent, MipAutogen);
            ResourceView.SetResource(srv);
        }

        /// <summary>
        /// Generates the resource view
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResource(Game game, ImageContent imageContent, bool mipAutogen = true)
        {
            if (imageContent.Stream != null)
            {
                return game.Graphics.LoadTexture(imageContent.Stream, mipAutogen);
            }
            else
            {
                if (imageContent.IsCubic)
                {
                    return CreateResourceCubic(game, imageContent, mipAutogen);
                }
                else if (imageContent.IsArray)
                {
                    return CreateResourceArray(game, imageContent, mipAutogen);
                }
                else
                {
                    return CreateResourceDefault(game, imageContent, mipAutogen);
                }
            }
        }
        /// <summary>
        /// Creates a resource view from image content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceDefault(Game game, ImageContent imageContent, bool mipAutogen = true)
        {
            if (!string.IsNullOrWhiteSpace(imageContent.Path))
            {
                return game.Graphics.LoadTexture(imageContent.Path, mipAutogen);
            }
            else if (imageContent.Stream != null)
            {
                return game.Graphics.LoadTexture(imageContent.Stream, mipAutogen);
            }

            return null;
        }
        /// <summary>
        /// Creates a resource view from image content array
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceArray(Game game, ImageContent imageContent, bool mipAutogen = true)
        {
            if (imageContent.Paths.Any())
            {
                return game.Graphics.LoadTextureArray(imageContent.Paths, mipAutogen);
            }
            else if (imageContent.Streams.Any())
            {
                return game.Graphics.LoadTextureArray(imageContent.Streams, mipAutogen);
            }

            return null;
        }
        /// <summary>
        /// Creates a resource view from cubic image content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceCubic(Game game, ImageContent imageContent, bool mipAutogen = true)
        {
            if (imageContent.IsArray)
            {
                if (imageContent.Paths.Any())
                {
                    return game.Graphics.LoadTextureArray(imageContent.Paths, mipAutogen);
                }
                else if (imageContent.Streams.Any())
                {
                    return game.Graphics.LoadTextureArray(imageContent.Streams, mipAutogen);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(imageContent.Path))
                {
                    return game.Graphics.LoadTexture(imageContent.Path, mipAutogen);
                }
                else if (imageContent.Stream != null)
                {
                    return game.Graphics.LoadTexture(imageContent.Stream, mipAutogen);
                }
            }

            return null;
        }
    }
}
