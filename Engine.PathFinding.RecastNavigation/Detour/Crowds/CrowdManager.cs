using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour.Crowds
{
    /// <summary>
    /// Crowd manager
    /// </summary>
    public class CrowdManager() : IGroupManager<CrowdAgentSettings>
    {
        /// <summary>
        /// Crowd list
        /// </summary>
        private readonly List<IGroup<CrowdAgentSettings>> crowds = [];

        /// <inheritdoc/>
        public void Add<TCrowd>(TCrowd crowd) where TCrowd : IGroup<CrowdAgentSettings>
        {
            crowds.Add(crowd);
        }
        /// <inheritdoc/>
        public void Update(IGameTime gameTime)
        {
            foreach (var crowd in crowds)
            {
                crowd.Update(gameTime);
            }
        }
    }
}
