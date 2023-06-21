using System;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
	/// A 2d vector represented by integers.
	/// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        /// <summary>
		/// A vector where both X and Y are <see cref="int.MinValue"/>.
		/// </summary>
		public static readonly Vector2Int Min = new Vector2Int(int.MinValue, int.MinValue);
        /// <summary>
        /// A vector where both X and Y are <see cref="int.MaxValue"/>.
        /// </summary>
        public static readonly Vector2Int Max = new Vector2Int(int.MaxValue, int.MaxValue);
        /// <summary>
        /// A vector where both X and Y are 0.
        /// </summary>
        public static readonly Vector2Int Zero = new Vector2Int(0, 0);

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
        public int X { get; set; }
        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2Int"/> struct with a specified coordinate.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Checks for equality between this instance and a specified object.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating whether this instance and the object are equal.</returns>
        public override readonly bool Equals(object obj)
        {
            Vector2Int? objV = obj as Vector2Int?;
            if (objV != null)
            {
                return Equals(objV);
            }

            return false;
        }
        /// <summary>
        /// Checks for equality between this instance and a specified instance of <see cref="Vector2Int"/>.
        /// </summary>
        /// <param name="other">An instance of <see cref="Vector2Int"/>.</param>
        /// <returns>A value indicating whether this instance and the other instance are equal.</returns>
        public readonly bool Equals(Vector2Int other)
        {
            return X == other.X && Y == other.Y;
        }
        /// <summary>
        /// Gets a unique hash code for this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        /// <summary>
        /// Turns the instance into a human-readable string.
        /// </summary>
        /// <returns>A string representing the instance.</returns>
        public override readonly string ToString()
        {
            return $"X: {X}; Y: {Y}";
        }
    }
}
