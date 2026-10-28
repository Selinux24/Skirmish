using System;
using System.Numerics;
using Vortice.Mathematics;

namespace Engine.Mathematics
{
    /// <summary>
    /// Provides extension methods for the Vector2 structure.
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Performs cross product between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns the cross product</returns>
        public static float Cross(Vector2 one, Vector2 two)
        {
            return one.X * two.Y - one.Y * two.X;
        }
        /// <summary>
        /// Gets the angle between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns the angle value in radians</returns>
        public static float Angle(Vector2 one, Vector2 two)
        {
            //Get the dot product
            float dot = Vector2.Dot(one, two);

            // Divide the dot by the product of the magnitudes of the vectors
            dot /= one.Length() * two.Length();

            //Get the arc cosin of the angle, you now have your angle in radians 
            return MathF.Acos(dot);
        }
        /// <summary>
        /// Gets the signed angle between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns the angle value in radians</returns>
        public static float AngleSigned(Vector2 one, Vector2 two)
        {
            return MathF.Atan2(Cross(one, two), Vector2.Dot(one, two));
        }

        public static bool NearEqual(Vector2 a, Vector2 b)
        {
            return NearEqual(a, b, Helper.ZeroToleranceVector2);
        }
        public static bool NearEqual(Vector2 a, Vector2 b, Vector2 epsilon)
        {
            return
                MathHelper.WithinEpsilon(a.X, b.X, epsilon.X) &&
                MathHelper.WithinEpsilon(a.Y, b.Y, epsilon.Y);
        }

        /// <summary>
        /// Returns xy components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xy components from Vector4</returns>
        public static Vector2 XY(this Vector4 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        /// <summary>
        /// Returns xy components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns xy components from Vector3</returns>
        public static Vector2 XY(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        /// <summary>
        /// Returns yx components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns yx components from Vector4</returns>
        public static Vector2 YX(this Vector4 vector)
        {
            return new Vector2(vector.Y, vector.X);
        }
        /// <summary>
        /// Returns yx components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns yx components from Vector3</returns>
        public static Vector2 YX(this Vector3 vector)
        {
            return new Vector2(vector.Y, vector.X);
        }

        /// <summary>
        /// Returns xz components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xz components from Vector4</returns>
        public static Vector2 XZ(this Vector4 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }
        /// <summary>
        /// Returns xz components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns xz components from Vector3</returns>
        public static Vector2 XZ(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }
        /// <summary>
        /// Returns zx components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns zx components from Vector4</returns>
        public static Vector2 ZX(this Vector4 vector)
        {
            return new Vector2(vector.Z, vector.X);
        }
        /// <summary>
        /// Returns zx components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns zx components from Vector3</returns>
        public static Vector2 ZX(this Vector3 vector)
        {
            return new Vector2(vector.Z, vector.X);
        }

        /// <summary>
        /// Returns yz components from Vector4
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns yz components from Vector4</returns>
        public static Vector2 YZ(this Vector4 vector)
        {
            return new Vector2(vector.Y, vector.Z);
        }
        /// <summary>
        /// Returns yz components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns yz components from Vector3</returns>
        public static Vector2 YZ(this Vector3 vector)
        {
            return new Vector2(vector.Y, vector.Z);
        }
        /// <summary>
        /// Returns zy components from Vector4
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns zy components from Vector4</returns>
        public static Vector2 ZY(this Vector4 vector)
        {
            return new Vector2(vector.Z, vector.Y);
        }
        /// <summary>
        /// Returns zy components from Vector3
        /// </summary>
        /// <param name="vector">Vector3</param>
        /// <returns>Returns zy components from Vector3</returns>
        public static Vector2 ZY(this Vector3 vector)
        {
            return new Vector2(vector.Z, vector.Y);
        }
        
        /// <summary>
        /// Gets whether almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector2 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y);
        }
    }
}
