using SharpDX.WIC;
using System.IO;

namespace Engine.Helpers
{
    using Engine.Helpers.DDS;

    /// <summary>
    /// Texture loader
    /// </summary>
    static class HelperTextures
    {
        private static BitmapSource ReadBitmap(string filename)
        {
            using (var factory = new ImagingFactory2())
            {
                var bitmapDecoder = new BitmapDecoder(factory, filename, DecodeOptions.CacheOnLoad);

                var formatConverter = new FormatConverter(factory);

                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    PixelFormat.Format32bppPRGBA,
                    BitmapDitherType.None,
                    null,
                    0.0,
                    BitmapPaletteType.Custom);

                return formatConverter;
            }
        }
        private static BitmapSource ReadBitmap(Stream stream)
        {
            using (var factory = new ImagingFactory2())
            {
                var bitmapDecoder = new BitmapDecoder(factory, stream, DecodeOptions.CacheOnLoad);

                var formatConverter = new FormatConverter(factory);

                formatConverter.Initialize(
                    bitmapDecoder.GetFrame(0),
                    PixelFormat.Format32bppPRGBA,
                    BitmapDitherType.None,
                    null,
                    0.0,
                    BitmapPaletteType.Custom);

                return formatConverter;
            }
        }
        public static TextureData ReadTexture(byte[] buffer)
        {
            DDSHeader header;
            int offset;
            if (DDSHeader.GetInfo(buffer, out header, out offset))
            {
                return new TextureData(header, null, buffer, offset, 0);
            }
            else
            {
                using (var stream = new MemoryStream(buffer))
                using (var bitmap = ReadBitmap(stream))
                {
                    return new TextureData(bitmap);
                }
            }
        }
        public static TextureData ReadTexture(string filename)
        {
            DDSHeader header;
            int offset;
            byte[] buffer;
            if (DDSHeader.GetInfo(filename, out header, out offset, out buffer))
            {
                return new TextureData(header, null, buffer, offset, 0);
            }
            else
            {
                using (var bitmap = ReadBitmap(filename))
                {
                    return new TextureData(bitmap);
                }
            }
        }
        public static TextureData ReadTexture(MemoryStream stream)
        {
            DDSHeader header;
            int offset;
            byte[] buffer;
            if (DDSHeader.GetInfo(stream, out header, out offset, out buffer))
            {
                return new TextureData(header, null, buffer, offset, 0);
            }
            else
            {
                using (var bitmap = ReadBitmap(stream))
                {
                    return new TextureData(bitmap);
                }
            }
        }
        public static TextureData[] ReadTexture(string[] filenames)
        {
            TextureData[] textureList = new TextureData[filenames.Length];

            for (int i = 0; i < filenames.Length; i++)
            {
                textureList[i] = ReadTexture(filenames[i]);
            }

            return textureList;
        }
        public static TextureData[] ReadTexture(MemoryStream[] streams)
        {
            TextureData[] textureList = new TextureData[streams.Length];

            for (int i = 0; i < streams.Length; i++)
            {
                textureList[i] = ReadTexture(streams[i]);
            }

            return textureList;
        }
    }
}
