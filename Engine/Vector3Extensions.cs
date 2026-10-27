using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Vortice.Mathematics;

namespace Engine
{
    /// <summary>
    /// Provides extension methods for the Vector3 structure.
    /// </summary>
    public static class Vector3Extensions
    {
        public class Vector3Comparer : IEqualityComparer<Vector3>
        {
            public bool Equals(Vector3 x, Vector3 y)
            {
                return Vector3Extensions.NearEqual(x, y);
            }
            public int GetHashCode([DisallowNull] Vector3 obj)
            {
                return obj.GetHashCode();
            }
        }

        public static readonly Vector3Comparer Comparer = new();

        /// <summary>
        /// Creates a new rgb color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        public static Vector3 FromRgb(byte r, byte g, byte b)
        {
            return new Vector3(r / 255f, g / 255f, b / 255f);
        }

        /// <summary>
        /// Up vector
        /// </summary>
        public static readonly Vector3 Up = Vector3.UnitY;
        /// <summary>
        /// Down vector
        /// </summary>
        public static readonly Vector3 Down = -Vector3.UnitY;
        /// <summary>
        /// Right vector
        /// </summary>
        public static readonly Vector3 Right = Vector3.UnitX;
        /// <summary>
        /// Left vector
        /// </summary>
        public static readonly Vector3 Left = -Vector3.UnitX;
        /// <summary>
        /// Forward vector (Left-Handed)
        /// </summary>
        public static readonly Vector3 ForwardLH = Vector3.UnitZ;
        /// <summary>
        /// Backward vector (Left-Handed)
        /// </summary>
        public static readonly Vector3 BackwardLH = -Vector3.UnitZ;
        /// <summary>
        /// Forward vector (Right-Handed)
        /// </summary>  
        public static readonly Vector3 ForwardRH = -Vector3.UnitZ;
        /// <summary>
        /// Backward vector (Right-Handed)
        /// </summary>
        public static readonly Vector3 BackwardRH = Vector3.UnitZ;

        /// <summary>
        /// Gets the signed angle between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns the angle value in radians</returns>
        public static float AngleSigned(Vector3 one, Vector3 two)
        {
            float dot = Vector3.Dot(one, two);

            return MathF.Acos(Math.Clamp(dot, -1f, 1f));
        }
        /// <summary>
        /// Gets the angle between two vectors in the same plane
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <param name="planeNormal">Plane normal</param>
        /// <returns>Returns the angle value in radians</returns>
        public static float AngleSigned(Vector3 one, Vector3 two, Vector3 planeNormal)
        {
            var p = new Plane(planeNormal, 0);

            float dot = Vector3.Dot(Vector3.Normalize(one), Vector3.Normalize(two));

            float angle = MathF.Acos(Math.Clamp(dot, 0, 1));

            var cross = Vector3.Cross(one, two);

            if (Vector3.Dot(p.Normal, cross) > 0)
            {
                angle = -angle;
            }

            return angle;
        }
        /// <summary>
        /// Gets the yaw and pitch values from vector
        /// </summary>
        /// <param name="vec">Vector</param>
        /// <param name="yaw">Yaw value</param>
        /// <param name="pitch">Pitch value</param>
        public static void GetAnglesFromVector(Vector3 vec, out float yaw, out float pitch)
        {
            yaw = MathF.Atan2(vec.X, vec.Y);

            if (yaw < 0.0f)
            {
                yaw += MathHelper.TwoPi;
            }

            if (MathF.Abs(vec.X) > MathF.Abs(vec.Y))
            {
                pitch = MathF.Atan2(MathF.Abs(vec.Z), MathF.Abs(vec.X));
            }
            else
            {
                pitch = MathF.Atan2(MathF.Abs(vec.Z), MathF.Abs(vec.Y));
            }

            if (vec.Z < 0.0f)
            {
                pitch = -pitch;
            }
        }
        /// <summary>
        /// Get the vector from yaw and pitch angles
        /// </summary>
        /// <param name="yaw">Yaw angle</param>
        /// <param name="pitch">Pitch angle</param>
        /// <param name="vec">Vector</param>
        public static void GetVectorFromAngles(float yaw, float pitch, out Vector3 vec)
        {
            Quaternion rot = Quaternion.CreateFromYawPitchRoll(-yaw, pitch, 0.0f);
            Matrix4x4 mat = Matrix4x4.CreateFromQuaternion(rot);

            vec = Vector3.Transform(Vector3Extensions.Up, mat);
        }

        /// <summary>
        /// Gets whether almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Vector3 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z);
        }
        /// <summary>
        /// Gets whether almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector3 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z);
        }

        public static Vector3 AdjustContrast(Vector3 value, float contrast)
        {
            return new(
                0.5f + contrast * (value.X - 0.5f),
                0.5f + contrast * (value.Y - 0.5f),
                0.5f + contrast * (value.Z - 0.5f));
        }
        public static Vector3 AdjustSaturation(Vector3 value, float saturation)
        {
            float grey = value.X * 0.2125f + value.Y * 0.7154f + value.Z * 0.0721f;

            return new Vector3(
                grey + saturation * (value.X - grey),
                grey + saturation * (value.Y - grey),
                grey + saturation * (value.Z - grey));
        }

        public static Vector3 Sum(Vector3 a, float b)
        {
            return new Vector3(
                a.X + b,
                a.Y + b,
                a.Z + b);
        }

        public static bool IsNormalized(this Vector3 vector)
        {
            return MathHelper.IsOne((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
        }
        public static void Normalize(this ref Vector3 vector)
        {
            vector = Vector3.Normalize(vector);
        }

        public static void Project(ref Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix4x4 worldViewProjection, out Vector3 result)
        {
            TransformCoordinate(ref vector, ref worldViewProjection, out var v);

            result = new Vector3(((1.0f + v.X) * 0.5f * width) + x, ((1.0f - v.Y) * 0.5f * height) + y, (v.Z * (maxZ - minZ)) + minZ);
        }
        public static Vector3 Project(Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix4x4 worldViewProjection)
        {
            Project(ref vector, x, y, width, height, minZ, maxZ, ref worldViewProjection, out Vector3 result);
            return result;
        }
        public static void Unproject(ref Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix4x4 worldViewProjection, out Vector3 result)
        {
            Vector3 v = new();
            Matrix4x4.Invert(worldViewProjection, out Matrix4x4 matrix);

            v.X = (((vector.X - x) / width) * 2.0f) - 1.0f;
            v.Y = -((((vector.Y - y) / height) * 2.0f) - 1.0f);
            v.Z = (vector.Z - minZ) / (maxZ - minZ);

            TransformCoordinate(ref v, ref matrix, out result);
        }
        public static Vector3 Unproject(Vector3 vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix4x4 worldViewProjection)
        {
            Unproject(ref vector, x, y, width, height, minZ, maxZ, ref worldViewProjection, out Vector3 result);
            return result;
        }

        public static void Transform(ref Vector3 vector, ref Matrix4x4 transform, out Vector4 result)
        {
            result = new Vector4(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + transform.M41,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + transform.M42,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + transform.M43,
                (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + transform.M44);
        }
        public static Vector4 Transform(Vector3 vector, Matrix4x4 transform)
        {
            Transform(ref vector, ref transform, out Vector4 result);
            return result;
        }

        public static void TransformCoordinate(ref Vector3 coord, ref Matrix4x4 trn, out Vector3 result)
        {
            Vector4 vector = new()
            {
                X = (coord.X * trn.M11) + (coord.Y * trn.M21) + (coord.Z * trn.M31) + trn.M41,
                Y = (coord.X * trn.M12) + (coord.Y * trn.M22) + (coord.Z * trn.M32) + trn.M42,
                Z = (coord.X * trn.M13) + (coord.Y * trn.M23) + (coord.Z * trn.M33) + trn.M43,
                W = 1f / ((coord.X * trn.M14) + (coord.Y * trn.M24) + (coord.Z * trn.M34) + trn.M44)
            };

            result = new Vector3(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W);
        }
        public static Vector3 TransformCoordinate(Vector3 coord, Matrix4x4 trn)
        {
            TransformCoordinate(ref coord, ref trn, out var res);
            return res;
        }
        public static void TransformCoordinate(Vector3[] source, ref Matrix4x4 trn, Vector3[] destination)
        {
            for (int i = 0; i < source.Length; ++i)
            {
                TransformCoordinate(ref source[i], ref trn, out destination[i]);
            }
        }

        /// <summary>
        /// Project the point in the vector
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="point">Point</param>
        public static Vector3 ProjectPoint(this Vector3 vector, Vector3 point)
        {
            // Normalize the projection vector
            if (!vector.IsNormalized())
            {
                vector = Vector3.Normalize(vector);
            }

            // Calculate the dot product between the point and the projection vector
            float dotProduct = Vector3.Dot(point, vector);

            // Calculate the projection of the point onto the projection vector
            return dotProduct * vector;
        }

        /// <summary>
        /// Limits the vector length to specified magnitude
        /// </summary>
        /// <param name="vector">Vector to limit</param>
        /// <param name="magnitude">Magnitude</param>
        /// <returns></returns>
        public static Vector3 Limit(this Vector3 vector, float magnitude)
        {
            if (vector.Length() > magnitude)
            {
                return Vector3.Normalize(vector) * magnitude;
            }

            return vector;
        }

        /// <summary>
        /// Gets the angle between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns the angle value in radians</returns>
        public static float Angle(Vector3 one, Vector3 two)
        {
            float dot = Vector3.Dot(one, two);

            return MathF.Acos(MathF.Min(MathF.Abs(dot), 1f));
        }

        public static bool NearEqual(Vector3 a, Vector3 b)
        {
            return NearEqual(a, b, Helper.ZeroToleranceVector3);
        }
        public static bool NearEqual(Vector3 a, Vector3 b, Vector3 epsilon)
        {
            return
                MathHelper.WithinEpsilon(a.X, b.X, epsilon.X) &&
                MathHelper.WithinEpsilon(a.Y, b.Y, epsilon.Y) &&
                MathHelper.WithinEpsilon(a.Z, b.Z, epsilon.Z);
        }

        /// <summary>
        /// Returns rgba components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns rgba components from Vector3</returns>
        public static Vector4 RGB(this Vector3 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z, 1f);
        }
        /// <summary>
        /// Returns xxx components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns xxx components from Vector3</returns>
        public static Vector3 XXX(this Vector3 vector)
        {
            return new Vector3(vector.X, vector.X, vector.X);
        }
        /// <summary>
        /// Returns yyy components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns yyy components from Vector3</returns>
        public static Vector3 YYY(this Vector3 vector)
        {
            return new Vector3(vector.Y, vector.Y, vector.Y);
        }
        /// <summary>
        /// Returns zzz components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns zzz components from Vector3</returns>
        public static Vector3 ZZZ(this Vector3 vector)
        {
            return new Vector3(vector.Z, vector.Z, vector.Z);
        }

        /// <summary>
        /// Gets vector description
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Return vector description</returns>
        public static string GetDescription(this Vector3 vector)
        {
            vector.X = MathF.Round(vector.X, 3);
            vector.Y = MathF.Round(vector.Y, 3);
            vector.Z = MathF.Round(vector.Z, 3);

            return string.Format("X:{0:0.000}; Y:{1:0.000}; Z:{2:0.000}", vector.X, vector.Y, vector.Z);
        }
    }
}
