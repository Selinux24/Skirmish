using SharpDX;
using System;

namespace Engine.Content
{
    /// <summary>
    /// 4D position
    /// </summary>
    public struct Position4 : IEquatable<Position4>
    {
        /// <summary>
        /// Position zero
        /// </summary>
        public static readonly Position4 Zero = new();

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
        /// The W component of the position.
        /// </summary>
        public float W { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Position4"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Position4(float value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Position4"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the position.</param>
        /// <param name="y">Initial value for the Y component of the position.</param>
        /// <param name="z">Initial value for the Z component of the position.</param>
        /// <param name="w">Initial value for the W component of the position.</param>
        public Position4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Position4"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the position. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Position4(float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be three and only four input values for Position4.");
            }

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        /// <inheritdoc/>
        public static bool operator ==(Position4 left, Position4 right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(Position4 left, Position4 right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static Position4 operator +(Position4 left, Position4 right)
        {
            return (Vector4)left + (Vector4)right;
        }
        /// <inheritdoc/>
        public static Position4 operator *(Position4 left, Position4 right)
        {
            return (Vector4)left * (Vector4)right;
        }
        /// <inheritdoc/>
        public static Position4 operator +(Position4 value)
        {
            return +(Vector4)value;
        }
        /// <inheritdoc/>
        public static Position4 operator -(Position4 left, Position4 right)
        {
            return (Vector4)left - (Vector4)right;
        }
        /// <inheritdoc/>
        public static Position4 operator -(Position4 value)
        {
            return -(Vector4)value;
        }
        /// <inheritdoc/>
        public static Position4 operator *(float scale, Position4 value)
        {
            return scale * (Vector4)value;
        }
        /// <inheritdoc/>
        public static Position4 operator *(Position4 value, float scale)
        {
            return (Vector4)value * scale;
        }
        /// <inheritdoc/>
        public static Position4 operator /(Position4 value, float scale)
        {
            return (Vector4)value / scale;
        }
        /// <inheritdoc/>
        public static Position4 operator /(float scale, Position4 value)
        {
            return scale / (Vector4)value;
        }
        /// <inheritdoc/>
        public static Position4 operator /(Position4 value, Position4 scale)
        {
            return (Vector4)value / (Vector4)scale;
        }
        /// <inheritdoc/>
        public static Position4 operator +(Position4 value, float scalar)
        {
            return (Vector4)value + scalar;
        }
        /// <inheritdoc/>
        public static Position4 operator +(float scalar, Position4 value)
        {
            return scalar + (Vector4)value;
        }
        /// <inheritdoc/>
        public static Position4 operator -(Position4 value, float scalar)
        {
            return (Vector4)value - scalar;
        }
        /// <inheritdoc/>
        public static Position4 operator -(float scalar, Position4 value)
        {
            return scalar - (Vector4)value;
        }

        /// <inheritdoc/>
        public static implicit operator Vector4(Position4 value)
        {
            return new Vector4(value.X, value.Y, value.Z, value.W);
        }
        /// <inheritdoc/>
        public static implicit operator Position4(Vector4 value)
        {
            return new Position4(value.X, value.Y, value.Z, value.W);
        }

        /// <inheritdoc/>
        public static implicit operator string(Position4 value)
        {
            return ContentHelper.WritePosition4(value);
        }
        /// <inheritdoc/>
        public static implicit operator Position4(string value)
        {
            return ContentHelper.ReadPosition4(value) ?? Zero;
        }

        /// <inheritdoc/>
        public readonly bool Equals(Position4 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not Position4)
                return false;

            var strongValue = (Position4)obj;
            return Equals(ref strongValue);
        }

        public readonly bool Equals(ref Position4 other)
        {
            return
                MathUtil.NearEqual(other.X, X) &&
                MathUtil.NearEqual(other.Y, Y) &&
                MathUtil.NearEqual(other.Z, Z) &&
                MathUtil.NearEqual(other.W, W);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"X:{X} Y:{Y} Z:{Z} W:{W}";
        }
    }
}
