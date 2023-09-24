using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

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
            List<byte> buffer = new();

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

            var meshFiles = graph.GetAgents().Select(a => (a.Agent, NavMeshFile.FromNavmesh(a.NavMesh))).ToList();

            return new GraphFile()
            {
                Settings = graph.Settings,
                GraphList = meshFiles,
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
            var graph = new Graph
            {
                Settings = file.Settings,
                Input = inputGeometry,
                Initialized = true,
            };

            await Task.Run(() =>
            {
                foreach (var agentData in file.GraphList)
                {
                    var agent = agentData.Agent;
                    var navMesh = NavMeshFile.FromNavmeshFile(agentData.NavMesh);

                    graph.AddAgent(agent, navMesh);
                }
            });

            return graph;
        }
        /// <summary>
        /// Loads the graph file from a file name
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the graph file</returns>
        public static async Task<GraphFile> Load(string fileName)
        {
            try
            {
                var buffer = await File.ReadAllBytesAsync(fileName);

                return buffer.Decompress<GraphFile>();
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(GraphFile), "Error loading the graph from a file.", ex);

                throw;
            }
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph</param>
        public static async Task Save(string fileName, Graph graph)
        {
            try
            {
                var graphFile = await FromGraph(graph);

                var buffer = graphFile.Compress();

                await File.WriteAllBytesAsync(fileName, buffer);
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(GraphFile), "Error saving the graph to a file.", ex);

                throw;
            }
        }

        /// <summary>
        /// Graph settings
        /// </summary>
        public BuildSettings Settings { get; set; }
        /// <summary>
        /// Graph list
        /// </summary>
        public List<(Agent Agent, NavMeshFile NavMesh)> GraphList { get; set; }
        /// <summary>
        /// File source hash
        /// </summary>
        public string Hash { get; set; }
    }
}
