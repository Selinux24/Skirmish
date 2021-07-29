using SharpDX;
using System;

namespace Engine.Content.Persistence
{
    /// <summary>
    /// 3D rotation
    /// </summary>
    public struct RotationQ : IEquatable<RotationQ>
    {
        /// <summary>
        /// A <see cref="RotationQ"/> with all of its components set to zero.
        /// </summary>
        public static readonly RotationQ Zero = new RotationQ();
        /// <summary>
        /// A <see cref="RotationQ"/> with all of its components set to one.
        /// </summary>
        public static readonly RotationQ One = new RotationQ(1.0f, 1.0f, 1.0f, 1.0f);
        /// <summary>
        /// The identity <see cref="RotationQ"/> (0, 0, 0, 1).
        /// </summary>
        public static readonly RotationQ Identity = new RotationQ(0.0f, 0.0f, 0.0f, 1.0f);

        /// <summary>
        /// The X component of the rotation.
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// The Y component of the rotation.
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// The Z component of the rotation.
        /// </summary>
        public float Z { get; set; }
        /// <summary>
        /// The W component of the rotation.
        /// </summary>
        public float W { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RotationQ"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public RotationQ(float value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RotationQ"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the rotation.</param>
        /// <param name="y">Initial value for the Y component of the rotation.</param>
        /// <param name="z">Initial value for the Z component of the rotation.</param>
        /// <param name="w">Initial value for the W component of the rotation.</param>
        public RotationQ(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RotationQ"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the rotation. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public RotationQ(float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for RotationQ.");
            }

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        public static bool operator ==(RotationQ left, RotationQ right)
        {
            return left.Equals(ref right);
        }
        public static bool operator !=(RotationQ left, RotationQ right)
        {
            return !left.Equals(ref right);
        }

        public static implicit operator Quaternion(RotationQ value)
        {
            return new Quaternion(value.X, value.Y, value.Z, value.W);
        }
        public static implicit operator RotationQ(Quaternion value)
        {
            return new RotationQ(value.X, value.Y, value.Z, value.W);
        }

        public static implicit operator string(RotationQ value)
        {
            return $"{value.X} {value.Y} {value.Z} {value.W}";
        }
        public static implicit operator RotationQ(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 4)
            {
                return new RotationQ(floats);
            }
            else
            {
                return PersistenceHelpers.ReadReservedWordsForRotation(value);
            }
        }

        /// <inheritdoc/>
        public bool Equals(RotationQ other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is RotationQ))
                return false;

            var strongValue = (RotationQ)obj;
            return Equals(ref strongValue);
        }

        public bool Equals(ref RotationQ other)
        {
            return MathUtil.NearEqual(other.X, X) && MathUtil.NearEqual(other.Y, Y) && MathUtil.NearEqual(other.Z, Z) && MathUtil.NearEqual(other.W, W);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"X:{X} Y:{Y} Z:{Z} W:{W}";
        }

        /// <summary>
        /// Creates a quaternion given a rotation and an axis.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation.</param>
        /// <param name="result">When the method completes, contains the newly created quaternion.</param>
        public static void RotationAxis(ref Direction3 axis, float angle, out RotationQ result)
        {
            Direction3.Normalize(ref axis, out var normalized);

            float half = angle * 0.5f;
            float sin = (float)Math.Sin(half);
            float cos = (float)Math.Cos(half);

            result = Identity;
            result.X = normalized.X * sin;
            result.Y = normalized.Y * sin;
            result.Z = normalized.Z * sin;
            result.W = cos;
        }
        /// <summary>
        /// Creates a quaternion given a rotation and an axis.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation.</param>
        /// <returns>The newly created quaternion.</returns>
        public static RotationQ RotationAxis(Direction3 axis, float angle)
        {
            RotationAxis(ref axis, angle, out var result);
            return result;
        }
    }
}
