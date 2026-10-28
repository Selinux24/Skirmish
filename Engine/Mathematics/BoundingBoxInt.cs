using System;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// A bounding box for vertices in a <see cref="PolyMesh"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BoundingBoxInt"/> struct.
    /// </remarks>
    /// <param name="minimum">The lower bound of the bounding box.</param>
    /// <param name="maximum">The upper bound of the bounding box.</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBoxInt(Vector3Int minimum, Vector3Int maximum) : IEquatable<BoundingBoxInt>
    {
        /// <summary>
        /// The lower bound of the bounding box.
        /// </summary>
        public Vector3Int Minimum { get; set; } = minimum;
        /// <summary>
        /// The upper bound of the bounding box.
        /// </summary>
        public Vector3Int Maximum { get; set; } = maximum;

        /// <summary>
        /// Checks whether two boudning boxes are intersecting.
        /// </summary>
        /// <param name="a">The first bounding box.</param>
        /// <param name="b">The second bounding box.</param>
        /// <returns>A value indicating whether the two bounding boxes are overlapping.</returns>
        public static bool Overlapping(ref BoundingBoxInt a, ref BoundingBoxInt b)
        {
            return !(a.Minimum.X > b.Maximum.X || a.Maximum.X < b.Minimum.X
                || a.Minimum.Y > b.Maximum.Y || a.Maximum.Y < b.Minimum.Y
                || a.Minimum.Z > b.Maximum.Z || a.Maximum.Z < b.Minimum.Z);
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
                Minimum == other.Minimum &&
                Maximum == other.Maximum;
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
            return Minimum.GetHashCode() ^ Maximum.GetHashCode();
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"[{Minimum}, {Maximum}]";
        }
    }
}
