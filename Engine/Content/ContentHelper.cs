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
        /// Parse culture
        /// </summary>
        public static CultureInfo Local { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Splits the text into a float array
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Returns a float array</returns>
        private static float[] SplitFloats(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return [];
            }

            var bits = text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            bool allOk = true;
            float[] res = new float[bits.Length];

            for (int i = 0; i < res.Length; i++)
            {
                if (float.TryParse(bits[i], NumberStyles.Float, Local, out float n))
                {
                    res[i] = n;
                }
                else
                {
                    allOk = false;
                    break;
                }
            }

            if (allOk)
            {
                return res;
            }

            return [];
        }

        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        public static CntPosition3? ReadReservedWordsForPosition3(string value)
        {
            if (string.Equals(value, "Zero", StringComparison.OrdinalIgnoreCase))
            {
                return CntPosition3.Zero;
            }
            else if (string.Equals(value, "Max", StringComparison.OrdinalIgnoreCase))
            {
                return new CntPosition3(float.MaxValue);
            }
            else if (string.Equals(value, "Min", StringComparison.OrdinalIgnoreCase))
            {
                return new CntPosition3(float.MinValue);
            }

            return null;
        }
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        public static CntPosition4? ReadReservedWordsForPosition4(string value)
        {
            if (string.Equals(value, "Zero", StringComparison.OrdinalIgnoreCase))
            {
                return CntPosition4.Zero;
            }
            else if (string.Equals(value, "Max", StringComparison.OrdinalIgnoreCase))
            {
                return new CntPosition4(float.MaxValue);
            }
            else if (string.Equals(value, "Min", StringComparison.OrdinalIgnoreCase))
            {
                return new CntPosition4(float.MinValue);
            }

            return null;
        }
        /// <summary>
        /// Parse value for rotation reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed rotation</returns>
        public static CntRotationQ? ReadReservedWordsForRotationQ(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (string.Equals(value, "Identity", StringComparison.OrdinalIgnoreCase))
                {
                    return CntRotationQ.Identity;
                }
                else if (value.StartsWith("Rot", StringComparison.OrdinalIgnoreCase))
                {
                    var degrees = value[3..];

                    if (float.TryParse(degrees, NumberStyles.Float, Local, out float d))
                    {
                        return CntRotationQ.RotationAxis(CntDirection3.Up, Helper.DegreesToRadians(d));
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
        public static CntScale3? ReadReservedWordsForScale3(string value)
        {
            if (string.Equals(value, "One", StringComparison.OrdinalIgnoreCase))
            {
                return CntScale3.One;
            }
            else if (string.Equals(value, "Two", StringComparison.OrdinalIgnoreCase))
            {
                return new CntScale3(2f);
            }
            else if (string.Equals(value, "1/5", StringComparison.OrdinalIgnoreCase))
            {
                return new CntScale3(1f / 5f);
            }
            else if (string.Equals(value, "1/4", StringComparison.OrdinalIgnoreCase))
            {
                return new CntScale3(1f / 4f);
            }
            else if (string.Equals(value, "1/3", StringComparison.OrdinalIgnoreCase))
            {
                return new CntScale3(1f / 3f);
            }
            else if (string.Equals(value, "1/2", StringComparison.OrdinalIgnoreCase))
            {
                return new CntScale3(1f / 2f);
            }

            return null;
        }
        /// <summary>
        /// Parse value for direction reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Direction</returns>
        public static CntDirection3? ReadReservedWordsForDirection3(string value)
        {
            if (string.Equals(value, "Up", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.Up;
            }
            else if (string.Equals(value, "Down", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.Down;
            }
            else if (string.Equals(value, "Forward", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.ForwardLH;
            }
            else if (string.Equals(value, "Backward", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.BackwardLH;
            }
            else if (string.Equals(value, "ForwardLH", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.ForwardLH;
            }
            else if (string.Equals(value, "BackwardLH", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.BackwardLH;
            }
            else if (string.Equals(value, "ForwardRH", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.ForwardRH;
            }
            else if (string.Equals(value, "BackwardRH", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.BackwardRH;
            }
            else if (string.Equals(value, "Left", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.Left;
            }
            else if (string.Equals(value, "Right", StringComparison.OrdinalIgnoreCase))
            {
                return CntDirection3.Right;
            }

            return null;
        }
        /// <summary>
        /// Parse value for color reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Color</returns>
        public static CntColor? ReadReservedWordsForColorRgba(string value)
        {
            if (string.Equals(value, "Transparent", StringComparison.OrdinalIgnoreCase))
            {
                return CntColor.Transparent;
            }
            else if (string.Equals(value, "White", StringComparison.OrdinalIgnoreCase))
            {
                return CntColor.White;
            }
            else if (string.Equals(value, "Black", StringComparison.OrdinalIgnoreCase))
            {
                return CntColor.Black;
            }

            return null;
        }

        /// <summary>
        /// Reads the position from the specified string value
        /// </summary>
        public static CntPosition3? ReadPosition3(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1)
            {
                return new CntPosition3(floats[0]);
            }
            else if (floats?.Length == 3)
            {
                return new CntPosition3(floats);
            }
            else
            {
                return ReadReservedWordsForPosition3(value);
            }
        }
        /// <summary>
        /// Reads the position from the specified string value
        /// </summary>
        public static CntPosition4? ReadPosition4(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1)
            {
                return new CntPosition4(floats[0]);
            }
            else if (floats?.Length == 4)
            {
                return new CntPosition4(floats);
            }
            else
            {
                return ReadReservedWordsForPosition4(value);
            }
        }
        /// <summary>
        /// Reads the rotation quaternion from the specified string value
        /// </summary>
        public static CntRotationQ? ReadRotationQ(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1)
            {
                return new CntRotationQ(floats[0]);
            }
            else if (floats?.Length == 4)
            {
                return new CntRotationQ(floats);
            }
            else
            {
                return ReadReservedWordsForRotationQ(value);
            }
        }
        /// <summary>
        /// Reads the scale from the specified string value
        /// </summary>
        public static CntScale3? ReadScale3(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1)
            {
                return new CntScale3(floats[0]);
            }
            else if (floats?.Length == 3)
            {
                return new CntScale3(floats);
            }
            else
            {
                return ReadReservedWordsForScale3(value);
            }
        }
        /// <summary>
        /// Reads the direction from the specified string value
        /// </summary>
        public static CntDirection3? ReadDirection3(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 3)
            {
                return new CntDirection3(floats);
            }
            else
            {
                return ReadReservedWordsForDirection3(value);
            }
        }
        /// <summary>
        /// Reads the color from the specified string value
        /// </summary>
        public static CntColor? ReadColorRgba(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1)
            {
                return new CntColor(floats[0]);
            }
            else if (floats?.Length == 3)
            {
                return new CntColor(floats[0], floats[1], floats[2], 1f);
            }
            else if (floats?.Length == 4)
            {
                return new CntColor(floats);
            }
            else
            {
                return ReadReservedWordsForColorRgba(value);
            }
        }

        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WritePosition3(CntPosition3 value)
        {
            return string.Format(Local, "{0} {1} {2}", value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WritePosition4(CntPosition4 value)
        {
            return string.Format(Local, "{0} {1} {2} {3}", value.X, value.Y, value.Z, value.W);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteRotationQ(CntRotationQ value)
        {
            return string.Format(Local, "{0} {1} {2} {3}", value.X, value.Y, value.Z, value.W);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteScale3(CntScale3 value)
        {
            return string.Format(Local, "{0} {1} {2}", value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteDirection3(CntDirection3 value)
        {
            return string.Format(Local, "{0} {1} {2}", value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteColorRgba(CntColor value)
        {
            return string.Format(Local, "{0} {1} {2} {3}", value.R, value.G, value.B, value.A);
        }
    }
}
