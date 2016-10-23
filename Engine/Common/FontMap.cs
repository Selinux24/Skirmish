using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using Bitmap = System.Drawing.Bitmap;
using Brushes = System.Drawing.Brushes;
using Device = SharpDX.Direct3D11.Device;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;
using Graph = System.Drawing.Graphics;
using GraphicsUnit = System.Drawing.GraphicsUnit;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using RectangleF = System.Drawing.RectangleF;
using Region = System.Drawing.Region;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using SizeF = System.Drawing.SizeF;
using StringFormat = System.Drawing.StringFormat;
using TextRenderingHint = System.Drawing.Text.TextRenderingHint;

namespace Engine.Common
{
    using Engine.Helpers;

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
        public int Size { get; private set; }
        /// <summary>
        /// Font texture
        /// </summary>
        public ShaderResourceView Texture { get; private set; }
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
        /// <param name="device">Graphics device</param>
        /// <param name="font">Font name</param>
        /// <param name="size">Size</param>
        /// <returns>Returns created font map</returns>
        public static FontMap Map(Device device, string font, int size)
        {
            FontMap map = gCache.Find(f => f.Font == font && f.Size == size);
            if (map == null)
            {
                map = new FontMap()
                {
                    Font = font,
                    Size = size,
                };

                using (Bitmap bmp = new Bitmap(TEXTURESIZE, TEXTURESIZE))
                using (Graph gra = Graph.FromImage(bmp))
                {
                    gra.TextContrast = 12;
                    gra.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                    gra.FillRegion(
                        Brushes.Transparent,
                        new Region(new RectangleF(0, 0, TEXTURESIZE, TEXTURESIZE)));

                    using (StringFormat fmt = StringFormat.GenericDefault)
                    using (Font fnt = new Font(font, size, FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        float left = 0f;
                        float top = 0f;

                        char[] keys = FontMap.ValidKeys;

                        for (int i = 0; i < keys.Length; i++)
                        {
                            char c = keys[i];

                            SizeF s = gra.MeasureString(
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
                                top += s.Height;
                            }

                            gra.DrawString(
                                c.ToString(),
                                fnt,
                                Brushes.White,
                                left,
                                top,
                                fmt);

                            FontMapChar chr = new FontMapChar()
                            {
                                X = (int)left,
                                Y = (int)top,
                                Width = (int)Math.Round(s.Width),
                                Height = (int)Math.Round(s.Height),
                            };

                            map.map.Add(c, chr);

                            left += s.Width;
                        }
                    }

                    using (MemoryStream mstr = new MemoryStream())
                    {
                        bmp.Save(mstr, ImageFormat.Png);

                        map.Texture = device.LoadTexture(mstr.GetBuffer());
                    }
                }

                gCache.Add(map);
            }

            return map;
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

            int height = 0;

            foreach (char c in text)
            {
                if (this.map.ContainsKey(c))
                {
                    FontMapChar chr = this.map[c];

                    VertexData[] cv;
                    uint[] ci;
                    VertexData.CreateSprite(
                        pos,
                        chr.Width,
                        chr.Height,
                        0,
                        0,
                        out cv,
                        out ci);

                    //Remap texture
                    float u0 = (chr.X) / (float)FontMap.TEXTURESIZE;
                    float v0 = (chr.Y) / (float)FontMap.TEXTURESIZE;
                    float u1 = (chr.X + chr.Width) / (float)FontMap.TEXTURESIZE;
                    float v1 = (chr.Y + chr.Height) / (float)FontMap.TEXTURESIZE;

                    cv[0].Texture0 = new Vector2(u0, v0);
                    cv[1].Texture0 = new Vector2(u1, v1);
                    cv[2].Texture0 = new Vector2(u0, v1);
                    cv[3].Texture0 = new Vector2(u1, v0);

                    Array.ForEach(ci, (i) => { indexList.Add(i + (uint)vertList.Count); });
                    Array.ForEach(cv, (v) => { vertList.Add(VertexData.CreateVertexPositionTexture(v)); });

                    pos.X += chr.Width - (int)(this.Size / 6);
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
