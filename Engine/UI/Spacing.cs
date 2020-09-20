using System;

namespace Engine.UI
{
    /// <summary>
    /// Spacing
    /// </summary>
    public struct Spacing : IEquatable<Spacing>
    {
        /// <summary>
        /// Gets the 0 spacing
        /// </summary>
        public static Spacing Zero
        {
            get
            {
                return new Spacing
                {
                    Horizontal = 0,
                    Vertical = 0,
                };
            }
        }

        /// <summary>
        /// Horizontal spacing
        /// </summary>
        public float Horizontal { get; set; }
        /// <summary>
        /// Vertical spacing
        /// </summary>
        public float Vertical { get; set; }

        /// <inheritdoc/>
        public bool Equals(Spacing other)
        {
            return
                other.Horizontal == Horizontal &&
                other.Vertical == Vertical;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is Spacing spacing)
            {
                return Equals(spacing);
            }

            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Horizontal.GetHashCode() * 397) ^ Vertical.GetHashCode();
            }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Horizontal: {Horizontal}; Vertical: {Vertical};";
        }

        public static bool operator ==(Spacing left, Spacing right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Spacing left, Spacing right)
        {
            return !left.Equals(right);
        }

        public static implicit operator Spacing(int value)
        {
            return new Spacing
            {
                Horizontal = value,
                Vertical = value,
            };
        }
        public static implicit operator Spacing(int[] value)
        {
            if (value?.Length == 1)
            {
                return new Spacing
                {
                    Horizontal = value[0],
                    Vertical = value[0],
                };
            }

            if (value?.Length == 2)
            {
                return new Spacing
                {
                    Horizontal = value[0],
                    Vertical = value[1],
                };
            }

            return new Spacing
            {
                Horizontal = float.NaN,
                Vertical = float.NaN,
            };
        }
    }
}
