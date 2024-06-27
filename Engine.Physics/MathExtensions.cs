using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Extensions
    /// </summary>
    static class MathExtensions
    {
        /// <summary>
        /// Transform a vector with the given matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="vector">Vector</param>
        public static Vector3 Transform(this Matrix3x3 matrix, Vector3 vector)
        {
            return new Vector3(
                vector.X * matrix.M11 + vector.Y * matrix.M12 + vector.Z * matrix.M13,
                vector.X * matrix.M21 + vector.Y * matrix.M22 + vector.Z * matrix.M23,
                vector.X * matrix.M31 + vector.Y * matrix.M32 + vector.Z * matrix.M33);
        }
        /// <summary>
        /// Applies the given 4x4 matrix transformation to the current 3x3 matrix
        /// </summary>
        /// <param name="matrix">3x3 matrix</param>
        /// <param name="transform">4x4 transform matrix</param>
        public static Matrix3x3 Transform(this Matrix3x3 matrix, Matrix transform)
        {
            var trn = new Matrix3x3()
            {
                M11 = transform.M11,
                M12 = transform.M12,
                M13 = transform.M13,
                M21 = transform.M21,
                M22 = transform.M22,
                M23 = transform.M23,
                M31 = transform.M31,
                M32 = transform.M32,
                M33 = transform.M33,
            };

            trn = matrix * trn;

            Matrix3x3 res = default;
            res.M11 = trn.M11 * transform.M11 + trn.M12 * transform.M12 + trn.M13 * transform.M13;
            res.M12 = trn.M11 * transform.M21 + trn.M12 * transform.M22 + trn.M13 * transform.M23;
            res.M13 = trn.M11 * transform.M31 + trn.M12 * transform.M32 + trn.M13 * transform.M33;
            res.M21 = trn.M21 * transform.M11 + trn.M22 * transform.M12 + trn.M23 * transform.M13;
            res.M22 = trn.M21 * transform.M21 + trn.M22 * transform.M22 + trn.M23 * transform.M23;
            res.M23 = trn.M21 * transform.M31 + trn.M22 * transform.M32 + trn.M23 * transform.M33;
            res.M31 = trn.M31 * transform.M11 + trn.M32 * transform.M12 + trn.M33 * transform.M13;
            res.M32 = trn.M31 * transform.M21 + trn.M32 * transform.M22 + trn.M33 * transform.M23;
            res.M33 = trn.M31 * transform.M31 + trn.M32 * transform.M32 + trn.M33 * transform.M33;
            return res;
        }
        /// <summary>
        /// Transforms the vector to the space that represents the transpose of the matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="vector">Vector</param>
        /// <returns>Returns the vector in the space represented by the transpose of the matrix</returns>
        public static Vector3 TransformTranspose(Matrix3x3 matrix, Vector3 vector)
        {
            Vector3 result;
            result.X = vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31;
            result.Y = vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32;
            result.Z = vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33;
            return result;
        }
    }
}
