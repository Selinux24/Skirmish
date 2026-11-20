using System;
using System.Collections.Generic;

namespace Engine.Physics
{
    /// <summary>
    /// Contact pair
    /// </summary>
    /// <remarks>Equality is ordered; reverse pairs are checked explicitly by the simulator.</remarks>
    public struct ContactPair() : IEquatable<ContactPair>
    {
        /// <summary>
        /// First physics object
        /// </summary>
        public IPhysicsObject Obj1 { get; set; }
        /// <summary>
        /// Second physics object
        /// </summary>
        public IPhysicsObject Obj2 { get; set; }

        /// <inheritdoc/>
        public readonly bool Equals(ContactPair other)
        {
            return EqualityComparer<IPhysicsObject>.Default.Equals(Obj1, other.Obj1)
                && EqualityComparer<IPhysicsObject>.Default.Equals(Obj2, other.Obj2);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is ContactPair other && Equals(other);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Obj1, Obj2);
        }

        /// <inheritdoc/>
        public static bool operator ==(ContactPair left, ContactPair right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(ContactPair left, ContactPair right)
        {
            return !(left == right);
        }
    }
}
