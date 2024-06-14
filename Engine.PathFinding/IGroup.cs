using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Group interface
    /// </summary>
    /// <typeparam name="TAgent">Agent type</typeparam>
    /// <typeparam name="TGroupAgent">Group agent type</typeparam>
    public interface IGroup<TAgentSettings> where TAgentSettings : IGroupAgentSettings
    {
        /// <summary>
        /// Adds a new agent to the group.
        /// </summary>
        /// <param name="pos">Agent position</param>
        /// <returns>Returns the new agent id</returns>
        int AddAgent(Vector3 pos);
        /// <summary>
        /// Removes the agent from the group.
        /// </summary>
        /// <param name="id">Agent id to remove</param>
        void RemoveAgent(int id);

        /// <summary>
        /// Gets the agent count
        /// </summary>
        int Count();

        /// <summary>
        /// Gets a list of the agent positions in the group.
        /// </summary>
        (int Id, Vector3 Position)[] GetPositions();
        /// <summary>
        /// Gets the agent position.
        /// </summary>
        /// <param name="id">Agent id</param>
        Vector3 GetPosition(int id);

        /// <summary>
        /// Updates group settings
        /// </summary>
        /// <param name="settings">Agent settings</param>
        void UpdateSettings(TAgentSettings settings);
        /// <summary>
        /// Updates group settings
        /// </summary>
        /// <param name="id">Agent id</param>
        /// <param name="settings">Agent settings</param>
        void UpdateSettings(int id, TAgentSettings settings);

        /// <summary>
        /// Gets the filter used by the group.
        /// </summary>
        /// <param name="i">Filter index</param>
        /// <returns>The filter used by the group.</returns>
        IGraphQueryFilter GetFilter(int i);
        /// <summary>
        /// Sets the filter for the specified index.
        /// </summary>
        /// <param name="i">The index</param>
        /// <param name="filter">The new filter</param>
        void SetFilter(int i, IGraphQueryFilter filter);

        /// <summary>
        /// Submits a new move request for all agents in the group.
        /// </summary>
        /// <param name="pos">New target</param>
        void RequestMove(Vector3 pos);
        /// <summary>
        /// Submits a new move request for the specified agent.
        /// </summary>
        /// <param name="id">Agent id</param>
        /// <param name="pos">New target</param>
        void RequestMove(int id, Vector3 pos);

        /// <summary>
        /// Updates the steering and positions of all agents.
        /// </summary>
        /// <param name="gameTime">The time to update the simulation. [Limit: > 0]</param>
        void Update(IGameTime gameTime);
    }
}
