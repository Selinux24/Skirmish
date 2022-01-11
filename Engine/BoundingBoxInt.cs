﻿using System;
using System.Runtime.InteropServices;

namespace Engine
{
    /// <summary>
    /// A bounding box for vertices in a <see cref="PolyMesh"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBoxInt : IEquatable<BoundingBoxInt>
    {
        /// <summary>
        /// The lower bound of the bounding box.
        /// </summary>
        public Vector3Int Min { get; set; }
        /// <summary>
        /// The upper bound of the bounding box.
        /// </summary>
        public Vector3Int Max { get; set; }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBoxInt"/> struct.
        /// </summary>
        /// <param name="min">The lower bound of the bounding box.</param>
        /// <param name="max">The upper bound of the bounding box.</param>
        public BoundingBoxInt(Vector3Int min, Vector3Int max)
        {
            Min = min;
            Max = max;
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
        public bool Equals(BoundingBoxInt other)
        {
            return
                Min == other.Min &&
                Max == other.Max;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var objV = obj as BoundingBoxInt?;
            if (objV != null)
            {
                return Equals(objV);
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{Min}, {Max}]";
        }
    }
}
