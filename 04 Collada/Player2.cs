using Engine.PathFinding.RecastNavigation;
using System;

namespace Collada
{
    [Serializable]
    public class Player2 : Agent
    {
        public float Velocity { get; set; }
        public float VelocitySlow { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;

            if (obj is Player2 other)
            {
                return
                    other.Velocity == this.Velocity &&
                    other.VelocitySlow == this.VelocitySlow;
            }

            return false;
        }
    }
}
