using System;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    /// <summary>
    /// Path finder grid description
    /// </summary>
    public class PathFinderDescription
    {
        /// <summary>
        /// Graph type
        /// </summary>
        public PathFinderSettings Settings { get; protected set; }
        /// <summary>
        /// Path finder input
        /// </summary>
        public PathFinderInput Input { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="input">Geometry input</param>
        public PathFinderDescription(PathFinderSettings settings, PathFinderInput input)
        {
            Settings = settings;
            Input = input;
        }

        /// <summary>
        /// Builds a graph from this settings
        /// </summary>
        /// <returns>Returns the generated graph</returns>
        public async Task<IGraph> Build()
        {
            IGraph result;

            try
            {
                result = await Input?.CreateGraph(Settings);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Error creating the graph: {ex.Message}", ex);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Gets the path finder hash
        /// </summary>
        /// <returns>Returns the path finder hash</returns>
        public async Task<string> GetHash()
        {
            return await Input.GetHash(Settings);
        }
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="hash">Source hash</param>
        /// <returns>Returns the loaded graph</returns>
        public async Task<IGraph> Load(string fileName, string hash = null)
        {
            return await Input.Load(fileName, hash);
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public async Task Save(string fileName, IGraph graph)
        {
            await Input.Save(fileName, graph);
        }
    }
}
