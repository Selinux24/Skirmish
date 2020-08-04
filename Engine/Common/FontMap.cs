using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Engine.Common
{
    using Engine.Content;
    using Engine.UI;
    using SharpDX;

    /// <summary>
    /// Font map
    /// </summary>
    class FontMap : IDisposable
    {
        /// <summary>
        /// Font cache
        /// </summary>
        private static readonly List<FontMap> gCache = new List<FontMap>();
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
        /// Clears and dispose font cache
        /// </summary>
        internal static void ClearCache()
        {
            foreach (var fmap in gCache)
            {
                fmap?.Dispose();
            }

            gCache.Clear();
        }

        /// <summary>
        /// Maximum text length
        /// </summary>
        public const int MAXTEXTLENGTH = 1024;
        /// <summary>
        /// Maximum texture size
        /// </summary>
        public const int MAXTEXTURESIZE = 1024 * 4;
        /// <summary>
        /// Key codes
        /// </summary>
        public const uint KeyCodes = 512;

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
        /// Gets map keys
        /// </summary>
        public static char[] ValidKeys
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
                Console.WriteLine($"Font resource not found: {fontFileName}");

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

            var fMap = gCache.FirstOrDefault(f => f != null && f.Font == fontName);
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
        /// Creates a font map of the specified font and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="font">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        public static FontMap FromFamily(Game game, string font, float size, FontMapStyles style)
        {
            if (!FontFamily.Families.Any(f => string.Equals(f.Name, font, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"Font not found: {font}");

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
            var fMap = gCache.FirstOrDefault(f => f != null && f.Font == family.Name && f.Size == size && f.Style == style);
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

                    for (int i = 0; i < ValidKeys.Length; i++)
                    {
                        char c = ValidKeys[i];

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
                gCache.Add(fMap);
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

                for (int i = 0; i < ValidKeys.Length; i++)
                {
                    char c = ValidKeys[i];

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
                Console.WriteLine($"Bad coordinate descriptor for X value. Single spected: {xValue}");
            }

            if (!float.TryParse(yValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                Console.WriteLine($"Bad coordinate descriptor for Y value. Single spected: {yValue}");
            }

            return new Vector2(x, y);
        }

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
        /// <param name="maxLength">Maximum length</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <param name="vertices">Gets generated vertices</param>
        /// <param name="indices">Gets generated indices</param>
        /// <param name="size">Gets generated sentence total size</param>
        public void MapSentence(
            string text,
            float maxLength,
            TextAlign horizontalAlign,
            VerticalAlign verticalAlign,
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

                if (!firstWord && pos.X > maxLength)
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

            vertices = AlignVertices(vertList, maxLength, horizontalAlign).ToArray();
            indices = indexList.ToArray();
            size = MeasureText(vertices);
        }

        private static IEnumerable<VertexPositionTexture> AlignVertices(IEnumerable<VertexPositionTexture> vertices, float maxLength, TextAlign horizontalAlign)
        {
            if (horizontalAlign == TextAlign.Left)
            {
                return vertices.ToArray();
            }

            var verts = vertices.ToArray();

            //Separate lines
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

            //Relocate lines
            List<VertexPositionTexture> res = new List<VertexPositionTexture>();
            foreach (var l in lines)
            {
                var size = MeasureText(l);

                float diff;

                if (horizontalAlign == TextAlign.Center)
                {
                    diff = (maxLength - size.X) * 0.5f;
                }
                else if (horizontalAlign == TextAlign.Right)
                {
                    diff = maxLength - size.X;
                }
                else
                {
                    continue;
                }

                for (int i = 0; i < l.Length; i++)
                {
                    l[i].Position.X += diff;
                }

                res.AddRange(l);
            }

            return res;
        }

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
        /// Parses a sentence
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Returns a list of words</returns>
        private static string[] ParseSentence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new[] { string.Empty.PadRight(MAXTEXTLENGTH) };
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
            width = map['X'].Width;
            height = map['X'].Height;
        }
    }
}
