using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Engine.UI
{
    using Engine.Common;
    using Engine.Content;
    using SharpDX;

    /// <summary>
    /// Font map
    /// </summary>
    class FontMap : IDisposable
    {
        /// <summary>
        /// Maximum texture size
        /// </summary>
        public const int MAXTEXTURESIZE = 1024 * 4;

        /// <summary>
        /// Default line separation in pixels
        /// </summary>
        private const int lineSeparationPixels = 10;
        /// <summary>
        /// Character separation threshold, based on font size
        /// </summary>
        /// <remarks>Separation = Font size * Thr</remarks>
        private const float charSeparationThr = 0.25f;

        /// <summary>
        /// Aligns the vertices into the rectangle
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <returns>Returns a new vertex list</returns>
        private static IEnumerable<VertexFont> AlignVertices(IEnumerable<VertexFont> vertices, RectangleF rect, TextHorizontalAlign horizontalAlign, TextVerticalAlign verticalAlign)
        {
            //Separate lines
            var lines = SeparateLines(vertices.ToArray());

            //Find total height (the column height)
            var textSize = MeasureText(vertices);

            //Relocate lines
            List<VertexFont> res = new List<VertexFont>();
            foreach (var l in lines)
            {
                //Find this line width
                var lineSize = MeasureText(l);

                //Calculate displacement deltas
                float diffX = GetDeltaX(horizontalAlign, rect.Width, lineSize.X);
                float diffY = GetDeltaY(verticalAlign, rect.Height, textSize.Y);

                if (diffX == 0 && diffY == 0)
                {
                    //No changes, add the line and skip the update
                    res.AddRange(l);

                    continue;
                }

                //Update all the coordinates
                for (int i = 0; i < l.Length; i++)
                {
                    l[i].Position.X += diffX;
                    l[i].Position.Y -= diffY;
                }

                //Add the updated line to the result
                res.AddRange(l);
            }

            return res;
        }
        /// <summary>
        /// Separate the vertex list into a list o vertex list by text line
        /// </summary>
        /// <param name="verts">Vertex list</param>
        private static IEnumerable<VertexFont[]> SeparateLines(VertexFont[] verts)
        {
            List<VertexFont[]> lines = new List<VertexFont[]>();

            List<VertexFont> line = new List<VertexFont>();

            for (int i = 0; i < verts.Length; i += 4)
            {
                if (verts[i].Position.X == 0)
                {
                    if (line.Count > 0)
                    {
                        lines.Add(line.ToArray());
                    }

                    line.Clear();
                }

                line.AddRange(verts.Skip(i).Take(4));
            }

            if (line.Count > 0)
            {
                lines.Add(line.ToArray());
            }

            return lines;
        }
        /// <summary>
        /// Gets the delta x
        /// </summary>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="maxWidth">Maximum width</param>
        /// <param name="lineWidth">Line width</param>
        private static float GetDeltaX(TextHorizontalAlign horizontalAlign, float maxWidth, float lineWidth)
        {
            float diffX;
            if (horizontalAlign == TextHorizontalAlign.Center)
            {
                diffX = -lineWidth * 0.5f;
            }
            else if (horizontalAlign == TextHorizontalAlign.Right)
            {
                diffX = (maxWidth * 0.5f) - lineWidth;
            }
            else
            {
                diffX = -maxWidth * 0.5f;
            }

            return diffX;
        }
        /// <summary>
        /// Gets the delta x
        /// </summary>
        /// <param name="verticalAlign">Vertical align</param>
        /// <param name="maxHeight">Maximum height</param>
        /// <param name="columnHeight">Column height</param>
        private static float GetDeltaY(TextVerticalAlign verticalAlign, float maxHeight, float columnHeight)
        {
            float diffY;
            if (verticalAlign == TextVerticalAlign.Middle)
            {
                diffY = -columnHeight * 0.5f;
            }
            else if (verticalAlign == TextVerticalAlign.Bottom)
            {
                diffY = (maxHeight * 0.5f) - columnHeight;
            }
            else
            {
                diffY = -maxHeight * 0.5f;
            }

            return diffY;
        }
        /// <summary>
        /// Measures the vertex list
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <returns>Returns a vector with the width in the x component, and the height in the y component</returns>
        private static Vector2 MeasureText(IEnumerable<VertexFont> vertices)
        {
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            float minX = float.MaxValue;
            float minY = float.MaxValue;

            foreach (var p in vertices.Select(v => v.Position))
            {
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, -p.Y);

                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, -p.Y);
            }

            return new Vector2(maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Creates a font map of the specified font file and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="contentPath">Content path</param>
        /// <param name="generator">Keycode generator</param>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        public static FontMap FromFile(Game game, string contentPath, FontMapKeycodeGenerator generator, string fontFileName, float size, FontMapStyles style)
        {
            var fileNames = ContentManager.FindPaths(contentPath, fontFileName);
            if (!fileNames.Any())
            {
                Logger.WriteWarning(nameof(FontMap), $"Font resource not found: {fontFileName}");

                return null;
            }

            using (PrivateFontCollection collection = new PrivateFontCollection())
            {
                collection.AddFontFile(fileNames.FirstOrDefault());

                using (FontFamily family = new FontFamily(collection.Families[0].Name, collection))
                {
                    return FromFamily(game, generator, family, size, style);
                }
            }
        }
        /// <summary>
        /// Creates a font map of the specified font mapping
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="contentPath">Content path</param>
        /// <param name="fontMapping">Font mapping</param>
        /// <returns>Returns the created font map</returns>
        public static FontMap FromMap(Game game, string contentPath, FontMapping fontMapping)
        {
            string fontName = Path.Combine(contentPath, fontMapping.ImageFile);

            var fMap = FontMapCache.Get(fontName);
            if (fMap == null)
            {
                fMap = new FontMap()
                {
                    FontName = fontName,
                };

                fMap.Texture = game.ResourceManager.RequestResource(fontName, false);

                string fontMapName = Path.Combine(contentPath, fontMapping.MapFile);

                string[] charMaps = File.ReadAllLines(fontMapName);
                foreach (var charMap in charMaps)
                {
                    if (string.IsNullOrWhiteSpace(charMap))
                    {
                        continue;
                    }

                    if (charMap.StartsWith("size:", StringComparison.OrdinalIgnoreCase))
                    {
                        Vector2 textureSize = FromMap(charMap.Substring(6));

                        fMap.TextureWidth = (int)textureSize.X;
                        fMap.TextureHeight = (int)textureSize.Y;

                        continue;
                    }

                    int leftTopIndex = charMap.IndexOf(":") + 1;
                    int rightBottomIndex = charMap.IndexOf(";", leftTopIndex) + 1;

                    char c = charMap[0];
                    Vector2 topLeft = FromMap(charMap.Substring(leftTopIndex, rightBottomIndex - leftTopIndex));
                    Vector2 bottomRight = FromMap(charMap.Substring(rightBottomIndex));

                    var chr = new FontMapChar()
                    {
                        X = topLeft.X,
                        Y = topLeft.Y,
                        Width = bottomRight.X - topLeft.X,
                        Height = bottomRight.Y - topLeft.Y,
                    };

                    fMap.map.Add(c, chr);
                }
            }

            return fMap;
        }
        /// <summary>
        /// Creates a font map of the specified font family and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="generator">Keycode generator</param>
        /// <param name="fontFamily">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        /// <remarks>The font family must exists in the FontFamily.Families collection</remarks>
        public static FontMap FromFamily(Game game, FontMapKeycodeGenerator generator, string fontFamily, float size, FontMapStyles style)
        {
            string[] fonts = ParseFontFamilies(fontFamily);
            if (!fonts.Any())
            {
                Logger.WriteWarning(nameof(FontMap), "Font family not specified");

                return null;
            }

            var font = fonts.FirstOrDefault(fnt =>
            {
                return FontFamily.Families.Any(f => string.Equals(f.Name, fnt, StringComparison.OrdinalIgnoreCase));
            });

            if (font == null)
            {
                Logger.WriteWarning(nameof(FontMap), $"Font familiy not found in the graphic context: {fontFamily}");

                return null;
            }

            using (FontFamily family = new FontFamily(font))
            {
                return FromFamily(game, generator, family, size, style);
            }
        }
        /// <summary>
        /// Creates a font map of the specified font and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="generator">Keycode generator</param>
        /// <param name="family">Font family</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        private static FontMap FromFamily(Game game, FontMapKeycodeGenerator generator, FontFamily family, float size, FontMapStyles style)
        {
            var fMap = FontMapCache.Get(family, size, style);
            if (fMap == null)
            {
                //Calc the destination texture width and height
                MeasureMap(generator, family, size, style, out int width, out int height);

                fMap = new FontMap()
                {
                    FontName = family.Name,
                    FontSize = size,
                    FontStyle = style,
                };

                using (var bmp = new Bitmap(width, height))
                using (var gra = System.Drawing.Graphics.FromImage(bmp))
                using (var fmt = StringFormat.GenericDefault)
                using (var fnt = new Font(family, size, (FontStyle)style, GraphicsUnit.Pixel))
                {
                    gra.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    gra.FillRegion(
                        Brushes.Transparent,
                        new Region(new System.Drawing.RectangleF(0, 0, width, height)));

                    float left = 0f;
                    float top = 0f;
                    int charSeparationPixels = (int)(size * charSeparationThr);

                    foreach (char c in generator.Keys)
                    {
                        var s = gra.MeasureString(
                            c.ToString(),
                            fnt,
                            int.MaxValue,
                            fmt);

                        if (left + s.Width >= width)
                        {
                            //Next texture line with lineSeparationPixels
                            left = 0;
                            top += (int)s.Height + lineSeparationPixels;
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

                        fMap.map.Add(c, chr);

                        left += (int)s.Width + charSeparationPixels;
                    }

                    fMap.GetSpaceSize(out float wsWidth, out float wsHeight);

                    //Adds the white space
                    var wsChr = new FontMapChar()
                    {
                        X = left,
                        Y = top,
                        Width = wsWidth,
                        Height = wsHeight,
                    };

                    fMap.map.Add(' ', wsChr);

                    fMap.TextureWidth = bmp.Width;
                    fMap.TextureHeight = bmp.Height;

                    //Generate the texture
                    fMap.bitmapStream = new MemoryStream();
                    bmp.Save(fMap.bitmapStream, ImageFormat.Png);
                    fMap.Texture = game.ResourceManager.RequestResource(fMap.bitmapStream, false);
                }

                //Add map to the font cache
                FontMapCache.Add(fMap);
            }

            return fMap;
        }
        /// <summary>
        /// Parses the specified font family string
        /// </summary>
        /// <param name="fontFamily">Comma separated font family string</param>
        /// <returns>Returns an array of families</returns>
        private static string[] ParseFontFamilies(string fontFamily)
        {
            string[] fonts = fontFamily.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (!fonts.Any())
            {
                return new string[] { };
            }

            for (int i = 0; i < fonts.Length; i++)
            {
                fonts[i] = fonts[i].Trim();
            }

            return fonts;
        }
        /// <summary>
        /// Measures the map to return the destination width and height of the texture
        /// </summary>
        /// <param name="generator">Keycode generator</param>
        /// <param name="family">Font family</param>
        /// <param name="size">Font size</param>
        /// <param name="style">Font Style</param>
        /// <param name="width">Resulting width</param>
        /// <param name="height">Resulting height</param>
        private static void MeasureMap(FontMapKeycodeGenerator generator, FontFamily family, float size, FontMapStyles style, out int width, out int height)
        {
            width = 0;
            height = 0;

            using (var bmp = new Bitmap(100, 100))
            using (var gra = System.Drawing.Graphics.FromImage(bmp))
            using (var fmt = StringFormat.GenericDefault)
            using (var fnt = new Font(family, size, (FontStyle)style, GraphicsUnit.Pixel))
            {
                float left = 0f;
                float top = 0f;
                int charSeparationPixels = (int)(size * charSeparationThr);

                foreach (char c in generator.Keys)
                {
                    var s = gra.MeasureString(
                        c.ToString(),
                        fnt,
                        int.MaxValue,
                        fmt);

                    if (left + s.Width >= MAXTEXTURESIZE)
                    {
                        //Next line with lineSeparationPixels
                        left = 0;
                        top += (int)s.Height + lineSeparationPixels;

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
        }
        /// <summary>
        /// Reads a vector from a font-map line
        /// </summary>
        /// <param name="mapBitz">Map text bitz</param>
        /// <returns>Returns a vector</returns>
        private static Vector2 FromMap(string mapBitz)
        {
            string[] bitz = mapBitz?.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            if (bitz?.Any() != true || bitz.Length < 2)
            {
                return Vector2.Zero;
            }

            //Clean ';'
            string xValue = bitz[0].Replace(";", "");
            string yValue = bitz[1].Replace(";", "");

            if (!float.TryParse(xValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
            {
                Logger.WriteWarning(nameof(FontMap), $"Bad coordinate descriptor for X value. Single spected: {xValue}");
            }

            if (!float.TryParse(yValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                Logger.WriteWarning(nameof(FontMap), $"Bad coordinate descriptor for Y value. Single spected: {yValue}");
            }

            return new Vector2(x, y);
        }

        /// <summary>
        /// Map
        /// </summary>
        private Dictionary<char, FontMapChar> map = new Dictionary<char, FontMapChar>();
        /// <summary>
        /// Bitmap stream
        /// </summary>
        private MemoryStream bitmapStream = null;

        /// <summary>
        /// Texure width
        /// </summary>
        protected int TextureWidth = 0;
        /// <summary>
        /// Texture height
        /// </summary>
        protected int TextureHeight = 0;

        /// <summary>
        /// Font name
        /// </summary>
        public string FontName { get; private set; }
        /// <summary>
        /// Font size
        /// </summary>
        public float FontSize { get; private set; }
        /// <summary>
        /// Font style
        /// </summary>
        public FontMapStyles FontStyle { get; private set; }
        /// <summary>
        /// Font texture
        /// </summary>
        public EngineShaderResourceView Texture { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FontMap()
            : base()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FontMap()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Texture?.Dispose();
                Texture = null;

                map?.Clear();
                map = null;

                bitmapStream?.Dispose();
                bitmapStream = null;
            }
        }

        /// <summary>
        /// Maps a sentence
        /// </summary>
        /// <param name="sentenceDesc">Sentence</param>
        /// <param name="processShadows">Process text shadow</param>
        /// <param name="rect">Bounds rectangle</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <returns>Returns a sentence descriptor</returns>
        public FontMapSentenceDescriptor MapSentence(
            FontMapParsedSentence sentenceDesc,
            bool processShadows,
            RectangleF rect,
            TextHorizontalAlign horizontalAlign,
            TextVerticalAlign verticalAlign)
        {
            if (sentenceDesc.Count() <= 0)
            {
                return new FontMapSentenceDescriptor
                {
                    Vertices = new VertexFont[] { },
                    Indices = new uint[] { },
                    Size = Vector2.Zero,
                };
            }

            List<VertexFont> vertList = new List<VertexFont>();
            List<uint> indexList = new List<uint>();

            var spaceSize = MapSpace();
            Vector2 pos = Vector2.Zero;
            bool firstWord = true;

            for (int i = 0; i < sentenceDesc.Count(); i++)
            {
                var wordDesc = sentenceDesc.GetWord(i);

                if (string.IsNullOrEmpty(wordDesc.Word))
                {
                    //Discard empty words
                    continue;
                }

                if (wordDesc.Word == Environment.NewLine)
                {
                    //Move the position to the new line
                    pos.X = 0;
                    pos.Y -= (int)spaceSize.Y;

                    continue;
                }

                if (wordDesc.Word == " ")
                {
                    //Add a space
                    pos.X += (int)spaceSize.X;

                    continue;
                }

                //Store previous cursor position
                Vector2 prevPos = pos;

                //Map the word
                var w = MapWord(wordDesc, processShadows, ref pos);

                //Store the indices adding last vertext index in the list
                w.Indices.ToList().ForEach((index) => { indexList.Add(index + (uint)vertList.Count); });

                if (!firstWord && pos.X > rect.Width)
                {
                    //Move the position to the last character of the new line
                    pos.X -= (int)prevPos.X;
                    pos.Y -= (int)w.Height;

                    //Move the word to the next line
                    Vector3 diff = new Vector3(prevPos.X, w.Height, 0);
                    for (int index = 0; index < w.Vertices.Length; index++)
                    {
                        w.Vertices[index].Position -= diff;
                    }
                }

                vertList.AddRange(w.Vertices);

                firstWord = false;
            }

            var vertices = AlignVertices(vertList, rect, horizontalAlign, verticalAlign).ToArray();
            var indices = indexList.ToArray();
            var size = MeasureText(vertices);

            return new FontMapSentenceDescriptor
            {
                Vertices = vertices,
                Indices = indices,
                Size = size,
            };
        }
        /// <summary>
        /// Maps a space
        /// </summary>
        /// <returns>Returns the space size</returns>
        private Vector2 MapSpace()
        {
            Vector2 tmpPos = Vector2.Zero;
            var wordDesc = MapWord(FontMapParsedWord.Space, false, ref tmpPos);

            return new Vector2(tmpPos.X, wordDesc.Height);
        }
        /// <summary>
        /// Maps a word
        /// </summary>
        /// <param name="wordDesc">Word to map</param>
        /// <param name="processShadows">Process text shadow</param>
        /// <param name="pos">Position</param>
        /// <returns>Returns a word description</returns>
        private FontMapWordDescriptor MapWord(
            FontMapParsedWord wordDesc,
            bool processShadows,
            ref Vector2 pos)
        {
            List<VertexFont> vertList = new List<VertexFont>();
            List<uint> indexList = new List<uint>();

            float height = 0;

            for (int i = 0; i < wordDesc.Count(); i++)
            {
                char c = wordDesc.Word[i];

                if (!map.ContainsKey(c))
                {
                    //Discard unmapped characters
                    continue;
                }

                var chr = map[c];
                var chrColor = processShadows ? wordDesc.GetShadowColor(i) : wordDesc.GetColor(i);

                MapChar(chr, chrColor, pos, vertList, indexList);

                //Move the cursor position to the next character
                float d = (float)(chr.Width - Math.Sqrt(chr.Width));
                pos.X += d;

                //Store maximum height
                height = Math.Max(height, chr.Height);
            }

            return new FontMapWordDescriptor
            {
                Vertices = vertList.ToArray(),
                Indices = indexList.ToArray(),
                Height = height,
            };
        }
        /// <summary>
        /// Maps a character
        /// </summary>
        /// <param name="chr">Character</param>
        /// <param name="color">Character color</param>
        /// <param name="pos">Position</param>
        /// <param name="vertList">Vertex list to fill</param>
        /// <param name="indexList">Index list to fill</param>
        private void MapChar(FontMapChar chr, Color4 color, Vector2 pos, List<VertexFont> vertList, List<uint> indexList)
        {
            //Creates the texture UVMap
            var uv = GeometryUtil.CreateUVMap(
                chr.Width, chr.Height,
                chr.X, chr.Y,
                TextureWidth, TextureHeight);

            //Creates the sprite
            var s = GeometryUtil.CreateSprite(
                pos,
                chr.Width, chr.Height, 0, 0,
                uv);

            //Add indices to word index list
            s.Indices.ToList().ForEach((i) => { indexList.Add(i + (uint)vertList.Count); });

            //Store the vertices
            vertList.AddRange(VertexFont.Generate(s.Vertices, s.Uvs, color));
        }
        /// <summary>
        /// Gets the font's white space size
        /// </summary>
        /// <param name="width">White space width</param>
        /// <param name="height">White space height</param>
        private void GetSpaceSize(out float width, out float height)
        {
            char defChar = GetSampleCharacter();

            var mapChar = map[defChar];

            width = mapChar.Width;
            height = mapChar.Height;
        }
        /// <summary>
        /// Gets the sample character
        /// </summary>
        /// <returns>Returns the sample character</returns>
        /// <remarks>Used for map the space if not specified</remarks>
        public char GetSampleCharacter()
        {
            char defChar = 'X';

            var keys = GetKeys();

            if (!keys.Any(c => c == defChar))
            {
                defChar = keys.FirstOrDefault();
            }

            return defChar;
        }
        /// <summary>
        /// Gets the map keys
        /// </summary>
        public char[] GetKeys()
        {
            return map.Keys.ToArray();
        }
    }
}

