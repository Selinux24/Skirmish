using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Input geometry
    /// </summary>
    public class InputGeometry : PathFinderInput
    {
        /// <summary>
        /// Calculates the hash of the specified triangle list
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <returns>Returns a hash into a string</returns>
        public static string GetHash(IEnumerable<Triangle> triangles)
        {
            var serTris = triangles
                .Select(t => new[]
                {
                    t.Point1.X, t.Point1.Y, t.Point1.Z,
                    t.Point2.X, t.Point2.Y, t.Point2.Z,
                    t.Point3.X, t.Point3.Y, t.Point3.Z,
                })
                .ToArray();

            byte[] buffer = serTris.Serialize();

            return buffer.GetMd5Sum();
        }

        /// <summary>
        /// Chunky mesh
        /// </summary>
        public ChunkyTriMesh ChunkyMesh { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fnc">Triangle function</param>
        public InputGeometry(Func<IEnumerable<Triangle>> fnc) : base(fnc)
        {

        }

        /// <summary>
        /// Creates a new graph from current geometry input
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Returns the new graph</returns>
        public override async Task<IGraph> CreateGraph(PathFinderSettings settings)
        {
            var triangles = await this.GetTriangles();

            return Create(settings, triangles);
        }
        /// <summary>
        /// Creates a new graph from current geometry input
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns the new graph</returns>
        private Graph Create(PathFinderSettings settings, IEnumerable<Triangle> triangles)
        {
            // Prepare input data
            this.ChunkyMesh = ChunkyTriMesh.Build(triangles);

            // Create graph
            var graph = new Graph()
            {
                Input = this,
                Settings = settings as BuildSettings,
            };

            // Generate navigation meshes and gueries for each agent
            foreach (var agent in graph.Settings.Agents)
            {
                var nm = NavMesh.Build(this, graph.Settings, agent);

                graph.AgentQueries.Add(new GraphAgentQuery
                {
                    Agent = agent,
                    NavMesh = nm,
                    MaxNodes = graph.Settings.MaxNodes,
                });
            }

            graph.Initialized = true;

            return graph;
        }
        /// <summary>
        /// Refresh
        /// </summary>
        public override async Task Refresh()
        {
            ChunkyTriMesh mesh = null;

            var triangles = await this.GetTriangles();

            await Task.Run(() =>
            {
                // Recreate the input data
                mesh = ChunkyTriMesh.Build(triangles);
            });

            this.ChunkyMesh = mesh;
        }

        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public override async Task<IGraph> Load(string fileName)
        {
            var triangles = await this.GetTriangles();
            var sourceHash = GetHash(triangles);

            // Load file
            Graph graph = await GraphFile.Load(fileName, sourceHash);
            if (graph == null)
            {
                return null;
            }

            // Set input data
            graph.Input = this;

            // Initialize the input data
            this.ChunkyMesh = ChunkyTriMesh.Build(triangles);

            return graph;
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Instance to save</param>
        public override async Task Save(string fileName, IGraph graph)
        {
            if (graph is Graph nmGraph)
            {
                await GraphFile.Save(fileName, nmGraph);
            }
            else
            {
                throw new EngineException($"Bad navigation mesh graph type: {graph}");
            }
        }
    }
}
