using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// 4x4 matrix
    /// </summary>
    public struct Matrix4X4 : IEquatable<Matrix4X4>
    {
        /// <summary>
        /// A <see cref="Matrix"/> with all of its components set to zero.
        /// </summary>
        public static readonly Matrix4X4 Zero = new Matrix4X4();
        /// <summary>
        /// The identity <see cref="Matrix"/>.
        /// </summary>
        public static readonly Matrix4X4 Identity = new Matrix4X4() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };

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
                throw new ArgumentNullException("values");
            if (values.Length != 16)
                throw new ArgumentOutOfRangeException("values", "There must be sixteen and only sixteen input values for Matrix.");

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

        public static implicit operator Matrix(Matrix4X4 value)
        {
            return new Matrix(
                value.M11, value.M12, value.M13, value.M14,
                value.M21, value.M22, value.M23, value.M24,
                value.M31, value.M32, value.M33, value.M34,
                value.M41, value.M42, value.M43, value.M44);
        }
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
        public bool Equals(Matrix4X4 other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is Matrix4X4))
            {
                return false;
            }

            Matrix4X4 strongType = (Matrix4X4)obj;
            return Equals(ref strongType);
        }

        public bool Equals(ref Matrix4X4 other)
        {
            return (
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
                MathUtil.NearEqual(other.M44, M44));
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = M11.GetHashCode();
                hashCode = (hashCode * 397) ^ M12.GetHashCode();
                hashCode = (hashCode * 397) ^ M13.GetHashCode();
                hashCode = (hashCode * 397) ^ M14.GetHashCode();
                hashCode = (hashCode * 397) ^ M21.GetHashCode();
                hashCode = (hashCode * 397) ^ M22.GetHashCode();
                hashCode = (hashCode * 397) ^ M23.GetHashCode();
                hashCode = (hashCode * 397) ^ M24.GetHashCode();
                hashCode = (hashCode * 397) ^ M31.GetHashCode();
                hashCode = (hashCode * 397) ^ M32.GetHashCode();
                hashCode = (hashCode * 397) ^ M33.GetHashCode();
                hashCode = (hashCode * 397) ^ M34.GetHashCode();
                hashCode = (hashCode * 397) ^ M41.GetHashCode();
                hashCode = (hashCode * 397) ^ M42.GetHashCode();
                hashCode = (hashCode * 397) ^ M43.GetHashCode();
                hashCode = (hashCode * 397) ^ M44.GetHashCode();
                return hashCode;
            }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[M11:{M11} M12:{M12} M13:{M13} M14:{M14}] [M21:{M21} M22:{M22} M23:{M23} M24:{M24}] [M31:{M31} M32:{M32} M33:{M33} M34:{M34}] [M41:{M41} M42:{M42} M43:{M43} M44:{M44}]";
        }
    }
}
