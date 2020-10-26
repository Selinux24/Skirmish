using Engine.PathFinding.RecastNavigation;
using System;

namespace Collada
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
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;

            if (obj is Player other)
            {
                return
                    other.Velocity == Velocity &&
                    other.VelocitySlow == VelocitySlow;
            }

            return false;
        }
    }
}
