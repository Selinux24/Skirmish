using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Contour vertex
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public struct ContourVertex(int x, int y, int z, int flag) : IEquatable<ContourVertex>
    {
        /// <summary>
        /// Converts the specified collection
        /// </summary>
        public static Int3[] ToInt3List(ContourVertex[] v)
        {
            return v?.Select(v => new Int3(v.X, v.Y, v.Z)).ToArray() ?? [];
        }

        /// <summary>
        /// X
        /// </summary>
        public int X { get; set; } = x;
        /// <summary>
        /// Y
        /// </summary>
        public int Y { get; set; } = y;
        /// <summary>
        /// Z
        /// </summary>
        public int Z { get; set; } = z;
        /// <summary>
        /// Gets the x, y and z coordinates
        /// </summary>
        public readonly Int3 Coords
        {
            get
            {
                return new Int3(X, Y, Z);
            }
        }
        /// <summary>
        /// Contour flag
        /// </summary>
        public int Flag { get; set; } = flag;
        /// <summary>
        /// Gets the vertex position
        /// </summary>
        public readonly Int3 Position
        {
            get
            {
                return new(X, Y, Z);
            }
        }

        /// <inheritdoc/>
        public readonly bool Equals(ref ContourVertex other)
        {
            if (other.X == X && other.Y == Y && other.Z == Z)
            {
                return other.Flag == Flag;
            }

            return false;
        }
        /// <inheritdoc/>
        public readonly bool Equals(ContourVertex other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not ContourVertex)
            {
                return false;
            }

            ContourVertex other = (ContourVertex)obj;
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, Flag);
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"X: {X}; Y: {Y}; Z: {Z}; Flag: {Flag};";
        }

        /// <inheritdoc/>
        public static bool operator ==(ContourVertex left, ContourVertex right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(ContourVertex left, ContourVertex right)
        {
            return !left.Equals(ref right);
        }
    }
}
