using System;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
    /// A bounding box for vertices in a <see cref="PolyMesh"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BoundingBoxInt"/> struct.
    /// </remarks>
    /// <param name="min">The lower bound of the bounding box.</param>
    /// <param name="max">The upper bound of the bounding box.</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBoxInt(Vector3Int min, Vector3Int max) : IEquatable<BoundingBoxInt>
    {
        /// <summary>
        /// The lower bound of the bounding box.
        /// </summary>
        public Vector3Int Min { get; set; } = min;
        /// <summary>
        /// The upper bound of the bounding box.
        /// </summary>
        public Vector3Int Max { get; set; } = max;

        /// <summary>
        /// Checks whether two boudning boxes are intersecting.
        /// </summary>
        /// <param name="a">The first bounding box.</param>
        /// <param name="b">The second bounding box.</param>
        /// <returns>A value indicating whether the two bounding boxes are overlapping.</returns>
        public static bool Overlapping(ref BoundingBoxInt a, ref BoundingBoxInt b)
        {
            return !(a.Min.X > b.Max.X || a.Max.X < b.Min.X
                || a.Min.Y > b.Max.Y || a.Max.Y < b.Min.Y
                || a.Min.Z > b.Max.Z || a.Max.Z < b.Min.Z);
        }

        /// <inheritdoc/>
        public static bool operator ==(BoundingBoxInt left, BoundingBoxInt right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(BoundingBoxInt left, BoundingBoxInt right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public readonly bool Equals(BoundingBoxInt other)
        {
            return
                Min == other.Min &&
                Max == other.Max;
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            var objV = obj as BoundingBoxInt?;
            if (objV != null)
            {
                return Equals(objV);
            }

            return false;
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"[{Min}, {Max}]";
        }
    }
}
