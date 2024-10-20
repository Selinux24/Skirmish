﻿using System;
using System.Collections.Concurrent;
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
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="game">Game</param>
    public class GameResourceManager(Game game) : IDisposable
    {
        /// <summary>
        /// Content loaders dictionary
        /// </summary>
        private static readonly ConcurrentDictionary<string, Func<ILoader>> contentLoaders = new();

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
        private readonly Game game = game;
        /// <summary>
        /// Requested resources dictionary
        /// </summary>
        private readonly ConcurrentDictionary<string, IGameResourceRequest> requestedResources = new();
        /// <summary>
        /// Resource dictionary
        /// </summary>
        private readonly Dictionary<string, EngineShaderResourceView> resources = [];
        /// <summary>
        /// Global resources dictionary
        /// </summary>
        private readonly Dictionary<string, EngineShaderResourceView> globalResources = [];
        /// <summary>
        /// Creating resources flag
        /// </summary>
        private bool allocating = false;

        /// <summary>
        /// Gets wheter the resource manager has requests to process or not
        /// </summary>
        public bool HasRequests
        {
            get
            {
                return !requestedResources.IsEmpty;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~GameResourceManager()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
        public void CreateResources(string id, IProgress<LoadResourceProgress> progress = null)
        {
            if (allocating)
            {
                return;
            }

            try
            {
                allocating = true;

                var pendingRequests = requestedResources.ToArray();
                if (pendingRequests.Length == 0)
                {
                    return;
                }

                Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Processing resource requests: {pendingRequests.Length}");

                ProcessPendingRequests(id, pendingRequests, progress);

                Logger.WriteTrace(this, $"Loading Group {id ?? "no-id"} => Resource requests processed: {pendingRequests.Length}");
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Loading Group {id ?? "no-id"} => Error creating resources: {ex.Message}", ex);

                throw;
            }
            finally
            {
                allocating = false;
            }
        }
        /// <summary>
        /// Process pending request list
        /// </summary>
        /// <param name="id">Load group id</param>
        /// <param name="progress">Progress helper</param>
        /// <param name="pendingRequests">Pending request list</param>
        private void ProcessPendingRequests(string id, IEnumerable<KeyValuePair<string, IGameResourceRequest>> pendingRequests, IProgress<LoadResourceProgress> progress = null)
        {
            // Get pending requests
            float total = pendingRequests.Count() + 1;
            float current = 0;

            // Process requests
            var toRemove = new List<string>();
            try
            {
                foreach (var resource in pendingRequests)
                {
                    resource.Value.Create(game);

                    if (resources.ContainsKey(resource.Key))
                    {
                        // Updates existing request
                        resources[resource.Key] = resource.Value.ResourceView;
                    }
                    else
                    {
                        // Adds the request
                        resources.Add(resource.Key, resource.Value.ResourceView);
                    }

                    // Adds the key to the processed key list
                    toRemove.Add(resource.Key);

                    progress?.Report(new LoadResourceProgress { Id = id, Progress = ++current / total });
                }
            }
            finally
            {
                // Remove requests
                RemoveRequests(toRemove);
            }

            progress?.Report(new LoadResourceProgress { Id = id, Progress = 1f });
        }
        /// <summary>
        /// Removes the specified requests keys
        /// </summary>
        /// <param name="requestKeys">Request keys list</param>
        private void RemoveRequests(IEnumerable<string> requestKeys)
        {
            var toRemove = requestKeys.ToList();

            while (toRemove.Count != 0)
            {
                if (!requestedResources.ContainsKey(toRemove[0]))
                {
                    toRemove.RemoveAt(0);

                    continue;
                }

                if (requestedResources.TryRemove(toRemove[0], out _))
                {
                    toRemove.RemoveAt(0);
                }
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
            var resource = new GameResourceValueArray<T>(name)
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
            var resource = new GameResourceRandomTexture(name)
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
            if (globalResources.TryGetValue(name, out var value))
            {
                var cRes = value;
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
        public EngineShaderResourceView RequestResource(IImageContent imageContent, bool mipAutogen = true, bool dynamic = false)
        {
            var (key, resource) = TryGetResource(imageContent);
            if (resource != null)
            {
                return resource;
            }

            if (requestedResources.TryGetValue(key, out var value))
            {
                return value.ResourceView;
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
        public EngineShaderResourceView RequestResource(string path, bool mipAutogen = true, bool dynamic = false)
        {
            return RequestResource(new FileImageContent(path), mipAutogen, dynamic);
        }
        /// <summary>
        /// Requests a new resource load
        /// </summary>
        /// <param name="stream">Data stream</param>
        /// <param name="mipAutogen">Try to generate texture mips</param>
        /// <param name="dynamic">Generates a writable texture</param>
        /// <returns>Returns the engine shader resource view</returns>
        public EngineShaderResourceView RequestResource(MemoryStream stream, bool mipAutogen = true, bool dynamic = false)
        {
            return RequestResource(new MemoryImageContent(stream), mipAutogen, dynamic);
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
        public EngineShaderResourceView RequestResource<T>(Guid identifier, IEnumerable<T> values, int size, bool dynamic = false) where T : struct
        {
            var (key, resource) = TryGetResource(identifier);
            if (resource != null)
            {
                return resource;
            }

            if (requestedResources.TryGetValue(key, out var value))
            {
                return value.ResourceView;
            }

            var request = new GameResourceValueArray<T>(identifier.ToString("B"))
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
        public EngineShaderResourceView RequestResource(Guid identifier, int size, float min, float max, int seed = 0, bool dynamic = false)
        {
            var (key, resource) = TryGetResource(identifier);
            if (resource != null)
            {
                return resource;
            }

            if (requestedResources.TryGetValue(key, out var value))
            {
                return value.ResourceView;
            }

            var request = new GameResourceRandomTexture(identifier.ToString("B"))
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
        private (string Key, EngineShaderResourceView Resource) TryGetResource(IImageContent imageContent)
        {
            string key = imageContent?.GetResourceKey();
            if (key == null)
            {
                return (key, null);
            }

            if (!resources.TryGetValue(key, out var resource))
            {
                return (key, null);
            }

            return (key, resource);
        }
        /// <summary>
        /// Trys to get a resource by identifier
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <returns>Returns the resource if exists</returns>
        private (string Key, EngineShaderResourceView Resource) TryGetResource(Guid identifier)
        {
            string key = identifier.ToByteArray().GetMd5Sum();
            if (key == null)
            {
                return (key, null);
            }

            if (!resources.TryGetValue(key, out var resource))
            {
                return (key, null);
            }

            return (key, resource);
        }
    }
}
