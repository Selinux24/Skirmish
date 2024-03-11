using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;

namespace TerrainSamples.SceneNavMeshTest
{
    [Serializable]
    public class Player : Agent
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
                    MathUtil.NearEqual(other.Velocity, Velocity) &&
                    MathUtil.NearEqual(other.VelocitySlow, VelocitySlow);
            }

            return false;
        }
    }
}
