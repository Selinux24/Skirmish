using Engine.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace Engine.Windows
{
    /// <summary>
    /// Windows fonts helper
    /// </summary>
    public class WindowsFonts : IFonts
    {
        /// <summary>
        /// Parses the specified font family string
        /// </summary>
        /// <param name="fontFamily">Comma separated font family string</param>
        /// <returns>Returns an array of families</returns>
        private static IEnumerable<string> ParseFontFamilies(string fontFamily)
        {
            string[] fonts = fontFamily.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (!fonts.Any())
            {
                return Enumerable.Empty<string>();
            }

            return fonts
                .Select(f => f?.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToArray();
        }
        /// <summary>
        /// Creates a font map
        /// </summary>
        /// <param name="generator">Keycode generator</param>
        /// <param name="mapParams">Font map parameters</param>
        /// <param name="family">Font family</param>
        /// <param name="size">Font size</param>
        /// <param name="style">Font style</param>
        /// <returns>Returns the font map description</returns>
        private static FontMapDescription FromFamily(FontMapKeycodeGenerator generator, FontMapProcessParameters mapParams, FontFamily family, float size, FontMapStyles style)
        {
            //Calc the destination texture width and height
            var mapSize = MeasureMap(generator, mapParams, family, size, style);

            var fontDesc = new FontMapDescription()
            {
                FontName = family.Name,
                FontSize = size,
                FontStyle = style,
                Map = new Dictionary<char, FontMapChar>(),
            };

            using (var bmp = new Bitmap(mapSize.Width, mapSize.Height))
            using (var gra = System.Drawing.Graphics.FromImage(bmp))
            using (var fmt = StringFormat.GenericDefault)
            using (var fnt = new Font(family, size, (FontStyle)style, GraphicsUnit.Pixel))
            {
                gra.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                gra.FillRegion(
                    Brushes.Transparent,
                    new Region(new RectangleF(0, 0, mapSize.Width, mapSize.Height)));

                float left = 0f;
                float top = 0f;
                int charSeparationPixels = (int)(size * mapParams.CharSeparationThr);

                foreach (char c in generator.Keys)
                {
                    var s = gra.MeasureString(
                        c.ToString(),
                        fnt,
                        int.MaxValue,
                        fmt);

                    if (left + s.Width >= mapSize.Width)
                    {
                        //Next texture line with lineSeparationPixels
                        left = 0;
                        top += (int)s.Height + mapParams.LineSeparationPixels;
                    }

                    gra.DrawString(
                        c.ToString(),
                        fnt,
                        Brushes.White,
                        left,
                        top,
                        fmt);

                    var chr = new FontMapChar()
                    {
                        X = left,
                        Y = top,
                        Width = s.Width,
                        Height = s.Height,
                    };

                    fontDesc.Map.Add(c, chr);

                    left += (int)s.Width + charSeparationPixels;
                }

                fontDesc.GetSpaceSize(out float wsWidth, out float wsHeight);

                //Adds the white space
                var wsChr = new FontMapChar()
                {
                    X = left,
                    Y = top,
                    Width = wsWidth,
                    Height = wsHeight,
                };
                fontDesc.Map.Add(' ', wsChr);

                fontDesc.TextureWidth = bmp.Width;
                fontDesc.TextureHeight = bmp.Height;

                //Generate the texture
                fontDesc.ImageStream = new MemoryStream();
                bmp.Save(fontDesc.ImageStream, ImageFormat.Png);
            }

            return fontDesc;
        }
        /// <summary>
        /// Gets the font map size
        /// </summary>
        /// <param name="generator">Keycode generator</param>
        /// <param name="mapParams">Font map parameters</param>
        /// <param name="family">Font family</param>
        /// <param name="size">Font size</param>
        /// <param name="style">Font style</param>
        /// <returns>Returns the font map size</returns>
        private static Size MeasureMap(FontMapKeycodeGenerator generator, FontMapProcessParameters mapParams, FontFamily family, float size, FontMapStyles style)
        {
            int width = 0;
            int height = 0;

            using (var bmp = new Bitmap(100, 100))
            using (var gra = System.Drawing.Graphics.FromImage(bmp))
            using (var fmt = StringFormat.GenericDefault)
            using (var fnt = new Font(family, size, (FontStyle)style, GraphicsUnit.Pixel))
            {
                float left = 0f;
                float top = 0f;
                int charSeparationPixels = (int)(size * mapParams.CharSeparationThr);

                foreach (char c in generator.Keys)
                {
                    var s = gra.MeasureString(
                        c.ToString(),
                        fnt,
                        int.MaxValue,
                        fmt);

                    if (left + s.Width >= mapParams.MaxTextureSize)
                    {
                        //Next line with lineSeparationPixels
                        left = 0;
                        top += (int)s.Height + mapParams.LineSeparationPixels;

                        //Store height
                        height = Math.Max(height, (int)top + (int)s.Height + 1);
                    }

                    //Store width
                    width = Math.Max(width, (int)left + (int)s.Width);

                    left += (int)s.Width + charSeparationPixels;
                }
            }

            width = Helper.NextPowerOfTwo(width);
            height = Helper.NextPowerOfTwo(height);

            return new Size(width, height);
        }

        /// <inheritdoc/>
        public FontMapDescription FromFile(FontMapKeycodeGenerator generator, FontMapProcessParameters mapParams, string fileName, float size, FontMapStyles style)
        {
            using (PrivateFontCollection collection = new PrivateFontCollection())
            {
                collection.AddFontFile(fileName);

                using (FontFamily family = new FontFamily(collection.Families[0].Name, collection))
                {
                    return FromFamily(generator, mapParams, family, size, style);
                }
            }
        }
        /// <inheritdoc/>
        public FontMapDescription FromFamilyName(FontMapKeycodeGenerator generator, FontMapProcessParameters mapParams, string familyName, float size, FontMapStyles style)
        {
            using (var fontFamily = new FontFamily(familyName))
            {
                return FromFamily(generator, mapParams, fontFamily, size, style);
            }
        }

        /// <inheritdoc/>
        public string FindFonts(string fontFamily)
        {
            if (string.IsNullOrWhiteSpace(fontFamily))
            {
                return null;
            }

            var fonts = ParseFontFamilies(fontFamily);
            if (!fonts.Any())
            {
                return null;
            }

            var font = fonts.FirstOrDefault(fnt =>
            {
                return Array.Exists(FontFamily.Families, f => string.Equals(f.Name, fnt, StringComparison.OrdinalIgnoreCase));
            });

            return font;
        }

        /// <inheritdoc/>
        public string GetFromFileFontName(string fileName)
        {
            using (PrivateFontCollection collection = new PrivateFontCollection())
            {
                collection.AddFontFile(fileName);

                return collection.Families[0].Name;
            }
        }
    }
}
