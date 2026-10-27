using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Vortice.Mathematics;

namespace Engine
{
    /// <summary>
    /// Quaternion extensions
    /// </summary>
    public static class QuaternionExtensions
    {
        public class QuaternionComparer : IEqualityComparer<Quaternion>
        {
            public bool Equals(Quaternion x, Quaternion y)
            {
                return QuaternionExtensions.NearEqual(x, y);
            }
            public int GetHashCode([DisallowNull] Quaternion obj)
            {
                return obj.GetHashCode();
            }
        }

        public static readonly QuaternionComparer Comparer = new();

        /// <summary>
        /// Gets the angle between two quaternions
        /// </summary>
        /// <param name="one">First quaternions</param>
        /// <param name="two">Second quaternions</param>
        /// <returns>Returns the angle value in radians</returns>
        public static float Angle(Quaternion one, Quaternion two)
        {
            float dot = Quaternion.Dot(one, two);

            return MathF.Acos(MathF.Min(MathF.Abs(dot), 1f)) * 2f;
        }

        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <returns>Returns rotation quaternion</returns>
        public static Quaternion CreateLookAtLeftHanded(Vector3 eyePosition, Vector3 target, Vector3 up)
        {
            var matrix = Matrix4x4.CreateLookAtLeftHanded(eyePosition, target, up);
            return Quaternion.CreateFromRotationMatrix(matrix);
        }
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <param name="axis">Relative rotation axis</param>
        /// <returns>Returns rotation quaternion</returns>
        public static Quaternion CreateLookAtLeftHanded(Vector3 eyePosition, Vector3 target, Vector3 up, Axis axis)
        {
            if (MathHelper.IsOne(Vector3.Dot(Vector3Extensions.Up, Vector3.Normalize(eyePosition - target))))
            {
                up = Vector3Extensions.Left;
            }

            Quaternion q = Quaternion.Inverse(CreateLookAtLeftHanded(target, eyePosition, up));

            if (axis != Axis.None)
            {
                q = q.ClampToAxis(axis);
            }

            return q;
        }
        /// <summary>
        /// Clamps the rotation to the specified axis
        /// </summary>
        /// <param name="q">Quaternion</param>
        /// <param name="axis">Axis</param>
        /// <returns>Returns the clamped quaternion</returns>
        public static Quaternion ClampToAxis(this Quaternion q, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    q.Y = 0;
                    q.Z = 0;
                    q = Quaternion.Normalize(q);
                    break;
                case Axis.Y:
                    q.X = 0;
                    q.Z = 0;
                    q = Quaternion.Normalize(q);
                    break;
                case Axis.Z:
                    q.X = 0;
                    q.Y = 0;
                    q = Quaternion.Normalize(q);
                    break;
            }

            return q;
        }
        /// <summary>
        /// Finds the quaternion between from and to quaternions traveling maxDelta radians
        /// </summary>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        /// <param name="maxDelta">Maximum radians</param>
        /// <returns>Gets the quaternion between from and to quaternions traveling maxDelta radians</returns>
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDelta)
        {
            float angle = Angle(from, to);
            if (MathHelper.IsZero(angle))
            {
                return to;
            }

            float delta = MathF.Min(1f, maxDelta / angle);

            return Quaternion.Slerp(from, to, delta);
        }
        /// <summary>
        /// Creates a rotation from the direction vector from the up vector
        /// </summary>
        /// <param name="direction">Direction vector</param>
        /// <param name="up">Up vector</param>
        /// <returns>Returns a rotation quaternion</returns>
        public static Quaternion RotateFromDirection(Vector3 direction, Vector3 up)
        {
            var a = Vector3.Cross(direction, up);
            var w = MathF.Sqrt(direction.LengthSquared() * up.LengthSquared() + Vector3.Dot(direction, up));
            Quaternion q = new(a, w);
            q = Quaternion.Normalize(q);

            return q;
        }
        public static Quaternion RotateAxis(Vector3 axis, float angle)
        {
            return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), angle);
        }
        /// <summary>
        /// Gets the quaternion axis
        /// </summary>
        /// <param name="q">Quaternion</param>
        /// <returns>Returns the quaternion axis vector</returns>
        public static Vector3 GetAxis(this Quaternion q)
        {
            float length = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z);
            if (MathHelper.IsZero(length)) return Vector3.UnitX;

            float inv = 1.0f / (float)Math.Sqrt(length);
            return new Vector3(q.X * inv, q.Y * inv, q.Z * inv);
        }
        /// <summary>
        /// Gets the quaternion angle
        /// </summary>
        /// <param name="q">Quaternion</param>
        /// <returns>Returns the quaternion angle in radians</returns>
        public static float GetAngle(this Quaternion q)
        {
            float length = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z);
            if (MathHelper.IsZero(length)) return 0.0f;

            return (float)(2.0 * Math.Acos(MathHelper.Clamp(q.W, -1f, 1f)));
        }

        public static bool NearEqual(Quaternion a, Quaternion b)
        {
            return
                Vector3Extensions.NearEqual(a.GetAxis(), b.GetAxis()) &&
                Helper.NearEqual(a.GetAngle(), b.GetAngle());
        }

        /// <summary>
        /// Gets quaternion description
        /// </summary>
        /// <param name="quaternion">Quaternion</param>
        /// <returns>Return quaternion description</returns>
        public static string GetDescription(this Quaternion quaternion)
        {
            Vector3 axis = quaternion.GetAxis();
            float angle = quaternion.GetAngle();

            return string.Format("Angle: {0:0.00} in axis {1}", angle, axis.GetDescription());
        }
    }
}
