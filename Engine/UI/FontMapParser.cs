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
        private static readonly Regex colorRegex = new(colorPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

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

            var sentenceParts = new List<string>();
            var colorParts = new List<Color4[]>();
            var shadowColorParts = new List<Color4[]>();

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
            var foreColorValues = new Dictionary<int, Color4>();
            var shadowColorValues = new Dictionary<int, Color4>();
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

            var foreColorsByChar = new List<Color4>();
            var shadowColorsByChar = new List<Color4>();

            // Fill result
            var currentForeColor = defaultForeColor;
            var currentShadowColor = defaultShadowColor;
            for (int i = 0; i < parsedString.Length; i++)
            {
                if (foreColorValues.TryGetValue(i, out var foreColor))
                {
                    currentForeColor = foreColor;
                }

                if (shadowColorValues.TryGetValue(i, out var shadowColor))
                {
                    currentShadowColor = shadowColor;
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
}
