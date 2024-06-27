using SharpDX;
using System.Collections.Concurrent;
using System.Linq;

namespace Engine.UI
{
    /// <summary>
    /// Font map caché helper
    /// </summary>
    static class FontMapCache
    {
        /// <summary>
        /// Font cache
        /// </summary>
        private static readonly ConcurrentBag<FontMap> gCache = [];

        /// <summary>
        /// Adds a new map to the caché
        /// </summary>
        /// <param name="fMap"></param>
        public static void Add(FontMap fMap)
        {
            gCache.Add(fMap);
        }
        /// <summary>
        /// Gets a font map by name
        /// </summary>
        /// <param name="fontName">Font name (case sensitive)</param>
        /// <returns>Returns the font map</returns>
        public static FontMap Get(string fontName)
        {
            return gCache.FirstOrDefault(f => f != null && string.Equals(f.FontName, fontName));
        }
        /// <summary>
        /// Gets a font map by family, size and style
        /// </summary>
        /// <param name="fontFamily">Font family</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the font map</returns>
        public static FontMap Get(string fontFamily, float size, FontMapStyles style)
        {
            return gCache.FirstOrDefault(f => f != null && string.Equals(f.FontName, fontFamily) && MathUtil.NearEqual(f.FontSize, size) && f.FontStyle == style);
        }

        /// <summary>
        /// Clears and dispose font cache
        /// </summary>
        public static void Clear()
        {
            while (!gCache.IsEmpty)
            {
                if (gCache.TryTake(out FontMap fmap))
                {
                    fmap.Dispose();
                }
            }
        }
    }
}
