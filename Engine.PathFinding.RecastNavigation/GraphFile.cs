using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Graph file
    /// </summary>
    [Serializable]
    public struct GraphFile : ISerializable
    {
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
