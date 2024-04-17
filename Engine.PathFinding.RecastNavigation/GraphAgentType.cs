using SharpDX;
using System;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Agent description
    /// </summary>
    [Serializable]
    public class GraphAgentType : AgentType
    {
        private const string agentDefaultString = "Graph-Agent";

        /// <summary>
        /// Default agent
        /// </summary>
        public static GraphAgentType Default
        {
            get
            {
                return new GraphAgentType()
                {
                    Name = agentDefaultString,
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
        public GraphAgentType() : base()
        {
            Radius = 0.6f;
            MaxClimb = 0.9f;
            MaxSlope = 45.0f;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;

            if (obj is GraphAgentType other)
            {
                return
                    MathUtil.NearEqual(other.Radius, Radius) &&
                    MathUtil.NearEqual(other.MaxClimb, MaxClimb) &&
                    MathUtil.NearEqual(other.MaxSlope, MaxSlope);
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Radius, MaxClimb, MaxSlope);
        }
    }
}
