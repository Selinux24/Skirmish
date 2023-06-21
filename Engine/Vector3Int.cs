using System;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
	/// A 3d vector represented by integers.
	/// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3Int : IEquatable<Vector3Int>
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
        public int X { get; set; }
        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// The Z coordinate.
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector3Int"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Vector3Int(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Compares another <see cref="Vector3Int"/> with this instance for equality.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>A value indicating whether the two vertices are equal.</returns>
        public readonly bool Equals(Vector3Int other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        /// <summary>
        /// Compares an object with this instance for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating whether the object is equal to this instance.</returns>
        public override readonly bool Equals(object obj)
        {
            Vector3Int? p = obj as Vector3Int?;
            if (p.HasValue)
            {
                return Equals(p.Value);
            }

            return false;
        }
        /// <summary>
        /// Gets a hash code unique to the contents of this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
        /// <summary>
        /// Gets a human-readable version of the vertex.
        /// </summary>
        /// <returns>A string.</returns>
        public override readonly string ToString()
        {
            return $"X: {X}; Y: {Y}; Z: {Z}";
        }
    }
}
