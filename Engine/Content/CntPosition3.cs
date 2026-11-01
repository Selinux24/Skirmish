using Engine.Mathematics;
using System;

namespace Engine.Content
{
    /// <summary>
    /// 3D position
    /// </summary>
    public struct CntPosition3 : IEquatable<CntPosition3>
    {
        /// <summary>
        /// Position zero
        /// </summary>
        public static readonly CntPosition3 Zero = new();

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
        /// Initializes a new instance of the <see cref="CntPosition3"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public CntPosition3(float value)
        {
            X = value;
            Y = value;
            Z = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntPosition3"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the position.</param>
        /// <param name="y">Initial value for the Y component of the position.</param>
        /// <param name="z">Initial value for the Z component of the position.</param>
        public CntPosition3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntPosition3"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, and Z components of the position. This must be an array with three elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than three elements.</exception>
        public CntPosition3(float[] values)
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
        public static bool operator ==(CntPosition3 left, CntPosition3 right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(CntPosition3 left, CntPosition3 right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static CntPosition3 operator +(CntPosition3 left, CntPosition3 right)
        {
            return (CntPosition3)((Vector3)left + (Vector3)right);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator +(CntPosition3 value)
        {
            return (CntPosition3)(+(Vector3)value);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator +(CntPosition3 value, float scalar)
        {
            return (CntPosition3)Vector3.Add(value, scalar);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator +(float scalar, CntPosition3 value)
        {
            return (CntPosition3)Vector3.Add(new(scalar), value);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator -(CntPosition3 left, CntPosition3 right)
        {
            return (CntPosition3)((Vector3)left - (Vector3)right);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator -(CntPosition3 value)
        {
            return (CntPosition3)(-(Vector3)value);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator -(CntPosition3 value, float scalar)
        {
            return (CntPosition3)Vector3.Subtract(value, scalar);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator -(float scalar, CntPosition3 value)
        {
            return (CntPosition3)Vector3.Subtract(scalar, value);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator *(CntPosition3 left, CntPosition3 right)
        {
            return (CntPosition3)((Vector3)left * (Vector3)right);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator *(float scale, CntPosition3 value)
        {
            return (CntPosition3)(scale * (Vector3)value);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator *(CntPosition3 value, float scale)
        {
            return (CntPosition3)((Vector3)value * scale);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator /(CntPosition3 value, float scale)
        {
            return (CntPosition3)((Vector3)value / scale);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator /(float scale, CntPosition3 value)
        {
            return (CntPosition3)Vector3.Divide(scale, value);
        }
        /// <inheritdoc/>
        public static CntPosition3 operator /(CntPosition3 value, CntPosition3 scale)
        {
            return (CntPosition3)((Vector3)value / (Vector3)scale);
        }

        /// <inheritdoc/>
        public static implicit operator Vector3(CntPosition3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
        /// <inheritdoc/>
        public static implicit operator CntPosition3(Vector3 value)
        {
            return new CntPosition3(value.X, value.Y, value.Z);
        }

        /// <inheritdoc/>
        public static implicit operator string(CntPosition3 value)
        {
            return ContentHelper.WritePosition3(value);
        }
        /// <inheritdoc/>
        public static implicit operator CntPosition3(string value)
        {
            return ContentHelper.ReadPosition3(value) ?? Zero;
        }

        /// <inheritdoc/>
        public readonly bool Equals(CntPosition3 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not CntPosition3)
                return false;

            var strongValue = (CntPosition3)obj;
            return Equals(ref strongValue);
        }
        /// <inheritdoc/>
        public readonly bool Equals(ref CntPosition3 other)
        {
            return Helper.NearEqual(other.X, X) && Helper.NearEqual(other.Y, Y) && Helper.NearEqual(other.Z, Z);
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
