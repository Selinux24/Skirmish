using System;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
	/// Describes the current state of a crowd agent
	/// </summary>
	[Flags]
    public enum AgentState
    {
        /// <summary>
        /// Not in any state
        /// </summary>
        Invalid,
        /// <summary>
        /// Walking on the navigation mesh
        /// </summary>
        Walking,
        /// <summary>
        /// Handling an offmesh connection
        /// </summary>
        Offmesh
    }
}
