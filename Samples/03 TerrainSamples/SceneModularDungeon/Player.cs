using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;

namespace TerrainSamples.SceneModularDungeon
{
    /// <summary>
    /// Player agent
    /// </summary>
    [Serializable]
    public class Player : GraphAgentType
    {
        /// <summary>
        /// Velocity
        /// </summary>
        public float Velocity { get; set; } = 4f;
        /// <summary>
        /// Slow velocity
        /// </summary>
        public float VelocitySlow { get; set; } = 1f;

        /// <summary>
        /// Constructor
        /// </summary>
        public Player() : base()
        {

        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode, Velocity, VelocitySlow);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;

            if (obj is Player other)
            {
                return
                    MathUtil.NearEqual(other.Velocity, Velocity) &&
                    MathUtil.NearEqual(other.VelocitySlow, VelocitySlow);
            }

            return false;
        }
    }
}
