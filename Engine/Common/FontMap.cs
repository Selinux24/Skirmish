using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

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
        private static List<FontMap> gCache = new List<FontMap>();

        /// <summary>
        /// Clears and dispose font cache
        /// </summary>
        internal static void ClearCache()
        {
            foreach (FontMap map in gCache)
            {
                map.Dispose();
            }

            gCache.Clear();
        }

        /// <summary>
        /// Maximum text length
        /// </summary>
        public const int MAXTEXTLENGTH = 1024;
        /// <summary>
        /// Texture size
        /// </summary>
        public const int TEXTURESIZE = 1024;
        /// <summary>
        /// Key codes
        /// </summary>
        public const uint KeyCodes = 512;

        /// <summary>
        /// Map
        /// </summary>
        private Dictionary<char, FontMapChar> map = new Dictionary<char, FontMapChar>();

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
        public FontMapStyleEnum Style { get; private set; }
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
        public static FontMap Map(Game game, string font, float size, FontMapStyleEnum style)
        {
            var fMap = gCache.Find(f => f.Font == font && f.Size == size && f.Style == style);
            if (fMap == null)
            {
                fMap = new FontMap()
                {
                    Font = font,
                    Size = size,
                    Style = style,
                };

                using (var bmp = new Bitmap(TEXTURESIZE, TEXTURESIZE))
                using (var gra = System.Drawing.Graphics.FromImage(bmp))
                {
                    gra.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                    gra.FillRegion(
                        Brushes.Transparent,
                        new Region(new System.Drawing.RectangleF(0, 0, TEXTURESIZE, TEXTURESIZE)));

                    using (var fmt = StringFormat.GenericDefault)
                    using (var fnt = new Font(font, size, (FontStyle)style, GraphicsUnit.Pixel))
                    {
                        float left = 0f;
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
                                s.Width = fnt.SizeInPoints;
                            }

                            if (left + s.Width >= TEXTURESIZE)
                            {
                                left = 0f;
                                top += (int)s.Height + 1;
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

                            left += s.Width;
                            left = (int)left;
                        }
                    }

                    using (var mstr = new MemoryStream())
                    {
                        bmp.Save(mstr, ImageFormat.Png);

                        fMap.Texture = game.ResourceManager.CreateResource(mstr.GetBuffer());
                    }
                }

                gCache.Add(fMap);
            }

            return fMap;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FontMap()
            : base()
        {

        }
        /// <summary>
        /// Dispose map resources
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.Texture);
            Helper.Dispose(this.map);
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

                    Vector3[] cv;
                    Vector2[] cuv;
                    uint[] ci;
                    GeometryUtil.CreateSprite(
                        pos,
                        chr.Width, chr.Height, 0, 0,
                        chr.X, (int)chr.Y,
                        TEXTURESIZE,
                        out cv, out cuv, out ci);

                    ci.ForEach((i) => { indexList.Add(i + (uint)vertList.Count); });

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
