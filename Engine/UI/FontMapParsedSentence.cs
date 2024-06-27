using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Font map parser sentence
    /// </summary>
    public struct FontMapParsedSentence
    {
        /// <summary>
        /// Empty parsed sentence
        /// </summary>
        public static readonly FontMapParsedSentence Empty = new()
        {
            Text = string.Empty,
            Parts = [],
            Colors = [],
            ShadowColors = [],
        };
        /// <summary>
        /// Creates a parsed sentencen result from sample
        /// </summary>
        /// <param name="sample">Text sample</param>
        public static FontMapParsedSentence FromSample(string sample)
        {
            return new()
            {
                Text = sample,
                Parts = [sample],
                Colors = [],
                ShadowColors = [],
            };
        }

        /// <summary>
        /// Parsed text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Part list
        /// </summary>
        public string[] Parts { get; set; }
        /// <summary>
        /// Colors by character of part list
        /// </summary>
        public Color4[][] Colors { get; set; }
        /// <summary>
        /// Shadow colors by character of part list
        /// </summary>
        public Color4[][] ShadowColors { get; set; }

        /// <summary>
        /// Gets the number of part in the sentence
        /// </summary>
        public readonly int Count()
        {
            return Parts?.Length ?? 0;
        }
        /// <summary>
        /// Gets the parsed part at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly FontMapParsedPart GetPart(int index)
        {
            if (index < 0 || index >= Count())
            {
                return new();
            }

            return new()
            {
                Text = Parts[index],
                Colors = index < Colors.Length ? Colors[index] : [],
                ShadowColors = index < ShadowColors.Length ? ShadowColors[index] : [],
            };
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{Text}";
        }
    }
}
