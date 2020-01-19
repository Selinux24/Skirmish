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
            return await this.Input?.CreateGraph(this.Settings);
        }

        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the loaded graph</returns>
        public async Task<IGraph> Load(string filename)
        {
            return await Input.Load(filename);
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public async Task Save(string filename, IGraph graph)
        {
            await Input.Save(filename, graph);
        }
    }
}
