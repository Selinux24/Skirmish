using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;

    /// <summary>
    /// Graph file
    /// </summary>
    [Serializable]
    public struct GraphFile : ISerializable
    {
        /// <summary>
        /// Navigation mesh file
        /// </summary>
        [Serializable]
        public struct NavMeshFile
        {
            /// <summary>
            /// Creates a navigation mesh file from a navigation mesh
            /// </summary>
            /// <param name="navmesh">Navigation mesh</param>
            /// <returns>Returns the navigation mesh file</returns>
            public static NavMeshFile FromNavmesh(NavMesh navmesh)
            {
                NavMeshFile file = new NavMeshFile
                {
                    NavMeshParams = navmesh.GetParams(),
                    NavMeshData = new List<MeshData>(),

                    HasTileCache = navmesh.TileCache != null,
                    TileCacheParams = navmesh.TileCache != null ? navmesh.TileCache.GetParams() : new TileCacheParams(),
                    TileCacheData = new List<TileCacheData>()
                };

                // Store navmesh tiles.
                for (int i = 0; i < navmesh.MaxTiles; ++i)
                {
                    var tile = navmesh.Tiles[i];
                    if (tile != null)
                    {
                        file.NavMeshData.Add(tile.Data);
                    }
                }

                if (navmesh.TileCache != null)
                {
                    // Store cache tiles.
                    var tileCount = navmesh.TileCache.GetTileCount();

                    for (int i = 0; i < tileCount; ++i)
                    {
                        var tile = navmesh.TileCache.GetTile(i);
                        if (tile != null)
                        {
                            file.TileCacheData.Add(new TileCacheData
                            {
                                Header = tile.Header,
                                Data = tile.Data
                            });
                        }
                    }
                }

                return file;
            }
            /// <summary>
            /// Creates a navigation mesh from a navigation mesh file
            /// </summary>
            /// <param name="file">Navigation mesh file</param>
            /// <returns>Returns the navigation mesh</returns>
            public static NavMesh FromNavmeshFile(NavMeshFile file)
            {
                NavMesh navmesh = new NavMesh();

                navmesh.Init(file.NavMeshParams);

                for (int i = 0; i < file.NavMeshData.Count; i++)
                {
                    var tile = file.NavMeshData[i];
                    if (tile == null || tile.Header.Magic != DetourUtils.DT_NAVMESH_MAGIC) continue;

                    navmesh.AddTile(tile, TileFlagTypes.DT_TILE_FREE_DATA, 0, out int res);
                }

                if (file.HasTileCache)
                {
                    var tmproc = new TileCacheMeshProcess(null);

                    navmesh.TileCache = new TileCache();
                    navmesh.TileCache.Init(file.TileCacheParams, tmproc);

                    for (int i = 0; i < file.TileCacheData.Count; i++)
                    {
                        var tile = file.TileCacheData[i];
                        if (tile.Header.Magic != DetourTileCache.DT_TILECACHE_MAGIC) continue;

                        navmesh.TileCache.AddTile(tile, CompressedTileFlagTypes.DT_COMPRESSEDTILE_FREE_DATA);
                    }
                }

                return navmesh;
            }

            /// <summary>
            /// Navigation mesh parameters
            /// </summary>
            public NavMeshParams NavMeshParams { get; set; }
            /// <summary>
            /// Mesh data
            /// </summary>
            public List<MeshData> NavMeshData { get; set; }

            /// <summary>
            /// Has tile cache
            /// </summary>
            public bool HasTileCache { get; set; }
            /// <summary>
            /// Tile cache parameters
            /// </summary>
            public TileCacheParams TileCacheParams { get; set; }
            /// <summary>
            /// Tile cache data
            /// </summary>
            public List<TileCacheData> TileCacheData { get; set; }
        }

        /// <summary>
        /// Creates a graph file from a graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <returns>Returns the graph file</returns>
        public static async Task<GraphFile> FromGraph(Graph graph)
        {
            var meshFileDict = new Dictionary<Agent, NavMeshFile>();

            foreach (var agentQ in graph.AgentQueries)
            {
                var nm = agentQ.NavMesh;

                var rcFile = NavMeshFile.FromNavmesh(nm);

                meshFileDict.Add(agentQ.Agent, rcFile);
            }

            var tris = await graph.Input.GetTriangles();
            var sourceHash = InputGeometry.GetHash(tris);

            return new GraphFile()
            {
                Settings = graph.Settings,
                Dictionary = meshFileDict,
                Hash = sourceHash,
            };
        }
        /// <summary>
        /// Creates a graph from a graph file
        /// </summary>
        /// <param name="file">Graph file</param>
        /// <returns>Returns the graph</returns>
        public static async Task<Graph> FromGraphFile(GraphFile file)
        {
            var agentQueries = new List<GraphAgentQuery>();

            await Task.Run(() =>
            {
                foreach (var agent in file.Dictionary.Keys)
                {
                    var rcFile = file.Dictionary[agent];
                    var nm = NavMeshFile.FromNavmeshFile(rcFile);

                    agentQueries.Add(new GraphAgentQuery
                    {
                        Agent = agent,
                        NavMesh = nm,
                        MaxNodes = file.Settings.MaxNodes,
                    });
                }
            });

            return new Graph
            {
                Settings = file.Settings,
                AgentQueries = agentQueries,
                Initialized = true,
            };
        }
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="hash">Source hash</param>
        /// <returns>Returns the graph</returns>
        public static async Task<Graph> Load(string fileName, string hash)
        {
            byte[] buffer = File.ReadAllBytes(fileName);

            var file = buffer.Decompress<GraphFile>();

            if (file.Hash == hash)
            {
                return await FromGraphFile(file);
            }

            return null;
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph</param>
        public static async Task Save(string fileName, Graph graph)
        {
            var file = await FromGraph(graph);

            byte[] buffer = file.Compress();

            File.WriteAllBytes(fileName, buffer);
        }

        /// <summary>
        /// Graph settings
        /// </summary>
        public BuildSettings Settings { get; set; }
        /// <summary>
        /// Graph dictionary
        /// </summary>
        public Dictionary<Agent, NavMeshFile> Dictionary { get; set; }
        /// <summary>
        /// File source hash
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        internal GraphFile(SerializationInfo info, StreamingContext context)
        {
            Settings = info.GetValue<BuildSettings>("Settings");
            Dictionary = info.GetValue<Dictionary<Agent, NavMeshFile>>("Dictionary");
            Hash = info.GetValue<string>("Hash");
        }
        /// <summary>
        /// Gets the object data for serialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Settings", Settings);
            info.AddValue("Dictionary", Dictionary);
            info.AddValue("Hash", Hash);
        }
    }
}
