using SharpDX;
using System;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Contour vertex
    /// </summary>
    public struct ContourVertex : IEquatable<ContourVertex>
    {
        /// <summary>
        /// Converts the specified collection
        /// </summary>
        public static Int3[] ToInt3List(ContourVertex[] v)
        {
            return v?.Select(v => new Int3(v.X, v.Y, v.Z)).ToArray() ?? Array.Empty<Int3>();
        }

        /// <summary>
        /// X
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// Y
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// Z
        /// </summary>
        public int Z { get; set; }
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
        public int Flag { get; set; }
        /// <summary>
        /// Gets the flag's stored direction
        /// </summary>
        public readonly int Dir
        {
            get
            {
                return IndexedPolygon.GetVertexDirection(Flag);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContourVertex(int x, int y, int z, int flag)
        {
            X = x;
            Y = y;
            Z = z;
            Flag = flag;
        }

        /// <summary>
        /// Gets whether the vertex has stored a direction in the flag or not
        /// </summary>
        public readonly bool HasDirection()
        {
            return IndexedPolygon.VertexHasDirection(Flag);
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
            return ((X * 397 ^ Y) * 397 ^ Z) * 397 ^ Flag;
        }

        public static bool operator ==(ContourVertex left, ContourVertex right)
        {
            return left.Equals(ref right);
        }
        public static bool operator !=(ContourVertex left, ContourVertex right)
        {
            return !left.Equals(ref right);
        }
    }
}
