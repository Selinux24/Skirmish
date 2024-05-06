using System;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
    /// A 3d vector represented by integers.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Vector3Int"/> struct.
    /// </remarks>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3Int(int x, int y, int z) : IEquatable<Vector3Int>
    {
        /// <summary>
        /// Calculates the component-wise minimum of two vertices.
        /// </summary>
        /// <param name="a">A vertex.</param>
        /// <param name="b">Another vertex.</param>
        /// <returns>The component-wise minimum of the two vertices.</returns>
        public static Vector3Int ComponentMin(Vector3Int a, Vector3Int b)
        {
            var x = a.X < b.X ? a.X : b.X;
            var y = a.Y < b.Y ? a.Y : b.Y;
            var z = a.Z < b.Z ? a.Z : b.Z;

            return new Vector3Int(x, y, z);
        }
        /// <summary>
        /// Calculates the component-wise maximum of two vertices.
        /// </summary>
        /// <param name="a">A vertex.</param>
        /// <param name="b">Another vertex.</param>
        /// <returns>The component-wise maximum of the two vertices.</returns>
        public static Vector3Int ComponentMax(Vector3Int a, Vector3Int b)
        {
            var x = a.X > b.X ? a.X : b.X;
            var y = a.Y > b.Y ? a.Y : b.Y;
            var z = a.Z > b.Z ? a.Z : b.Z;

            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Compares two vertices for equality.
        /// </summary>
        /// <param name="left">A vertex.</param>
        /// <param name="right">Another vertex.</param>
        /// <returns>A value indicating whether the two vertices are equal.</returns>
        public static bool operator ==(Vector3Int left, Vector3Int right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares two vertices for inequality.
        /// </summary>
        /// <param name="left">A vertex.</param>
        /// <param name="right">Another vertex.</param>
        /// <returns>A value indicating whether the two vertices are not equal.</returns>
        public static bool operator !=(Vector3Int left, Vector3Int right)
        {
            return !(left == right);
        }

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public int X { get; set; } = x;
        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y { get; set; } = y;
        /// <summary>
        /// The Z coordinate.
        /// </summary>
        public int Z { get; set; } = z;

        /// <inheritdoc/>
        public readonly bool Equals(Vector3Int other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            Vector3Int? p = obj as Vector3Int?;
            if (p.HasValue)
            {
                return Equals(p.Value);
            }

            return false;
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"X: {X}; Y: {Y}; Z: {Z}";
        }
    }
}
