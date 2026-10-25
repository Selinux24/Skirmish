using System.Numerics;

namespace Engine
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Returns rgb components from Color
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns rgb components from Vector3</returns>
        public static Vector3 RGB(this System.Drawing.Color color)
        {
            return new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
        }
        /// <summary>
        /// Creates a new rgb color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <returns>Returns rgb components from Vector3</returns>
        public static Vector3 RGB(byte r, byte g, byte b)
        {
            return new Vector3(r / 255f, g / 255f, b / 255f);
        }

        /// <summary>
        /// Returns rgba components from Color
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>Returns rgb components from Vector4</returns>
        public static Vector4 RGBA(this System.Drawing.Color color)
        {
            return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
        /// <summary>
        /// Creates a new rgb color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        /// <returns>Returns rgb components from Vector4</returns>
        public static Vector4 RGBA(byte r, byte g, byte b, byte a)
        {
            return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        /// <summary>
        /// Converts an integer value to Color4
        /// </summary>
        /// <param name="value">Integer value</param>
        /// <param name="alpha">Alpha value from 0 to 255</param>
        /// <returns>Returns the Color4 value</returns>
        public static Vector4 IntToCol(int value, int alpha)
        {
            int r = Bit(value, 0) + Bit(value, 3) * 2 + 1;
            int g = Bit(value, 1) + Bit(value, 4) * 2 + 1;
            int b = Bit(value, 2) + Bit(value, 5) * 2 + 1;

            return new Vector4(
                1 - r * 63.0f / 255.0f,
                1 - g * 63.0f / 255.0f,
                1 - b * 63.0f / 255.0f,
                alpha / 255.0f);
        }
        /// <summary>
        /// Bitwise secret wisdoms
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int Bit(int a, int b)
        {
            return (a & (1 << b)) >> b;
        }
    }
}
