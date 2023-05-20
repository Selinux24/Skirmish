using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Engine.UI
{
    /// <summary>
    /// Font map text parser
    /// </summary>
    public abstract class FontMapParser
    {
        /// <summary>
        /// Color pattern used for text parse
        /// </summary>
        public const string colorPattern = @"(?<cA>A|Alpha):(?<fA>\d+(?:(?:\.|\,)\d+)?) (?<cR>R|Red):(?<fR>\d+(?:(?:\.|\,)\d+)?) (?<cG>G|Green):(?<fG>\d+(?:(?:\.|\,)\d+)?) (?<cB>B|Blue):(?<fB>\d+(?:(?:\.|\,)\d+)?)(?:\|(?<sA>A|Alpha):(?<sfA>\d+(?:(?:\.|\,)\d+)?) (?<sR>R|Red):(?<sfR>\d+(?:(?:\.|\,)\d+)?) (?<sG>G|Green):(?<sfG>\d+(?:(?:\.|\,)\d+)?) (?<sB>B|Blue):(?<sfB>\d+(?:(?:\.|\,)\d+)?)|)";
        /// <summary>
        /// Color validator
        /// </summary>
        private static readonly Regex colorRegex = new Regex(colorPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

        /// <summary>
        /// Parses a sentence
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="defaultForeColor">Default fore color</param>
        /// <param name="defaultShadowColor">Default shadow color</param>
        /// <returns>Returns the parsed text struct</returns>
        public static FontMapParsedSentence ParseSentence(string text, Color4 defaultForeColor, Color4 defaultShadowColor)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new FontMapParsedSentence();
            }

            List<string> sentenceParts = new List<string>();
            List<Color4[]> colorParts = new List<Color4[]>();
            List<Color4[]> shadowColorParts = new List<Color4[]>();

            //Find lines
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int l = 0; l < lines.Length; l++)
            {
                string line = lines[l];
                bool lastLine = l == lines.Length - 1;

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                ParseLine(line, defaultForeColor, defaultShadowColor, out string parsedLine, out var charColors, out var charShadowColors);

                var parts = parsedLine.Split(new[] { " " }, StringSplitOptions.None);

                for (int p = 0; p < parts.Length; p++)
                {
                    string part = parts[p];
                    bool lastPart = p == parts.Length - 1;

                    var partColors = charColors.Take(part.Length + 1);
                    charColors = charColors.Skip(part.Length + 1);

                    var partShadowColors = charShadowColors.Take(part.Length + 1);
                    charShadowColors = charShadowColors.Skip(part.Length + 1);

                    sentenceParts.Add(part);
                    colorParts.Add(partColors.ToArray());
                    shadowColorParts.Add(partShadowColors.ToArray());

                    if (lastPart)
                    {
                        break;
                    }

                    sentenceParts.Add(" ");
                    colorParts.Add(new Color4[] { Color.Transparent });
                    shadowColorParts.Add(new Color4[] { Color.Transparent });
                }

                if (lastLine)
                {
                    break;
                }

                sentenceParts.Add(Environment.NewLine);
                colorParts.Add(new Color4[] { Color.Transparent });
                shadowColorParts.Add(new Color4[] { Color.Transparent });
            }

            return new FontMapParsedSentence
            {
                Text = string.Join(string.Empty, sentenceParts.ToArray()),
                Words = sentenceParts.ToArray(),
                Colors = colorParts.ToArray(),
                ShadowColors = shadowColorParts.ToArray(),
            };
        }
        /// <summary>
        /// Parses a line
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="defaultForeColor">Default fore color</param>
        /// <param name="defaultShadowColor">Default shadow color</param>
        /// <param name="parsedString">Returns the parsed line</param>
        /// <param name="foreColors">Returns a fore color by character list</param>
        /// <param name="shadowColors">Returns a shadow color by character list</param>
        private static void ParseLine(string text, Color4 defaultForeColor, Color4 defaultShadowColor, out string parsedString, out IEnumerable<Color4> foreColors, out IEnumerable<Color4> shadowColors)
        {
            Dictionary<int, Color4> foreColorValues = new Dictionary<int, Color4>();
            Dictionary<int, Color4> shadowColorValues = new Dictionary<int, Color4>();
            int deletedSize = 0;

            parsedString = colorRegex.Replace(
                text,
                (m) =>
                {
                    ReadMatch(m, out var mForeColor, out var mShadowColor);

                    if (mForeColor.HasValue)
                    {
                        foreColorValues.Add(m.Index - deletedSize, mForeColor.Value);
                    }
                    if (mShadowColor.HasValue)
                    {
                        shadowColorValues.Add(m.Index - deletedSize, mShadowColor.Value);
                    }

                    deletedSize += m.Length;

                    return string.Empty;
                });

            List<Color4> foreColorsByChar = new List<Color4>();
            List<Color4> shadowColorsByChar = new List<Color4>();

            // Fill result
            Color4 currentForeColor = defaultForeColor;
            Color4 currentShadowColor = defaultShadowColor;
            for (int i = 0; i < parsedString.Length; i++)
            {
                if (foreColorValues.ContainsKey(i))
                {
                    currentForeColor = foreColorValues[i];
                }

                if (shadowColorValues.ContainsKey(i))
                {
                    currentShadowColor = shadowColorValues[i];
                }

                foreColorsByChar.Add(currentForeColor);
                shadowColorsByChar.Add(currentShadowColor);
            }

            foreColors = foreColorsByChar;
            shadowColors = shadowColorsByChar;
        }
        /// <summary>
        /// Reads a semantic match in the parsed text
        /// </summary>
        /// <param name="match">Match</param>
        /// <param name="foreColor">Returns the fore color value if any</param>
        /// <param name="shadowColor">Returns the shadow color value if any</param>
        private static void ReadMatch(Match match, out Color4? foreColor, out Color4? shadowColor)
        {
            foreColor = null;
            shadowColor = null;

            foreach (var group in match.Groups)
            {
                if (group is not Group gr)
                {
                    continue;
                }

                if (gr.Name == "cA")
                {
                    string vA = match.Groups["fA"].Value;
                    string vR = match.Groups["fR"].Value;
                    string vG = match.Groups["fG"].Value;
                    string vB = match.Groups["fB"].Value;

                    if (gr.Value == "Alpha")
                    {
                        float a = float.Parse(vA);
                        float r = float.Parse(vR);
                        float g = float.Parse(vG);
                        float b = float.Parse(vB);
                        foreColor = new Color4(r, g, b, a);
                    }
                    else if (gr.Value == "A")
                    {
                        int a = int.Parse(vA);
                        int r = int.Parse(vR);
                        int g = int.Parse(vG);
                        int b = int.Parse(vB);
                        foreColor = new Color(r, g, b, a);
                    }
                }

                if (gr.Name == "sA")
                {
                    string vA = match.Groups["sfA"].Value;
                    string vR = match.Groups["sfR"].Value;
                    string vG = match.Groups["sfG"].Value;
                    string vB = match.Groups["sfB"].Value;

                    if (gr.Value == "Alpha")
                    {
                        float a = float.Parse(vA);
                        float r = float.Parse(vR);
                        float g = float.Parse(vG);
                        float b = float.Parse(vB);
                        shadowColor = new Color4(r, g, b, a);
                    }
                    else if (gr.Value == "A")
                    {
                        int a = int.Parse(vA);
                        int r = int.Parse(vR);
                        int g = int.Parse(vG);
                        int b = int.Parse(vB);
                        shadowColor = new Color(r, g, b, a);
                    }
                }
            }
        }
    }

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
        public int Count()
        {
            return Words?.Count() ?? 0;
        }
        /// <summary>
        /// Gets the parsed word at index
        /// </summary>
        /// <param name="index">Index</param>
        public FontMapParsedWord GetWord(int index)
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
        public int Count()
        {
            return Word?.Length ?? 0;
        }
        /// <summary>
        /// Gets the color of the character at index
        /// </summary>
        /// <param name="index">Index</param>
        public Color4 GetColor(int index)
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
        public Color4 GetShadowColor(int index)
        {
            if (index < Count())
            {
                return ShadowColors.ElementAtOrDefault(index);
            }

            return Color.Transparent;
        }
    }
}
