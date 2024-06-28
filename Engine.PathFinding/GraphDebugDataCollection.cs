using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Debug data collection
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="topology">Topology</param>
    /// <param name="data">Data</param>
    /// <param name="trace">Trace text</param>
    public struct GraphDebugDataCollection(string name, Topology topology, Dictionary<Color4, IEnumerable<Vector3>> data, string trace = null)
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = name;
        /// <summary>
        /// Topology
        /// </summary>
        public Topology Topology { get; set; } = topology;
        /// <summary>
        /// Data
        /// </summary>
        public Dictionary<Color4, IEnumerable<Vector3>> Data { get; set; } = data;
        /// <summary>
        /// Trace text
        /// </summary>
        public string Trace { get; set; } = trace;
    }
}
