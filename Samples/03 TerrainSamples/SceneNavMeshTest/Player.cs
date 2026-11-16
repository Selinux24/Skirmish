using Engine.Mathematics;
using Engine.PathFinding.RecastNavigation;
using System;

namespace TerrainSamples.SceneNavMeshTest
{
    [Serializable]
    public class Player : GraphAgentType
    {
        public float Velocity { get; set; }
        public float VelocitySlow { get; set; }

        public Player() : base()
        {
            Velocity = 4f;
            VelocitySlow = 1f;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode, Velocity, VelocitySlow);
        }
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;

            if (obj is Player other)
            {
                return
                    MathUtils.NearEqual(other.Velocity, Velocity) &&
                    MathUtils.NearEqual(other.VelocitySlow, VelocitySlow);
            }

            return false;
        }
    }
}
