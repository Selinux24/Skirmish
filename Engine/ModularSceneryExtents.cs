using SharpDX;
using System;
using System.Globalization;

namespace Engine
{
    /// <summary>
    /// Modular scenery extents helper class
    /// </summary>
    static class ModularSceneryExtents
    {
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        public static Vector3 ReadReservedWordsForPosition(string value)
        {
            if (string.Equals(value, "Zero", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Zero;
            }
            else if (string.Equals(value, "Max", StringComparison.OrdinalIgnoreCase))
            {
                return new Vector3(float.MaxValue);
            }
            else if (string.Equals(value, "Min", StringComparison.OrdinalIgnoreCase))
            {
                return new Vector3(float.MinValue);
            }
            else
            {
                return Vector3.Zero;
            }
        }
        /// <summary>
        /// Parse value for rotation reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed rotation</returns>
        public static Quaternion ReadReservedWordsForRotation(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (string.Equals(value, "Identity", StringComparison.OrdinalIgnoreCase))
                {
                    return Quaternion.Identity;
                }
                else if (value.StartsWith("Rot", StringComparison.OrdinalIgnoreCase))
                {
                    var degrees = value.Substring(3);

                    if (float.TryParse(degrees, NumberStyles.Float, CultureInfo.InvariantCulture, out float d))
                    {
                        return Quaternion.RotationAxis(Vector3.Up, MathUtil.DegreesToRadians(d));
                    }
                }
            }

            return Quaternion.Identity;
        }
        /// <summary>
        /// Parse value for scale reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed scale</returns>
        public static Vector3 ReadReservedWordsForScale(string value)
        {
            if (string.Equals(value, "One", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One;
            }
            else if (string.Equals(value, "Two", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One * 2f;
            }
            else if (string.Equals(value, "1/5", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One / 5f;
            }
            else if (string.Equals(value, "1/4", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One / 4f;
            }
            else if (string.Equals(value, "1/3", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One / 3f;
            }
            else if (string.Equals(value, "1/2", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.One / 2f;
            }
            else
            {
                return Vector3.One;
            }
        }
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        public static Vector3 ReadReservedWordsForDirection(string value)
        {
            if (string.Equals(value, "Up", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Up;
            }
            else if (string.Equals(value, "Down", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Down;
            }
            else if (string.Equals(value, "Forward", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.ForwardLH;
            }
            else if (string.Equals(value, "Backward", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.BackwardLH;
            }
            else if (string.Equals(value, "Left", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Left;
            }
            else if (string.Equals(value, "Right", StringComparison.OrdinalIgnoreCase))
            {
                return Vector3.Right;
            }
            else
            {
                return Vector3.Zero;
            }
        }

        /// <summary>
        /// Gets the asset transform
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        /// <returns>Returns a matrix with the reference transform</returns>
        public static Matrix Transformation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return Matrix.Transformation(
                Vector3.Zero, Quaternion.Identity, scale,
                Vector3.Zero, rotation,
                position);
        }
    }
}
