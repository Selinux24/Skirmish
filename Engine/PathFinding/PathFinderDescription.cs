
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
        public IGraph Build()
        {
            return this.Input?.CreateGraph(this.Settings);
        }

        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the loaded graph</returns>
        public IGraph Load(string filename)
        {
            return Input.Load(filename);
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public void Save(string filename, IGraph graph)
        {
            Input.Save(filename, graph);
        }
    }
}
