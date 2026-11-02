using Engine.Mathematics;
using System;

namespace Engine.Content
{
    /// <summary>
    /// RGBA color
    /// </summary>
    public struct CntColor : IEquatable<CntColor>
    {
        /// <summary>
        /// Transparent
        /// </summary>
        public static readonly CntColor Transparent = new(0f, 0f, 0f, 0f);
        /// <summary>
        /// White
        /// </summary>
        public static readonly CntColor White = new(0f, 0f, 0f, 1f);
        /// <summary>
        /// Black
        /// </summary>
        public static readonly CntColor Black = new(1f, 1f, 1f, 1f);

        /// <summary>
        /// The red component of the color.
        /// </summary>
        public float R { get; set; }
        /// <summary>
        /// The green component of the color.
        /// </summary>
        public float G { get; set; }
        /// <summary>
        /// The blue component of the color.
        /// </summary>
        public float B { get; set; }
        /// <summary>
        /// The alpha component of the color.
        /// </summary>
        public float A { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public CntColor(byte value)
        {
            R = value / 255f;
            G = value / 255f;
            B = value / 255f;
            A = value / 255f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public CntColor(int value)
        {
            R = value / 255f;
            G = value / 255f;
            B = value / 255f;
            A = value / 255f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public CntColor(float value)
        {
            R = value;
            G = value;
            B = value;
            A = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="r">Initial value for the red component of the color.</param>
        /// <param name="g">Initial value for the green component of the color.</param>
        /// <param name="b">Initial value for the blue component of the color.</param>
        /// <param name="a">Initial value for the alpha component of the color.</param>
        public CntColor(byte r, byte g, byte b, byte a)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            A = a / 255f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="r">Initial value for the red component of the color.</param>
        /// <param name="g">Initial value for the green component of the color.</param>
        /// <param name="b">Initial value for the blue component of the color.</param>
        /// <param name="a">Initial value for the alpha component of the color.</param>
        public CntColor(int r, int g, int b, int a)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            A = a / 255f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="r">Initial value for the red component of the color.</param>
        /// <param name="g">Initial value for the green component of the color.</param>
        /// <param name="b">Initial value for the blue component of the color.</param>
        /// <param name="a">Initial value for the alpha component of the color.</param>
        public CntColor(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the R, G, B and A components of the color. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public CntColor(byte[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for ColorRGBA.");
            }

            R = values[0] / 255f;
            G = values[1] / 255f;
            B = values[2] / 255f;
            A = values[3] / 255f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the R, G, B and A components of the color. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public CntColor(int[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for ColorRGBA.");
            }

            R = values[0] / 255f;
            G = values[1] / 255f;
            B = values[2] / 255f;
            A = values[3] / 255f;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CntColor"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the R, G, B and A components of the color. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public CntColor(float[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "There must be four and only four input values for ColorRGBA.");
            }

            R = values[0];
            G = values[1];
            B = values[2];
            A = values[3];
        }

        /// <inheritdoc/>
        public static bool operator ==(CntColor left, CntColor right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(CntColor left, CntColor right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static implicit operator Color4(CntColor value)
        {
            return new(value.R, value.G, value.B, value.A);
        }
        /// <inheritdoc/>
        public static implicit operator CntColor(Color4 value)
        {
            return new(value.Red, value.Green, value.Blue, value.Alpha);
        }

        /// <inheritdoc/>
        public static implicit operator Color3(CntColor value)
        {
            return new(value.R, value.G, value.B);
        }
        /// <inheritdoc/>
        public static implicit operator CntColor(Color3 value)
        {
            return new(value.Red, value.Green, value.Blue, 1f);
        }

        /// <inheritdoc/>
        public static implicit operator Color(CntColor value)
        {
            return new(value.R, value.G, value.B, value.A);
        }
        /// <inheritdoc/>
        public static implicit operator CntColor(Color value)
        {
            return new(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
        }

        /// <inheritdoc/>
        public static implicit operator string(CntColor value)
        {
            return ContentHelper.WriteColorRgba(value);
        }
        /// <inheritdoc/>
        public static implicit operator CntColor(string value)
        {
            return ContentHelper.ReadColorRgba(value) ?? Black;
        }

        /// <inheritdoc/>
        public readonly bool Equals(CntColor other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not CntColor) return false;

            var strongValue = (CntColor)obj;
            return Equals(ref strongValue);
        }
        /// <inheritdoc/>
        public readonly bool Equals(ref CntColor other)
        {
            return Helper.NearEqual(other.R, R) && Helper.NearEqual(other.G, G) && Helper.NearEqual(other.B, B) && Helper.NearEqual(other.A, A);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"R:{R} G:{G} B:{B} A:{A}";
        }
    }
}
