
namespace Engine.UI
{
    using Engine.Mathematics;

    /// <summary>
    /// Font map parsed part
    /// </summary>
    public struct FontMapParsedPart
    {
        /// <summary>
        /// Gets the space parsed part
        /// </summary>
        public static FontMapParsedPart Space
        {
            get
            {
                return new()
                {
                    Text = " ",
                    Colors = [],
                    ShadowColors = [],
                };
            }
        }
        /// <summary>
        /// Gets the parsed part from a string
        /// </summary>
        /// <param name="text">Text</param>
        public static FontMapParsedPart FromString(string text)
        {
            return new()
            {
                Text = text,
                Colors = [],
                ShadowColors = [],
            };
        }
        /// <summary>
        /// Gets the parsed part from a char
        /// </summary>
        /// <param name="character">Character</param>
        public static FontMapParsedPart FromChar(char character)
        {
            return new()
            {
                Text = character.ToString(),
                Colors = [],
                ShadowColors = [],
            };
        }

        /// <summary>
        /// Part text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Colors by character of part
        /// </summary>
        public Vector4[] Colors { get; set; }
        /// <summary>
        /// Shadow colors by character of part
        /// </summary>
        public Vector4[] ShadowColors { get; set; }

        /// <summary>
        /// Gets the number of characters in the part text
        /// </summary>
        public readonly int Count()
        {
            return Text?.Length ?? 0;
        }
        /// <summary>
        /// Gets the color of the character at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly Vector4 GetColor(int index)
        {
            if (index >= 0 && index < Colors.Length)
            {
                return Colors[index];
            }

            return Vector4.Zero;
        }
        /// <summary>
        /// Gets the shadow color of the character at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly Vector4 GetShadowColor(int index)
        {
            if (index >= 0 && index < ShadowColors.Length)
            {
                return ShadowColors[index];
            }

            return Vector4.Zero;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{Text}";
        }
    }
}
