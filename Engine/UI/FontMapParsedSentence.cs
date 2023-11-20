using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.UI
{
    /// <summary>
    /// Font map parser sentence
    /// </summary>
    public struct FontMapParsedSentence
    {
        /// <summary>
        /// Creates a parsed sentencen result from sample
        /// </summary>
        /// <param name="sample">Text sample</param>
        public static FontMapParsedSentence FromSample(string sample)
        {
            return new FontMapParsedSentence
            {
                Words = new[] { sample },
                Colors = Enumerable.Empty<IEnumerable<Color4>>(),
                ShadowColors = Enumerable.Empty<IEnumerable<Color4>>(),
            };
        }

        /// <summary>
        /// Parsed text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Word list
        /// </summary>
        public IEnumerable<string> Words { get; set; }
        /// <summary>
        /// Colors by character of word list
        /// </summary>
        public IEnumerable<IEnumerable<Color4>> Colors { get; set; }
        /// <summary>
        /// Shadow colors by character of word list
        /// </summary>
        public IEnumerable<IEnumerable<Color4>> ShadowColors { get; set; }

        /// <summary>
        /// Gets the number of words in the sentence
        /// </summary>
        public readonly int Count()
        {
            return Words?.Count() ?? 0;
        }
        /// <summary>
        /// Gets the parsed word at index
        /// </summary>
        /// <param name="index">Index</param>
        public readonly FontMapParsedWord GetWord(int index)
        {
            if (index < Count())
            {
                return new FontMapParsedWord
                {
                    Word = Words.ElementAt(index),
                    Colors = Colors.ElementAtOrDefault(index)?.ToArray() ?? Enumerable.Empty<Color4>(),
                    ShadowColors = ShadowColors.ElementAtOrDefault(index)?.ToArray() ?? Enumerable.Empty<Color4>(),
                };
            }

            return new FontMapParsedWord();
        }
    }
}
