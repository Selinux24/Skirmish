using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// 4D position
    /// </summary>
    public struct Position4 : IEquatable<Position4>
    {
        /// <summary>
        /// Position zero
        /// </summary>
        public static readonly Position4 Zero = new Position4();

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
                throw new ArgumentNullException("values");
            }
            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException("values", "There must be three and only four input values for Position4.");
            }

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        public static bool operator ==(Position4 left, Position4 right)
        {
            return left.Equals(ref right);
        }
        public static bool operator !=(Position4 left, Position4 right)
        {
            return !left.Equals(ref right);
        }

        public static implicit operator Vector4(Position4 value)
        {
            return new Vector4(value.X, value.Y, value.Z, value.W);
        }
        public static implicit operator Position4(Vector4 value)
        {
            return new Position4(value.X, value.Y, value.Z, value.W);
        }

        public static implicit operator string(Position4 value)
        {
            return $"{value.X} {value.Y} {value.Z} {value.W}";
        }
        public static implicit operator Position4(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 1)
            {
                return new Position4(floats[0]);
            }
            else if (floats?.Length == 4)
            {
                return new Position4(floats);
            }
            else
            {
                return PersistenceHelpers.ReadReservedWordsForPosition4(value);
            }
        }

        /// <inheritdoc/>
        public bool Equals(Position4 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Position4))
                return false;

            var strongValue = (Position4)obj;
            return Equals(ref strongValue);
        }

        public bool Equals(ref Position4 other)
        {
            return MathUtil.NearEqual(other.X, X) && MathUtil.NearEqual(other.Y, Y) && MathUtil.NearEqual(other.Z, Z);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"X:{X} Y:{Y} Z:{Z}";
        }
    }
}
