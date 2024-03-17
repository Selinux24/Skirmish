using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Debug data
    /// </summary>
    public class GraphDebugData(IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> data) : IGraphDebugData
    {
        /// <summary>
        /// Debug data collection
        /// </summary>
        private readonly List<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>>)> data = new(data);

        /// <inheritdoc/>
        public (string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data) GetValueByName(string name)
        {
            return data.Find(d => d.Name == name);
        }

        /// <inheritdoc/>
        public IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetValues()
        {
            return [.. data];
        }
    }
}
