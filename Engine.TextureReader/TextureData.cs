using Engine.TextureReader.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;

namespace Engine.TextureReader
{
    /// <summary>
    /// Texture data
    /// </summary>
    public class TextureData : IDisposable
    {
        /// <summary>
        /// BitmapData
        /// </summary>
        struct BitmapData
        {
            /// <summary>
            /// Bitmap width
            /// </summary>
            public uint Width;
            /// <summary>
            /// Bitmap height
            /// </summary>
            public uint Height;
            /// <summary>
            /// Buffer
            /// </summary>
            public byte[] Buffer;
        }

        /// <summary>
        /// Byte data
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Format
        /// </summary>
        public Format Format { get; private set; }
        /// <summary>
        /// Is cube map
        /// </summary>
        public bool IsCubeMap { get; private set; }
        /// <summary>
        /// Width
        /// </summary>
        public uint Width { get; private set; }
        /// <summary>
        /// Height
        /// </summary>
        public uint Height { get; private set; }
        /// <summary>
        /// Mipmap count
        /// </summary>
        public uint MipMaps { get; private set; }
        /// <summary>
        /// Depth
        /// </summary>
        public uint Depth { get; private set; }
        /// <summary>
        /// Array size
        /// </summary>
        public uint ArraySize { get; private set; }

        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(string filename)
        {
            return ReadTextureArray(filename, []).FirstOrDefault();
        }
        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(string filename, RectI rectangle)
        {
            return ReadTextureArray(filename, [rectangle]).FirstOrDefault();
        }
        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(MemoryStream stream)
        {
            return ReadTextureArray(stream, []).FirstOrDefault();
        }
        /// <summary>
        /// Reads a texture data from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(MemoryStream stream, RectI rectangle)
        {
            return ReadTextureArray(stream, [rectangle]).FirstOrDefault();
        }
        /// <summary>
        /// Reads a texture data list from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="rectangles">Crop rectangles</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(string filename, IEnumerable<RectI> rectangles)
        {
            var result = new List<TextureData>();

            if (DdsHeader.GetInfo(filename, out DdsHeader header, out DdsHeaderDX10? header10, out uint offset, out byte[] buffer))
            {
                result.Add(new TextureData(header, header10, buffer, offset));
            }
            else
            {
                var bitmaps = ReadBitmap(filename, rectangles);
                foreach (var bitmap in bitmaps)
                {
                    result.Add(new TextureData(bitmap));
                }
            }

            return result;
        }
        /// <summary>
        /// Reads a texture data list from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="rectangles">Crop rectangles</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(MemoryStream stream, IEnumerable<RectI> rectangles)
        {
            var result = new List<TextureData>();

            if (DdsHeader.GetInfo(stream, out DdsHeader header, out DdsHeaderDX10? header10, out uint offset, out byte[] buffer))
            {
                result.Add(new TextureData(header, header10, buffer, offset));
            }
            else
            {
                var bitmaps = ReadBitmap(stream, rectangles);
                foreach (var bitmap in bitmaps)
                {
                    result.Add(new TextureData(bitmap));
                }
            }

            return result;
        }
        /// <summary>
        /// Reads a texture data list from a file list
        /// </summary>
        /// <param name="filenames">File name list</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<string> filenames)
        {
            var textureList = new List<TextureData>();

            foreach (var file in filenames)
            {
                textureList.Add(ReadTexture(file));
            }

            return textureList;
        }
        /// <summary>
        /// Reads a texture data list from a file list
        /// </summary>
        /// <param name="filenames">File name list</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<string> filenames, RectI rectangle)
        {
            var textureList = new List<TextureData>();

            foreach (var file in filenames)
            {
                textureList.Add(ReadTexture(file, rectangle));
            }

            return textureList;
        }
        /// <summary>
        /// Reads a texture data list from a file list
        /// </summary>
        /// <param name="filenames">File name list</param>
        /// <param name="rectangles">Crop rectangle list</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<string> filenames, IEnumerable<RectI> rectangles)
        {
            var textureList = new List<TextureData>();

            foreach (var file in filenames)
            {
                foreach (var rectangle in rectangles)
                {
                    textureList.Add(ReadTexture(file, rectangle));
                }
            }

            return textureList;
        }
        /// <summary>
        /// Reads a texture data list from a stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<MemoryStream> streams)
        {
            var textureList = new List<TextureData>();

            foreach (var stream in streams)
            {
                textureList.Add(ReadTexture(stream));
            }

            return textureList;
        }
        /// <summary>
        /// Reads a texture data list from a stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<MemoryStream> streams, RectI rectangle)
        {
            var textureList = new List<TextureData>();

            foreach (var stream in streams)
            {
                textureList.Add(ReadTexture(stream, rectangle));
            }

            return textureList;
        }
        /// <summary>
        /// Reads a texture data list from a stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <param name="rectangles">Crop rectangle list</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<MemoryStream> streams, IEnumerable<RectI> rectangles)
        {
            var textureList = new List<TextureData>();

            foreach (var stream in streams)
            {
                foreach (var rectangle in rectangles)
                {
                    textureList.Add(ReadTexture(stream, rectangle));
                }
            }

            return textureList;
        }
        /// <summary>
        /// Reads a cube texture data from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="faces">Cube faces</param>
        /// <returns>Returns the texture data</returns>
        public static IEnumerable<TextureData> ReadTextureCubic(string filename, IEnumerable<RectI> faces)
        {
            ArgumentNullException.ThrowIfNull(faces);

            if (faces.Count() != 6)
            {
                throw new ArgumentException("A cubic texture must have 6 faces.", nameof(faces));
            }

            return ReadTextureArray(filename, faces);
        }
        /// <summary>
        /// Reads a cube texture data from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="faces">Cube faces</param>
        /// <returns>Returns the texture data</returns>
        public static IEnumerable<TextureData> ReadTextureCubic(MemoryStream stream, IEnumerable<RectI> faces)
        {
            ArgumentNullException.ThrowIfNull(faces);

            if (faces.Count() != 6)
            {
                throw new ArgumentException("A cubic texture must have 6 faces.", nameof(faces));
            }

            return ReadTextureArray(stream, faces);
        }

        /// <summary>
        /// Reads a bitmap a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="rectangles">Crop rectangles</param>
        /// <returns>Returns a bitmap data list</returns>
        private static BitmapData[] ReadBitmap(string filename, IEnumerable<RectI> rectangles)
        {
            using var factory = new IWICImagingFactory2();
            using var bitmapDecoder = factory.CreateDecoderFromFileName(filename, FileAccess.Read, DecodeOptions.CacheOnLoad);
            return ReadBitmap(factory, bitmapDecoder, PixelFormat.Format32bppPRGBA, rectangles);
        }
        /// <summary>
        /// Reads a bitmap from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="rectangles">Crop rectangles</param>
        /// <returns>Returns a bitmap data list</returns>
        private static BitmapData[] ReadBitmap(MemoryStream stream, IEnumerable<RectI> rectangles)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using var factory = new IWICImagingFactory2();
            using var bitmapDecoder = factory.CreateDecoderFromStream(stream, DecodeOptions.CacheOnLoad);
            return ReadBitmap(factory, bitmapDecoder, PixelFormat.Format32bppPRGBA, rectangles);
        }
        /// <summary>
        /// Reads a bitmap from a decoder
        /// </summary>
        /// <param name="factory">Imaging factory</param>
        /// <param name="bitmapDecoder">Bitmap decoder</param>
        /// <param name="format">Target format</param>
        /// <param name="rectangles">Crop rectangles</param>
        /// <returns>Returns the readed bitmap data</returns>
        private static BitmapData[] ReadBitmap(IWICImagingFactory factory, IWICBitmapDecoder bitmapDecoder, Guid format, IEnumerable<RectI> rectangles)
        {
            var frame = bitmapDecoder.GetFrame(0);
            var cropRectangles = rectangles?.ToArray();

            if (cropRectangles == null || cropRectangles.Length == 0)
            {
                var bitmap = ReadBitmap(factory, frame, format);
                return bitmap.HasValue ? [bitmap.Value] : [];
            }

            var result = new List<BitmapData>(cropRectangles.Length);
            foreach (var rectangle in cropRectangles)
            {
                if (rectangle == Rect.Empty)
                {
                    var bitmap = ReadBitmap(factory, frame, format);
                    if (!bitmap.HasValue)
                    {
                        return [];
                    }

                    result.Add(bitmap.Value);
                    continue;
                }

                using var croppedFrame = factory.CreateBitmapFromSourceRect(
                    frame,
                    checked((uint)rectangle.X),
                    checked((uint)rectangle.Y),
                    checked((uint)rectangle.Width),
                    checked((uint)rectangle.Height));
                var croppedBitmap = ReadBitmap(factory, croppedFrame, format);
                if (!croppedBitmap.HasValue)
                {
                    return [];
                }

                result.Add(croppedBitmap.Value);
            }

            return [.. result];
        }

        private static BitmapData? ReadBitmap(IWICImagingFactory factory, IWICBitmapSource source, Guid format)
        {
            uint width = (uint)source.Size.Width;
            uint height = (uint)source.Size.Height;

            if (source.PixelFormat == format)
            {
                uint rowStride = PixelFormat.GetStride(source.PixelFormat, width);
                byte[] dataBuffer = new byte[height * rowStride];
                source.CopyPixels(rowStride, dataBuffer);

                return new BitmapData
                {
                    Width = width,
                    Height = height,
                    Buffer = dataBuffer,
                };
            }

            using var formatConverter = factory.CreateFormatConverter();
            if (!formatConverter.CanConvert(source.PixelFormat, format))
            {
                return null;
            }

            formatConverter.Initialize(
                source,
                format,
                BitmapDitherType.ErrorDiffusion,
                null,
                0.0,
                BitmapPaletteType.MedianCut);

            uint convertedWidth = (uint)formatConverter.Size.Width;
            uint convertedHeight = (uint)formatConverter.Size.Height;
            uint convertedRowStride = PixelFormat.GetStride(formatConverter.PixelFormat, convertedWidth);
            byte[] convertedBuffer = new byte[convertedHeight * convertedRowStride];
            formatConverter.CopyPixels(convertedRowStride, convertedBuffer);

            return new BitmapData
            {
                Width = convertedWidth,
                Height = convertedHeight,
                Buffer = convertedBuffer,
            };
        }

        /// <summary>
        /// Gets the next cicle bounds
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        private static void GetMipMapBounds(ref uint width, ref uint height, ref uint depth)
        {
            width >>= 1;
            height >>= 1;
            depth >>= 1;
            if (width == 0) width = 1;
            if (height == 0) height = 1;
            if (depth == 0) depth = 1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bitmap">Bitmap data</param>
        private TextureData(BitmapData bitmap) : this(bitmap.Width, bitmap.Height, bitmap.Buffer)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">Bitmap width</param>
        /// <param name="height">Bitmap height</param>
        /// <param name="buffer">Bitmap data buffer</param>
        private TextureData(uint width, uint height, byte[] buffer)
        {
            Width = width;
            Height = height;
            Depth = 1;
            Format = Format.R8G8B8A8_UNorm;
            ArraySize = 1;
            IsCubeMap = false;
            MipMaps = 1;
            data = buffer;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="header">DDS Header</param>
        /// <param name="header10">DDSDX10 Header</param>
        /// <param name="bitData">Bit data</param>
        /// <param name="offset">Offset</param>
        private TextureData(DdsHeader header, DdsHeaderDX10? header10, byte[] bitData, uint offset)
        {
            bool validFile = DdsHeader.ValidateTexture(
                header, header10,
                out uint depth, out var format, out _, out uint arraySize, out bool isCubeMap);
            if (validFile)
            {
                Width = header.Width;
                Height = header.Height;
                Depth = depth;
                Format = format;
                ArraySize = arraySize;
                IsCubeMap = isCubeMap;
                MipMaps = header.MipMapCount == 0 ? 1 : header.MipMapCount;

                var bytes = new byte[bitData.Length - offset];
                Array.Copy(bitData, offset, bytes, 0, bytes.Length);

                data = bytes;
            }
            else
            {
                throw new TextureReaderException("Bad DDS File");
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TextureData()
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
        /// Resource disposal
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                data = null;
            }
        }

        /// <summary>
        /// Gets the data memory for the specified slice and mip index
        /// </summary>
        /// <param name="slice">Array slice</param>
        /// <param name="mip">Mip index</param>
        /// <param name="stride">Returns the slice and mip stride</param>
        /// <returns>Returns a view of the slice and mip data</returns>
        public ReadOnlyMemory<byte> GetDataOffset(uint slice, uint mip, out uint stride)
        {
            uint offset = 0;
            uint size = 0;
            stride = 0;

            for (uint j = 0; j < ArraySize; j++)
            {
                uint index = 0;
                uint width = Width;
                uint height = Height;
                uint depth = Depth;

                for (uint i = 0; i < MipMaps; i++)
                {
                    DdsPixelFormat.GetSurfaceInfo(
                        width,
                        height,
                        Format,
                        out uint numBytes,
                        out uint rowBytes,
                        out _);

                    if (slice == j && index == mip)
                    {
                        size = numBytes;
                        stride = rowBytes;
                        return new ReadOnlyMemory<byte>(data, checked((int)offset), checked((int)size));
                    }

                    offset += numBytes * depth;

                    if (offset > data.Length)
                    {
                        throw new TextureReaderException("File too short");
                    }

                    GetMipMapBounds(ref width, ref height, ref depth);

                    index++;
                }
            }

            return new ReadOnlyMemory<byte>(data, checked((int)offset), checked((int)size));
        }
    }
}
