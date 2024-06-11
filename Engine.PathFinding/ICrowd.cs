using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Crowd interface
    /// </summary>
    /// <typeparam name="TAgent">Agent type</typeparam>
    /// <typeparam name="TCrowdAgent">Crowd agent type</typeparam>
    public interface ICrowd<out TAgent, TCrowdAgent>
        where TAgent : AgentType
        where TCrowdAgent : ICrowdAgent
    {
        /// <summary>
        /// Agent type
        /// </summary>
        public TAgent Agent { get; }

        /// <summary>
        /// Initializes the crowd graph
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <param name="settings">Crowd settings</param>
        void Initialize(IGraph graph, CrowdSettings settings);

        /// <summary>
        /// Adds a new agent to the crowd.
        /// </summary>
        /// <param name="ag">Crowd agent</param>
        /// <param name="pos">Agent position</param>
        void AddAgent(TCrowdAgent ag, Vector3 pos);
        /// <summary>
        /// Removes the agent from the crowd.
        /// </summary>
        /// <param name="ag">Agent to remove</param>
        void RemoveAgent(TCrowdAgent ag);
        /// <summary>
        /// Gets the agents int the agent pool.
        /// </summary>
        /// <returns>The collection of agents.</returns>
        TCrowdAgent[] GetAgents();
        /// <summary>
        /// Gets the active agents int the agent pool.
        /// </summary>
        /// <returns>The collection of active agents.</returns>
        TCrowdAgent[] GetActiveAgents();

        /// <summary>
        /// Gets the filter used by the crowd.
        /// </summary>
        /// <param name="i">Filter index</param>
        /// <returns>The filter used by the crowd.</returns>
        IGraphQueryFilter GetFilter(int i);
        /// <summary>
        /// Sets the filter for the specified index.
        /// </summary>
        /// <param name="i">The index</param>
        /// <param name="filter">The new filter</param>
        void SetFilter(int i, IGraphQueryFilter filter);
        /// <summary>
        /// Gets the search halfExtents [(x, y, z)] used by the crowd for query operations. 
        /// </summary>
        /// <returns>The search halfExtents used by the crowd. [(x, y, z)]</returns>
        Vector3 GetQueryHalfExtents();
        /// <summary>
        /// Same as getQueryHalfExtents. Left to maintain backwards compatibility.
        /// </summary>
        /// <returns>The search halfExtents used by the crowd. [(x, y, z)]</returns>
        Vector3 GetQueryExtents();

        /// <summary>
        /// Updates the steering and positions of all agents.
        /// </summary>
        /// <param name="gameTime">The time to update the simulation. [Limit: > 0]</param>
        void Update(IGameTime gameTime);
    }
}
