
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Agent description
    /// </summary>
    public class Agent : AgentType
    {
        /// <summary>
        /// Default agent
        /// </summary>
        public static Agent Default
        {
            get
            {
                return new Agent()
                {
                    Name = "Default",
                    Height = 2.0f,
                    Radius = 0.6f,
                    MaxClimb = 0.9f,
                    MaxSlope = 45.0f,
                };
            }
        }

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
