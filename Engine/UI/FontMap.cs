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
        /// Maximum text length
        /// </summary>
        public const int MAXTEXTLENGTH = 1024 * 10;
        /// <summary>
        /// Maximum texture size
        /// </summary>
        public const int MAXTEXTURESIZE = 1024 * 4;
        /// <summary>
        /// Key codes
        /// </summary>
        public const uint KeyCodes = 512;

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
        /// Gets the default key list
        /// </summary>
        public static char[] DefaultKeys
        {
            get
            {
                List<char> cList = new List<char>((int)KeyCodes);

                for (uint i = 1; i < KeyCodes; i++)
                {
                    char c = (char)i;

                    if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }

                    if (char.IsControl(c))
                    {
                        continue;
                    }

                    cList.Add(c);
                }

                return cList.ToArray();
            }
        }
        /// <summary>
        /// Aligns the vertices into the rectangle
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <returns>Returns a new vertex list</returns>
        private static IEnumerable<VertexPositionTexture> AlignVertices(IEnumerable<VertexPositionTexture> vertices, RectangleF rect, HorizontalTextAlign horizontalAlign, VerticalTextAlign verticalAlign)
        {
            if (horizontalAlign == HorizontalTextAlign.Left && verticalAlign == VerticalTextAlign.Top)
            {
                //Return a copy of the original enumerable
                return vertices.ToArray();
            }

            //Separate lines
            var lines = SeparateLines(vertices.ToArray());

            //Find total height (the column height)
            var textSize = MeasureText(vertices);

            //Relocate lines
            List<VertexPositionTexture> res = new List<VertexPositionTexture>();
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
        private static IEnumerable<VertexPositionTexture[]> SeparateLines(VertexPositionTexture[] verts)
        {
            List<VertexPositionTexture[]> lines = new List<VertexPositionTexture[]>();

            List<VertexPositionTexture> line = new List<VertexPositionTexture>();

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
        private static float GetDeltaX(HorizontalTextAlign horizontalAlign, float maxWidth, float lineWidth)
        {
            float diffX;
            if (horizontalAlign == HorizontalTextAlign.Center)
            {
                diffX = (maxWidth - lineWidth) * 0.5f;
            }
            else if (horizontalAlign == HorizontalTextAlign.Right)
            {
                diffX = maxWidth - lineWidth;
            }
            else
            {
                diffX = 0;
            }

            return diffX;
        }
        /// <summary>
        /// Gets the delta x
        /// </summary>
        /// <param name="verticalAlign">Vertical align</param>
        /// <param name="maxHeight">Maximum height</param>
        /// <param name="columnHeight">Column height</param>
        private static float GetDeltaY(VerticalTextAlign verticalAlign, float maxHeight, float columnHeight)
        {
            float diffY;
            if (verticalAlign == VerticalTextAlign.Middle)
            {
                diffY = (maxHeight - columnHeight) * 0.5f;
            }
            else if (verticalAlign == VerticalTextAlign.Bottom)
            {
                diffY = maxHeight - columnHeight;
            }
            else
            {
                diffY = 0;
            }

            return diffY;
        }
        /// <summary>
        /// Measures the vertex list
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <returns>Returns a vector with the width in the x component, and the height in the y component</returns>
        private static Vector2 MeasureText(IEnumerable<VertexPositionTexture> vertices)
        {
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            float minX = float.MaxValue;
            float minY = float.MaxValue;

            foreach (var v in vertices)
            {
                var p = v.Position;
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, -p.Y);

                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, -p.Y);
            }

            return new Vector2(maxX - minX, maxY - minY);
        }
        /// <summary>
        /// Parses a sentence
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Returns a list of words</returns>
        private static string[] ParseSentence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new string[] { };
            }

            if (text.Length > MAXTEXTLENGTH)
            {
                text = text.Substring(0, MAXTEXTLENGTH);
            }

            List<string> sentenceParts = new List<string>();

            //Find lines
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var words = line.Split(new[] { " " }, StringSplitOptions.None);
                    foreach (var word in words)
                    {
                        sentenceParts.Add(word);
                        sentenceParts.Add(" ");
                    }
                }

                sentenceParts.Add(Environment.NewLine);
            }

            return sentenceParts.ToArray();
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
        /// Creates a font map of the specified font file and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="contentPath">Content path</param>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        public static FontMap FromFile(Game game, string contentPath, string fontFileName, float size, FontMapStyles style)
        {
            var fileNames = ContentManager.FindPaths(contentPath, fontFileName);
            if (!fileNames.Any())
            {
                Logger.WriteWarning($"Font resource not found: {fontFileName}");

                return null;
            }

            using (PrivateFontCollection collection = new PrivateFontCollection())
            {
                collection.AddFontFile(fileNames.FirstOrDefault());

                using (FontFamily family = new FontFamily(collection.Families[0].Name, collection))
                {
                    return FromFamily(game, family, size, style);
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
                    Font = fontName,
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
        /// <param name="fontFamily">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        /// <remarks>The font family must exists in the FontFamily.Families collection</remarks>
        public static FontMap FromFamily(Game game, string fontFamily, float size, FontMapStyles style)
        {
            string[] fonts = ParseFontFamilies(fontFamily);
            if (!fonts.Any())
            {
                Logger.WriteWarning("Font family not specified");

                return null;
            }

            var font = fonts.FirstOrDefault(fnt =>
            {
                return FontFamily.Families.Any(f => string.Equals(f.Name, fnt, StringComparison.OrdinalIgnoreCase));
            });

            if (font == null)
            {
                Logger.WriteWarning($"Font familiy not found in the graphic context: {fontFamily}");

                return null;
            }

            using (FontFamily family = new FontFamily(font))
            {
                return FromFamily(game, family, size, style);
            }
        }
        /// <summary>
        /// Creates a font map of the specified font and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="family">Font family</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        private static FontMap FromFamily(Game game, FontFamily family, float size, FontMapStyles style)
        {
            var fMap = FontMapCache.Get(family, size, style);
            if (fMap == null)
            {
                //Calc the destination texture width and height
                MeasureMap(family, size, style, out int width, out int height);

                fMap = new FontMap()
                {
                    Font = family.Name,
                    Size = size,
                    Style = style,
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

                    for (int i = 0; i < DefaultKeys.Length; i++)
                    {
                        char c = DefaultKeys[i];

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
        /// Measures the map to return the destination width and height of the texture
        /// </summary>
        /// <param name="family">Font family</param>
        /// <param name="size">Font size</param>
        /// <param name="style">Font Style</param>
        /// <param name="width">Resulting width</param>
        /// <param name="height">Resulting height</param>
        private static void MeasureMap(FontFamily family, float size, FontMapStyles style, out int width, out int height)
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

                for (int i = 0; i < DefaultKeys.Length; i++)
                {
                    char c = DefaultKeys[i];

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
                Logger.WriteWarning($"Bad coordinate descriptor for X value. Single spected: {xValue}");
            }

            if (!float.TryParse(yValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                Logger.WriteWarning($"Bad coordinate descriptor for Y value. Single spected: {yValue}");
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
        public string Font { get; private set; }
        /// <summary>
        /// Font size
        /// </summary>
        public float Size { get; private set; }
        /// <summary>
        /// Font style
        /// </summary>
        public FontMapStyles Style { get; private set; }
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
        /// <param name="text">Sentence text</param>
        /// <param name="rect">Bounds rectangle</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <param name="vertices">Gets generated vertices</param>
        /// <param name="indices">Gets generated indices</param>
        /// <param name="size">Gets generated sentence total size</param>
        public void MapSentence(
            string text,
            RectangleF rect,
            HorizontalTextAlign horizontalAlign,
            VerticalTextAlign verticalAlign,
            out VertexPositionTexture[] vertices,
            out uint[] indices,
            out Vector2 size)
        {
            size = Vector2.Zero;
            vertices = null;
            indices = null;

            var words = ParseSentence(text);
            if (!words.Any())
            {
                return;
            }

            List<VertexPositionTexture> vertList = new List<VertexPositionTexture>();
            List<uint> indexList = new List<uint>();

            var spaceSize = MapSpace();
            Vector2 pos = Vector2.Zero;
            bool firstWord = true;

            foreach (var word in words)
            {
                if (string.IsNullOrEmpty(word))
                {
                    //Discard empty words
                    continue;
                }

                if (word == Environment.NewLine)
                {
                    //Move the position to the new line
                    pos.X = 0;
                    pos.Y -= (int)spaceSize.Y;

                    continue;
                }

                if (word == " ")
                {
                    //Add a space
                    pos.X += (int)spaceSize.X;

                    continue;
                }

                //Store previous cursor position
                Vector2 prevPos = pos;

                //Map the word
                MapWord(word, ref pos, out var wVerts, out var wIndices, out var wHeight);

                //Store the indices adding last vertext index in the list
                wIndices.ToList().ForEach((i) => { indexList.Add(i + (uint)vertList.Count); });

                if (!firstWord && pos.X > rect.Width)
                {
                    //Move the position to the last character of the new line
                    pos.X -= (int)prevPos.X;
                    pos.Y -= (int)wHeight;

                    //Move the word to the next line
                    Vector3 diff = new Vector3(prevPos.X, wHeight, 0);
                    for (int i = 0; i < wVerts.Length; i++)
                    {
                        wVerts[i].Position -= diff;
                    }
                }

                vertList.AddRange(wVerts);

                firstWord = false;
            }

            vertices = AlignVertices(vertList, rect, horizontalAlign, verticalAlign).ToArray();
            indices = indexList.ToArray();
            size = MeasureText(vertices);
        }
        /// <summary>
        /// Maps a space
        /// </summary>
        /// <returns>Returns the space size</returns>
        private Vector2 MapSpace()
        {
            Vector2 tmpPos = Vector2.Zero;
            MapWord(" ", ref tmpPos, out _, out _, out var tmpHeight);

            return new Vector2(tmpPos.X, tmpHeight);
        }
        /// <summary>
        /// Maps a word
        /// </summary>
        /// <param name="word">Word to map</param>
        /// <param name="pos">Position</param>
        /// <param name="vertices">Gets generated vertices</param>
        /// <param name="indices">Gets generated indices</param>
        /// <param name="height">Gets generated word height</param>
        private void MapWord(
            string word,
            ref Vector2 pos,
            out VertexPositionTexture[] vertices,
            out uint[] indices,
            out float height)
        {
            List<VertexPositionTexture> vertList = new List<VertexPositionTexture>();
            List<uint> indexList = new List<uint>();

            height = 0;

            foreach (char c in word)
            {
                if (!this.map.ContainsKey(c))
                {
                    //Discard unmapped characters
                    continue;
                }

                var chr = this.map[c];

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
                vertList.AddRange(VertexPositionTexture.Generate(s.Vertices, s.Uvs));

                //Move the cursor position to the next character
                float d = (float)(chr.Width - Math.Sqrt(chr.Width));
                pos.X += d;

                //Store maximum height
                height = Math.Max(height, chr.Height);
            }

            vertices = vertList.ToArray();
            indices = indexList.ToArray();
        }
        /// <summary>
        /// Gets the font's white space size
        /// </summary>
        /// <param name="width">White space width</param>
        /// <param name="height">White space height</param>
        private void GetSpaceSize(out float width, out float height)
        {
            char defChar = this.GetSampleCharacter();

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

            var keys = this.GetKeys();

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
