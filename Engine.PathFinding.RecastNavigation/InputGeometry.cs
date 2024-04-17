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
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="fnc">Triangle function</param>
    public class InputGeometry(Func<IEnumerable<Triangle>> fnc) : PathFinderInput(fnc)
    {
        /// <summary>
        /// Chunky mesh
        /// </summary>
        public ChunkyTriMesh ChunkyMesh { get; private set; }

        /// <inheritdoc/>
        public override async Task<IGraph> CreateGraphAsync(PathFinderSettings settings, AgentType[] agentTypes, Action<float> progressCallback = null)
        {
            return Create(settings, agentTypes, await GetTrianglesAsync(settings.Bounds), progressCallback);
        }
        /// <inheritdoc/>
        public override IGraph CreateGraph(PathFinderSettings settings, AgentType[] agentTypes, Action<float> progressCallback = null)
        {
            return Create(settings, agentTypes, GetTriangles(settings.Bounds), progressCallback);
        }
        /// <summary>
        /// Creates a new graph from current geometry input
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="agentTypes">Agent type list</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new graph</returns>
        private Graph Create(PathFinderSettings settings, AgentType[] agentTypes, IEnumerable<Triangle> triangles, Action<float> progressCallback)
        {
            var buildSettings = settings as BuildSettings;
            ArgumentNullException.ThrowIfNull(buildSettings);

            var agentList = agentTypes?.Cast<GraphAgentType>().ToArray() ?? [];
            ArgumentOutOfRangeException.ThrowIfZero(agentList.Length);

            if (!triangles.Any())
            {
                return new()
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
                Settings = buildSettings,
            };

            // Generate navigation meshes and gueries for each agent
            var agentCount = agentList.Length;
            for (int i = 0; i < agentCount; i++)
            {
                var agent = agentList[i];

                var nm = NavMesh.Build(graph.Settings, this, agent, (progress) =>
                {
                    progressCallback?.Invoke(progress * (i + 1) / agentCount);
                });

                graph.AddAgent(agent, nm);
            }

            graph.Initialized = true;

            return graph;
        }
        /// <inheritdoc/>
        public override async Task RefreshAsync(PathFinderSettings settings)
        {
            ChunkyMesh = ChunkyTriMesh.Build(await GetTrianglesAsync(settings.Bounds));
        }
        /// <inheritdoc/>
        public override void Refresh(PathFinderSettings settings)
        {
            ChunkyMesh = ChunkyTriMesh.Build(GetTriangles(settings.Bounds));
        }

        /// <inheritdoc/>
        public override async Task<string> GetHashAsync(PathFinderSettings settings)
        {
            return GraphFile.GetHash(settings, await GetTrianglesAsync(settings.Bounds));
        }
        /// <inheritdoc/>
        public override string GetHash(PathFinderSettings settings)
        {
            return GraphFile.GetHash(settings, GetTriangles(settings.Bounds));
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
            ChunkyMesh = ChunkyTriMesh.Build(await GetTrianglesAsync(graph.Settings.Bounds));

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
            ChunkyMesh = ChunkyTriMesh.Build(GetTriangles(graph.Settings.Bounds));

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
