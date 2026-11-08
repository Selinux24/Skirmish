using Engine.Mathematics;
using System;
using System.Globalization;

namespace Engine.Content
{
    /// <summary>
    /// Content helper class
    /// </summary>
    public static class ContentHelper
    {
        const string Position3Format = "{0} {1} {2}";
        const string Position4Format = "{0} {1} {2} {3}";
        const string PositionZero = "Zero";
        const string PositionMax = "Max";
        const string PositionMin = "Min";

        const string RotationFormat = "{0} {1} {2} {3}";
        const string RotationPrefix = "Rot";
        const string RotationIdentity = "Identity";

        const string ScaleFormat = "{0} {1} {2}";
        const string ScaleOne = "One";
        const string ScaleTwo = "Two";
        const string ScaleHalf = "1/2";
        const string ScaleOverThree = "1/3";
        const string ScaleOverFour = "1/4";
        const string ScaleOverFive = "1/5";

        const string DirectionFormat = "{0} {1} {2}";
        const string DirectionUp = "Up";
        const string DirectionDown = "Down";
        const string DirectionForward = "Forward";
        const string DirectionBackward = "Backward";
        const string DirectionForwardLH = "ForwardLH";
        const string DirectionBackwardLH = "BackwardLH";
        const string DirectionForwardRH = "ForwardRH";
        const string DirectionBackwardRH = "BackwardRH";
        const string DirectionLeft = "Left";
        const string DirectionRight = "Right";

        const string ColorFormat = "{0} {1} {2} {3}";
        const string ColorTransparent = "Transparent";
        const string ColorWhite = "White";
        const string ColorBlack = "Black";

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
            if (string.IsNullOrWhiteSpace(text)) return [];

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

            return allOk ? res : [];
        }

        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        private static Vector3 ReadReservedWordsForPosition3(string value)
        {
            if (string.Equals(value, PositionZero, StringComparison.OrdinalIgnoreCase)) return Vector3.Zero;
            if (string.Equals(value, PositionMax, StringComparison.OrdinalIgnoreCase)) return new(float.MaxValue);
            if (string.Equals(value, PositionMin, StringComparison.OrdinalIgnoreCase)) return new(float.MinValue);
            return Vector3.Zero;
        }
        /// <summary>
        /// Parse value for position reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Position</returns>
        private static Vector4 ReadReservedWordsForPosition4(string value)
        {
            if (string.Equals(value, PositionZero, StringComparison.OrdinalIgnoreCase)) return Vector4.Zero;
            if (string.Equals(value, PositionMax, StringComparison.OrdinalIgnoreCase)) return new(float.MaxValue);
            if (string.Equals(value, PositionMin, StringComparison.OrdinalIgnoreCase)) return new(float.MinValue);
            return Vector4.Zero;
        }
        /// <summary>
        /// Parse value for rotation reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed rotation</returns>
        private static Quaternion ReadReservedWordsForRotation(string value)
        {
            if (string.IsNullOrEmpty(value)) return Quaternion.Identity;
            if (string.Equals(value, RotationIdentity, StringComparison.OrdinalIgnoreCase)) return Quaternion.Identity;
            if (value.StartsWith(RotationPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var degrees = value[3..];

                if (float.TryParse(degrees, NumberStyles.Float, Local, out float d))
                {
                    return Quaternion.RotationAxis(Vector3.Up, Helper.DegreesToRadians(d));
                }
            }

            return Quaternion.Identity;
        }
        /// <summary>
        /// Parse value for scale reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed scale</returns>
        private static Vector3 ReadReservedWordsForScale(string value)
        {
            if (string.Equals(value, ScaleOne, StringComparison.OrdinalIgnoreCase)) return Vector3.One;
            if (string.Equals(value, ScaleTwo, StringComparison.OrdinalIgnoreCase)) return new Vector3(2f);
            if (string.Equals(value, ScaleOverFive, StringComparison.OrdinalIgnoreCase)) return new Vector3(1f / 5f);
            if (string.Equals(value, ScaleOverFour, StringComparison.OrdinalIgnoreCase)) return new Vector3(1f / 4f);
            if (string.Equals(value, ScaleOverThree, StringComparison.OrdinalIgnoreCase)) return new Vector3(1f / 3f);
            if (string.Equals(value, ScaleHalf, StringComparison.OrdinalIgnoreCase)) return new Vector3(1f / 2f);
            return Vector3.One;
        }
        /// <summary>
        /// Parse value for direction reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Direction</returns>
        private static Vector3 ReadReservedWordsForDirection(string value)
        {
            if (string.Equals(value, DirectionUp, StringComparison.OrdinalIgnoreCase)) return Vector3.Up;
            if (string.Equals(value, DirectionDown, StringComparison.OrdinalIgnoreCase)) return Vector3.Down;
            if (string.Equals(value, DirectionForward, StringComparison.OrdinalIgnoreCase)) return Vector3.ForwardLH;
            if (string.Equals(value, DirectionBackward, StringComparison.OrdinalIgnoreCase)) return Vector3.BackwardLH;
            if (string.Equals(value, DirectionForwardLH, StringComparison.OrdinalIgnoreCase)) return Vector3.ForwardLH;
            if (string.Equals(value, DirectionBackwardLH, StringComparison.OrdinalIgnoreCase)) return Vector3.BackwardLH;
            if (string.Equals(value, DirectionForwardRH, StringComparison.OrdinalIgnoreCase)) return Vector3.ForwardRH;
            if (string.Equals(value, DirectionBackwardRH, StringComparison.OrdinalIgnoreCase)) return Vector3.BackwardRH;
            if (string.Equals(value, DirectionLeft, StringComparison.OrdinalIgnoreCase)) return Vector3.Left;
            if (string.Equals(value, DirectionRight, StringComparison.OrdinalIgnoreCase)) return Vector3.Right;
            return Vector3.ForwardLH;
        }
        /// <summary>
        /// Parse value for color reserved words
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns>Returns parsed Color</returns>
        private static Color4 ReadReservedWordsForColor(string value)
        {
            if (string.Equals(value, ColorTransparent, StringComparison.OrdinalIgnoreCase)) return Color4.Transparent;
            if (string.Equals(value, ColorWhite, StringComparison.OrdinalIgnoreCase)) return Color4.White;
            if (string.Equals(value, ColorBlack, StringComparison.OrdinalIgnoreCase)) return Color4.Black;
            return Color4.Black;
        }

        /// <summary>
        /// Reads the position from the specified string value
        /// </summary>
        public static Vector3 ReadPosition3(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1) return new(floats[0]);
            if (floats?.Length == 3) return new(floats);
            return ReadReservedWordsForPosition3(value);
        }
        /// <summary>
        /// Reads the position from the specified string value
        /// </summary>
        public static Vector4 ReadPosition4(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1) return new(floats[0]);
            if (floats?.Length == 4) return new(floats);
            return ReadReservedWordsForPosition4(value);
        }
        /// <summary>
        /// Reads the rotation quaternion from the specified string value
        /// </summary>
        public static Quaternion ReadRotation(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1) return new(floats[0]);
            if (floats?.Length == 4) return new(floats);
            return ReadReservedWordsForRotation(value);
        }
        /// <summary>
        /// Reads the scale from the specified string value
        /// </summary>
        public static Vector3 ReadScale(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1) return new(floats[0]);
            if (floats?.Length == 3) return new(floats);
            return ReadReservedWordsForScale(value);
        }
        /// <summary>
        /// Reads the direction from the specified string value
        /// </summary>
        public static Vector3 ReadDirection(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 3) return new(floats);
            return ReadReservedWordsForDirection(value);
        }
        /// <summary>
        /// Reads the color from the specified string value
        /// </summary>
        public static Color4 ReadColor(string value)
        {
            var floats = SplitFloats(value);
            if (floats?.Length == 1) return new(floats[0]);
            if (floats?.Length == 3) return new(floats[0], floats[1], floats[2], 1f);
            if (floats?.Length == 4) return new(floats);
            return ReadReservedWordsForColor(value);
        }

        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WritePosition3(Vector3 value)
        {
            return string.Format(Local, Position3Format, value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WritePosition4(Vector4 value)
        {
            return string.Format(Local, Position4Format, value.X, value.Y, value.Z, value.W);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteRotation(Quaternion value)
        {
            return string.Format(Local, RotationFormat, value.X, value.Y, value.Z, value.W);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteScale(Vector3 value)
        {
            return string.Format(Local, ScaleFormat, value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteDirection(Vector3 value)
        {
            return string.Format(Local, DirectionFormat, value.X, value.Y, value.Z);
        }
        /// <summary>
        /// Writes the specified value into a string
        /// </summary>
        public static string WriteColor(Color4 value)
        {
            return string.Format(Local, ColorFormat, value.Red, value.Green, value.Blue, value.Alpha);
        }
    }
}
