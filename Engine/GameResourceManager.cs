using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

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
        /// Creating resources flag
        /// </summary>
        private bool creatingResources = false;

        /// <summary>
        /// Gets wheter the resource manager has requests to process or not
        /// </summary>
        public bool HasRequests
        {
            get
            {
                return requestedResources.Any();
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
            if (!HasRequests)
            {
                return;
            }

            if (creatingResources)
            {
                return;
            }

            creatingResources = true;

            foreach (var resource in requestedResources)
            {
                resource.Value.Create(this.game);

                resources.Add(resource.Key, resource.Value.ResourceView);
            }

            requestedResources.Clear();

            creatingResources = false;
        }

        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResource(string name, ImageContent imageContent, bool mipAutogen = true)
        {
            GameResourceImageContent resource = new GameResourceImageContent()
            {
                ImageContent = imageContent,
                MipAutogen = mipAutogen,
            };

            resource.Create(this.game);
            this.SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="path">Resource file path</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns></returns>
        public EngineShaderResourceView CreateGlobalResource(string name, string path, bool mipAutogen = true)
        {
            GameResourceImageContent resource = new GameResourceImageContent()
            {
                ImageContent = new ImageContent() { Path = path },
                MipAutogen = mipAutogen,
            };

            resource.Create(this.game);
            this.SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="stream">Resource data stream</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <returns></returns>
        public EngineShaderResourceView CreateGlobalResource(string name, MemoryStream stream, bool mipAutogen = true)
        {
            GameResourceImageContent resource = new GameResourceImageContent()
            {
                ImageContent = new ImageContent() { Stream = stream },
                MipAutogen = mipAutogen,
            };

            resource.Create(this.game);
            this.SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="values">Values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResource(string name, IEnumerable<Vector4> values, int size)
        {
            GameResourceValueArray resource = new GameResourceValueArray()
            {
                Values = values,
                Size = size,
            };

            resource.Create(this.game);
            this.SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
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
        public EngineShaderResourceView CreateGlobalResource(string name, int size, float min, float max, int seed = 0)
        {
            GameResourceRandomTexture resource = new GameResourceRandomTexture()
            {
                Size = size,
                Min = min,
                Max = max,
                Seed = seed,
            };

            resource.Create(this.game);
            this.SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
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
                var srv = cRes.GetResource();
                srv?.Dispose();
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

            var request = new GameResourceImageContent
            {
                ImageContent = imageContent,
                MipAutogen = mipAutogen,
            };

            requestedResources.Add(resourceKey, request);

            return request.ResourceView;
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

            var request = new GameResourceValueArray
            {
                Values = values,
                Size = size,
            };

            requestedResources.Add(resourceKey, request);

            return request.ResourceView;
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

            var request = new GameResourceRandomTexture
            {
                Size = size,
                Min = min,
                Max = max,
                Seed = seed,
            };

            requestedResources.Add(resourceKey, request);

            return request.ResourceView;
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
    }
}
