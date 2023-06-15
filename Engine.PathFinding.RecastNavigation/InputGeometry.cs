using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Input geometry
    /// </summary>
    public class InputGeometry : PathFinderInput
    {
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

        /// <inheritdoc/>
        public override async Task<IGraph> CreateGraph(PathFinderSettings settings)
        {
            var triangles = await GetTriangles();

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
            if (!triangles.Any())
            {
                return new Graph
                {
                    Input = this,
                    Settings = settings as BuildSettings,
                };
            }

            // Prepare input data
            ChunkyMesh = ChunkyTriMesh.Build(triangles);

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
        /// <inheritdoc/>
        public override async Task Refresh()
        {
            ChunkyTriMesh mesh = null;

            var triangles = await GetTriangles();

            await Task.Run(() =>
            {
                // Recreate the input data
                mesh = ChunkyTriMesh.Build(triangles);
            });

            ChunkyMesh = mesh;
        }

        /// <inheritdoc/>
        public override async Task<string> GetHash(PathFinderSettings settings)
        {
            var tris = await GetTriangles();
            return GraphFile.GetHash(settings, tris);
        }
        /// <inheritdoc/>
        public override async Task<IGraph> Load(string fileName, string hash = null)
        {
            // Load file
            var file = await GraphFile.Load(fileName);

            // Test hash
            if (!string.IsNullOrEmpty(hash) && file.Hash != hash)
            {
                return null;
            }

            // Create graph
            Graph graph = await GraphFile.FromGraphFile(file, this);

            // Initialize the input data
            ChunkyMesh = ChunkyTriMesh.Build(await GetTriangles());

            return graph;
        }
        /// <inheritdoc/>
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
