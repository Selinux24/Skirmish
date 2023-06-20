using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// 3D rotation
    /// </summary>
    public struct RotationQ : IEquatable<RotationQ>
    {
        /// <summary>
        /// A <see cref="RotationQ"/> with all of its components set to zero.
        /// </summary>
        public static readonly RotationQ Zero = new();
        /// <summary>
        /// A <see cref="RotationQ"/> with all of its components set to one.
        /// </summary>
        public static readonly RotationQ One = new(1.0f, 1.0f, 1.0f, 1.0f);
        /// <summary>
        /// The identity <see cref="RotationQ"/> (0, 0, 0, 1).
        /// </summary>
        public static readonly RotationQ Identity = new(0.0f, 0.0f, 0.0f, 1.0f);

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
                throw new ArgumentNullException(nameof(values));
            }
            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for RotationQ.");
            }

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        /// <inheritdoc/>
        public static bool operator ==(RotationQ left, RotationQ right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(RotationQ left, RotationQ right)
        {
            return !left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static RotationQ operator +(RotationQ left, RotationQ right)
        {
            return Add(left, right);
        }
        /// <inheritdoc/>
        public static RotationQ operator -(RotationQ left, RotationQ right)
        {
            return Subtract(left, right);
        }
        /// <inheritdoc/>
        public static RotationQ operator -(RotationQ value)
        {
            return Negate(value);
        }
        /// <inheritdoc/>
        public static RotationQ operator *(float scale, RotationQ value)
        {
            return Multiply(value, scale);
        }
        /// <inheritdoc/>
        public static RotationQ operator *(RotationQ value, float scale)
        {
            return Multiply(value, scale);
        }
        /// <inheritdoc/>
        public static RotationQ operator *(RotationQ left, RotationQ right)
        {
            return Multiply(left, right);
        }

        /// <inheritdoc/>
        public static implicit operator Quaternion(RotationQ value)
        {
            return new Quaternion(value.X, value.Y, value.Z, value.W);
        }
        /// <inheritdoc/>
        public static implicit operator RotationQ(Quaternion value)
        {
            return new RotationQ(value.X, value.Y, value.Z, value.W);
        }

        /// <inheritdoc/>
        public static implicit operator string(RotationQ value)
        {
            return $"{value.X} {value.Y} {value.Z} {value.W}";
        }
        /// <inheritdoc/>
        public static implicit operator RotationQ(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 1)
            {
                return new RotationQ(floats[0]);
            }
            else if (floats?.Length == 4)
            {
                return new RotationQ(floats);
            }
            else
            {
                return PersistenceHelpers.ReadReservedWordsForRotationQ(value);
            }
        }

        /// <inheritdoc/>
        public readonly bool Equals(RotationQ other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not RotationQ)
                return false;

            var strongValue = (RotationQ)obj;
            return Equals(ref strongValue);
        }

        public readonly bool Equals(ref RotationQ other)
        {
            return MathUtil.NearEqual(other.X, X) && MathUtil.NearEqual(other.Y, Y) && MathUtil.NearEqual(other.Z, Z) && MathUtil.NearEqual(other.W, W);
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

        /// <summary>
        /// Adds two rotation quaternions
        /// </summary>
        public static RotationQ Add(RotationQ left, RotationQ right)
        {
            RotationQ result = Identity;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
            return result;
        }
        /// <summary>
        /// Subtracts two rotation quaternions
        /// </summary>
        public static RotationQ Subtract(RotationQ left, RotationQ right)
        {
            RotationQ result = Identity;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
            return result;
        }
        /// <summary>
        /// Scales a rotation quaternion
        /// </summary>
        public static RotationQ Multiply(RotationQ value, float scale)
        {
            RotationQ result = Identity;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            result.W = value.W * scale;
            return result;
        }
        /// <summary>
        /// Multiplies two rotation quaternions
        /// </summary>
        public static RotationQ Multiply(RotationQ left, RotationQ right)
        {
            float x = left.X;
            float y = left.Y;
            float z = left.Z;
            float w = left.W;
            float x2 = right.X;
            float y2 = right.Y;
            float z2 = right.Z;
            float w2 = right.W;
            float num = y * z2 - z * y2;
            float num2 = z * x2 - x * z2;
            float num3 = x * y2 - y * x2;
            float num4 = x * x2 + y * y2 + z * z2;
            RotationQ result = Identity;
            result.X = x * w2 + x2 * w + num;
            result.Y = y * w2 + y2 * w + num2;
            result.Z = z * w2 + z2 * w + num3;
            result.W = w * w2 - num4;
            return result;
        }
        /// <summary>
        /// Negates a rotation quaternion
        /// </summary>
        public static RotationQ Negate(RotationQ value)
        {
            RotationQ result = Identity;
            result.X = 0f - value.X;
            result.Y = 0f - value.Y;
            result.Z = 0f - value.Z;
            result.W = 0f - value.W;
            return result;
        }
    }
}
