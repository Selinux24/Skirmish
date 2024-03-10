using SharpDX;
using System;

namespace Engine.Content
{
    /// <summary>
    /// 3D scale
    /// </summary>
    public struct Scale3 : IEquatable<Scale3>
    {
        /// <summary>
        /// Scale zero
        /// </summary>
        public static readonly Scale3 Zero = new();
        /// <summary>
        /// Scale one
        /// </summary>
        public static readonly Scale3 One = new(1, 1, 1);

        /// <summary>
        /// The X component of the scale.
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// The Y component of the scale.
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// The Z component of the scale.
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scale3"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Scale3(float value)
        {
            X = value;
            Y = value;
            Z = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Scale3"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the scale.</param>
        /// <param name="y">Initial value for the Y component of the scale.</param>
        /// <param name="z">Initial value for the Z component of the scale.</param>
        public Scale3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Scale3"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, and Z components of the scale. This must be an array with three elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than three elements.</exception>
        public Scale3(float[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (values.Length != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be three and only three input values for Scale3.");
            }

            X = values[0];
            Y = values[1];
            Z = values[2];
        }

        /// <inheritdoc/>
        public static bool operator ==(Scale3 left, Scale3 right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(Scale3 left, Scale3 right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static Scale3 operator +(Scale3 left, Scale3 right)
        {
            return (Vector3)left + (Vector3)right;
        }
        /// <inheritdoc/>
        public static Scale3 operator +(Scale3 value)
        {
            return +(Vector3)value;
        }
        /// <inheritdoc/>
        public static Scale3 operator +(Scale3 value, float scalar)
        {
            return (Vector3)value + scalar;
        }
        /// <inheritdoc/>
        public static Scale3 operator +(float scalar, Scale3 value)
        {
            return scalar + (Vector3)value;
        }
        /// <inheritdoc/>
        public static Scale3 operator -(Scale3 left, Scale3 right)
        {
            return (Vector3)left - (Vector3)right;
        }
        /// <inheritdoc/>
        public static Scale3 operator -(Scale3 value)
        {
            return -(Vector3)value;
        }
        /// <inheritdoc/>
        public static Scale3 operator -(Scale3 value, float scalar)
        {
            return (Vector3)value - scalar;
        }
        /// <inheritdoc/>
        public static Scale3 operator -(float scalar, Scale3 value)
        {
            return scalar - (Vector3)value;
        }
        /// <inheritdoc/>
        public static Scale3 operator *(Scale3 left, Scale3 right)
        {
            return (Vector3)left * (Vector3)right;
        }
        /// <inheritdoc/>
        public static Scale3 operator *(float scale, Scale3 value)
        {
            return scale * (Vector3)value;
        }
        /// <inheritdoc/>
        public static Scale3 operator *(Scale3 value, float scale)
        {
            return (Vector3)value * scale;
        }
        /// <inheritdoc/>
        public static Scale3 operator /(Scale3 value, Scale3 scale)
        {
            return (Vector3)value / (Vector3)scale;
        }
        /// <inheritdoc/>
        public static Scale3 operator /(Scale3 value, float scale)
        {
            return (Vector3)value / scale;
        }
        /// <inheritdoc/>
        public static Scale3 operator /(float scale, Scale3 value)
        {
            return scale / (Vector3)value;
        }

        /// <inheritdoc/>
        public static implicit operator Vector3(Scale3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
        /// <inheritdoc/>
        public static implicit operator Scale3(Vector3 value)
        {
            return new Scale3(value.X, value.Y, value.Z);
        }

        /// <inheritdoc/>
        public static implicit operator string(Scale3 value)
        {
            return ContentHelper.WriteScale3(value);
        }
        /// <inheritdoc/>
        public static implicit operator Scale3(string value)
        {
            return ContentHelper.ReadScale3(value) ?? One;
        }

        /// <inheritdoc/>
        public readonly bool Equals(Scale3 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not Scale3)
                return false;

            var strongValue = (Scale3)obj;
            return Equals(ref strongValue);
        }

        public readonly bool Equals(ref Scale3 other)
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
