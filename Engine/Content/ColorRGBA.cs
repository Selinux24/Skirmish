using SharpDX;
using System;

namespace Engine.Content
{
    /// <summary>
    /// RGBA color
    /// </summary>
    public struct ColorRgba : IEquatable<ColorRgba>
    {
        /// <summary>
        /// Transparent
        /// </summary>
        public static readonly ColorRgba Transparent = new(0, 0, 0, 0);
        /// <summary>
        /// White
        /// </summary>
        public static readonly ColorRgba White = new(0, 0, 0, 1);
        /// <summary>
        /// Black
        /// </summary>
        public static readonly ColorRgba Black = new(1, 1, 1, 1);

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
        /// Initializes a new instance of the <see cref="ColorRgba"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public ColorRgba(float value)
        {
            R = value;
            G = value;
            B = value;
            A = value;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorRgba"/> struct.
        /// </summary>
        /// <param name="r">Initial value for the red component of the color.</param>
        /// <param name="g">Initial value for the green component of the color.</param>
        /// <param name="b">Initial value for the blue component of the color.</param>
        /// <param name="a">Initial value for the alpha component of the color.</param>
        public ColorRgba(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorRgba"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the R, G, B and A components of the color. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public ColorRgba(float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
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
        public static bool operator ==(ColorRgba left, ColorRgba right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        public static bool operator !=(ColorRgba left, ColorRgba right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public static implicit operator Color(ColorRgba value)
        {
            return new Color(value.R, value.G, value.B, value.A);
        }
        /// <inheritdoc/>
        public static implicit operator ColorRgba(Color value)
        {
            var color4 = value.ToColor4();
            return new ColorRgba(color4.Red, color4.Green, color4.Blue, color4.Alpha);
        }

        /// <inheritdoc/>
        public static implicit operator Color3(ColorRgba value)
        {
            return new Color3(value.R, value.G, value.B);
        }
        /// <inheritdoc/>
        public static implicit operator ColorRgba(Color3 value)
        {
            return new ColorRgba(value.Red, value.Green, value.Blue, 1f);
        }

        /// <inheritdoc/>
        public static implicit operator Color4(ColorRgba value)
        {
            return new Color4(value.R, value.G, value.B, value.A);
        }
        /// <inheritdoc/>
        public static implicit operator ColorRgba(Color4 value)
        {
            return new ColorRgba(value.Red, value.Green, value.Blue, value.Alpha);
        }

        /// <inheritdoc/>
        public static implicit operator string(ColorRgba value)
        {
            return ContentHelper.WriteColorRgba(value);
        }
        /// <inheritdoc/>
        public static implicit operator ColorRgba(string value)
        {
            return ContentHelper.ReadColorRgba(value) ?? Black;
        }

        /// <inheritdoc/>
        public readonly bool Equals(ColorRgba other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj is not ColorRgba)
                return false;

            var strongValue = (ColorRgba)obj;
            return Equals(ref strongValue);
        }

        public readonly bool Equals(ref ColorRgba other)
        {
            return MathUtil.NearEqual(other.R, R) && MathUtil.NearEqual(other.G, G) && MathUtil.NearEqual(other.B, B) && MathUtil.NearEqual(other.A, A);
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
