using System;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
    /// A 2d vector represented by integers.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Vector2Int"/> struct with a specified coordinate.
    /// </remarks>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2Int(int x, int y) : IEquatable<Vector2Int>
    {
        /// <summary>
		/// A vector where both X and Y are <see cref="int.MinValue"/>.
		/// </summary>
		public static readonly Vector2Int Min = new(int.MinValue, int.MinValue);
        /// <summary>
        /// A vector where both X and Y are <see cref="int.MaxValue"/>.
        /// </summary>
        public static readonly Vector2Int Max = new(int.MaxValue, int.MaxValue);
        /// <summary>
        /// A vector where both X and Y are 0.
        /// </summary>
        public static readonly Vector2Int Zero = new(0, 0);

        /// <summary>
        /// Compares two instances of <see cref="Vector2Int"/> for equality.
        /// </summary>
        /// <param name="left">An instance of <see cref="Vector2Int"/>.</param>
        /// <param name="right">Another instance of <see cref="Vector2Int"/>.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public static bool operator ==(Vector2Int left, Vector2Int right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares two instances of <see cref="Vector2Int"/> for inequality.
        /// </summary>
        /// <param name="left">An instance of <see cref="Vector2Int"/>.</param>
        /// <param name="right">Another instance of <see cref="Vector2Int"/>.</param>
        /// <returns>A value indicating whether the two instances are unequal.</returns>
        public static bool operator !=(Vector2Int left, Vector2Int right)
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

        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            Vector2Int? objV = obj as Vector2Int?;
            if (objV != null)
            {
                return Equals(objV);
            }

            return false;
        }
        /// <inheritdoc/>
        public readonly bool Equals(Vector2Int other)
        {
            return X == other.X && Y == other.Y;
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"X: {X}; Y: {Y}";
        }
    }
}
