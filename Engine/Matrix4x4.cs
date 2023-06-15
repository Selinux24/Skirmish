using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// 4x4 matrix
    /// </summary>
    public struct Matrix4X4 : IEquatable<Matrix4X4>
    {
        /// <inheritdoc/>
        public static bool operator ==(Matrix4X4 left, Matrix4X4 right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(Matrix4X4 left, Matrix4X4 right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public static implicit operator Matrix(Matrix4X4 value)
        {
            return new Matrix(
                value.M11, value.M12, value.M13, value.M14,
                value.M21, value.M22, value.M23, value.M24,
                value.M31, value.M32, value.M33, value.M34,
                value.M41, value.M42, value.M43, value.M44);
        }
        /// <inheritdoc/>
        public static implicit operator Matrix4X4(Matrix value)
        {
            return new Matrix4X4
            {
                M11 = value.M11,
                M12 = value.M12,
                M13 = value.M13,
                M14 = value.M14,

                M21 = value.M21,
                M22 = value.M22,
                M23 = value.M23,
                M24 = value.M24,

                M31 = value.M31,
                M32 = value.M32,
                M33 = value.M33,
                M34 = value.M34,

                M41 = value.M41,
                M42 = value.M42,
                M43 = value.M43,
                M44 = value.M44,
            };
        }
        /// <inheritdoc/>
        public static Matrix4X4 operator *(Matrix4X4 left, Matrix4X4 right)
        {
            return (Matrix)left * (Matrix)right;
        }

        /// <summary>
        /// A <see cref="Matrix"/> with all of its components set to zero.
        /// </summary>
        public static readonly Matrix4X4 Zero = new();
        /// <summary>
        /// The identity <see cref="Matrix"/>.
        /// </summary>
        public static readonly Matrix4X4 Identity = new() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };
        /// <summary>
        /// Gets the scaling matrix
        /// </summary>
        /// <param name="scale">Scaling vector</param>
        public static Matrix4X4 Scaling(Scale3 scale)
        {
            return Matrix.Scaling(scale);
        }
        /// <summary>
        /// Gets the rotation matrix
        /// </summary>
        /// <param name="rotation">Rotation quaternion</param>
        public static Matrix4X4 Rotation(RotationQ rotation)
        {
            return Matrix.RotationQuaternion(rotation);
        }
        /// <summary>
        /// Gets the translation matrix
        /// </summary>
        /// <param name="position">Position</param>
        public static Matrix4X4 Translation(Position3 position)
        {
            return Matrix.Translation(position);
        }

        /// <summary>
        /// Value at row 1 column 1 of the matrix.
        /// </summary>
        public float M11 { get; set; }
        /// <summary>
        /// Value at row 1 column 2 of the matrix.
        /// </summary>
        public float M12 { get; set; }
        /// <summary>
        /// Value at row 1 column 3 of the matrix.
        /// </summary>
        public float M13 { get; set; }
        /// <summary>
        /// Value at row 1 column 4 of the matrix.
        /// </summary>
        public float M14 { get; set; }
        /// <summary>
        /// Value at row 2 column 1 of the matrix.
        /// </summary>
        public float M21 { get; set; }
        /// <summary>
        /// Value at row 2 column 2 of the matrix.
        /// </summary>
        public float M22 { get; set; }
        /// <summary>
        /// Value at row 2 column 3 of the matrix.
        /// </summary>
        public float M23 { get; set; }
        /// <summary>
        /// Value at row 2 column 4 of the matrix.
        /// </summary>
        public float M24 { get; set; }
        /// <summary>
        /// Value at row 3 column 1 of the matrix.
        /// </summary>
        public float M31 { get; set; }
        /// <summary>
        /// Value at row 3 column 2 of the matrix.
        /// </summary>
        public float M32 { get; set; }
        /// <summary>
        /// Value at row 3 column 3 of the matrix.
        /// </summary>
        public float M33 { get; set; }
        /// <summary>
        /// Value at row 3 column 4 of the matrix.
        /// </summary>
        public float M34 { get; set; }
        /// <summary>
        /// Value at row 4 column 1 of the matrix.
        /// </summary>
        public float M41 { get; set; }
        /// <summary>
        /// Value at row 4 column 2 of the matrix.
        /// </summary>
        public float M42 { get; set; }
        /// <summary>
        /// Value at row 4 column 3 of the matrix.
        /// </summary>
        public float M43 { get; set; }
        /// <summary>
        /// Value at row 4 column 4 of the matrix.
        /// </summary>
        public float M44 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix4X4"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Matrix4X4(float value)
        {
            M11 = M12 = M13 = M14 =
            M21 = M22 = M23 = M24 =
            M31 = M32 = M33 = M34 =
            M41 = M42 = M43 = M44 = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix4X4"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the components of the matrix. This must be an array with sixteen elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than sixteen elements.</exception>
        public Matrix4X4(float[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(values), "There must be sixteen and only sixteen input values for Matrix.");

            M11 = values[0];
            M12 = values[1];
            M13 = values[2];
            M14 = values[3];

            M21 = values[4];
            M22 = values[5];
            M23 = values[6];
            M24 = values[7];

            M31 = values[8];
            M32 = values[9];
            M33 = values[10];
            M34 = values[11];

            M41 = values[12];
            M42 = values[13];
            M43 = values[14];
            M44 = values[15];
        }

        /// <inheritdoc/>
        public readonly bool Equals(Matrix4X4 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not Matrix4X4 strongType)
            {
                return false;
            }

            return Equals(ref strongType);
        }

        public readonly bool Equals(ref Matrix4X4 other)
        {
            return
                MathUtil.NearEqual(other.M11, M11) &&
                MathUtil.NearEqual(other.M12, M12) &&
                MathUtil.NearEqual(other.M13, M13) &&
                MathUtil.NearEqual(other.M14, M14) &&
                MathUtil.NearEqual(other.M21, M21) &&
                MathUtil.NearEqual(other.M22, M22) &&
                MathUtil.NearEqual(other.M23, M23) &&
                MathUtil.NearEqual(other.M24, M24) &&
                MathUtil.NearEqual(other.M31, M31) &&
                MathUtil.NearEqual(other.M32, M32) &&
                MathUtil.NearEqual(other.M33, M33) &&
                MathUtil.NearEqual(other.M34, M34) &&
                MathUtil.NearEqual(other.M41, M41) &&
                MathUtil.NearEqual(other.M42, M42) &&
                MathUtil.NearEqual(other.M43, M43) &&
                MathUtil.NearEqual(other.M44, M44);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(M11);
            hash.Add(M12);
            hash.Add(M13);
            hash.Add(M14);
            hash.Add(M21);
            hash.Add(M22);
            hash.Add(M23);
            hash.Add(M24);
            hash.Add(M31);
            hash.Add(M32);
            hash.Add(M33);
            hash.Add(M34);
            hash.Add(M41);
            hash.Add(M42);
            hash.Add(M43);
            hash.Add(M44);
            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"[M11:{M11} M12:{M12} M13:{M13} M14:{M14}] [M21:{M21} M22:{M22} M23:{M23} M24:{M24}] [M31:{M31} M32:{M32} M33:{M33} M34:{M34}] [M41:{M41} M42:{M42} M43:{M43} M44:{M44}]";
        }
    }
}
