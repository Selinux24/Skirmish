namespace Engine.Physics
{
    using Engine.Mathematics;

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
        public static Vector3 Transform(this Matrix4x4 matrix, Vector3 vector)
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
        public static Matrix4x4 Transform(this Matrix4x4 matrix, Matrix4x4 transform)
        {
            var trn = new Matrix4x4(
                transform.M11, transform.M12, transform.M13, 0f,
                transform.M21, transform.M22, transform.M23, 0f,
                transform.M31, transform.M32, transform.M33, 0f,
                0f, 0f, 0f, 1f);

            trn = matrix * trn;

            var res = new Matrix4x4(
                trn.M11 * transform.M11 + trn.M12 * transform.M12 + trn.M13 * transform.M13,
                trn.M11 * transform.M21 + trn.M12 * transform.M22 + trn.M13 * transform.M23,
                trn.M11 * transform.M31 + trn.M12 * transform.M32 + trn.M13 * transform.M33,
                0f,

                trn.M21 * transform.M11 + trn.M22 * transform.M12 + trn.M23 * transform.M13,
                trn.M21 * transform.M21 + trn.M22 * transform.M22 + trn.M23 * transform.M23,
                trn.M21 * transform.M31 + trn.M22 * transform.M32 + trn.M23 * transform.M33,
                0f,

                trn.M31 * transform.M11 + trn.M32 * transform.M12 + trn.M33 * transform.M13,
                trn.M31 * transform.M21 + trn.M32 * transform.M22 + trn.M33 * transform.M23,
                trn.M31 * transform.M31 + trn.M32 * transform.M32 + trn.M33 * transform.M33,
                0f,

                0f, 0f, 0f, 1f);

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
