using System;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Agent description
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Constructor
        /// </summary>
        public Agent() : base()
        {
            Radius = 0.6f;
            MaxClimb = 0.9f;
            MaxSlope = 45.0f;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;

            if (obj is Agent other)
            {
                return
                    other.Radius == Radius &&
                    other.MaxClimb == MaxClimb &&
                    other.MaxSlope == MaxSlope;
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
