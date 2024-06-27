using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph debug information helper
    /// </summary>
    public interface IGraphDebug
    {
        /// <summary>
        /// Graph
        /// </summary>
        IGraph Graph { get; }
        /// <summary>
        /// Agent
        /// </summary>
        AgentType Agent { get; }

        /// <summary>
        /// Gets the available debug information
        /// </summary>
        IEnumerable<(int Id, string Information)> GetAvailableDebugInformation();
        /// <summary>
        /// Gets the debug information specified by id
        /// </summary>
        /// <param name="id">Information id</param>
        /// <param name="point">Information point to test</param>
        IGraphDebugData GetInfo(int id, Vector3 point);
    }
}
