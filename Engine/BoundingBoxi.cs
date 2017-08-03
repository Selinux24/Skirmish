using System;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
    /// A bounding box for vertices in a <see cref="PolyMesh"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBoxi : IEquatable<BoundingBoxi>
    {
        /// <summary>
        /// The lower bound of the bounding box.
        /// </summary>
        public Vertex3i Min;
        /// <summary>
        /// The upper bound of the bounding box.
        /// </summary>
        public Vertex3i Max;

        /// <summary>
        /// Checks whether two boudning boxes are intersecting.
        /// </summary>
        /// <param name="a">The first bounding box.</param>
        /// <param name="b">The second bounding box.</param>
        /// <returns>A value indicating whether the two bounding boxes are overlapping.</returns>
        public static bool Overlapping(ref BoundingBoxi a, ref BoundingBoxi b)
        {
            return !(a.Min.X > b.Max.X || a.Max.X < b.Min.X
                || a.Min.Y > b.Max.Y || a.Max.Y < b.Min.Y
                || a.Min.Z > b.Max.Z || a.Max.Z < b.Min.Z);
        }
        /// <summary>
        /// Compares two <see cref="BoundingBoxi"/> instances for equality.
        /// </summary>
        /// <param name="left">A bounding box.</param>
        /// <param name="right">Another bounding box.</param>
        /// <returns>A value indicating whether the two bounding boxes are equal.</returns>
        public static bool operator ==(BoundingBoxi left, BoundingBoxi right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares two <see cref="BoundingBoxi"/> instances for inequality.
        /// </summary>
        /// <param name="left">A bounding box.</param>
        /// <param name="right">Another bounding box.</param>
        /// <returns>A value indicating whether the two bounding boxes are not equal.</returns>
        public static bool operator !=(BoundingBoxi left, BoundingBoxi right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBoxi"/> struct.
        /// </summary>
        /// <param name="min">The lower bound of the bounding box.</param>
        /// <param name="max">The upper bound of the bounding box.</param>
        public BoundingBoxi(Vertex3i min, Vertex3i max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Compares another <see cref="BoundingBoxi"/> instance with this instance for equality.
        /// </summary>
        /// <param name="other">A bounding box.</param>
        /// <returns>A value indicating whether the bounding box is equal to this instance.</returns>
        public bool Equals(BoundingBoxi other)
        {
            return Min == other.Min && Max == other.Max;
        }
        /// <summary>
        /// Compares another object with this instance for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating whether the object is equal to this instance.</returns>
        public override bool Equals(object obj)
        {
            BoundingBoxi? b = obj as BoundingBoxi?;
            if (b.HasValue)
                return this.Equals(b.Value);

            return false;
        }
        /// <summary>
        /// Calculates a hash code unique to the contents of this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }
        /// <summary>
        /// Creates a human-readable string with the contents of this instance.
        /// </summary>
        /// <returns>A human-readable string.</returns>
        public override string ToString()
        {
            return "[" + Min.ToString() + ", " + Max.ToString() + "]";
        }
    }
}
