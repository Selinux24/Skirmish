using System;
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
        public EngineShaderResourceView ResourceView { get; private set; } = new EngineShaderResourceView("ImageContent");
        /// <summary>
        /// Image content
        /// </summary>
        public ImageContent ImageContent { get; set; }
        /// <summary>
        /// Mip autogen
        /// </summary>
        public bool MipAutogen { get; set; }
        /// <summary>
        /// Dynamic resource
        /// </summary>
        public bool Dynamic { get; set; }

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
        /// <param name="game">Game instance</param>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic texture</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResource(Game game, ImageContent imageContent, bool mipAutogen = true, bool dynamic = false)
        {
            if (imageContent.Stream != null)
            {
                return game.Graphics.LoadTexture(imageContent.Stream, imageContent.CropRectangle, mipAutogen, dynamic);
            }
            else
            {
                if (imageContent.IsCubic)
                {
                    return CreateResourceCubic(game, imageContent, mipAutogen, dynamic);
                }
                else if (imageContent.IsArray)
                {
                    return CreateResourceArray(game, imageContent, mipAutogen, dynamic);
                }
                else
                {
                    return CreateResourceDefault(game, imageContent, mipAutogen, dynamic);
                }
            }
        }
        /// <summary>
        /// Creates a resource view from image content
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic texture</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceDefault(Game game, ImageContent imageContent, bool mipAutogen = true, bool dynamic = false)
        {
            if (!string.IsNullOrWhiteSpace(imageContent.Path))
            {
                return game.Graphics.LoadTexture(imageContent.Path, imageContent.CropRectangle, mipAutogen, dynamic);
            }
            else if (imageContent.Stream != null)
            {
                return game.Graphics.LoadTexture(imageContent.Stream, imageContent.CropRectangle, mipAutogen, dynamic);
            }

            return null;
        }
        /// <summary>
        /// Creates a resource view from image content array
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic texture</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceArray(Game game, ImageContent imageContent, bool mipAutogen = true, bool dynamic = false)
        {
            if (imageContent.Paths.Any())
            {
                return game.Graphics.LoadTextureArray(imageContent.Paths, imageContent.CropRectangle, mipAutogen, dynamic);
            }
            else if (imageContent.Streams.Any())
            {
                return game.Graphics.LoadTextureArray(imageContent.Streams, imageContent.CropRectangle, mipAutogen, dynamic);
            }

            return null;
        }
        /// <summary>
        /// Creates a resource view from cubic image content
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Dynamic texture</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceCubic(Game game, ImageContent imageContent, bool mipAutogen = true, bool dynamic = false)
        {
            if (imageContent.IsArray)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(imageContent.Path))
                {
                    return game.Graphics.LoadTextureCubic(imageContent.Path, imageContent.Faces, mipAutogen, dynamic);
                }
                else if (imageContent.Stream != null)
                {
                    return game.Graphics.LoadTextureCubic(imageContent.Stream, imageContent.Faces, mipAutogen, dynamic);
                }
            }

            return null;
        }
    }
}
