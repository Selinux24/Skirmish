using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.PathFinding.RecastNavigation
{
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

        /// <summary>
        /// Creates a new graph from current geometry input
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Returns the new graph</returns>
        public override async Task<IGraph> CreateGraph(PathFinderSettings settings)
        {
            Graph graph = null;

            var triangles = await this.GetTriangles();

            await Task.Run(() =>
            {
                graph = Create(settings, triangles);
            });

            return graph;
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
            Graph graph = null;

            var triangles = await this.GetTriangles();

            await Task.Run(() =>
            {
                // Initialize the input data
                this.ChunkyMesh = ChunkyTriMesh.Build(triangles);

                // Load file
                graph = GraphFile.Load(fileName);

                // Set input data
                graph.Input = this;
            });

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
                await Task.Run(() => GraphFile.Save(fileName, nmGraph));
            }
            else
            {
                throw new EngineException(string.Format("Bad navigation mesh graph type: {0}", graph.GetType()));
            }
        }
    }
}
