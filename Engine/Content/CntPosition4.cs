using Engine.Mathematics;
using System;

namespace Engine.Content
{
    /// <summary>
    /// 4D position
    /// </summary>
    public struct CntPosition4 : IEquatable<CntPosition4>
    {
        /// <summary>
        /// Position zero
        /// </summary>
        public static readonly CntPosition4 Zero = new();

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
        /// Initializes a new instance of the <see cref="CntPosition4"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public CntPosition4(float value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntPosition4"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the position.</param>
        /// <param name="y">Initial value for the Y component of the position.</param>
        /// <param name="z">Initial value for the Z component of the position.</param>
        /// <param name="w">Initial value for the W component of the position.</param>
        public CntPosition4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntPosition4"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the position. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public CntPosition4(float[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

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
        public static bool operator ==(CntPosition4 left, CntPosition4 right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(CntPosition4 left, CntPosition4 right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static CntPosition4 operator +(CntPosition4 left, CntPosition4 right)
        {
            return (CntPosition4)((Vector4)left + (Vector4)right);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator *(CntPosition4 left, CntPosition4 right)
        {
            return (CntPosition4)((Vector4)left * (Vector4)right);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator +(CntPosition4 value)
        {
            return (CntPosition4)(+(Vector4)value);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator -(CntPosition4 left, CntPosition4 right)
        {
            return (CntPosition4)((Vector4)left - (Vector4)right);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator -(CntPosition4 value)
        {
            return (CntPosition4)(-(Vector4)value);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator *(float scale, CntPosition4 value)
        {
            return (CntPosition4)(scale * (Vector4)value);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator *(CntPosition4 value, float scale)
        {
            return (CntPosition4)((Vector4)value * scale);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator /(CntPosition4 value, float scale)
        {
            return (CntPosition4)((Vector4)value / scale);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator /(float scale, CntPosition4 value)
        {
            return (CntPosition4)Vector4.Divide(scale, value);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator /(CntPosition4 value, CntPosition4 scale)
        {
            return (CntPosition4)((Vector4)value / (Vector4)scale);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator +(CntPosition4 value, float scalar)
        {
            return (CntPosition4)Vector4.Add(value, scalar);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator +(float scalar, CntPosition4 value)
        {
            return (CntPosition4)Vector4.Add(new(scalar), value);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator -(CntPosition4 value, float scalar)
        {
            return (CntPosition4)Vector4.Subtract(value, scalar);
        }
        /// <inheritdoc/>
        public static CntPosition4 operator -(float scalar, CntPosition4 value)
        {
            return (CntPosition4)Vector4.Subtract(scalar, value);
        }

        /// <inheritdoc/>
        public static implicit operator Vector4(CntPosition4 value)
        {
            return new Vector4(value.X, value.Y, value.Z, value.W);
        }
        /// <inheritdoc/>
        public static implicit operator CntPosition4(Vector4 value)
        {
            return new CntPosition4(value.X, value.Y, value.Z, value.W);
        }

        /// <inheritdoc/>
        public static implicit operator string(CntPosition4 value)
        {
            return ContentHelper.WritePosition4(value);
        }
        /// <inheritdoc/>
        public static implicit operator CntPosition4(string value)
        {
            return ContentHelper.ReadPosition4(value) ?? Zero;
        }

        /// <inheritdoc/>
        public readonly bool Equals(CntPosition4 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not CntPosition4)
                return false;

            var strongValue = (CntPosition4)obj;
            return Equals(ref strongValue);
        }
        /// <inheritdoc/>
        public readonly bool Equals(ref CntPosition4 other)
        {
            return
                Helper.NearEqual(other.X, X) &&
                Helper.NearEqual(other.Y, Y) &&
                Helper.NearEqual(other.Z, Z) &&
                Helper.NearEqual(other.W, W);
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
