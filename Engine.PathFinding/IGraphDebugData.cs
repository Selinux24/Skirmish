using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph debug data interface
    /// </summary>
    public interface IGraphDebugData
    {
        /// <summary>
        /// Gets the debug data value list
        /// </summary>
        IEnumerable<(string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data)> GetValues();
        /// <summary>
        /// Gets the debug data value list by name
        /// </summary>
        /// <param name="name">Value name</param>
        (string Name, Topology Topology, Dictionary<Color4, IEnumerable<Vector3>> Data) GetValueByName(string name);
    }
}
