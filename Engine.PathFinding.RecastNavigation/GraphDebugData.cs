using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Debug data
    /// </summary>
    public class GraphDebugData(IEnumerable<GraphDebugDataCollection> data) : IGraphDebugData
    {
        /// <summary>
        /// Debug data collection
        /// </summary>
        private readonly List<GraphDebugDataCollection> data = new(data);

        /// <inheritdoc/>
        public GraphDebugDataCollection GetValueByName(string name)
        {
            return data.Find(d => d.Name == name);
        }

        /// <inheritdoc/>
        public IEnumerable<GraphDebugDataCollection> GetValues()
        {
            return [.. data];
        }
    }
}
