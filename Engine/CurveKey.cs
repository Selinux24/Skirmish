using System;

namespace Engine
{
    /// <summary>
    /// Key point on the <see cref="Curve"/>.
    /// </summary>
    public sealed class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
    {
        /// <summary>
        /// Gets or sets the indicator whether the segment between this point and the next point on the curve is discrete or continuous.
        /// </summary>
        public CurveContinuity Continuity { get; set; }
        /// <summary>
        /// Gets a position of the key on the curve.
        /// </summary>
        public float Position { get; private set; }
        /// <summary>
        /// Gets or sets a tangent when approaching this point from the previous point on the curve.
        /// </summary>
        public float TangentIn { get; set; }
        /// <summary>
        /// Gets or sets a tangent when leaving this point to the next point on the curve.
        /// </summary>
        public float TangentOut { get; set; }
        /// <summary>
        /// Gets a value of this point.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        public CurveKey(float position, float value)
            : this(position, value, 0, 0, CurveContinuity.Smooth)
        {

        }
        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        public CurveKey(float position, float value, float tangentIn, float tangentOut)
            : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
        {

        }
        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        /// <param name="continuity">Indicates whether the curve is discrete or continuous.</param>
        public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
        {
            Position = position;
            Value = value;
            TangentIn = tangentIn;
            TangentOut = tangentOut;
            Continuity = continuity;
        }

        /// <inheritdoc/>
        public static bool operator !=(CurveKey value1, CurveKey value2)
        {
            return !(value1 == value2);
        }
        /// <inheritdoc/>
        public static bool operator ==(CurveKey value1, CurveKey value2)
        {
            if (Equals(value1, null))
            {
                return Equals(value2, null);
            }

            if (Equals(value2, null))
            {
                return Equals(value1, null);
            }

            return (value1.Position == value2.Position)
                && (value1.Value == value2.Value)
                && (value1.TangentIn == value2.TangentIn)
                && (value1.TangentOut == value2.TangentOut)
                && (value1.Continuity == value2.Continuity);
        }
        /// <inheritdoc/>
        public static bool operator >(CurveKey left, CurveKey right)
        {
            return left.CompareTo(right) > 0;
        }
        /// <inheritdoc/>
        public static bool operator <(CurveKey left, CurveKey right)
        {
            return left.CompareTo(right) < 0;
        }
        /// <inheritdoc/>
        public static bool operator >=(CurveKey left, CurveKey right)
        {
            return left.CompareTo(right) >= 0;
        }
        /// <inheritdoc/>
        public static bool operator <=(CurveKey left, CurveKey right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <inheritdoc/>
        public int CompareTo(CurveKey other)
        {
            return Position.CompareTo(other.Position);
        }
        /// <inheritdoc/>
        public bool Equals(CurveKey other)
        {
            return this == other;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return (obj is CurveKey curveKey) && Equals(curveKey);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Value, TangentIn, TangentOut, Continuity);
        }
    }
}
