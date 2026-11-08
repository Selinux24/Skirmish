using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Segment
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Segment : IEquatable<Segment>, IFormattable
    {
        const string SegmentStr = "Point1:{0} Point2:{1}";

        /// <summary>
        /// First point
        /// </summary>
        [JsonProperty]
        public Vector3 Point1;
        /// <summary>
        /// Second point
        /// </summary>
        [JsonProperty]
        public Vector3 Point2;

        /// <summary>
        /// Segment length
        /// </summary>
        public readonly float Length => Vector3.Distance(Point1, Point2);
        /// <summary>
        /// Segment squared length
        /// </summary>
        public readonly float LengthSquared => Vector3.DistanceSquared(Point1, Point2);
        /// <summary>
        /// Gets the normalized direction vector
        /// </summary>
        public readonly Vector3 Direction => Vector3.Normalize(Point2 - Point1);

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
        public Segment(Vector3[] points)
        {
            if (points?.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(points), "A segment must contains two defined points.");
            }

            Point1 = points[0];
            Point2 = points[1];
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
        public override readonly string ToString()
        {
            return string.Format(
                SegmentStr,
                Point1.ToString(),
                Point2.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                SegmentStr,
                Point1.ToString(format, CultureInfo.CurrentCulture),
                Point2.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                SegmentStr,
                Point1.ToString(),
                Point2.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                SegmentStr,
                Point1.ToString(format, formatProvider),
                Point2.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Point1, Point2);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref Segment other)
        {
            return NearEqual(this, other);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>
        /// Tests whether one segment is near another segment.
        /// </summary>
        /// <param name="left">The left segment.</param>
        /// <param name="right">The right segment.</param>
        /// <param name="epsilon">Toleranze epsilon</param>
        /// <returns><c>true</c> if left and right are near another segment, <c>false</c> otherwise</returns>
        public static bool NearEqual(Segment left, Segment right, float epsilon = Helper.ZeroTolerance)
        {
            return
                Vector3.NearEqual(left.Point1, right.Point1, epsilon) &&
                Vector3.NearEqual(left.Point2, right.Point2, epsilon);
        }
    }
}
