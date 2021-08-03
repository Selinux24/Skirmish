using SharpDX;
using System;
using System.Globalization;

namespace Engine
{
    /// <summary>
    /// Modular scenery extents helper class
    /// </summary>
    static class PersistenceHelpers
    {
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        public static Position3 ReadReservedWordsForPosition3(string value)
        {
            if (string.Equals(value, "Zero", StringComparison.OrdinalIgnoreCase))
            {
                return Position3.Zero;
            }
            else if (string.Equals(value, "Max", StringComparison.OrdinalIgnoreCase))
            {
                return new Position3(float.MaxValue);
            }
            else if (string.Equals(value, "Min", StringComparison.OrdinalIgnoreCase))
            {
                return new Position3(float.MinValue);
            }
            else
            {
                return Position3.Zero;
            }
        }
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        public static Position4 ReadReservedWordsForPosition4(string value)
        {
            if (string.Equals(value, "Zero", StringComparison.OrdinalIgnoreCase))
            {
                return Position4.Zero;
            }
            else if (string.Equals(value, "Max", StringComparison.OrdinalIgnoreCase))
            {
                return new Position4(float.MaxValue);
            }
            else if (string.Equals(value, "Min", StringComparison.OrdinalIgnoreCase))
            {
                return new Position4(float.MinValue);
            }
            else
            {
                return Position4.Zero;
            }
        }
        /// <summary>
        /// Parse value for rotation reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed rotation</returns>
        public static RotationQ ReadReservedWordsForRotationQ(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (string.Equals(value, "Identity", StringComparison.OrdinalIgnoreCase))
                {
                    return RotationQ.Identity;
                }
                else if (value.StartsWith("Rot", StringComparison.OrdinalIgnoreCase))
                {
                    var degrees = value.Substring(3);

                    if (float.TryParse(degrees, NumberStyles.Float, CultureInfo.InvariantCulture, out float d))
                    {
                        return RotationQ.RotationAxis(Direction3.Up, MathUtil.DegreesToRadians(d));
                    }
                }
            }

            return RotationQ.Identity;
        }
        /// <summary>
        /// Parse value for scale reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed scale</returns>
        public static Scale3 ReadReservedWordsForScale3(string value)
        {
            if (string.Equals(value, "One", StringComparison.OrdinalIgnoreCase))
            {
                return Scale3.One;
            }
            else if (string.Equals(value, "Two", StringComparison.OrdinalIgnoreCase))
            {
                return new Scale3(2f);
            }
            else if (string.Equals(value, "1/5", StringComparison.OrdinalIgnoreCase))
            {
                return new Scale3(1f / 5f);
            }
            else if (string.Equals(value, "1/4", StringComparison.OrdinalIgnoreCase))
            {
                return new Scale3(1f / 4f);
            }
            else if (string.Equals(value, "1/3", StringComparison.OrdinalIgnoreCase))
            {
                return new Scale3(1f / 3f);
            }
            else if (string.Equals(value, "1/2", StringComparison.OrdinalIgnoreCase))
            {
                return new Scale3(1f / 2f);
            }
            else
            {
                return Scale3.One;
            }
        }
        /// <summary>
        /// Parse value for direction reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Direction</returns>
        public static Direction3 ReadReservedWordsForDirection3(string value)
        {
            if (string.Equals(value, "Up", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.Up;
            }
            else if (string.Equals(value, "Down", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.Down;
            }
            else if (string.Equals(value, "Forward", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.ForwardLH;
            }
            else if (string.Equals(value, "Backward", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.BackwardLH;
            }
            else if (string.Equals(value, "Left", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.Left;
            }
            else if (string.Equals(value, "Right", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.Right;
            }
            else
            {
                return Direction3.ForwardLH;
            }
        }
        /// <summary>
        /// Parse value for color reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Color</returns>
        public static ColorRgba ReadReservedWordsForColorRgba(string value)
        {
            if (string.Equals(value, "Transparent", StringComparison.OrdinalIgnoreCase))
            {
                return ColorRgba.Transparent;
            }
            else if (string.Equals(value, "White", StringComparison.OrdinalIgnoreCase))
            {
                return ColorRgba.White;
            }
            else if (string.Equals(value, "Black", StringComparison.OrdinalIgnoreCase))
            {
                return ColorRgba.Black;
            }
            else
            {
                return ColorRgba.Black;
            }
        }
    }
}
