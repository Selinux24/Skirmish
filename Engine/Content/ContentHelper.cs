using SharpDX;
using System;
using System.Globalization;

namespace Engine.Content
{
    /// <summary>
    /// Content helper class
    /// </summary>
    static class ContentHelper
    {
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        public static Position3? ReadReservedWordsForPosition3(string value)
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

            return null;
        }
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        public static Position4? ReadReservedWordsForPosition4(string value)
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

            return null;
        }
        /// <summary>
        /// Parse value for rotation reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed rotation</returns>
        public static RotationQ? ReadReservedWordsForRotationQ(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (string.Equals(value, "Identity", StringComparison.OrdinalIgnoreCase))
                {
                    return RotationQ.Identity;
                }
                else if (value.StartsWith("Rot", StringComparison.OrdinalIgnoreCase))
                {
                    var degrees = value[3..];

                    if (float.TryParse(degrees, NumberStyles.Float, CultureInfo.InvariantCulture, out float d))
                    {
                        return RotationQ.RotationAxis(Direction3.Up, MathUtil.DegreesToRadians(d));
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Parse value for scale reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed scale</returns>
        public static Scale3? ReadReservedWordsForScale3(string value)
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

            return null;
        }
        /// <summary>
        /// Parse value for direction reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Direction</returns>
        public static Direction3? ReadReservedWordsForDirection3(string value)
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
            else if (string.Equals(value, "ForwardLH", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.ForwardLH;
            }
            else if (string.Equals(value, "BackwardLH", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.BackwardLH;
            }
            else if (string.Equals(value, "ForwardRH", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.ForwardRH;
            }
            else if (string.Equals(value, "BackwardRH", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.BackwardRH;
            }
            else if (string.Equals(value, "Left", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.Left;
            }
            else if (string.Equals(value, "Right", StringComparison.OrdinalIgnoreCase))
            {
                return Direction3.Right;
            }

            return null;
        }
        /// <summary>
        /// Parse value for color reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Color</returns>
        public static ColorRgba? ReadReservedWordsForColorRgba(string value)
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

            return null;
        }

        /// <summary>
        /// Reads the position from the specified string value
        /// </summary>
        public static Position3? ReadPosition3(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 1)
            {
                return new Position3(floats[0]);
            }
            else if (floats?.Length == 3)
            {
                return new Position3(floats);
            }
            else
            {
                return ReadReservedWordsForPosition3(value);
            }
        }
        /// <summary>
        /// Reads the position from the specified string value
        /// </summary>
        public static Position4? ReadPosition4(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 1)
            {
                return new Position4(floats[0]);
            }
            else if (floats?.Length == 4)
            {
                return new Position4(floats);
            }
            else
            {
                return ReadReservedWordsForPosition4(value);
            }
        }
        /// <summary>
        /// Reads the rotation quaternion from the specified string value
        /// </summary>
        public static RotationQ? ReadRotationQ(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 1)
            {
                return new RotationQ(floats[0]);
            }
            else if (floats?.Length == 4)
            {
                return new RotationQ(floats);
            }
            else
            {
                return ReadReservedWordsForRotationQ(value);
            }
        }
        /// <summary>
        /// Reads the scale from the specified string value
        /// </summary>
        public static Scale3? ReadScale3(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 1)
            {
                return new Scale3(floats[0]);
            }
            else if (floats?.Length == 3)
            {
                return new Scale3(floats);
            }
            else
            {
                return ReadReservedWordsForScale3(value);
            }
        }
        /// <summary>
        /// Reads the direction from the specified string value
        /// </summary>
        public static Direction3? ReadDirection3(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 3)
            {
                return new Direction3(floats);
            }
            else
            {
                return ReadReservedWordsForDirection3(value);
            }
        }
        /// <summary>
        /// Reads the color from the specified string value
        /// </summary>
        public static ColorRgba? ReadColorRgba(string value)
        {
            var floats = value?.SplitFloats();
            if (floats?.Length == 1)
            {
                return new ColorRgba(floats[0]);
            }
            else if (floats?.Length == 3)
            {
                return new ColorRgba(floats[0], floats[1], floats[2], 1f);
            }
            else if (floats?.Length == 4)
            {
                return new ColorRgba(floats);
            }
            else
            {
                return ReadReservedWordsForColorRgba(value);
            }
        }

        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WritePosition3(Position3 value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WritePosition4(Position4 value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", value.X, value.Y, value.Z, value.W);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteRotationQ(RotationQ value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", value.X, value.Y, value.Z, value.W);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteScale3(Scale3 value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteDirection3(Direction3 value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteColorRgba(ColorRgba value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", value.R, value.G, value.B, value.A);
        }
    }
}
