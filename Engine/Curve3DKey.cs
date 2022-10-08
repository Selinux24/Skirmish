using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Key point on the <see cref="Curve3D"/>.
    /// </summary>
    public sealed class Curve3DKey : IEquatable<Curve3DKey>, IComparable<Curve3DKey>
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
        public Vector3 TangentIn { get; set; }
        /// <summary>
        /// Gets or sets a tangent when leaving this point to the next point on the curve.
        /// </summary>
        public Vector3 TangentOut { get; set; }
        /// <summary>
        /// Gets a value of this point.
        /// </summary>
        public Vector3 Value { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="Curve3DKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        public Curve3DKey(float position, Vector3 value)
            : this(position, value, Vector3.Zero, Vector3.Zero, CurveContinuity.Smooth)
        {

        }
        /// <summary>
        /// Creates a new instance of <see cref="Curve3DKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        public Curve3DKey(float position, Vector3 value, Vector3 tangentIn, Vector3 tangentOut)
            : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
        {

        }
        /// <summary>
        /// Creates a new instance of <see cref="Curve3DKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        /// <param name="continuity">Indicates whether the curve is discrete or continuous.</param>
        public Curve3DKey(float position, Vector3 value, Vector3 tangentIn, Vector3 tangentOut, CurveContinuity continuity)
        {
            this.Position = position;
            this.Value = value;
            this.TangentIn = tangentIn;
            this.TangentOut = tangentOut;
            this.Continuity = continuity;
        }

        /// <summary>
        /// Compares whether two <see cref="CurveKey"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="CurveKey"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="CurveKey"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Curve3DKey value1, Curve3DKey value2)
        {
            return !(value1 == value2);
        }
        /// <summary>
        /// Compares whether two <see cref="CurveKey"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="CurveKey"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="CurveKey"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Curve3DKey value1, Curve3DKey value2)
        {
            if (object.Equals(value1, null))
            {
                return object.Equals(value2, null);
            }

            if (object.Equals(value2, null))
            {
                return object.Equals(value1, null);
            }

            return (value1.Position == value2.Position)
                && (value1.Value == value2.Value)
                && (value1.TangentIn == value2.TangentIn)
                && (value1.TangentOut == value2.TangentOut)
                && (value1.Continuity == value2.Continuity);
        }
        /// <summary>
        /// Gets wether the left curve is major than the right one
        /// </summary>
        /// <param name="left">Left curve</param>
        /// <param name="right">Right curve</param>
        /// <returns>Returns wether the left curve is major than the right one</returns>
        public static bool operator >(Curve3DKey left, Curve3DKey right)
        {
            return left.CompareTo(right) > 0;
        }
        /// <summary>
        /// Gets wether the left curve is minor than the right one
        /// </summary>
        /// <param name="left">Left curve</param>
        /// <param name="right">Right curve</param>
        /// <returns>Returns wether the left curve is minor than the right one</returns>
        public static bool operator <(Curve3DKey left, Curve3DKey right)
        {
            return left.CompareTo(right) < 0;
        }
        /// <summary>
        /// Gets wether the left curve is major than or equal to the right one
        /// </summary>
        /// <param name="left">Left curve</param>
        /// <param name="right">Right curve</param>
        /// <returns>Returns wether the left curve is major than or equal to the right one</returns>
        public static bool operator >=(Curve3DKey left, Curve3DKey right)
        {
            return left.CompareTo(right) >= 0;
        }
        /// <summary>
        /// Gets wether the left curve is minor than or equal to the right one
        /// </summary>
        /// <param name="left">Left curve</param>
        /// <param name="right">Right curve</param>
        /// <returns>Returns wether the left curve is minor than or equal to the right one</returns>
        public static bool operator <=(Curve3DKey left, Curve3DKey right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Compares another curve with this instance for equality.
        /// </summary>
        /// <param name="other">A curve</param>
        /// <returns>A value indicating whether the object is equal to this instance.</returns>
        public int CompareTo(Curve3DKey other)
        {
            return this.Position.CompareTo(other.Position);
        }
        /// <summary>
        /// Compares another curve with this instance for equality.
        /// </summary>
        /// <param name="other">A curve</param>
        /// <returns>A value indicating whether the object is equal to this instance.</returns>
        public bool Equals(Curve3DKey other)
        {
            return (this == other);
        }
        /// <summary>
        /// Compares another object with this instance for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating whether the object is equal to this instance.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Curve3DKey key) && Equals(key);
        }
        /// <summary>
        /// Calculates a hash code unique to the contents of this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return
                this.Position.GetHashCode() ^
                this.Value.GetHashCode() ^
                this.TangentIn.GetHashCode() ^
                this.TangentOut.GetHashCode() ^
                this.Continuity.GetHashCode();
        }
    }
}
