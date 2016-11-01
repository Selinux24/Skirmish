using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    using Engine.Content;
    using Engine.Helpers;

    /// <summary>
    /// Engine resource manager
    /// </summary>
    public class GameResourceManager : IDisposable
    {
        /// <summary>
        /// Game instance
        /// </summary>
        private Game game;
        /// <summary>
        /// Resource dictionary
        /// </summary>
        private Dictionary<string, ShaderResourceView> resources = new Dictionary<string, ShaderResourceView>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public GameResourceManager(Game game)
        {
            this.game = game;
        }
        /// <summary>
        /// Resource disposing
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.resources);
        }

        /// <summary>
        /// Generate the resource view
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <returns>Returns the created resource view</returns>
        public ShaderResourceView CreateResource(ImageContent imageContent)
        {
            ShaderResourceView view = null;

            if (imageContent.Stream != null)
            {
                byte[] buffer = imageContent.Stream.GetBuffer();

                view = this.Get(buffer);
            }
            else
            {
                if (imageContent.IsArray)
                {
                    if (imageContent.Paths != null && imageContent.Paths.Length > 0)
                    {
                        view = this.Get(imageContent.Paths);
                    }
                    else if (imageContent.Streams != null && imageContent.Streams.Length > 0)
                    {
                        view = this.Get(imageContent.Streams);
                    }
                }
                else if (imageContent.IsCubic)
                {
                    int faceSize = imageContent.CubicFaceSize;

                    if (imageContent.Path != null)
                    {
                        view = this.Get(imageContent.Path, faceSize);
                    }
                    else if (imageContent.Stream != null)
                    {
                        view = this.Get(imageContent.Stream, faceSize);
                    }
                }
                else
                {
                    if (imageContent.Path != null)
                    {
                        view = this.Get(imageContent.Path);
                    }
                    else if (imageContent.Stream != null)
                    {
                        view = this.Get(imageContent.Stream);
                    }
                }
            }

            return view;
        }
        /// <summary>
        /// Generate the resource view
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>Returns the created resource view</returns>
        public ShaderResourceView CreateResource(string path)
        {
            return this.Get(path);
        }
        /// <summary>
        /// Creates a 2d texture of Vector4 values
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="values">Values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <returns>Returns the created resource view</returns>
        public ShaderResourceView CreateTexture2D(Guid identifier, Vector4[] values, int size)
        {
            string md5 = identifier.ToByteArray().GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.Device.CreateTexture2D(size, values);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Creates a 1d texture of random values
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns the created resource view</returns>
        public ShaderResourceView CreateRandomTexture(Guid identifier, int size, float min, float max, int seed = 0)
        {
            string md5 = identifier.ToByteArray().GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.Device.CreateRandomTexture(size, min, max, seed);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }

        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView Get(byte[] buffer)
        {
            string md5 = buffer.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.Device.LoadTexture(buffer);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView Get(string path)
        {
            if (!this.resources.ContainsKey(path))
            {
                var view = this.game.Graphics.Device.LoadTexture(path);
                this.resources.Add(path, view);
            }

            return this.resources[path];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="stream">Memory stream</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView Get(MemoryStream stream)
        {
            string md5 = stream.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                stream.Position = 0;
                var view = this.game.Graphics.Device.LoadTexture(stream);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="paths">Path list</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView Get(string[] paths)
        {
            string md5 = paths.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.Device.LoadTextureArray(paths);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView Get(MemoryStream[] streams)
        {
            string md5 = streams.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.Device.LoadTextureArray(streams);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="size">Cube size</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView Get(string path, int size)
        {
            if (!this.resources.ContainsKey(path))
            {
                var view = this.game.Graphics.Device.LoadTextureCube(path, size);
                this.resources.Add(path, view);
            }

            return this.resources[path];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="stream">Memory stream</param>
        /// <param name="size">Cube size</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView Get(MemoryStream stream, int size)
        {
            string md5 = stream.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                stream.Position = 0;
                var view = this.game.Graphics.Device.LoadTextureCube(stream, size);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
    }
}
