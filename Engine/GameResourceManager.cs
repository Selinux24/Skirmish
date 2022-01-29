using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using System.Threading.Tasks;

    /// <summary>
    /// Engine resource manager
    /// </summary>
    public class GameResourceManager : IDisposable
    {
        /// <summary>
        /// Content loaders dictionary
        /// </summary>
        private static readonly ConcurrentDictionary<string, Func<ILoader>> contentLoaders = new ConcurrentDictionary<string, Func<ILoader>>();

        /// <summary>
        /// Register content loader
        /// </summary>
        /// <typeparam name="T">Type of loader</typeparam>
        /// <returns>Returns true if all extensions were registered</returns>
        public static bool RegisterLoader<T>() where T : class, ILoader
        {
            T loader = Activator.CreateInstance<T>();

            var extensions = loader.GetExtensions();
            if (!extensions.Any())
            {
                return false;
            }

            bool allRegistered = true;

            var loaderDelegate = loader.GetLoaderDelegate();

            foreach (var extension in extensions)
            {
                if (!RegisterLoaderForFile(extension, loaderDelegate))
                {
                    allRegistered = false;
                }
            }

            return allRegistered;
        }
        /// <summary>
        /// Register a loader for the specified extension
        /// </summary>
        /// <param name="extension">Extension</param>
        /// <param name="loaderDelegate">Delegate</param>
        /// <returns>Returns true if the extension was registered</returns>
        public static bool RegisterLoaderForFile(string extension, Func<ILoader> loaderDelegate)
        {
            if (contentLoaders.ContainsKey(extension))
            {
                Logger.WriteWarning(nameof(GameResourceManager), $"Extension {extension} is currently registered.");
                return false;
            }

            if (!contentLoaders.TryAdd(extension, loaderDelegate))
            {
                Logger.WriteWarning(nameof(GameResourceManager), $"Cannot get {extension} loader delegate from concurrent delegate.");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Gets a content loader for the specified file name
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns a loader, or null if not exists</returns>
        public static ILoader GetLoaderForFile(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            if (!contentLoaders.ContainsKey(extension))
            {
                Logger.WriteWarning(nameof(GameResourceManager), $"Extension {extension} is not registered. A valid content loader must be added first.");
                return null;
            }

            if (contentLoaders.TryGetValue(extension, out var loaderDelegate))
            {
                return loaderDelegate.Invoke();
            }
            else
            {
                Logger.WriteWarning(nameof(GameResourceManager), $"Cannot get {extension} loader delegate from concurrent delegate.");

                return null;
            }
        }

        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game;
        /// <summary>
        /// Requested resources dictionary
        /// </summary>
        private readonly ConcurrentDictionary<string, IGameResourceRequest> requestedResources = new ConcurrentDictionary<string, IGameResourceRequest>();
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
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="callback">Callback</param>
        public void CreateResources(string id, IProgress<LoadResourceProgress> progress, Action callback = null)
        {
            if (creatingResources)
            {
                callback?.Invoke();

                return;
            }

            var pendingRequests = GetPendingRequests();
            if (!pendingRequests.Any())
            {
                callback?.Invoke();

                return;
            }

            try
            {
                creatingResources = true;

                // Get pending requests
                float total = pendingRequests.Count() + 1f;
                float current = 0;

                // Process requests
                foreach (var resource in pendingRequests)
                {
                    resource.Value.Create(game);

                    resources.Add(resource.Key, resource.Value.ResourceView);

                    progress?.Report(new LoadResourceProgress { Id = id, Progress = ++current / total });
                }

                // Remove requests
                RemoveRequests(pendingRequests.Select(r => r.Key));

                progress?.Report(new LoadResourceProgress { Id = id, Progress = 1f });
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Error creating new resources: {ex.Message}", ex);
            }
            finally
            {
                creatingResources = false;

                callback?.Invoke();
            }
        }
        /// <summary>
        /// Gets the pending requests
        /// </summary>
        private IEnumerable<KeyValuePair<string, IGameResourceRequest>> GetPendingRequests()
        {
            return requestedResources.ToArray();
        }
        /// <summary>
        /// Removes the specified requests keys
        /// </summary>
        /// <param name="requestKeys">Request keys list</param>
        private void RemoveRequests(IEnumerable<string> requestKeys)
        {
            foreach (string key in requestKeys)
            {
                requestedResources.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResource(string name, IImageContent imageContent, bool mipAutogen = true, bool dynamic = false)
        {
            var resource = new GameResourceImageContent(imageContent, mipAutogen, dynamic);

            resource.Create(game);
            SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="path">Resource file path</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResource(string name, string path, bool mipAutogen = true, bool dynamic = false)
        {
            var resource = new GameResourceImageContent(new FileImageContent(path), mipAutogen, dynamic);

            resource.Create(game);
            SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="stream">Resource data stream</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResource(string name, MemoryStream stream, bool mipAutogen = true, bool dynamic = false)
        {
            var resource = new GameResourceImageContent(new MemoryImageContent(stream), mipAutogen, dynamic);

            resource.Create(game);
            SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Resource name</param>
        /// <param name="values">Values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResource<T>(string name, IEnumerable<T> values, int size, bool dynamic = false) where T : struct
        {
            GameResourceValueArray<T> resource = new GameResourceValueArray<T>()
            {
                Values = values,
                Size = size,
                Dynamic = dynamic,
            };

            resource.Create(game);
            SetGlobalResource(name, resource.ResourceView);
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
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResource(string name, int size, float min, float max, int seed = 0, bool dynamic = false)
        {
            GameResourceRandomTexture resource = new GameResourceRandomTexture()
            {
                Size = size,
                Min = min,
                Max = max,
                Seed = seed,
                Dynamic = dynamic,
            };

            resource.Create(game);
            SetGlobalResource(name, resource.ResourceView);
            return resource.ResourceView;
        }
        /// <summary>
        /// Set global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="resource">Resource content</param>
        private void SetGlobalResource(string name, EngineShaderResourceView resource)
        {
            if (globalResources.ContainsKey(name))
            {
                var cRes = globalResources[name];
                var srv = cRes.GetResource();
                srv?.Dispose();
                cRes.SetResource(resource.GetResource());
            }
            else
            {
                globalResources.Add(name, resource);
            }
        }

        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the engine shader resource view</returns>
        public async Task<EngineShaderResourceView> RequestResource(IImageContent imageContent, bool mipAutogen = true, bool dynamic = false)
        {
            var (key, resource) = await TryGetResource(imageContent);
            if (resource != null)
            {
                return resource;
            }

            if (requestedResources.ContainsKey(key))
            {
                return requestedResources[key].ResourceView;
            }

            var request = new GameResourceImageContent(imageContent, mipAutogen, dynamic);

            if (!requestedResources.TryAdd(key, request))
            {
                return null;
            }

            return request.ResourceView;
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="path">Path to resource</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the engine shader resource view</returns>
        public async Task<EngineShaderResourceView> RequestResource(string path, bool mipAutogen = true, bool dynamic = false)
        {
            return await RequestResource(new FileImageContent(path), mipAutogen, dynamic);
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="stream">Data stream</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the engine shader resource view</returns>
        public async Task<EngineShaderResourceView> RequestResource(MemoryStream stream, bool mipAutogen = true, bool dynamic = false)
        {
            return await RequestResource(new MemoryImageContent(stream), mipAutogen, dynamic);
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="identifier">Resource identifier</param>
        /// <param name="values">Values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the engine shader resource view</returns>
        public async Task<EngineShaderResourceView> RequestResource<T>(Guid identifier, IEnumerable<T> values, int size, bool dynamic = false) where T : struct
        {
            var (key, resource) = await TryGetResource(identifier);
            if (resource != null)
            {
                return resource;
            }

            if (requestedResources.ContainsKey(key))
            {
                return requestedResources[key].ResourceView;
            }

            var request = new GameResourceValueArray<T>
            {
                Values = values,
                Size = size,
                Dynamic = dynamic,
            };

            if (!requestedResources.TryAdd(key, request))
            {
                return null;
            }

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
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the engine shader resource view</returns>
        public async Task<EngineShaderResourceView> RequestResource(Guid identifier, int size, float min, float max, int seed = 0, bool dynamic = false)
        {
            var (key, resource) = await TryGetResource(identifier);
            if (resource != null)
            {
                return resource;
            }

            if (requestedResources.ContainsKey(key))
            {
                return requestedResources[key].ResourceView;
            }

            var request = new GameResourceRandomTexture
            {
                Size = size,
                Min = min,
                Max = max,
                Seed = seed,
                Dynamic = dynamic,
            };

            if (!requestedResources.TryAdd(key, request))
            {
                return null;
            }

            return request.ResourceView;
        }

        /// <summary>
        /// Trys to get a resource by content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <param name="key">Resource key</param>
        /// <returns>Returns the resource if exists</returns>
        private async Task<(string Key, EngineShaderResourceView Resource)> TryGetResource(IImageContent imageContent)
        {
            EngineShaderResourceView resource = null;

            string key = imageContent?.GetResourceKey();
            if (key == null)
            {
                return await Task.FromResult((key, resource));
            }

            if (!resources.ContainsKey(key))
            {
                return await Task.FromResult((key, resource));
            }

            resource = resources[key];

            return await Task.FromResult((key, resource));
        }
        /// <summary>
        /// Trys to get a resource by identifier
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <returns>Returns the resource if exists</returns>
        private async Task<(string Key, EngineShaderResourceView Resource)> TryGetResource(Guid identifier)
        {
            EngineShaderResourceView resource = null;

            string key = identifier.ToByteArray().GetMd5Sum();
            if (key == null)
            {
                return await Task.FromResult((key, resource));
            }

            if (!resources.ContainsKey(key))
            {
                return await Task.FromResult((key, resource));
            }

            resource = resources[key];

            return await Task.FromResult((key, resource));
        }
    }
}
