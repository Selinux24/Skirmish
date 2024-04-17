using SharpDX;
using System;
using System.Collections.Generic;
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
    /// <param name="agents">Agent type list</param>
    public class PathFinderDescription(PathFinderSettings settings, PathFinderInput input, AgentType[] agents)
    {
        /// <summary>
        /// Graph type
        /// </summary>
        private readonly PathFinderSettings settings = settings ?? throw new ArgumentNullException(nameof(settings));
        /// <summary>
        /// Path finder input
        /// </summary>
        private readonly PathFinderInput input = input ?? throw new ArgumentNullException(nameof(input));
        /// <summary>
        /// Agent list
        /// </summary>
        private readonly AgentType[] agents = agents ?? throw new ArgumentNullException(nameof(agents));

        /// <summary>
        /// Adds a new area to input
        /// </summary>
        /// <param name="graphArea">Area</param>
        /// <returns>Returns the area id</returns>
        public int AddArea(IGraphArea graphArea)
        {
            return input.AddArea(graphArea);
        }
        /// <summary>
        /// Gets an area by id
        /// </summary>
        /// <param name="id">Area id</param>
        /// <returns>Returns an area</returns>
        public IGraphArea GetArea(int id)
        {
            return input.GetArea(id);
        }
        /// <summary>
        /// Removes area by id
        /// </summary>
        /// <param name="id">Area id</param>
        public void RemoveArea(int id)
        {
            input.RemoveArea(id);
        }
        /// <summary>
        /// Gets area list
        /// </summary>
        /// <returns>Returns the area list</returns>
        public IGraphArea[] GetAreas()
        {
            return input.GetAreas();
        }
        /// <summary>
        /// Clears all areas
        /// </summary>
        public void ClearAreas()
        {
            input.ClearAreas();
        }

        /// <summary>
        /// Adds a new connection
        /// </summary>
        /// <param name="spos">Start position</param>
        /// <param name="epos">End position</param>
        /// <param name="rad">Point radius</param>
        /// <param name="bidir">Connection bidirectional</param>
        /// <param name="area">Connection area type</param>
        /// <param name="flags">Connection area flags</param>
        /// <returns>Returns the connection id</returns>
        public int AddConnection<T, Y>(Vector3 spos, Vector3 epos, float rad, bool bidir, T area, Y flags) where T : Enum where Y : Enum
        {
            return input.AddConnection(spos, epos, rad, bidir, area, flags);
        }
        /// <summary>
        /// Gets a connection by id
        /// </summary>
        /// <param name="id">Connection id</param>
        /// <returns>Returns a connection</returns>
        public IGraphConnection GetConnection(int id)
        {
            return input.GetConnection(id);
        }
        /// <summary>
        /// Deletes a connection by id
        /// </summary>
        /// <param name="id">Connection id</param>
        public void RemoveConnection(int id)
        {
            input.RemoveConnection(id);
        }
        /// <summary>
        /// Gets the connection list
        /// </summary>
        /// <returns>Returns the connection list</returns>
        public IEnumerable<IGraphConnection> GetConnections()
        {
            return input.GetConnections();
        }
        /// <summary>
        /// Clears all connections
        /// </summary>
        public void ClearConnections()
        {
            input.ClearConnections();
        }
        /// <summary>
        /// Gets the connection count
        /// </summary>
        /// <returns>Returns the connection count</returns>
        public int GetConnectionCount()
        {
            return input.GetConnectionCount();
        }

        /// <summary>
        /// Builds a graph from this settings
        /// </summary>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the generated graph</returns>
        public async Task<IGraph> BuildAsync(Action<float> progressCallback = null)
        {
            return await input.CreateGraphAsync(settings, agents, progressCallback);
        }
        /// <summary>
        /// Builds a graph from this settings
        /// </summary>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the generated graph</returns>
        public IGraph Build(Action<float> progressCallback = null)
        {
            return input.CreateGraph(settings, agents, progressCallback);
        }
        /// <summary>
        /// Refresh the graph input geometry
        /// </summary>
        public async Task RefreshAsync()
        {
            await input.RefreshAsync(settings);
        }
        /// <summary>
        /// Refresh the graph input geometry
        /// </summary>
        public void Refresh()
        {
            input.Refresh(settings);
        }

        /// <summary>
        /// Gets the path finder hash
        /// </summary>
        /// <returns>Returns the path finder hash</returns>
        public async Task<string> GetHashAsync()
        {
            return await input.GetHashAsync(settings);
        }
        /// <summary>
        /// Gets the path finder hash
        /// </summary>
        /// <returns>Returns the path finder hash</returns>
        public string GetHash()
        {
            return input.GetHash(settings);
        }
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="hash">Source hash</param>
        /// <returns>Returns the loaded graph</returns>
        public async Task<IGraph> LoadAsync(string fileName, string hash = null)
        {
            return await input.LoadAsync(fileName, hash);
        }
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="hash">Source hash</param>
        /// <returns>Returns the loaded graph</returns>
        public IGraph Load(string fileName, string hash = null)
        {
            return input.Load(fileName, hash);
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public async Task SaveAsync(string fileName, IGraph graph)
        {
            await input.SaveAsync(fileName, graph);
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public void Save(string fileName, IGraph graph)
        {
            input.Save(fileName, graph);
        }
    }
}
