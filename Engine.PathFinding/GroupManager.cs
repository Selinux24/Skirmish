using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Group manager
    /// </summary>
    public class GroupManager<TAgentParameters>()
        where TAgentParameters : IGroupAgentSettings
    {
        /// <summary>
        /// Group list
        /// </summary>
        private readonly List<IGroup<TAgentParameters>> groups = [];

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="group">Group</param>
        public void Add<TGroup>(TGroup group) where TGroup : IGroup<TAgentParameters>
        {
            groups.Add(group);
        }
        /// <summary>
        /// Updates state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(IGameTime gameTime)
        {
            foreach (var group in groups)
            {
                group.Update(gameTime);
            }
        }
    }
}
