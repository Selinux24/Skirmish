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
        public override async Task<IGraph> CreateGraphAsync(PathFinderSettings settings, Action<float> progressCallback = null)
        {
            var triangles = await GetTrianglesAsync();

            try
            {
                return Create(settings, triangles, progressCallback);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, "Error creating the graph.", ex);

                throw;
            }
        }
        /// <inheritdoc/>
        public override IGraph CreateGraph(PathFinderSettings settings, Action<float> progressCallback = null)
        {
            var triangles = GetTriangles();

            try
            {
                return Create(settings, triangles, progressCallback);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, "Error creating the graph.", ex);

                throw;
            }
        }
        /// <summary>
        /// Creates a new graph from current geometry input
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new graph</returns>
        private Graph Create(PathFinderSettings settings, IEnumerable<Triangle> triangles, Action<float> progressCallback)
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
            var agentList = graph.Settings.Agents;
            var agentCount = agentList.Length;
            for (int i = 0; i < agentCount; i++)
            {
                var agent = agentList[i];

                var nm = NavMesh.Build(this, graph.Settings, agent, (progress) =>
                {
                    progressCallback?.Invoke(progress * (i + 1) / agentCount);
                });

                graph.AddAgent(agent, nm);
            }

            graph.Initialized = true;

            return graph;
        }
        /// <inheritdoc/>
        public override async Task RefreshAsync()
        {
            var triangles = await GetTrianglesAsync();

            try
            {
                ChunkyMesh = ChunkyTriMesh.Build(triangles);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, "Error creating the graph.", ex);

                throw;
            }
        }
        /// <inheritdoc/>
        public override void Refresh()
        {
            var triangles = GetTriangles();

            try
            {
                ChunkyMesh = ChunkyTriMesh.Build(triangles);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, "Error creating the graph.", ex);

                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<string> GetHashAsync(PathFinderSettings settings)
        {
            var triangles = await GetTrianglesAsync();

            return GraphFile.GetHash(settings, triangles);
        }
        /// <inheritdoc/>
        public override string GetHash(PathFinderSettings settings)
        {
            var triangles = GetTriangles();

            return GraphFile.GetHash(settings, triangles);
        }
        /// <inheritdoc/>
        public override async Task<IGraph> LoadAsync(string fileName, string hash = null)
        {
            // Load file
            var file = await GraphFile.LoadAsync(fileName);

            // Test hash
            if (!string.IsNullOrEmpty(hash) && file.Hash != hash)
            {
                return null;
            }

            // Create graph
            var graph = await GraphFile.FromGraphFileAsync(file, this);

            // Initialize the input data
            ChunkyMesh = ChunkyTriMesh.Build(await GetTrianglesAsync());

            return graph;
        }
        /// <inheritdoc/>
        public override IGraph Load(string fileName, string hash = null)
        {
            // Load file
            var file = GraphFile.Load(fileName);

            // Test hash
            if (!string.IsNullOrEmpty(hash) && file.Hash != hash)
            {
                return null;
            }

            // Create graph
            var graph = GraphFile.FromGraphFile(file, this);

            // Initialize the input data
            ChunkyMesh = ChunkyTriMesh.Build(GetTriangles());

            return graph;
        }
        /// <inheritdoc/>
        public override async Task SaveAsync(string fileName, IGraph graph)
        {
            if (graph is not Graph nmGraph)
            {
                throw new ArgumentException($"Bad navigation mesh graph type: {graph}", nameof(graph));
            }

            await GraphFile.SaveAsync(fileName, nmGraph);
        }
        /// <inheritdoc/>
        public override void Save(string fileName, IGraph graph)
        {
            if (graph is not Graph nmGraph)
            {
                throw new ArgumentException($"Bad navigation mesh graph type: {graph}", nameof(graph));
            }

            GraphFile.Save(fileName, nmGraph);
        }
    }
}
