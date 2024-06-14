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

        /// <inheritdoc/>
        public void Add<TGroup>(TGroup group) where TGroup : IGroup<TAgentParameters>
        {
            groups.Add(group);
        }
        /// <inheritdoc/>
        public void Update(IGameTime gameTime)
        {
            foreach (var group in groups)
            {
                group.Update(gameTime);
            }
        }
    }
}
