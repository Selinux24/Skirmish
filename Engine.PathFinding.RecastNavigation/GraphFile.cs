using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using System.Linq;

    /// <summary>
    /// Graph file
    /// </summary>
    [Serializable]
    public struct GraphFile
    {
        /// <summary>
        /// Calculates a hash string from a list of triangles
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns the hash string</returns>
        public static string GetHash(PathFinderSettings settings, IEnumerable<Triangle> triangles)
        {
            List<byte> buffer = new List<byte>();

            var tris = triangles.ToList();
            tris.Sort((t1, t2) =>
            {
                return t1.GetHashCode().CompareTo(t2.GetHashCode());
            });

            var serTris = tris
                .Select(t => new[]
                {
                    t.Point1.X, t.Point1.Y, t.Point1.Z,
                    t.Point2.X, t.Point2.Y, t.Point2.Z,
                    t.Point3.X, t.Point3.Y, t.Point3.Z,
                })
                .ToArray();

            buffer.AddRange(serTris.SerializeJson());
            buffer.AddRange(settings.SerializeJson());

            return buffer.ToArray().GetMd5Sum();
        }

        /// <summary>
        /// Creates a graph file from a graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <returns>Returns the graph file</returns>
        public static async Task<GraphFile> FromGraph(Graph graph)
        {
            //Calculate hash
            var tris = await graph.Input.GetTriangles();
            string hash = GetHash(graph.Settings, tris);

            var meshFileDict = new Dictionary<Agent, NavMeshFile>();

            foreach (var agentQ in graph.AgentQueries)
            {
                var nm = agentQ.NavMesh;

                var rcFile = NavMeshFile.FromNavmesh(nm);

                meshFileDict.Add(agentQ.Agent, rcFile);
            }

            return new GraphFile()
            {
                Settings = graph.Settings,
                Dictionary = meshFileDict,
                Hash = hash,
            };
        }
        /// <summary>
        /// Creates a graph from a graph file
        /// </summary>
        /// <param name="file">Graph file</param>
        /// <param name="inputGeometry">Input geometry</param>
        /// <returns>Returns the graph</returns>
        public static async Task<Graph> FromGraphFile(GraphFile file, InputGeometry inputGeometry)
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
                Input = inputGeometry,
                Initialized = true,
            };
        }
        /// <summary>
        /// Loads the graph file from a file name
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the graph file</returns>
        public static async Task<GraphFile> Load(string fileName)
        {
            byte[] buffer = File.ReadAllBytes(fileName);

            try
            {
                return await Task.FromResult(buffer.Decompress<GraphFile>());
            }
            catch (Exception ex)
            {
                throw new EngineException("Error loading the graph from a file.", ex);
            }
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph</param>
        public static async Task Save(string fileName, Graph graph)
        {
            var file = await FromGraph(graph);

            try
            {
                byte[] buffer = file.Compress();

                File.WriteAllBytes(fileName, buffer);
            }
            catch (Exception ex)
            {
                throw new EngineException("Error saving the graph to a file.", ex);
            }
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
    }
}
