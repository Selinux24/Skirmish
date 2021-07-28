using SharpDX;
using System;

namespace Engine
{
    public struct Position3 : IEquatable<Position3>
    {
        /// <summary>
        /// The X component of the position.
        /// </summary>
        public float X;
        /// <summary>
        /// The Y component of the position.
        /// </summary>
        public float Y;
        /// <summary>
        /// The Z component of the position.
        /// </summary>
        public float Z;

        public static readonly Position3 Zero = new Position3();

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
                throw new ArgumentNullException("values");
            }
            if (values.Length != 3)
            {
                throw new ArgumentOutOfRangeException("values", "There must be three and only three input values for Position3.");
            }

            X = values[0];
            Y = values[1];
            Z = values[2];
        }

        public static bool operator ==(Position3 left, Position3 right)
        {
            return left.Equals(ref right);
        }
        public static bool operator !=(Position3 left, Position3 right)
        {
            return !left.Equals(ref right);
        }

        public static implicit operator Vector3(Position3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
        public static implicit operator Position3(Vector3 value)
        {
            return new Position3(value.X, value.Y, value.Z);
        }

        public static implicit operator string(Position3 value)
        {
            return $"{value.X} {value.Y} {value.Z}";
        }
        public static implicit operator Position3(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 3)
            {
                return new Position3(floats);
            }
            else
            {
                return ModularSceneryExtents.ReadReservedWordsForPosition(value);
            }
        }

        /// <inheritdoc/>
        public bool Equals(Position3 other)
        {
            return Equals(other);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Position3))
                return false;

            var strongValue = (Position3)obj;
            return Equals(ref strongValue);
        }

        public bool Equals(ref Position3 other)
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
