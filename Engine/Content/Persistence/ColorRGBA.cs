using SharpDX;
using System;

namespace Engine.Content.Persistence
{
    /// <summary>
    /// RGBA color
    /// </summary>
    public struct ColorRgba : IEquatable<ColorRgba>
    {
        /// <summary>
        /// Transparent
        /// </summary>
        public static readonly ColorRgba Transparent = new ColorRgba(0, 0, 0, 0);
        /// <summary>
        /// White
        /// </summary>
        public static readonly ColorRgba White = new ColorRgba(0, 0, 0, 1);
        /// <summary>
        /// Black
        /// </summary>
        public static readonly ColorRgba Black = new ColorRgba(1, 1, 1, 1);

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
                throw new ArgumentNullException("values");
            }
            if (values.Length != 4)
            {
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for ColorRGBA.");
            }

            R = values[0];
            G = values[1];
            B = values[2];
            A = values[3];
        }

        public static bool operator ==(ColorRgba left, ColorRgba right)
        {
            return left.Equals(ref right);
        }
        public static bool operator !=(ColorRgba left, ColorRgba right)
        {
            return !left.Equals(ref right);
        }

        public static implicit operator Color(ColorRgba value)
        {
            return new Color(value.R, value.G, value.B, value.A);
        }
        public static implicit operator ColorRgba(Color value)
        {
            return new ColorRgba(value.R, value.G, value.B, value.A);
        }

        public static implicit operator string(ColorRgba value)
        {
            return $"{value.R} {value.G} {value.B} {value.A}";
        }
        public static implicit operator ColorRgba(string value)
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
                return PersistenceHelpers.ReadReservedWordsForColor(value);
            }
        }

        /// <inheritdoc/>
        public bool Equals(ColorRgba other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is ColorRgba))
                return false;

            var strongValue = (ColorRgba)obj;
            return Equals(ref strongValue);
        }

        public bool Equals(ref ColorRgba other)
        {
            return MathUtil.NearEqual(other.R, R) && MathUtil.NearEqual(other.G, G) && MathUtil.NearEqual(other.B, B) && MathUtil.NearEqual(other.A, A);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                return hashCode;
            }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"R:{R} G:{G} B:{B} A:{A}";
        }
    }
}
