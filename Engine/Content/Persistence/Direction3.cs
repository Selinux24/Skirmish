using SharpDX;
using System;

namespace Engine.Content.Persistence
{
    /// <summary>
    /// 3D direction
    /// </summary>
    public struct Direction3 : IEquatable<Direction3>
    {
        /// <summary>
        /// A unit <see cref="Direction3"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Direction3 Up = new Direction3(0.0f, 1.0f, 0.0f);
        /// <summary>
        /// A unit <see cref="Direction3"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Direction3 Down = new Direction3(0.0f, -1.0f, 0.0f);
        /// <summary>
        /// A unit <see cref="Direction3"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Direction3 Left = new Direction3(-1.0f, 0.0f, 0.0f);
        /// <summary>
        /// A unit <see cref="Direction3"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Direction3 Right = new Direction3(1.0f, 0.0f, 0.0f);
        /// <summary>
        /// A unit <see cref="Direction3"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Direction3 ForwardRH = new Direction3(0.0f, 0.0f, -1.0f);
        /// <summary>
        /// A unit <see cref="Direction3"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Direction3 ForwardLH = new Direction3(0.0f, 0.0f, 1.0f);
        /// <summary>
        /// A unit <see cref="Vector3"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Direction3 BackwardRH = new Direction3(0.0f, 0.0f, 1.0f);
        /// <summary>
        /// A unit <see cref="Direction3"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Direction3 BackwardLH = new Direction3(0.0f, 0.0f, -1.0f);

        /// <summary>
        /// The X component of the direction.
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// The Y component of the direction.
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// The Z component of the direction.
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Direction3"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Direction3(float value)
        {
            X = value;
            Y = value;
            Z = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Direction3"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the direction.</param>
        /// <param name="y">Initial value for the Y component of the direction.</param>
        /// <param name="z">Initial value for the Z component of the direction.</param>
        public Direction3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Direction3"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, and Z components of the direction. This must be an array with three elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than three elements.</exception>
        public Direction3(float[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be three and only three input values for Direction3.");

            X = values[0];
            Y = values[1];
            Z = values[2];
        }

        public static bool operator ==(Direction3 left, Direction3 right)
        {
            return left.Equals(ref right);
        }
        public static bool operator !=(Direction3 left, Direction3 right)
        {
            return !left.Equals(ref right);
        }

        public static implicit operator Vector3(Direction3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
        public static implicit operator Direction3(Vector3 value)
        {
            return new Direction3(value.X, value.Y, value.Z);
        }

        public static implicit operator string(Direction3 value)
        {
            return $"{value.X} {value.Y} {value.Z}";
        }
        public static implicit operator Direction3(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 3)
            {
                return new Direction3(floats);
            }
            else
            {
                return PersistenceHelpers.ReadReservedWordsForDirection(value);
            }
        }

        /// <inheritdoc/>
        public bool Equals(Direction3 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Direction3))
                return false;

            var strongValue = (Direction3)obj;
            return Equals(ref strongValue);
        }

        public bool Equals(ref Direction3 other)
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

        /// <summary>
        /// Calculates the length of the vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        /// <remarks>
        /// <see cref="LengthSquared"/> may be preferred when only the relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public float Length()
        {
            return (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
        }
        /// <summary>
        /// Calculates the squared length of the vector.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }
        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        public void Normalize()
        {
            float length = Length();
            if (!MathUtil.IsZero(length))
            {
                float inv = 1.0f / length;
                X *= inv;
                Y *= inv;
                Z *= inv;
            }
        }
        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <param name="result">When the method completes, contains the normalized vector.</param>
        public static void Normalize(ref Direction3 value, out Direction3 result)
        {
            result = value;
            result.Normalize();
        }
    }
}
