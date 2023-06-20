using SharpDX;
using System;

namespace Engine
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
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
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
            return new Position3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        /// <inheritdoc/>
        public static Position3 operator *(Position3 left, Position3 right)
        {
            return new Position3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }
        /// <inheritdoc/>
        public static Position3 operator +(Position3 value)
        {
            return value;
        }
        /// <inheritdoc/>
        public static Position3 operator -(Position3 left, Position3 right)
        {
            return new Position3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        /// <inheritdoc/>
        public static Position3 operator -(Position3 value)
        {
            return new Position3(0f - value.X, 0f - value.Y, 0f - value.Z);
        }
        /// <inheritdoc/>
        public static Position3 operator *(float scale, Position3 value)
        {
            return new Position3(value.X * scale, value.Y * scale, value.Z * scale);
        }
        /// <inheritdoc/>
        public static Position3 operator *(Position3 value, float scale)
        {
            return new Position3(value.X * scale, value.Y * scale, value.Z * scale);
        }
        /// <inheritdoc/>
        public static Position3 operator /(Position3 value, float scale)
        {
            return new Position3(value.X / scale, value.Y / scale, value.Z / scale);
        }
        /// <inheritdoc/>
        public static Position3 operator /(float scale, Position3 value)
        {
            return new Position3(scale / value.X, scale / value.Y, scale / value.Z);
        }
        /// <inheritdoc/>
        public static Position3 operator /(Position3 value, Position3 scale)
        {
            return new Position3(value.X / scale.X, value.Y / scale.Y, value.Z / scale.Z);
        }
        /// <inheritdoc/>
        public static Position3 operator +(Position3 value, float scalar)
        {
            return new Position3(value.X + scalar, value.Y + scalar, value.Z + scalar);
        }
        /// <inheritdoc/>
        public static Position3 operator +(float scalar, Position3 value)
        {
            return new Position3(scalar + value.X, scalar + value.Y, scalar + value.Z);
        }
        /// <inheritdoc/>
        public static Position3 operator -(Position3 value, float scalar)
        {
            return new Position3(value.X - scalar, value.Y - scalar, value.Z - scalar);
        }
        /// <inheritdoc/>
        public static Position3 operator -(float scalar, Position3 value)
        {
            return new Position3(scalar - value.X, scalar - value.Y, scalar - value.Z);
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
            return $"{value.X} {value.Y} {value.Z}";
        }
        /// <inheritdoc/>
        public static implicit operator Position3(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 1)
            {
                return new Position3(floats[0]);
            }
            else if (floats?.Length == 3)
            {
                return new Position3(floats);
            }
            else
            {
                return PersistenceHelpers.ReadReservedWordsForPosition3(value);
            }
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
