using System.Collections.Generic;
using System.IO;
using Bitmap = System.Drawing.Bitmap;
using Brushes = System.Drawing.Brushes;
using Font = System.Drawing.Font;
using Graph = System.Drawing.Graphics;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using RectangleF = System.Drawing.RectangleF;
using Region = System.Drawing.Region;
using SizeF = System.Drawing.SizeF;
using StringFormat = System.Drawing.StringFormat;
using StringFormatFlags = System.Drawing.StringFormatFlags;
using Device = SharpDX.Direct3D11.Device;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Common
{
    public static class FontMapper
    {
        public static ShaderResourceView MapFont(Device device, string font, int fontSize, int mapSize, out Dictionary<char, FontChar> map)
        {
            map = new Dictionary<char, FontChar>();

            using (Bitmap bmp = new Bitmap(mapSize, mapSize))
            using (Graph gra = Graph.FromImage(bmp))
            {
                gra.FillRegion(
                    Brushes.Black,
                    new Region(new RectangleF(0, 0, mapSize, mapSize)));

                using (StringFormat fmt = new StringFormat())
                using (Font fnt = new Font(font, fontSize))
                {
                    float positionX = 0f;
                    float positionY = 0f;
                    float separation = 0f;
                    float height = fnt.GetHeight();

                    for (int i = 1; i < 256; i++)
                    {
                        char c = (char)i;
                        if (!char.IsControl(c))
                        {
                            SizeF s = gra.MeasureString(
                                c.ToString(),
                                fnt,
                                int.MaxValue,
                                fmt);

                            if (positionX + s.Width >= mapSize)
                            {
                                positionX = 0f;
                                positionY += height + separation;
                            }

                            gra.DrawString(
                                c.ToString(),
                                fnt,
                                Brushes.White,
                                positionX,
                                positionY,
                                fmt);

                            map.Add(
                                c,
                                new FontChar()
                                {
                                    X = positionX,
                                    Y = positionY,
                                    Width = s.Width,
                                    Height = s.Height,
                                });

                            positionX += s.Width;
                        }
                    }
                }

                using (MemoryStream mstr = new MemoryStream())
                {
                    bmp.Save(mstr, ImageFormat.Png);

                    return ShaderResourceView.FromMemory(device, mstr.GetBuffer());
                }
            }
        }
    }
}
