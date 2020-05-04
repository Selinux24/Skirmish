using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace Engine.Common
{
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
        /// Clears and dispose font cache
        /// </summary>
        internal static void ClearCache()
        {
            foreach (var fmap in gCache)
            {
                fmap.Dispose();
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
        public const int MAXTEXTURESIZE = 1024 * 8;
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
        /// Character margin delta
        /// </summary>
        public int Delta { get; private set; }
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
        /// Creates a font map of the specified font and size
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="font">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="bold">Weight</param>
        /// <returns>Returns created font map</returns>
        public static FontMap Map(Game game, string font, float size, FontMapStyles style)
        {
            var fMap = gCache.Find(f => f.Font == font && f.Size == size && f.Style == style);
            if (fMap == null)
            {
                //Calc the destination texture width and height
                MeasureMap(font, size, style, out int width, out int height);

                //Calc the delta value for margins an new lines
                float delta = (float)Math.Sqrt(size) + (size / 40f);

                fMap = new FontMap()
                {
                    Font = font,
                    Size = size,
                    Style = style,
                    Delta = (int)delta,
                    TextureWidth = width,
                    TextureHeight = height,
                };

                using (var bmp = new Bitmap(width, height))
                using (var gra = System.Drawing.Graphics.FromImage(bmp))
                using (var fmt = StringFormat.GenericDefault)
                using (var fnt = new Font(font, size, (FontStyle)style, GraphicsUnit.Pixel))
                {
                    gra.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    gra.FillRegion(
                        Brushes.Transparent,
                        new Region(new System.Drawing.RectangleF(0, 0, width, height)));

                    float left = fMap.Delta;
                    float top = 0f;

                    for (int i = 0; i < ValidKeys.Length; i++)
                    {
                        char c = ValidKeys[i];

                        var s = gra.MeasureString(
                            c.ToString(),
                            fnt,
                            int.MaxValue,
                            fmt);

                        if (left + s.Width + fMap.Delta >= width)
                        {
                            //Next texture line
                            left = fMap.Delta;
                            top += s.Height + fMap.Delta;
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

                        left += s.Width + fMap.Delta;
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
        /// <param name="font">Font name</param>
        /// <param name="size">Font size</param>
        /// <param name="style">Font Style</param>
        /// <param name="width">Resulting width</param>
        /// <param name="height">Resulting height</param>
        public static void MeasureMap(string font, float size, FontMapStyles style, out int width, out int height)
        {
            using (var bmp = new Bitmap(100, 100))
            using (var gra = System.Drawing.Graphics.FromImage(bmp))
            using (var fmt = StringFormat.GenericDefault)
            using (var fnt = new Font(font, size, (FontStyle)style, GraphicsUnit.Pixel))
            {
                string str = new string(ValidKeys);

                var s = gra.MeasureString(
                    str,
                    fnt,
                    int.MaxValue,
                    fmt);

                if (s.Width <= MAXTEXTURESIZE)
                {
                    width = (int)s.Width + 1;
                    height = (int)s.Height + 1;
                }
                else
                {
                    width = MAXTEXTURESIZE;
                    int a = (int)s.Width / MAXTEXTURESIZE;
                    height = ((int)s.Height + 1) * (a + 1);
                }
            }
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
        /// <param name="textArea">Rectangle area</param>
        /// <param name="vertices">Gets generated vertices</param>
        /// <param name="indices">Gets generated indices</param>
        /// <param name="size">Gets generated sentence total size</param>
        public void MapSentence(
            string text,
            Rectangle? textArea,
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

                if (textArea.HasValue && pos.X > textArea.Value.Width)
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
            }

            vertices = vertList.ToArray();
            indices = indexList.ToArray();

            float maxX = float.MinValue;
            float maxY = float.MaxValue;
            foreach (var v in vertices)
            {
                var p = v.Position;
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Min(maxY, p.Y);
            }

            size = new Vector2(maxX, -maxY);
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
