using System;

namespace Engine.UI
{
    /// <summary>
    /// Padding
    /// </summary>
    public struct Padding
    {
        /// <summary>
        /// Gets the 0 padding
        /// </summary>
        public static Padding Zero
        {
            get
            {
                return new Padding
                {
                    Left = 0,
                    Top = 0,
                    Bottom = 0,
                    Right = 0,
                };
            }
        }

        /// <summary>
        /// Padding left
        /// </summary>
        public float Left;
        /// <summary>
        /// Pading top
        /// </summary>
        public float Top;
        /// <summary>
        /// Padding botton
        /// </summary>
        public float Bottom;
        /// <summary>
        /// Padding right
        /// </summary>
        public float Right;

        public static implicit operator Padding(int value)
        {
            return new Padding
            {
                Left = value,
                Top = value,
                Bottom = value,
                Right = value,
            };
        }
        public static implicit operator Padding(int[] value)
        {
            if (value?.Length == 1)
            {
                return new Padding
                {
                    Left = value[0],
                    Top = value[0],
                    Bottom = value[0],
                    Right = value[0],
                };
            }

            if (value.Length == 2)
            {
                return new Padding
                {
                    Left = value[0],
                    Top = value[1],
                    Bottom = value[1],
                    Right = value[0],
                };
            }

            if (value.Length == 4)
            {
                return new Padding
                {
                    Left = value[1],
                    Top = value[2],
                    Bottom = value[3],
                    Right = value[4],
                };
            }

            throw new ArgumentException(nameof(value));
        }
    }
}
