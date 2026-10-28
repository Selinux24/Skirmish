using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Engine.Mathematics
{
    public static class Matrix4x4Extensions
    {
        public class Matrix4x4Comparer : IEqualityComparer<Matrix4x4>
        {
            public bool Equals(Matrix4x4 x, Matrix4x4 y)
            {
                return Matrix4x4Extensions.NearEqual(x, y);
            }
            public int GetHashCode([DisallowNull] Matrix4x4 obj)
            {
                return obj.GetHashCode();
            }
        }

        public static readonly Matrix4x4Comparer Comparer = new();

        public static Matrix4x4 Zero => new(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        public static Vector3 GetUp(this Matrix4x4 m)
        {
            Vector3 vector3;
            vector3.X = m.M21;
            vector3.Y = m.M22;
            vector3.Z = m.M23;
            return vector3;
        }
        public static void SetUp(this Matrix4x4 m, Vector3 value)
        {
            m.M21 = value.X;
            m.M22 = value.Y;
            m.M23 = value.Z;
        }
        public static Vector3 GetDown(this Matrix4x4 m)
        {
            Vector3 vector3;
            vector3.X = -m.M21;
            vector3.Y = -m.M22;
            vector3.Z = -m.M23;
            return vector3;
        }
        public static void SetDown(this Matrix4x4 m, Vector3 value)
        {
            m.M21 = -value.X;
            m.M22 = -value.Y;
            m.M23 = -value.Z;
        }

        public static Vector3 GetRight(this Matrix4x4 m)
        {
            Vector3 vector3;
            vector3.X = m.M11;
            vector3.Y = m.M12;
            vector3.Z = m.M13;
            return vector3;
        }
        public static void SetRight(this Matrix4x4 m, Vector3 value)
        {
            m.M11 = value.X;
            m.M12 = value.Y;
            m.M13 = value.Z;
        }
        public static Vector3 GetLeft(this Matrix4x4 m)
        {
            Vector3 vector3;
            vector3.X = -m.M11;
            vector3.Y = -m.M12;
            vector3.Z = -m.M13;
            return vector3;
        }
        public static void SetLeft(this Matrix4x4 m, Vector3 value)
        {
            m.M11 = -value.X;
            m.M12 = -value.Y;
            m.M13 = -value.Z;
        }

        public static Vector3 GetForward(this Matrix4x4 m)
        {
            Vector3 vector3;
            vector3.X = -m.M31;
            vector3.Y = -m.M32;
            vector3.Z = -m.M33;
            return vector3;
        }
        public static void SetForward(this Matrix4x4 m, Vector3 value)
        {
            m.M31 = -value.X;
            m.M32 = -value.Y;
            m.M33 = -value.Z;
        }
        public static Vector3 GetBackward(this Matrix4x4 m)
        {
            Vector3 vector3;
            vector3.X = m.M31;
            vector3.Y = m.M32;
            vector3.Z = m.M33;
            return vector3;
        }
        public static void SetBackward(this Matrix4x4 m, Vector3 value)
        {
            m.M31 = value.X;
            m.M32 = value.Y;
            m.M33 = value.Z;
        }

        public static Matrix4x4 RotationAxis(Vector3 axis, float angle)
        {
            return Matrix4x4.CreateFromAxisAngle(Vector3.Normalize(axis), angle);
        }

        public static void Transformation(ref Vector3 scalingCenter, ref Quaternion scalingRotation, ref Vector3 scaling, ref Vector3 rotationCenter, ref Quaternion rotation, ref Vector3 translation, out Matrix4x4 result)
        {
            Matrix4x4 scalingRot = Matrix4x4.CreateFromQuaternion(scalingRotation);

            result =
                Matrix4x4.CreateTranslation(-scalingCenter) *
                Matrix4x4.Transpose(scalingRot) *
                Matrix4x4.CreateScale(scaling) *
                scalingRot *
                Matrix4x4.CreateTranslation(scalingCenter) *
                Matrix4x4.CreateTranslation(-rotationCenter) *
                Matrix4x4.CreateFromQuaternion(rotation) *
                Matrix4x4.CreateTranslation(rotationCenter) *
                Matrix4x4.CreateTranslation(translation);
        }
        public static Matrix4x4 Transformation(Vector3 scalingCenter, Quaternion scalingRotation, Vector3 scaling, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
        {
            Transformation(
                ref scalingCenter,
                ref scalingRotation,
                ref scaling,
                ref rotationCenter,
                ref rotation,
                ref translation,
                out var result);

            return result;
        }

        public static void Transformation2D(ref Vector2 scalingCenter, float scalingRotation, ref Vector2 scaling, ref Vector2 rotationCenter, float rotation, ref Vector2 translation, out Matrix4x4 result)
        {
            result =
                Matrix4x4.CreateTranslation(new Vector3(-scalingCenter, 0f)) *
                Matrix4x4.CreateRotationZ(-scalingRotation) *
                Matrix4x4.CreateScale(new Vector3(scaling, 0f)) *
                Matrix4x4.CreateRotationZ(scalingRotation) *
                Matrix4x4.CreateTranslation(new Vector3(scalingCenter, 0f)) *
                Matrix4x4.CreateTranslation(new Vector3(-rotationCenter, 0f)) *
                Matrix4x4.CreateRotationZ(rotation) *
                Matrix4x4.CreateTranslation(new Vector3(rotationCenter, 0f)) *
                Matrix4x4.CreateTranslation(new Vector3(translation, 0f));

            result.M33 = 1f;
            result.M44 = 1f;
        }
        public static Matrix4x4 Transformation2D(Vector2 scalingCenter, float scalingRotation, Vector2 scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
        {
            Transformation2D(
                ref scalingCenter,
                scalingRotation,
                ref scaling,
                ref rotationCenter,
                rotation,
                ref translation,
                out var result);

            return result;
        }

        public static bool NearEqual(Matrix4x4 a, Matrix4x4 b)
        {
            return
                Helper.NearEqual(a.M11, b.M11) &&
                Helper.NearEqual(a.M12, b.M12) &&
                Helper.NearEqual(a.M13, b.M13) &&
                Helper.NearEqual(a.M14, b.M14) &&

                Helper.NearEqual(a.M21, b.M21) &&
                Helper.NearEqual(a.M22, b.M22) &&
                Helper.NearEqual(a.M23, b.M23) &&
                Helper.NearEqual(a.M24, b.M24) &&

                Helper.NearEqual(a.M31, b.M31) &&
                Helper.NearEqual(a.M32, b.M32) &&
                Helper.NearEqual(a.M33, b.M33) &&
                Helper.NearEqual(a.M34, b.M34) &&

                Helper.NearEqual(a.M41, b.M41) &&
                Helper.NearEqual(a.M42, b.M42) &&
                Helper.NearEqual(a.M43, b.M43) &&
                Helper.NearEqual(a.M44, b.M44);
        }

        /// <summary>
        /// Gets matrix description
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>Return matrix description</returns>
        public static string GetDescription(this Matrix4x4 matrix)
        {
            if (Matrix4x4.Decompose(matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
            {
                return string.Format("S:{0,-30} T:{1,-30} R:{2,-30}",
                    scale.GetDescription(),
                    translation.GetDescription(),
                    rotation.GetDescription());
            }
            else
            {
                return "Bad transform matrix";
            }
        }

        /// <summary>
        /// Creates a new world Matrix
        /// </summary>
        /// <param name="position">The position vector.</param>
        /// <param name="forward">The forward direction vector.</param>
        /// <param name="up">The upward direction vector. Usually <see cref="Vector3.Up"/>.</param>
        /// <returns>The world Matrix</returns>
        public static Matrix4x4 CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
        {
            var result = new Matrix4x4();

            var z = Vector3.Normalize(forward);
            var x = Vector3.Cross(forward, up);
            var y = Vector3.Cross(x, forward);
            x = Vector3.Normalize(x);
            y = Vector3.Normalize(y);

            result.M11 = x.X;
            result.M12 = x.Y;
            result.M13 = x.Z;

            result.M21 = y.X;
            result.M22 = y.Y;
            result.M23 = y.Z;

            result.M31 = z.X;
            result.M32 = z.Y;
            result.M33 = z.Z;

            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
            result.M44 = 1f;

            return result;
        }
    }
}
