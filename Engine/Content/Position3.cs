using SharpDX;
using System;

namespace Engine.Content
{
    /// <summary>
    /// 3D position
    /// </summary>
    public struct Position3 : IEquatable<Position3>
    {
        /// <summary>
        /// Position zero
        /// </summary>
        public static readonly Position3 Zero = new();

        /// <summary>
        /// The X component of the position.
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// The Y component of the position.
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// The Z component of the position.
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Position3"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Position3(float value)
        {
            X = value;
            Y = value;
            Z = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Position3"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the position.</param>
        /// <param name="y">Initial value for the Y component of the position.</param>
        /// <param name="z">Initial value for the Z component of the position.</param>
        public Position3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Position3"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, and Z components of the position. This must be an array with three elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than three elements.</exception>
        public Position3(float[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (values.Length != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be three and only three input values for Position3.");
            }

            X = values[0];
            Y = values[1];
            Z = values[2];
        }

        /// <inheritdoc/>
        public static bool operator ==(Position3 left, Position3 right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(Position3 left, Position3 right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static Position3 operator +(Position3 left, Position3 right)
        {
            return (Vector3)left + (Vector3)right;
        }
        /// <inheritdoc/>
        public static Position3 operator +(Position3 value)
        {
            return +(Vector3)value;
        }
        /// <inheritdoc/>
        public static Position3 operator +(Position3 value, float scalar)
        {
            return (Vector3)value + scalar;
        }
        /// <inheritdoc/>
        public static Position3 operator +(float scalar, Position3 value)
        {
            return scalar + (Vector3)value;
        }
        /// <inheritdoc/>
        public static Position3 operator -(Position3 left, Position3 right)
        {
            return (Vector3)left - (Vector3)right;
        }
        /// <inheritdoc/>
        public static Position3 operator -(Position3 value)
        {
            return -(Vector3)value;
        }
        /// <inheritdoc/>
        public static Position3 operator -(Position3 value, float scalar)
        {
            return (Vector3)value - scalar;
        }
        /// <inheritdoc/>
        public static Position3 operator -(float scalar, Position3 value)
        {
            return scalar - (Vector3)value;
        }
        /// <inheritdoc/>
        public static Position3 operator *(Position3 left, Position3 right)
        {
            return (Vector3)left * (Vector3)right;
        }
        /// <inheritdoc/>
        public static Position3 operator *(float scale, Position3 value)
        {
            return scale * (Vector3)value;
        }
        /// <inheritdoc/>
        public static Position3 operator *(Position3 value, float scale)
        {
            return (Vector3)value * scale;
        }
        /// <inheritdoc/>
        public static Position3 operator /(Position3 value, float scale)
        {
            return (Vector3)value / scale;
        }
        /// <inheritdoc/>
        public static Position3 operator /(float scale, Position3 value)
        {
            return scale / (Vector3)value;
        }
        /// <inheritdoc/>
        public static Position3 operator /(Position3 value, Position3 scale)
        {
            return (Vector3)value / (Vector3)scale;
        }

        /// <inheritdoc/>
        public static implicit operator Vector3(Position3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
        /// <inheritdoc/>
        public static implicit operator Position3(Vector3 value)
        {
            return new Position3(value.X, value.Y, value.Z);
        }

        /// <inheritdoc/>
        public static implicit operator string(Position3 value)
        {
            return ContentHelper.WritePosition3(value);
        }
        /// <inheritdoc/>
        public static implicit operator Position3(string value)
        {
            return ContentHelper.ReadPosition3(value) ?? Zero;
        }

        /// <inheritdoc/>
        public readonly bool Equals(Position3 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not Position3)
                return false;

            var strongValue = (Position3)obj;
            return Equals(ref strongValue);
        }

        public readonly bool Equals(ref Position3 other)
        {
            return MathUtil.NearEqual(other.X, X) && MathUtil.NearEqual(other.Y, Y) && MathUtil.NearEqual(other.Z, Z);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"X:{X} Y:{Y} Z:{Z}";
        }
    }
}
