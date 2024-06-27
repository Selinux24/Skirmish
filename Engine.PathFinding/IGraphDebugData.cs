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
        IEnumerable<GraphDebugDataCollection> GetValues();
        /// <summary>
        /// Gets the debug data value list by name
        /// </summary>
        /// <param name="name">Value name</param>
        GraphDebugDataCollection GetValueByName(string name);
    }
}
