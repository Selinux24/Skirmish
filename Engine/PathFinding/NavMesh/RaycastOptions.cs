using System;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
	/// Options for raycasting.
	/// </summary>
	[Flags]
    public enum RaycastOptions
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Calculate and use movement costs across the ray.
        /// </summary>
        UseCosts = 0x01,
    }
}
