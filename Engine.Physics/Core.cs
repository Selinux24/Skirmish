using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Core operations
    /// </summary>
    static class Core
    {
        public static Matrix3x3 FromMatrix(Matrix matrix)
        {
            return new Matrix3x3()
            {
                M11 = matrix.M11,
                M12 = matrix.M11,
                M13 = matrix.M11,
                M21 = matrix.M21,
                M22 = matrix.M21,
                M23 = matrix.M21,
                M31 = matrix.M31,
                M32 = matrix.M31,
                M33 = matrix.M31,
            };
        }

        public static Vector3 Transform(Matrix3x3 matrix, Vector3 vector)
        {
            return new Vector3(
                vector.X * matrix.M11 + vector.Y * matrix.M12 + vector.Z * matrix.M13,
                vector.X * matrix.M21 + vector.Y * matrix.M22 + vector.Z * matrix.M23,
                vector.X * matrix.M31 + vector.Y * matrix.M32 + vector.Z * matrix.M33);
        }

        public static Matrix3x3 Transform(Matrix3x3 matrix, Matrix transform)
        {
            Matrix3x3 mult = matrix * FromMatrix(transform);

            Matrix3x3 res = default;
            res.M11 = mult.M11 * transform.M11 + mult.M12 * transform.M12 + mult.M13 * transform.M13;
            res.M12 = mult.M11 * transform.M21 + mult.M12 * transform.M22 + mult.M13 * transform.M23;
            res.M13 = mult.M11 * transform.M31 + mult.M12 * transform.M32 + mult.M13 * transform.M33;
            res.M21 = mult.M21 * transform.M11 + mult.M22 * transform.M12 + mult.M23 * transform.M13;
            res.M22 = mult.M21 * transform.M21 + mult.M22 * transform.M22 + mult.M23 * transform.M23;
            res.M23 = mult.M21 * transform.M31 + mult.M22 * transform.M32 + mult.M23 * transform.M33;
            res.M31 = mult.M31 * transform.M11 + mult.M32 * transform.M12 + mult.M33 * transform.M13;
            res.M32 = mult.M31 * transform.M21 + mult.M32 * transform.M22 + mult.M33 * transform.M23;
            res.M33 = mult.M31 * transform.M31 + mult.M32 * transform.M32 + mult.M33 * transform.M33;
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
        /// <summary>
        /// Gets the inertia tensor coefficient matrix
        /// </summary>
        /// <param name="ix">X coefficient</param>
        /// <param name="iy">Y coefficient</param>
        /// <param name="iz">Z coefficient</param>
        /// <returns>Returns the matrix of inertia tensor coefficients</returns>
        public static Matrix3x3 CreateFromInertiaTensorCoeffs(float ix, float iy, float iz)
        {
            return CreateFromInertiaTensorCoeffs(ix, iy, iz, 0f, 0f, 0f);
        }
        /// <summary>
        /// Gets the inertia tensor coefficient matrix
        /// </summary>
        /// <param name="ix">X coefficient</param>
        /// <param name="iy">Y coefficient</param>
        /// <param name="iz">Z coefficient</param>
        /// <param name="ixy">XY coefficient</param>
        /// <param name="ixz">XZ coefficient</param>
        /// <param name="iyz">YZ coefficient</param>
        /// <returns>Returns the matrix of inertia tensor coefficients</returns>
        public static Matrix3x3 CreateFromInertiaTensorCoeffs(float ix, float iy, float iz, float ixy, float ixz, float iyz)
        {
            return new Matrix3x3()
            {
                M11 = ix,
                M12 = -ixy,
                M13 = -ixz,

                M21 = -ixy,
                M22 = iy,
                M23 = -iyz,

                M31 = -ixz,
                M32 = -iyz,
                M33 = iz,
            };
        }
        /// <summary>
        /// Gets the symmetric matrix based on the specified vector
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns the symmetric matrix based on the specified vector</returns>
        /// <remarks>
        /// The symmetric matrix is the equivalent to the cross product of the specified vector, such that a x b = A_s b, where a and b are vectors and A_s is the symmetric form of a
        /// </remarks>
        public static Matrix3x3 SkewSymmetric(Vector3 vector)
        {
            Matrix3x3 result;
            result.M11 = result.M22 = result.M33 = 0f;
            result.M12 = -vector.Z;
            result.M13 = vector.Y;
            result.M21 = vector.Z;
            result.M23 = -vector.X;
            result.M31 = -vector.Y;
            result.M32 = vector.X;
            return result;
        }


        public static Quaternion AddScaledVector(Quaternion rotation, Vector3 vector, float scale)
        {
            Quaternion res = rotation;

            Quaternion q = new Quaternion(vector.X * scale, vector.Y * scale, vector.Z * scale, 0f);

            q *= res;
            res.W += q.W * 0.5f;
            res.X += q.X * 0.5f;
            res.Y += q.Y * 0.5f;
            res.Z += q.Z * 0.5f;

            return Quaternion.Normalize(res);
        }


        public static float ProjectToVector(OrientedBoundingBox box, Vector3 vector)
        {
            var trn = box.Transformation;
            var xAxis = trn.Left;
            var yAxis = trn.Up;
            var zAxis = trn.Backward;

            return
                box.Extents.X * Math.Abs(Vector3.Dot(vector, xAxis)) +
                box.Extents.Y * Math.Abs(Vector3.Dot(vector, yAxis)) +
                box.Extents.Z * Math.Abs(Vector3.Dot(vector, zAxis));
        }

        public static float ProjectToVector(Triangle tri, Vector3 vector)
        {
            float d1 = Math.Abs(Vector3.Dot(tri.GetEdge1(), vector));
            float d2 = Math.Abs(Vector3.Dot(tri.GetEdge2(), vector));
            float d3 = Math.Abs(Vector3.Dot(tri.GetEdge3(), vector));

            return Math.Max(d1, Math.Max(d2, d3));
        }
    }
}
