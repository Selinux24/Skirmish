using System;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.Helpers;

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
        public InputGeometry(Func<Triangle[]> fnc) : base(fnc)
        {
            
        }

        /// <summary>
        /// Creates a new graph from current geometry input
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Returns the new graph</returns>
        public override IGraph CreateGraph(PathFinderSettings settings)
        {
            // Prepare input data
            this.ChunkyMesh = ChunkyTriMesh.Build(this);

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
                var mmQuery = new NavMeshQuery();
                mmQuery.Init(nm, graph.Settings.MaxNodes);

                graph.MeshQueryDictionary.Add(agent, mmQuery);
            }

            return graph;
        }
        /// <summary>
        /// Refresh
        /// </summary>
        public override void Refresh()
        {
            // Recreate the input data
            this.ChunkyMesh = ChunkyTriMesh.Build(this);
        }

        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public override IGraph Load(string fileName)
        {
            // Initialize the input data
            this.ChunkyMesh = ChunkyTriMesh.Build(this);

            // Load file
            var graph = GraphFile.Load(fileName);

            // Set input data
            graph.Input = this;

            return graph;
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Instance to save</param>
        public override void Save(string fileName, IGraph graph)
        {
            if (graph is Graph nmGraph)
            {
                GraphFile.Save(fileName, nmGraph);
            }
            else
            {
                throw new EngineException(string.Format("Bad navigation mesh graph type: {0}", graph.GetType()));
            }
        }
    }
}
