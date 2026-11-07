// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Represents a 32-bit color (4 bytes) in the form of RGBA (in byte order: R, G, B, A).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public partial struct Color : IEquatable<Color>, IFormattable
    {
        private const string ColorStr = "A:{0} R:{1} G:{2} B:{3}";

        private static byte FloatToByte(float component)
        {
            return IntToByte((int)(component * 255.0f));
        }
        private static byte IntToByte(int value)
        {
            return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
        }

        /// <summary>
        /// The red component of the color.
        /// </summary>
        public byte R;
        /// <summary>
        /// The green component of the color.
        /// </summary>
        public byte G;
        /// <summary>
        /// The blue component of the color.
        /// </summary>
        public byte B;
        /// <summary>
        /// The alpha component of the color.
        /// </summary>
        public byte A;

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the alpha, red, green, or blue component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the alpha component, 1 for the red component, 2 for the green component, and 3 for the blue component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        public byte this[int index]
        {
            readonly get => index switch
            {
                0 => R,
                1 => G,
                2 => B,
                3 => A,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Indices for Color run from 0 to 3, inclusive."),
            };
            set
            {
                switch (index)
                {
                    case 0: R = value; break;
                    case 1: G = value; break;
                    case 2: B = value; break;
                    case 3: A = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Indices for Color run from 0 to 3, inclusive.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Color(byte value)
        {
            A = R = G = B = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="rgba">A packed integer containing all four color components in RGBA order.</param>
        public Color(int rgba)
            : this((uint)rgba) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="rgba">A packed integer containing all four color components in RGBA order.</param>
        public Color(uint rgba)
        {
            A = (byte)((rgba >> 24) & 255);
            B = (byte)((rgba >> 16) & 255);
            G = (byte)((rgba >> 8) & 255);
            R = (byte)(rgba & 255);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Color(float value)
        {
            A = R = G = B = FloatToByte(value);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <param name="alpha">The alpha component of the color.</param>
        public Color(byte red, byte green, byte blue, byte alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.  Alpha is set to 255.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        public Color(byte red, byte green, byte blue)
            : this(red, green, blue, (byte)255) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.  Passed values are clamped within byte range.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <param name="alpha">The alpha component of the color</param>
        public Color(int red, int green, int blue, int alpha)
        {
            R = IntToByte(red);
            G = IntToByte(green);
            B = IntToByte(blue);
            A = IntToByte(alpha);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.  Alpha is set to 255.  Passed values are clamped within byte range.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        public Color(int red, int green, int blue)
            : this(red, green, blue, 255) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <param name="alpha">The alpha component of the color.</param>
        public Color(float red, float green, float blue, float alpha)
        {
            R = FloatToByte(red);
            G = FloatToByte(green);
            B = FloatToByte(blue);
            A = FloatToByte(alpha);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.  Alpha is set to 255.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        public Color(float red, float green, float blue)
            : this(red, green, blue, 1.0f) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="value">The red, green, blue, and alpha components of the color.</param>
        public Color(Vector4 value)
        {
            R = FloatToByte(value.X);
            G = FloatToByte(value.Y);
            B = FloatToByte(value.Z);
            A = FloatToByte(value.W);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="value">The red, green, and blue components of the color.</param>
        /// <param name="alpha">The alpha component of the color.</param>
        public Color(Vector3 value, float alpha)
        {
            R = FloatToByte(value.X);
            G = FloatToByte(value.Y);
            B = FloatToByte(value.Z);
            A = FloatToByte(alpha);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct. Alpha is set to 255.
        /// </summary>
        /// <param name="value">The red, green, and blue components of the color.</param>
        public Color(Vector3 value)
            : this(value, 1.0f) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the alpha, red, green, and blue components of the color. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Color(byte[] values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length != 4) throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for Color.");

            R = values[0];
            G = values[1];
            B = values[2];
            A = values[3];
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the alpha, red, green, and blue components of the color. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Color(int[] values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length != 4) throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for Color.");

            R = IntToByte(values[0]);
            G = IntToByte(values[1]);
            B = IntToByte(values[2]);
            A = IntToByte(values[3]);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the red, green, and blue, alpha components of the color. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public Color(float[] values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length != 4) throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for Color.");

            R = FloatToByte(values[0]);
            G = FloatToByte(values[1]);
            B = FloatToByte(values[2]);
            A = FloatToByte(values[3]);
        }

        /// <summary>
        /// Gets the brightness.
        /// </summary>
        /// <returns>The Hue-Saturation-Brightness (HSB) brightness for this <see cref="Color"/></returns>
        public readonly float GetBrightness()
        {
            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;

            float max, min;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            return (max + min) / 2;
        }
        /// <summary>
        /// Gets the hue.
        /// </summary>
        /// <returns>The Hue-Saturation-Brightness (HSB) hue for this <see cref="Color"/></returns>
        public readonly float GetHue()
        {
            if (R == G && G == B) return 0; // 0 makes as good an UNDEFINED value as any

            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;

            float max, min;
            float delta;
            float hue = 0.0f;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            delta = max - min;

            if (r == max)
            {
                hue = (g - b) / delta;
            }
            else if (g == max)
            {
                hue = 2 + (b - r) / delta;
            }
            else if (b == max)
            {
                hue = 4 + (r - g) / delta;
            }
            hue *= 60;

            if (hue < 0.0f)
            {
                hue += 360.0f;
            }
            return hue;
        }
        /// <summary>
        /// Gets the saturation.
        /// </summary>
        /// <returns>The Hue-Saturation-Brightness (HSB) saturation for this <see cref="Color"/></returns>
        public readonly float GetSaturation()
        {
            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;

            float max, min;
            float l, s = 0;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            // if max == min, then there is no color and the saturation is zero.
            if (max != min)
            {
                l = (max + min) / 2;

                if (l <= .5)
                {
                    s = (max - min) / (max + min);
                }
                else
                {
                    s = (max - min) / (2 - max - min);
                }
            }
            return s;
        }

        /// <summary>
        /// Adds two colors.
        /// </summary>
        /// <param name="left">The first color to add.</param>
        /// <param name="right">The second color to add.</param>
        /// <param name="result">When the method completes, completes the sum of the two colors.</param>
        public static void Add(ref Color left, ref Color right, out Color result)
        {
            result.A = (byte)(left.A + right.A);
            result.R = (byte)(left.R + right.R);
            result.G = (byte)(left.G + right.G);
            result.B = (byte)(left.B + right.B);
        }
        /// <summary>
        /// Adds two colors.
        /// </summary>
        /// <param name="left">The first color to add.</param>
        /// <param name="right">The second color to add.</param>
        /// <returns>The sum of the two colors.</returns>
        public static Color Add(Color left, Color right)
        {
            Add(ref left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Subtracts two colors.
        /// </summary>
        /// <param name="left">The first color to subtract.</param>
        /// <param name="right">The second color to subtract.</param>
        /// <param name="result">WHen the method completes, contains the difference of the two colors.</param>
        public static void Subtract(ref Color left, ref Color right, out Color result)
        {
            result.A = (byte)(left.A - right.A);
            result.R = (byte)(left.R - right.R);
            result.G = (byte)(left.G - right.G);
            result.B = (byte)(left.B - right.B);
        }
        /// <summary>
        /// Subtracts two colors.
        /// </summary>
        /// <param name="left">The first color to subtract.</param>
        /// <param name="right">The second color to subtract</param>
        /// <returns>The difference of the two colors.</returns>
        public static Color Subtract(Color left, Color right)
        {
            Subtract(ref left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Modulates two colors.
        /// </summary>
        /// <param name="left">The first color to modulate.</param>
        /// <param name="right">The second color to modulate.</param>
        /// <param name="result">When the method completes, contains the modulated color.</param>
        public static void Modulate(ref Color left, ref Color right, out Color result)
        {
            result.A = (byte)(left.A * right.A / 255f);
            result.R = (byte)(left.R * right.R / 255f);
            result.G = (byte)(left.G * right.G / 255f);
            result.B = (byte)(left.B * right.B / 255f);
        }
        /// <summary>
        /// Modulates two colors.
        /// </summary>
        /// <param name="left">The first color to modulate.</param>
        /// <param name="right">The second color to modulate.</param>
        /// <returns>The modulated color.</returns>
        public static Color Modulate(Color left, Color right)
        {
            Modulate(ref left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Scales a color.
        /// </summary>
        /// <param name="value">The color to scale.</param>
        /// <param name="scale">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled color.</param>
        public static void Scale(ref Color value, float scale, out Color result)
        {
            result.A = (byte)(value.A * scale);
            result.R = (byte)(value.R * scale);
            result.G = (byte)(value.G * scale);
            result.B = (byte)(value.B * scale);
        }
        /// <summary>
        /// Scales a color.
        /// </summary>
        /// <param name="value">The color to scale.</param>
        /// <param name="scale">The amount by which to scale.</param>
        /// <returns>The scaled color.</returns>
        public static Color Scale(Color value, float scale)
        {
            Scale(ref value, scale, out var result);
            return result;
        }

        /// <summary>
        /// Negates a color.
        /// </summary>
        /// <param name="value">The color to negate.</param>
        /// <param name="result">When the method completes, contains the negated color.</param>
        public static void Negate(ref Color value, out Color result)
        {
            result.A = (byte)(255 - value.A);
            result.R = (byte)(255 - value.R);
            result.G = (byte)(255 - value.G);
            result.B = (byte)(255 - value.B);
        }
        /// <summary>
        /// Negates a color.
        /// </summary>
        /// <param name="value">The color to negate.</param>
        /// <returns>The negated color.</returns>
        public static Color Negate(Color value)
        {
            Negate(ref value, out var result);
            return result;
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="result">When the method completes, contains the clamped value.</param>
        public static void Clamp(ref Color value, ref Color min, ref Color max, out Color result)
        {
            byte alpha = value.A;
            alpha = (alpha > max.A) ? max.A : alpha;
            alpha = (alpha < min.A) ? min.A : alpha;

            byte red = value.R;
            red = (red > max.R) ? max.R : red;
            red = (red < min.R) ? min.R : red;

            byte green = value.G;
            green = (green > max.G) ? max.G : green;
            green = (green < min.G) ? min.G : green;

            byte blue = value.B;
            blue = (blue > max.B) ? max.B : blue;
            blue = (blue < min.B) ? min.B : blue;

            result = new(red, green, blue, alpha);
        }
        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static Color Clamp(Color value, Color min, Color max)
        {
            Clamp(ref value, ref min, ref max, out var result);
            return result;
        }

        /// <summary>
        /// Computes the premultiplied value of the provided color.
        /// </summary>
        /// <param name="value">The non-premultiplied value.</param>
        /// <param name="result">The premultiplied result.</param>
        public static void Premultiply(ref Color value, out Color result)
        {
            var a = value.A / (255f * 255f);
            result.A = value.A;
            result.R = FloatToByte(value.R * a);
            result.G = FloatToByte(value.G * a);
            result.B = FloatToByte(value.B * a);
        }
        /// <summary>
        /// Computes the premultiplied value of the provided color.
        /// </summary>
        /// <param name="value">The non-premultiplied value.</param>
        /// <returns>The premultiplied result.</returns>
        public static Color Premultiply(Color value)
        {
            Premultiply(ref value, out var result);
            return result;
        }

        /// <summary>
        /// Converts the color from a packed BGRA integer.
        /// </summary>
        /// <param name="color">A packed integer containing all four color components in BGRA order</param>
        /// <returns>A color.</returns>
        public static Color FromBgra(int color)
        {
            return new(
                (byte)((color >> 16) & 255),
                (byte)((color >> 8) & 255),
                (byte)(color & 255),
                (byte)((color >> 24) & 255));
        }
        /// <summary>
        /// Converts the color from a packed BGRA integer.
        /// </summary>
        /// <param name="color">A packed integer containing all four color components in BGRA order</param>
        /// <returns>A color.</returns>
        public static Color FromBgra(uint color)
        {
            return FromBgra(unchecked((int)color));
        }

        /// <summary>
        /// Converts the color from a packed ABGR integer.
        /// </summary>
        /// <param name="color">A packed integer containing all four color components in ABGR order</param>
        /// <returns>A color.</returns>
        public static Color FromAbgr(int color)
        {
            return new(
                (byte)(color >> 24),
                (byte)(color >> 16),
                (byte)(color >> 8),
                (byte)color);
        }
        /// <summary>
        /// Converts the color from a packed ABGR integer.
        /// </summary>
        /// <param name="color">A packed integer containing all four color components in ABGR order</param>
        /// <returns>A color.</returns>
        public static Color FromAbgr(uint color)
        {
            return FromAbgr(unchecked((int)color));
        }

        /// <summary>
        /// Converts the color from a packed BGRA integer.
        /// </summary>
        /// <param name="color">A packed integer containing all four color components in RGBA order</param>
        /// <returns>A color.</returns>
        public static Color FromRgba(int color)
        {
            return new(color);
        }
        /// <summary>
        /// Converts the color from a packed BGRA integer.
        /// </summary>
        /// <param name="color">A packed integer containing all four color components in RGBA order</param>
        /// <returns>A color.</returns>
        public static Color FromRgba(uint color)
        {
            return new(color);
        }

        /// <summary>
        /// Performs a linear interpolation between two colors.
        /// </summary>
        /// <param name="start">Start color.</param>
        /// <param name="end">End color.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two colors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Color start, ref Color end, float amount, out Color result)
        {
            result.R = (byte)float.Lerp(start.R, end.R, amount);
            result.G = (byte)float.Lerp(start.G, end.G, amount);
            result.B = (byte)float.Lerp(start.B, end.B, amount);
            result.A = (byte)float.Lerp(start.A, end.A, amount);
        }
        /// <summary>
        /// Performs a linear interpolation between two colors.
        /// </summary>
        /// <param name="start">Start color.</param>
        /// <param name="end">End color.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The linear interpolation of the two colors.</returns>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Color Lerp(Color start, Color end, float amount)
        {
            Lerp(ref start, ref end, amount, out var result);
            return result;
        }

        /// <summary>
        /// Performs a cubic interpolation between two colors.
        /// </summary>
        /// <param name="start">Start color.</param>
        /// <param name="end">End color.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the cubic interpolation of the two colors.</param>
        public static void SmoothStep(ref Color start, ref Color end, float amount, out Color result)
        {
            amount = Helper.SmoothStep(amount);
            Lerp(ref start, ref end, amount, out result);
        }
        /// <summary>
        /// Performs a cubic interpolation between two colors.
        /// </summary>
        /// <param name="start">Start color.</param>
        /// <param name="end">End color.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two colors.</returns>
        public static Color SmoothStep(Color start, Color end, float amount)
        {
            SmoothStep(ref start, ref end, amount, out var result);
            return result;
        }

        /// <summary>
        /// Returns a color containing the smallest components of the specified colors.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <param name="result">When the method completes, contains an new color composed of the largest components of the source colors.</param>
        public static void Max(ref Color left, ref Color right, out Color result)
        {
            result.A = (left.A > right.A) ? left.A : right.A;
            result.R = (left.R > right.R) ? left.R : right.R;
            result.G = (left.G > right.G) ? left.G : right.G;
            result.B = (left.B > right.B) ? left.B : right.B;
        }
        /// <summary>
        /// Returns a color containing the largest components of the specified colorss.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>A color containing the largest components of the source colors.</returns>
        public static Color Max(Color left, Color right)
        {
            Max(ref left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Returns a color containing the smallest components of the specified colors.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <param name="result">When the method completes, contains an new color composed of the smallest components of the source colors.</param>
        public static void Min(ref Color left, ref Color right, out Color result)
        {
            result.A = (left.A < right.A) ? left.A : right.A;
            result.R = (left.R < right.R) ? left.R : right.R;
            result.G = (left.G < right.G) ? left.G : right.G;
            result.B = (left.B < right.B) ? left.B : right.B;
        }
        /// <summary>
        /// Returns a color containing the smallest components of the specified colors.
        /// </summary>
        /// <param name="left">The first source color.</param>
        /// <param name="right">The second source color.</param>
        /// <returns>A color containing the smallest components of the source colors.</returns>
        public static Color Min(Color left, Color right)
        {
            Min(ref left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Adjusts the contrast of a color.
        /// </summary>
        /// <param name="value">The color whose contrast is to be adjusted.</param>
        /// <param name="contrast">The amount by which to adjust the contrast.</param>
        /// <param name="result">When the method completes, contains the adjusted color.</param>
        public static void AdjustContrast(ref Color value, float contrast, out Color result)
        {
            result.A = value.A;
            result.R = FloatToByte(0.5f + contrast * (value.R / 255.0f - 0.5f));
            result.G = FloatToByte(0.5f + contrast * (value.G / 255.0f - 0.5f));
            result.B = FloatToByte(0.5f + contrast * (value.B / 255.0f - 0.5f));
        }
        /// <summary>
        /// Adjusts the contrast of a color.
        /// </summary>
        /// <param name="value">The color whose contrast is to be adjusted.</param>
        /// <param name="contrast">The amount by which to adjust the contrast.</param>
        /// <returns>The adjusted color.</returns>
        public static Color AdjustContrast(Color value, float contrast)
        {
            AdjustContrast(ref value, contrast, out var result);
            return result;
        }

        /// <summary>
        /// Adjusts the saturation of a color.
        /// </summary>
        /// <param name="value">The color whose saturation is to be adjusted.</param>
        /// <param name="saturation">The amount by which to adjust the saturation.</param>
        /// <param name="result">When the method completes, contains the adjusted color.</param>
        public static void AdjustSaturation(ref Color value, float saturation, out Color result)
        {
            float grey = value.R / 255.0f * 0.2125f + value.G / 255.0f * 0.7154f + value.B / 255.0f * 0.0721f;

            result.A = value.A;
            result.R = FloatToByte(grey + saturation * (value.R / 255.0f - grey));
            result.G = FloatToByte(grey + saturation * (value.G / 255.0f - grey));
            result.B = FloatToByte(grey + saturation * (value.B / 255.0f - grey));
        }
        /// <summary>
        /// Adjusts the saturation of a color.
        /// </summary>
        /// <param name="value">The color whose saturation is to be adjusted.</param>
        /// <param name="saturation">The amount by which to adjust the saturation.</param>
        /// <returns>The adjusted color.</returns>
        public static Color AdjustSaturation(Color value, float saturation)
        {
            AdjustSaturation(ref value, saturation, out var result);
            return result;
        }

        /// <summary>
        /// Adds two colors.
        /// </summary>
        /// <param name="left">The first color to add.</param>
        /// <param name="right">The second color to add.</param>
        /// <returns>The sum of the two colors.</returns>
        public static Color operator +(Color left, Color right)
        {
            Add(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Assert a color (return it unchanged).
        /// </summary>
        /// <param name="value">The color to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) color.</returns>
        public static Color operator +(Color value)
        {
            return value;
        }
        /// <summary>
        /// Subtracts two colors.
        /// </summary>
        /// <param name="left">The first color to subtract.</param>
        /// <param name="right">The second color to subtract.</param>
        /// <returns>The difference of the two colors.</returns>
        public static Color operator -(Color left, Color right)
        {
            Subtract(ref left, ref right, out var result);
            return result;
        }
        /// <summary>
        /// Negates a color.
        /// </summary>
        /// <param name="value">The color to negate.</param>
        /// <returns>A negated color.</returns>
        public static Color operator -(Color value)
        {
            Negate(ref value, out var result);
            return result;
        }
        /// <summary>
        /// Scales a color.
        /// </summary>
        /// <param name="scale">The factor by which to scale the color.</param>
        /// <param name="value">The color to scale.</param>
        /// <returns>The scaled color.</returns>
        public static Color operator *(float scale, Color value)
        {
            Scale(ref value, scale, out var result);
            return result;
        }
        /// <summary>
        /// Scales a color.
        /// </summary>
        /// <param name="value">The factor by which to scale the color.</param>
        /// <param name="scale">The color to scale.</param>
        /// <returns>The scaled color.</returns>
        public static Color operator *(Color value, float scale)
        {
            Scale(ref value, scale, out var result);
            return result;
        }
        /// <summary>
        /// Modulates two colors.
        /// </summary>
        /// <param name="left">The first color to modulate.</param>
        /// <param name="right">The second color to modulate.</param>
        /// <returns>The modulated color.</returns>
        public static Color operator *(Color left, Color right)
        {
            Modulate(ref left, ref right, out var result);
            return result;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static explicit operator Vector3(Color value)
        {
            return value.ToVector3();
        }
        /// <inheritdoc/>
        public static explicit operator Color(Vector3 value)
        {
            return new(value.X, value.Y, value.Z, 1.0f);
        }
        /// <inheritdoc/>
        public static implicit operator Color3(Color value)
        {
            return value.ToColor3();
        }
        /// <inheritdoc/>
        public static explicit operator Color(Color3 value)
        {
            return new(value.Red, value.Green, value.Blue, 1.0f);
        }

        /// <inheritdoc/>
        public static explicit operator Vector4(Color value)
        {
            return value.ToVector4();
        }
        /// <inheritdoc/>
        public static explicit operator Color(Vector4 value)
        {
            return new(value.X, value.Y, value.Z, value.W);
        }
        /// <inheritdoc/>
        public static implicit operator Color4(Color value)
        {
            return value.ToColor4();
        }
        /// <inheritdoc/>
        public static explicit operator Color(Color4 value)
        {
            return new(value.Red, value.Green, value.Blue, value.Alpha);
        }

        /// <inheritdoc/>
        public static implicit operator System.Drawing.Color(Color value)
        {
            return System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
        }
        /// <inheritdoc/>
        public static implicit operator Color(System.Drawing.Color value)
        {
            return new(value.R, value.G, value.B, value.A);
        }

        /// <inheritdoc/>
        public static implicit operator Vortice.Mathematics.Color4(Color value)
        {
            return new(
                value.R / 255f,
                value.G / 255f,
                value.B / 255f,
                value.A / 255f);
        }
        /// <inheritdoc/>
        public static implicit operator Color(Vortice.Mathematics.Color4 value)
        {
            return new(value.R, value.G, value.B, value.A);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }
        /// <inheritdoc/>
        public readonly string ToString(IFormatProvider formatProvider)
        {
            return string.Format(
                formatProvider,
                ColorStr,
                A, R, G, B);
        }
        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString(formatProvider);

            return string.Format(
                formatProvider,
                ColorStr,
                A.ToString(format, formatProvider),
                R.ToString(format, formatProvider),
                G.ToString(format, formatProvider),
                B.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref Color other)
        {
            return
                R == other.R &&
                G == other.G &&
                B == other.B &&
                A == other.A;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Color other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object value)
        {
            if (value is not Color) return false;

            var strongValue = (Color)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Converts the color into a packed integer.
        /// </summary>
        /// <returns>A packed integer containing all four color components.</returns>
        public readonly int ToBgra()
        {
            int value = B;
            value |= G << 8;
            value |= R << 16;
            value |= A << 24;

            return value;
        }
        /// <summary>
        /// Converts the color into a packed integer.
        /// </summary>
        /// <returns>A packed integer containing all four color components.</returns>
        public readonly int ToRgba()
        {
            int value = R;
            value |= G << 8;
            value |= B << 16;
            value |= A << 24;

            return value;
        }
        /// <summary>
        /// Converts the color into a packed integer.
        /// </summary>
        /// <returns>A packed integer containing all four color components.</returns>
        public readonly int ToAbgr()
        {
            int value = A;
            value |= B << 8;
            value |= G << 16;
            value |= R << 24;

            return value;
        }
        /// <summary>
        /// Converts the color into a three component vector.
        /// </summary>
        /// <returns>A three component vector containing the red, green, and blue components of the color.</returns>
        public readonly Vector3 ToVector3()
        {
            return new(
                R / 255.0f,
                G / 255.0f,
                B / 255.0f);
        }
        /// <summary>
        /// Converts the color into a three component color.
        /// </summary>
        /// <returns>A three component color containing the red, green, and blue components of the color.</returns>
        public readonly Color3 ToColor3()
        {
            return new(
                R / 255.0f,
                G / 255.0f,
                B / 255.0f);
        }
        /// <summary>
        /// Converts the color into a four component vector.
        /// </summary>
        /// <returns>A four component vector containing all four color components.</returns>
        public readonly Vector4 ToVector4()
        {
            return new(
                R / 255.0f,
                G / 255.0f,
                B / 255.0f,
                A / 255.0f);
        }
        /// <summary>
        /// Convert this instance to a <see cref="Color4"/>
        /// </summary>
        /// <returns>The result of the conversion.</returns>
        public readonly Color4 ToColor4()
        {
            return new(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }
        /// <summary>
        /// Creates an array containing the elements of the color.
        /// </summary>
        /// <returns>A four-element array containing the components of the color in RGBA order.</returns>
        public readonly byte[] ToArray()
        {
            return [R, G, B, A];
        }
    }
}
