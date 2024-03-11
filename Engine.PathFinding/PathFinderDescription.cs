using System;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    /// <summary>
    /// Path finder grid description
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="settings">Settings</param>
    /// <param name="input">Geometry input</param>
    public class PathFinderDescription(PathFinderSettings settings, PathFinderInput input)
    {
        /// <summary>
        /// Graph type
        /// </summary>
        public PathFinderSettings Settings { get; protected set; } = settings ?? throw new ArgumentNullException(nameof(settings));
        /// <summary>
        /// Path finder input
        /// </summary>
        public PathFinderInput Input { get; protected set; } = input ?? throw new ArgumentNullException(nameof(input));

        /// <summary>
        /// Builds a graph from this settings
        /// </summary>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the generated graph</returns>
        public async Task<IGraph> BuildAsync(Action<float> progressCallback = null)
        {
            return await Input.CreateGraphAsync(Settings, progressCallback);
        }
        /// <summary>
        /// Builds a graph from this settings
        /// </summary>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the generated graph</returns>
        public IGraph Build(Action<float> progressCallback = null)
        {
            return Input.CreateGraph(Settings, progressCallback);
        }

        /// <summary>
        /// Gets the path finder hash
        /// </summary>
        /// <returns>Returns the path finder hash</returns>
        public async Task<string> GetHashAsync()
        {
            return await Input.GetHashAsync(Settings);
        }
        /// <summary>
        /// Gets the path finder hash
        /// </summary>
        /// <returns>Returns the path finder hash</returns>
        public string GetHash()
        {
            return Input.GetHash(Settings);
        }
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="hash">Source hash</param>
        /// <returns>Returns the loaded graph</returns>
        public async Task<IGraph> LoadAsync(string fileName, string hash = null)
        {
            return await Input.LoadAsync(fileName, hash);
        }
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="hash">Source hash</param>
        /// <returns>Returns the loaded graph</returns>
        public IGraph Load(string fileName, string hash = null)
        {
            return Input.Load(fileName, hash);
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public async Task SaveAsync(string fileName, IGraph graph)
        {
            await Input.SaveAsync(fileName, graph);
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public void Save(string fileName, IGraph graph)
        {
            Input.Save(fileName, graph);
        }
    }
}
