using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Helpers;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Engine resource manager
    /// </summary>
    public class GameResourceManager : IDisposable
    {
        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game;
        /// <summary>
        /// Requested resources dictionary
        /// </summary>
        private readonly Dictionary<string, IGameResourceRequest> requestedResources = new Dictionary<string, IGameResourceRequest>();
        /// <summary>
        /// Resource dictionary
        /// </summary>
        private readonly Dictionary<string, EngineShaderResourceView> resources = new Dictionary<string, EngineShaderResourceView>();
        /// <summary>
        /// Global resources dictionary
        /// </summary>
        private readonly Dictionary<string, EngineShaderResourceView> globalResources = new Dictionary<string, EngineShaderResourceView>();

        /// <summary>
        /// Requested resource interface
        /// </summary>
        interface IGameResourceRequest
        {
            /// <summary>
            /// Engine resource view
            /// </summary>
            EngineShaderResourceView ResourceView { get; set; }

            /// <summary>
            /// Creates the resource
            /// </summary>
            /// <param name="resourceManager">Resource manager</param>
            void Create(GameResourceManager resourceManager);
        }

        /// <summary>
        /// Image content resource request
        /// </summary>
        class ResourceImageContent : IGameResourceRequest
        {
            /// <summary>
            /// Engine resource view
            /// </summary>
            public EngineShaderResourceView ResourceView { get; set; }
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
            /// <param name="resourceManager">Resource manager</param>
            public void Create(GameResourceManager resourceManager)
            {
                var srv = resourceManager.CreateResource(ImageContent, MipAutogen);
                ResourceView.SetResource(srv);
            }
        }

        /// <summary>
        /// Vector4 value array resource request
        /// </summary>
        class ResourceValueArray : IGameResourceRequest
        {
            /// <summary>
            /// Engine resource view
            /// </summary>
            public EngineShaderResourceView ResourceView { get; set; }
            /// <summary>
            /// Size
            /// </summary>
            public int Size { get; set; }
            /// <summary>
            /// Vector4 values
            /// </summary>
            public IEnumerable<Vector4> Values { get; set; }

            /// <summary>
            /// Creates the resource
            /// </summary>
            /// <param name="resourceManager">Resource manager</param>
            public void Create(GameResourceManager resourceManager)
            {
                var srv = resourceManager.CreateResource(Values, Size);
                ResourceView.SetResource(srv);
            }
        }

        /// <summary>
        /// Random texture resource request
        /// </summary>
        class ResourceRandomTexture : IGameResourceRequest
        {
            /// <summary>
            /// Engine resource view
            /// </summary>
            public EngineShaderResourceView ResourceView { get; set; }
            /// <summary>
            /// Size
            /// </summary>
            public int Size { get; set; }
            /// <summary>
            /// Minimum value
            /// </summary>
            public float Min { get; set; }
            /// <summary>
            /// Maximum value
            /// </summary>
            public float Max { get; set; }
            /// <summary>
            /// Random seed
            /// </summary>
            public int Seed { get; set; }

            /// <summary>
            /// Creates the resource
            /// </summary>
            /// <param name="resourceManager">Resource manager</param>
            public void Create(GameResourceManager resourceManager)
            {
                var srv = resourceManager.CreateResource(Size, Min, Max, Seed);
                ResourceView.SetResource(srv);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public GameResourceManager(Game game)
        {
            this.game = game;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameResourceManager()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var item in resources)
                {
                    item.Value?.Dispose();
                }
                resources.Clear();

                foreach (var item in globalResources)
                {
                    item.Value?.Dispose();
                }
                globalResources.Clear();
            }
        }

        /// <summary>
        /// Creates the requested resources
        /// </summary>
        public void CreateResources()
        {
            if (!requestedResources.Any())
            {
                return;
            }

            foreach (var resource in requestedResources)
            {
                resource.Value.Create(this);
            }

            requestedResources.Clear();
        }

        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="bytes">Resource bytes</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns></returns>
        public EngineShaderResourceView CreateGlobalResourceTexture(string name, byte[] bytes, bool mipAutogen = true)
        {
            var view = new EngineShaderResourceView(this.LoadTexture(bytes, mipAutogen));
            this.SetGlobalResource(name, view);
            return view;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="values">Values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResourceTexture2D(string name, IEnumerable<Vector4> values, int size)
        {
            var view = new EngineShaderResourceView(this.game.Graphics.CreateTexture2D(size, values));
            this.SetGlobalResource(name, view);
            return view;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResourceRandomTexture(string name, int size, float min, float max, int seed = 0)
        {
            var view = new EngineShaderResourceView(this.CreateRandomTexture(size, min, max, seed));
            this.SetGlobalResource(name, view);
            return view;
        }
        /// <summary>
        /// Set global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="resource">Resource content</param>
        private void SetGlobalResource(string name, EngineShaderResourceView resource)
        {
            if (this.globalResources.ContainsKey(name))
            {
                var cRes = this.globalResources[name];
                cRes.GetResource().Dispose();
                cRes.SetResource(resource.GetResource());
            }
            else
            {
                this.globalResources.Add(name, resource);
            }
        }

        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the engine shader resource view</returns>
        public EngineShaderResourceView RequestResource(ImageContent imageContent, bool mipAutogen = true)
        {
            var existingResource = this.TryGetResource(imageContent, out string resourceKey);
            if (existingResource != null)
            {
                return existingResource;
            }

            if (requestedResources.ContainsKey(resourceKey))
            {
                return requestedResources[resourceKey].ResourceView;
            }

            var srv = new EngineShaderResourceView();

            var request = new ResourceImageContent
            {
                ImageContent = imageContent,
                MipAutogen = mipAutogen,
                ResourceView = srv,
            };

            requestedResources.Add(resourceKey, request);

            return srv;
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="path">Path to resource</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the engine shader resource view</returns>
        public EngineShaderResourceView RequestResource(string path, bool mipAutogen = true)
        {
            return RequestResource(new ImageContent { Path = path }, mipAutogen);
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="stream">Data stream</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the engine shader resource view</returns>
        public EngineShaderResourceView RequestResource(MemoryStream stream, bool mipAutogen = true)
        {
            return RequestResource(new ImageContent { Stream = stream }, mipAutogen);
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="values">Buffer</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the engine shader resource view</returns>
        public EngineShaderResourceView RequestResource(byte[] values, bool mipAutogen = true)
        {
            return RequestResource(new ImageContent { Stream = new MemoryStream(values) }, mipAutogen);
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="identifier">Resource identifier</param>
        /// <param name="values">Vector4 values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <returns>Returns the engine shader resource view</returns>
        public EngineShaderResourceView RequestResource(Guid identifier, IEnumerable<Vector4> values, int size)
        {
            string resourceKey = identifier.ToByteArray().GetMd5Sum();
            if (this.resources.ContainsKey(resourceKey))
            {
                return this.resources[resourceKey];
            }

            if (requestedResources.ContainsKey(resourceKey))
            {
                return requestedResources[resourceKey].ResourceView;
            }

            var srv = new EngineShaderResourceView();

            var request = new ResourceValueArray
            {
                Values = values,
                Size = size,
                ResourceView = srv,
            };

            requestedResources.Add(resourceKey, request);

            return srv;
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="identifier">Resource identifier</param>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns the engine shader resource view</returns>
        public EngineShaderResourceView RequestResource(Guid identifier, int size, float min, float max, int seed = 0)
        {
            string resourceKey = identifier.ToByteArray().GetMd5Sum();
            if (this.resources.ContainsKey(resourceKey))
            {
                return this.resources[resourceKey];
            }

            if (requestedResources.ContainsKey(resourceKey))
            {
                return requestedResources[resourceKey].ResourceView;
            }

            var srv = new EngineShaderResourceView();

            var request = new ResourceRandomTexture
            {
                Size = size,
                Min = min,
                Max = max,
                Seed = seed,
                ResourceView = srv,
            };

            requestedResources.Add(resourceKey, request);

            return srv;
        }

        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private EngineShaderResourceView TryGetResource(ImageContent imageContent, out string key)
        {
            if (imageContent.IsCubic)
            {
                return TryGetResourceCubic(imageContent, out key);
            }
            else if (imageContent.IsArray)
            {
                return TryGetResourceArray(imageContent, out key);
            }
            else
            {
                return TryGetResourceDefault(imageContent, out key);
            }
        }
        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private EngineShaderResourceView TryGetResourceDefault(ImageContent imageContent, out string key)
        {
            key = null;

            if (!string.IsNullOrWhiteSpace(imageContent.Path))
            {
                return this.TryGet(imageContent.Path, out key);
            }
            else if (imageContent.Stream != null)
            {
                return this.TryGet(imageContent.Stream, out key);
            }

            return null;
        }
        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private EngineShaderResourceView TryGetResourceArray(ImageContent imageContent, out string key)
        {
            key = null;

            if (imageContent.Paths.Any())
            {
                return this.TryGet(imageContent.Paths, out key);
            }
            else if (imageContent.Streams.Any())
            {
                return this.TryGet(imageContent.Streams, out key);
            }

            return null;
        }
        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private EngineShaderResourceView TryGetResourceCubic(ImageContent imageContent, out string key)
        {
            key = null;

            if (imageContent.IsArray)
            {
                if (imageContent.Paths.Any())
                {
                    return this.TryGet(imageContent.Paths, out key);
                }
                else if (imageContent.Streams.Any())
                {
                    return this.TryGet(imageContent.Streams, out key);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(imageContent.Path))
                {
                    return this.TryGet(imageContent.Path, out key);
                }
                else if (imageContent.Stream != null)
                {
                    return this.TryGet(imageContent.Stream, out key);
                }
            }

            return null;
        }
        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private EngineShaderResourceView TryGet(string path, out string key)
        {
            key = path;
            if (!this.resources.ContainsKey(key))
            {
                return null;
            }

            return this.resources[key];
        }
        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="stream">Memory stream</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private EngineShaderResourceView TryGet(MemoryStream stream, out string key)
        {
            key = stream.GetMd5Sum();
            if (!this.resources.ContainsKey(key))
            {
                stream.Position = 0;
                return null;
            }

            return this.resources[key];
        }
        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="paths">Path list</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private EngineShaderResourceView TryGet(IEnumerable<string> paths, out string key)
        {
            key = paths.GetMd5Sum();
            if (!this.resources.ContainsKey(key))
            {
                return null;
            }

            return this.resources[key];
        }
        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private EngineShaderResourceView TryGet(IEnumerable<MemoryStream> streams, out string key)
        {
            key = streams.GetMd5Sum();
            if (!this.resources.ContainsKey(key))
            {
                return null;
            }

            return this.resources[key];
        }

        /// <summary>
        /// Loads a texture from memory in the graphics device
        /// </summary>
        /// <param name="buffer">Data buffer</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTexture(byte[] buffer, bool mipAutogen)
        {
            try
            {
                Counters.Textures++;

                using (var resource = Helper.Attempt(TextureData.ReadTexture, buffer, 5))
                {
                    return this.game.Graphics.CreateResource(resource, mipAutogen);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from byte array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTexture(string filename, bool mipAutogen)
        {
            try
            {
                Counters.Textures++;

                using (var resource = Helper.Attempt(TextureData.ReadTexture, filename, 5))
                {
                    return this.game.Graphics.CreateResource(resource, mipAutogen);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTexture(MemoryStream stream, bool mipAutogen)
        {
            try
            {
                Counters.Textures++;

                using (var resource = Helper.Attempt(TextureData.ReadTexture, stream, 5))
                {
                    return this.game.Graphics.CreateResource(resource, mipAutogen);
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="filenames">Path file collection</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTextureArray(IEnumerable<string> filenames, bool mipAutogen)
        {
            try
            {
                var textureList = Helper.Attempt(TextureData.ReadTexture, filenames, 5);

                return LoadTextureArray(textureList, mipAutogen);
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="streams">Stream collection</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        internal ShaderResourceView1 LoadTextureArray(IEnumerable<MemoryStream> streams, bool mipAutogen)
        {
            try
            {
                var textureList = Helper.Attempt(TextureData.ReadTexture, streams, 5);

                return LoadTextureArray(textureList, mipAutogen);
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array in the graphics device
        /// </summary>
        /// <param name="textureList">Texture array</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the resource view</returns>
        private ShaderResourceView1 LoadTextureArray(IEnumerable<TextureData> textureList, bool mipAutogen)
        {
            Counters.Textures++;

            var resource = this.game.Graphics.CreateResource(textureList, mipAutogen);

            foreach (var item in textureList)
            {
                item?.Dispose();
            }

            return resource;
        }
        /// <summary>
        /// Creates a random 1D texture
        /// </summary>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns created texture</returns>
        private ShaderResourceView1 CreateRandomTexture(int size, float min, float max, int seed = 0)
        {
            try
            {
                Counters.Textures++;

                Random rnd = new Random(seed);

                var randomValues = new List<Vector4>();
                for (int i = 0; i < size; i++)
                {
                    randomValues.Add(rnd.NextVector4(new Vector4(min), new Vector4(max)));
                }

                return this.game.Graphics.CreateTexture1D(size, randomValues.ToArray());
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRandomTexture Error. See inner exception for details", ex);
            }
        }

        /// <summary>
        /// Generates the resource view
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResource(ImageContent imageContent, bool mipAutogen = true)
        {
            if (imageContent.Stream != null)
            {
                byte[] buffer = imageContent.Stream.GetBuffer();
                return this.LoadTexture(buffer, mipAutogen);
            }
            else
            {
                if (imageContent.IsCubic)
                {
                    return CreateResourceCubic(imageContent, mipAutogen);
                }
                else if (imageContent.IsArray)
                {
                    return CreateResourceArray(imageContent, mipAutogen);
                }
                else
                {
                    return CreateResourceDefault(imageContent, mipAutogen);
                }
            }
        }
        /// <summary>
        /// Creates a 2d texture of Vector4 values
        /// </summary>
        /// <param name="values">Values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResource(IEnumerable<Vector4> values, int size)
        {
            return this.game.Graphics.CreateTexture2D(size, values);
        }
        /// <summary>
        /// Creates a 1d texture of random values
        /// </summary>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResource(int size, float min, float max, int seed = 0)
        {
            return this.CreateRandomTexture(size, min, max, seed);
        }
        /// <summary>
        /// Creates a resource view from image content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceDefault(ImageContent imageContent, bool mipAutogen = true)
        {
            if (!string.IsNullOrWhiteSpace(imageContent.Path))
            {
                return this.LoadTexture(imageContent.Path, mipAutogen);
            }
            else if (imageContent.Stream != null)
            {
                return this.LoadTexture(imageContent.Stream, mipAutogen);
            }

            return null;
        }
        /// <summary>
        /// Creates a resource view from image content array
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceArray(ImageContent imageContent, bool mipAutogen = true)
        {
            if (imageContent.Paths.Any())
            {
                return this.LoadTextureArray(imageContent.Paths, mipAutogen);
            }
            else if (imageContent.Streams.Any())
            {
                return this.LoadTextureArray(imageContent.Streams, mipAutogen);
            }

            return null;
        }
        /// <summary>
        /// Creates a resource view from cubic image content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        private ShaderResourceView1 CreateResourceCubic(ImageContent imageContent, bool mipAutogen = true)
        {
            if (imageContent.IsArray)
            {
                if (imageContent.Paths.Any())
                {
                    return this.LoadTextureArray(imageContent.Paths, mipAutogen);
                }
                else if (imageContent.Streams.Any())
                {
                    return this.LoadTextureArray(imageContent.Streams, mipAutogen);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(imageContent.Path))
                {
                    return this.LoadTexture(imageContent.Path, mipAutogen);
                }
                else if (imageContent.Stream != null)
                {
                    return this.LoadTexture(imageContent.Stream, mipAutogen);
                }
            }

            return null;
        }
    }
}
