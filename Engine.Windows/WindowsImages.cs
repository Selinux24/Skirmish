using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Windows
{
    /// <summary>
    /// Images helper
    /// </summary>
    public class WindowsImages : IImages
    {
        /// <inheritdoc/>
        public Image FromStream(Stream data)
        {
            using var bitmap = System.Drawing.Image.FromStream(data) as Bitmap;

            var image = new Image(bitmap.Width, bitmap.Height);

            for (int h = 0; h < bitmap.Height; h++)
            {
                for (int w = 0; w < bitmap.Width; w++)
                {
                    var color = bitmap.GetPixel(w, h);

                    image.SetPixel(w, h, new SharpDX.Color(color.R, color.G, color.B, color.A));
                }
            }

            return image;
        }
        /// <inheritdoc/>
        public void SaveToFile(string fileName, Image image)
        {
            int stride = sizeof(int);
            int[] bits = image.Flatten().Select(c => c.ToRgba()).ToArray();

            var bitsHandle = GCHandle.Alloc(bits, GCHandleType.Pinned);
            using var bitmap = new Bitmap(image.Width, image.Height, image.Width * stride, PixelFormat.Format32bppPArgb, bitsHandle.AddrOfPinnedObject());
            bitmap.Save(fileName);
        }
    }
}
