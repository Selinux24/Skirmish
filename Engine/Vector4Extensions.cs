using System.Numerics;
using Vortice.Mathematics;

namespace Engine
{
    /// <summary>
    /// Provides extension methods for the Vector4 structure.
    /// </summary>
    public static class Vector4Extensions
    {
        /// <summary>
        /// Creates a new rgba color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        public static Vector4 FromRgba(byte r, byte g, byte b, byte a)
        {
            return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
        }
        /// <summary>
        /// Creates a new rgb color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        public static Vector4 FromRgb(byte r, byte g, byte b)
        {
            return new Vector4(r / 255f, g / 255f, b / 255f, 1f);
        }

        /// <summary>
        /// Gets whether almost one of the instance attributes is not a number
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is not a number</returns>
        public static bool IsNaN(this Vector4 vector)
        {
            return float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z) || float.IsNaN(vector.W);
        }
        /// <summary>
        /// Gets whether almost one of the instance attributes is infinity
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <returns>Returns true if almost one of the instance attributes is infinity</returns>
        public static bool IsInfinity(this Vector4 vector)
        {
            return float.IsInfinity(vector.X) || float.IsInfinity(vector.Y) || float.IsInfinity(vector.Z) || float.IsInfinity(vector.W);
        }

        public static bool NearEqual(Vector4 a, Vector4 b)
        {
            return NearEqual(a, b, Helper.ZeroToleranceVector4);
        }
        public static bool NearEqual(Vector4 a, Vector4 b, Vector4 epsilon)
        {
            return
                MathHelper.WithinEpsilon(a.X, b.X, epsilon.X) &&
                MathHelper.WithinEpsilon(a.Y, b.Y, epsilon.Y) &&
                MathHelper.WithinEpsilon(a.Z, b.Z, epsilon.Z) &&
                MathHelper.WithinEpsilon(a.W, b.W, epsilon.W);
        }

        public static Vector4 AdjustContrast(Vector4 value, float contrast)
        {
            return new(
                0.5f + contrast * (value.X - 0.5f),
                0.5f + contrast * (value.Y - 0.5f),
                0.5f + contrast * (value.Z - 0.5f),
                value.W);
        }
        public static Vector4 AdjustSaturation(Vector4 value, float saturation)
        {
            float grey = value.X * 0.2125f + value.Y * 0.7154f + value.Z * 0.0721f;

            return new Vector4(
                grey + saturation * (value.X - grey),
                grey + saturation * (value.Y - grey),
                grey + saturation * (value.Z - grey),
                value.W);
        }

        /// <summary>
        /// Returns rgb components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns rgb components from Vector4</returns>
        public static Vector3 RGB(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        /// <summary>
        /// Returns xyz components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xyz components from Vector4</returns>
        public static Vector3 XYZ(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        /// <summary>
        /// Returns xxx components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns xxx components from Vector4</returns>
        public static Vector3 XXX(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.X, vector.X);
        }
        /// <summary>
        /// Returns yyy components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns yyy components from Vector4</returns>
        public static Vector3 YYY(this Vector4 vector)
        {
            return new Vector3(vector.Y, vector.Y, vector.Y);
        }
        /// <summary>
        /// Returns zzz components from Vector4
        /// </summary>
        /// <param name="vector">Vector4</param>
        /// <returns>Returns zzz components from Vector4</returns>
        public static Vector3 ZZZ(this Vector4 vector)
        {
            return new Vector3(vector.Z, vector.Z, vector.Z);
        }
    }
}
