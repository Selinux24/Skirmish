using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.UI
{
    /// <summary>
    /// Font map description
    /// </summary>
    public struct FontMapDescription
    {
        /// <summary>
        /// Font name
        /// </summary>
        public string FontName { get; set; }
        /// <summary>
        /// Font size
        /// </summary>
        public float FontSize { get; set; }
        /// <summary>
        /// Font style
        /// </summary>
        public FontMapStyles FontStyle { get; set; }
        /// <summary>
        /// Character map
        /// </summary>
        public Dictionary<char, FontMapChar> Map { get; set; }

        /// <summary>
        /// Texture width
        /// </summary>
        public int TextureWidth { get; set; }
        /// <summary>
        /// Texture height
        /// </summary>
        public int TextureHeight { get; set; }
        /// <summary>
        /// Generated texture stream
        /// </summary>
        public MemoryStream ImageStream { get; set; }

        /// <summary>
        /// Gets the font's white space size
        /// </summary>
        /// <param name="width">White space width</param>
        /// <param name="height">White space height</param>
        public readonly void GetSpaceSize(out float width, out float height)
        {
            char defChar = GetSampleCharacter();

            var mapChar = Map[defChar];

            width = mapChar.Width;
            height = mapChar.Height;
        }
        /// <summary>
        /// Gets the sample character
        /// </summary>
        /// <returns>Returns the sample character</returns>
        /// <remarks>Used for map the space if not specified</remarks>
        public readonly char GetSampleCharacter()
        {
            char defChar = 'X';

            var keys = GetKeys();

            if (!Array.Exists(keys, c => c == defChar))
            {
                defChar = keys.FirstOrDefault();
            }

            return defChar;
        }
        /// <summary>
        /// Gets the map keys
        /// </summary>
        public readonly char[] GetKeys()
        {
            return [.. Map.Keys];
        }
    }
}
