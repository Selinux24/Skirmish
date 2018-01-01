
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Agent description
    /// </summary>
    public struct Agent
    {
        /// <summary>
        /// Gets or sets the height of the agent
        /// </summary>
        public float Height { get; set; }
        /// <summary>
        /// Gets or sets the radius of the agent
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Gets or sets the maximum climb height.
        /// </summary>
        public float MaxClimb { get; set; }
        /// <summary>
        /// Gets or sets the maximum slope inclination (degrees)
        /// </summary>
        public float MaxSlope { get; set; }
    }
}
