using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.UI
{
    /// <summary>
    /// Font map parsed word
    /// </summary>
    public struct FontMapParsedWord
    {
        /// <summary>
        /// Gets the space parsed word
        /// </summary>
        public static FontMapParsedWord Space
        {
            get
            {
                return new FontMapParsedWord
                {
                    Word = " ",
                    Colors = Enumerable.Empty<Color4>(),
                    ShadowColors = Enumerable.Empty<Color4>(),
                };
            }
        }

        /// <summary>
        /// Word
        /// </summary>
        public string Word { get; set; }
        /// <summary>
        /// Colors by character of word
        /// </summary>
        public IEnumerable<Color4> Colors { get; set; }
        /// <summary>
        /// Shadow colors by character of word
        /// </summary>
        public IEnumerable<Color4> ShadowColors { get; set; }

        /// <summary>
        /// Gets the number of characters in the word
        /// </summary>
        public readonly int Count()
        {
            return Word?.Length ?? 0;
        }
        /// <summary>
        /// Gets the color of the character at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly Color4 GetColor(int index)
        {
            if (index < Count())
            {
                return Colors.ElementAtOrDefault(index);
            }

            return Color.Transparent;
        }
        /// <summary>
        /// Gets the shadow color of the character at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly Color4 GetShadowColor(int index)
        {
            if (index < Count())
            {
                return ShadowColors.ElementAtOrDefault(index);
            }

            return Color.Transparent;
        }
    }
}
