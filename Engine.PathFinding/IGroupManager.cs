
namespace Engine.PathFinding
{
    /// <summary>
    /// Group manager interface
    /// </summary>
    /// <typeparam name="TAgent">Agent type</typeparam>
    /// <typeparam name="TGroupAgent">Group agent type</typeparam>
    public interface IGroupManager<TAgentParameters>
        where TAgentParameters : IGroupAgentSettings
    {
        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="group">Group</param>
        void Add<TGroup>(TGroup group) where TGroup : IGroup<TAgentParameters>;
        /// <summary>
        /// Updates state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(IGameTime gameTime);
    }
}
