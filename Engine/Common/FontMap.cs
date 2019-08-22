using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Font map
    /// </summary>
    public class FontMap : IDisposable
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
                    if (!char.IsControl(c))
                    {
                        cList.Add(c);
                    }
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

                        if (c == ' ')
                        {
                            //White space
                            s.Width = fnt.SizeInPoints;
                        }

                        if (left + (int)s.Width + fMap.Delta + fMap.Delta >= width)
                        {
                            //Next texture line
                            left = fMap.Delta;
                            top += (int)s.Height + fMap.Delta;
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
                            Height = s.Height + (fMap.Delta / 2),
                        };

                        fMap.map.Add(c, chr);

                        left += (int)s.Width + fMap.Delta + fMap.Delta;
                    }

                    //Generate the texture
                    fMap.bitmapStream = new MemoryStream();
                    bmp.Save(fMap.bitmapStream, ImageFormat.Png);
                    fMap.Texture = game.ResourceManager.CreateResource(fMap.bitmapStream, false);
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
        /// <param name="vertices">Gets generated vertices</param>
        /// <param name="indices">Gets generated indices</param>
        /// <param name="size">Gets generated sentence total size</param>
        public void MapSentence(
            string text,
            out VertexPositionTexture[] vertices,
            out uint[] indices,
            out Vector2 size)
        {
            size = Vector2.Zero;

            Vector2 pos = Vector2.Zero;

            List<VertexPositionTexture> vertList = new List<VertexPositionTexture>();
            List<uint> indexList = new List<uint>();

            if (string.IsNullOrWhiteSpace(text))
            {
                text = string.Empty.PadRight(MAXTEXTLENGTH);
            }
            else if (text.Length > MAXTEXTLENGTH)
            {
                text = text.Substring(0, MAXTEXTLENGTH);
            }

            float height = 0;

            foreach (char c in text)
            {
                if (this.map.ContainsKey(c))
                {
                    var chr = this.map[c];

                    var uv = GeometryUtil.CreateUVMap(
                        chr.Width, chr.Height,
                        chr.X, chr.Y,
                        TextureWidth, TextureHeight);

                    GeometryUtil.CreateSprite(
                        pos,
                        chr.Width, chr.Height, 0, 0,
                        uv,
                        out Vector3[] cv, out Vector2[] cuv, out uint[] ci);

                    ci.ToList().ForEach((i) => { indexList.Add(i + (uint)vertList.Count); });

                    vertList.AddRange(VertexPositionTexture.Generate(cv, cuv));

                    pos.X += chr.Width - (chr.Width * 0.3333f);
                    if (chr.Height > height) height = chr.Height;
                }
            }

            pos.Y = height;

            vertices = vertList.ToArray();
            indices = indexList.ToArray();
            size = pos;
        }
    }
}
