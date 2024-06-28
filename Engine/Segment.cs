using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Segment
    /// </summary>
    public struct Segment : IEquatable<Segment>
    {
        /// <summary>
        /// First point
        /// </summary>
        public Vector3 Point1 { get; set; }
        /// <summary>
        /// Second point
        /// </summary>
        public Vector3 Point2 { get; set; }
        /// <summary>
        /// Segment length
        /// </summary>
        public readonly float Length
        {
            get
            {
                return Vector3.Distance(Point1, Point2);
            }
        }
        /// <summary>
        /// Segment squared length
        /// </summary>
        public readonly float LengthSquared
        {
            get
            {
                return Vector3.DistanceSquared(Point1, Point2);
            }
        }
        /// <summary>
        /// Gets the normalized direction vector
        /// </summary>
        public readonly Vector3 Direction
        {
            get
            {
                return Vector3.Normalize(Point2 - Point1);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Segment()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public Segment(Vector3 point1, Vector3 point2)
        {
            Point1 = point1;
            Point2 = point2;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public Segment(IEnumerable<Vector3> points)
        {
            if (points?.Count() != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(points), "A segment must contains two defined points.");
            }

            Point1 = points.ElementAt(0);
            Point2 = points.ElementAt(1);
        }

        /// <inheritdoc/>
        public static bool operator ==(Segment left, Segment right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(Segment left, Segment right)
        {
            return !left.Equals(ref right);
        }
        /// <inheritdoc/>
        public readonly bool Equals(Segment other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not Segment)
            {
                return false;
            }

            var strongValue = (Segment)obj;
            return Equals(ref strongValue);
        }

        /// <inheritdoc/>
        public readonly bool Equals(ref Segment other)
        {
            return
                other.Point1.Equals(Point1) &&
                other.Point2.Equals(Point2);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Point1, Point2);
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Point 1 {Point1}; Point 2 {Point2};";
        }
    }
}
