using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// A bounding rectangle represeted by integers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct BoundingRectangleI : IEquatable<BoundingRectangleI>, IFormattable
    {
        const string RectIStr = "Min:{0} Max:{1}";

        /// <summary>
        /// The minimum of the bounding box.
        /// </summary>
        [JsonProperty]
        public Vector2I Minimum;
        /// <summary>
        /// The maximum of the bounding box.
        /// </summary>
        [JsonProperty]
        public Vector2I Maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="BBox2i"/> struct.
        /// </summary>
        /// <param name="min">A minimum bound.</param>
        /// <param name="max">A maximum bound.</param>
        public BoundingRectangleI(Vector2I min, Vector2I max)
        {
            Minimum = min;
            Maximum = max;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BBox2i"/> struct.
        /// </summary>
        /// <param name="minX">The minimum X bound.</param>
        /// <param name="minY">The minimum Y bound.</param>
        /// <param name="maxX">The maximum X bound.</param>
        /// <param name="maxY">The maximum Y bound.</param>
        public BoundingRectangleI(int minX, int minY, int maxX, int maxY)
        {
            Minimum = new Vector2I(minX, minY);
            Maximum = new Vector2I(maxX, maxY);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingRectangleI left, BoundingRectangleI right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingRectangleI left, BoundingRectangleI right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                RectIStr,
                Minimum.ToString(),
                Maximum.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            if (format == null) return ToString();

            return string.Format(
                CultureInfo.CurrentCulture,
                RectIStr,
                Minimum.ToString(format, CultureInfo.CurrentCulture),
                Maximum.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                RectIStr,
                Minimum.ToString(),
                Maximum.ToString());
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString(formatProvider);

            return string.Format(
                formatProvider,
                RectIStr,
                Minimum.ToString(format, formatProvider),
                Maximum.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Minimum, Maximum);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref BoundingRectangleI value)
        {
            return
                Minimum == value.Minimum &&
                Maximum == value.Maximum;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BoundingRectangleI value)
        {
            return Equals(ref value);
        }
        /// <inheritdoc/>
        public readonly override bool Equals(object value)
        {
            if (value is not BoundingRectangleI) return false;

            var strongValue = (BoundingRectangleI)value;
            return Equals(ref strongValue);
        }
    }
}
