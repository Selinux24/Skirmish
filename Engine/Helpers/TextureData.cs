using SharpDX;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine.Helpers
{
    using Engine.Helpers.DDS;

    /// <summary>
    /// Texture data
    /// </summary>
    class TextureData : IDisposable
    {
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
        public int Width { get; private set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Mipmap count
        /// </summary>
        public int MipMaps { get; private set; }
        /// <summary>
        /// Depth
        /// </summary>
        public int Depth { get; private set; }
        /// <summary>
        /// Array size
        /// </summary>
        public int ArraySize { get; private set; }

        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(string filename)
        {
            return ReadTexture(filename, Rectangle.Empty);
        }
        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(string filename, Rectangle rectangle)
        {
            if (DdsHeader.GetInfo(filename, out DdsHeader header, out DdsHeaderDX10? header10, out int offset, out byte[] buffer))
            {
                return new TextureData(header, header10, buffer, offset);
            }
            else
            {
                ReadBitmap(filename, rectangle, out int width, out int height, out byte[] dataBuffer);

                return new TextureData(width, height, dataBuffer);
            }
        }
        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(MemoryStream stream)
        {
            return ReadTexture(stream, Rectangle.Empty);
        }
        /// <summary>
        /// Reads a texture data from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(MemoryStream stream, Rectangle rectangle)
        {
            if (DdsHeader.GetInfo(stream, out DdsHeader header, out DdsHeaderDX10? header10, out int offset, out byte[] buffer))
            {
                return new TextureData(header, header10, buffer, offset);
            }
            else
            {
                ReadBitmap(stream, rectangle, out int width, out int height, out byte[] dataBuffer);

                return new TextureData(width, height, dataBuffer);
            }
        }
        /// <summary>
        /// Reads a texture data list from a file list
        /// </summary>
        /// <param name="filenames">File name list</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<string> filenames)
        {
            return ReadTextureArray(filenames, Rectangle.Empty);
        }
        /// <summary>
        /// Reads a texture data list from a file list
        /// </summary>
        /// <param name="filenames">File name list</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<string> filenames, Rectangle rectangle)
        {
            List<TextureData> textureList = new List<TextureData>();

            foreach (var file in filenames)
            {
                textureList.Add(ReadTexture(file, rectangle));
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
            return ReadTextureArray(streams, Rectangle.Empty);
        }
        /// <summary>
        /// Reads a texture data list from a stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<MemoryStream> streams, Rectangle rectangle)
        {
            List<TextureData> textureList = new List<TextureData>();

            foreach (var stream in streams)
            {
                textureList.Add(ReadTexture(stream, rectangle));
            }

            return textureList;
        }
        /// <summary>
        /// Reads a cube texture data from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="faces">Cube faces</param>
        /// <returns>Returns the texture data</returns>
        public static IEnumerable<TextureData> ReadTextureCubic(string filename, Rectangle[] faces)
        {
            if (faces == null)
            {
                throw new ArgumentNullException(nameof(faces));
            }

            if (faces.Length != 6)
            {
                throw new ArgumentException("A cubic texture must have 6 faces.", nameof(faces));
            }

            List<TextureData> textureList = new List<TextureData>();

            foreach (var face in faces)
            {
                textureList.Add(ReadTexture(filename, face));
            }

            return textureList;
        }
        /// <summary>
        /// Reads a cube texture data from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="faces">Cube faces</param>
        /// <returns>Returns the texture data</returns>
        public static IEnumerable<TextureData> ReadTextureCubic(MemoryStream stream, Rectangle[] faces)
        {
            if (faces == null)
            {
                throw new ArgumentNullException(nameof(faces));
            }

            if (faces.Length != 6)
            {
                throw new ArgumentException("A cubic texture must have 6 faces.", nameof(faces));
            }

            List<TextureData> textureList = new List<TextureData>();

            foreach (var face in faces)
            {
                textureList.Add(ReadTexture(stream, face));
            }

            return textureList;
        }

        /// <summary>
        /// Reads a bitmap a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="rectangle">Rectangle</param>
        /// <param name="width">Resulting texture width</param>
        /// <param name="height">Resulting texture height</param>
        /// <param name="buffer">Resulting data buffer</param>
        private static void ReadBitmap(string filename, Rectangle rectangle, out int width, out int height, out byte[] buffer)
        {
            using (var factory = new ImagingFactory2())
            using (var bitmapDecoder = new BitmapDecoder(factory, filename, DecodeOptions.CacheOnLoad))
            {
                if (!ReadBitmap(factory, bitmapDecoder, PixelFormat.Format32bppPRGBA, rectangle, out width, out height, out buffer))
                {
                    throw new ArgumentException($"Cannot convert to 32bppPRGBA: {filename}", nameof(filename));
                }
            }
        }
        /// <summary>
        /// Reads a bitmap from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="rectangle">Rectangle</param>
        /// <param name="width">Resulting texture width</param>
        /// <param name="height">Resulting texture height</param>
        /// <param name="buffer">Resulting data buffer</param>
        private static void ReadBitmap(Stream stream, Rectangle rectangle, out int width, out int height, out byte[] buffer)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using (var factory = new ImagingFactory2())
            using (var bitmapDecoder = new BitmapDecoder(factory, stream, DecodeOptions.CacheOnLoad))
            {
                if (!ReadBitmap(factory, bitmapDecoder, PixelFormat.Format32bppPRGBA, rectangle, out width, out height, out buffer))
                {
                    throw new ArgumentException($"Cannot convert to 32bppPRGBA", nameof(stream));
                }
            }
        }
        /// <summary>
        /// Reads a bitmap from a decoder
        /// </summary>
        /// <param name="factory">Imaging factory</param>
        /// <param name="bitmapDecoder">Bitmap decoder</param>
        /// <param name="format">Target format</param>
        /// <param name="rectangle">Rectangle</param>
        /// <param name="width">Resulting texture width</param>
        /// <param name="height">Resulting texture height</param>
        /// <param name="buffer">Resulting data buffer</param>
        /// <returns>Returns true if the texture can be read</returns>
        private static bool ReadBitmap(ImagingFactory factory, BitmapDecoder bitmapDecoder, Guid format, Rectangle rectangle, out int width, out int height, out byte[] buffer)
        {
            width = 0;
            height = 0;
            buffer = null;

            var frame = bitmapDecoder.GetFrame(0);

            if (frame.PixelFormat == format)
            {
                int rowStride = PixelFormat.GetStride(frame.PixelFormat, frame.Size.Width);

                // Allocate DataStream to receive the WIC image pixels
                byte[] dataBuffer = new byte[frame.Size.Height * rowStride];
                // Copy the content of the WIC to the buffer
                frame.CopyPixels(dataBuffer, rowStride);

                if (rectangle != Rectangle.Empty)
                {
                    width = rectangle.Width;
                    height = rectangle.Height;
                    buffer = ReadRectangle(dataBuffer, frame.Size.Width, rowStride, rectangle);
                }
                else
                {
                    width = frame.Size.Width;
                    height = frame.Size.Height;
                    buffer = dataBuffer;
                }
            }
            else
            {
                using (var formatConverter = new FormatConverter(factory))
                {
                    if (!formatConverter.CanConvert(frame.PixelFormat, format))
                    {
                        return false;
                    }

                    formatConverter.Initialize(
                        frame,
                        format,
                        BitmapDitherType.ErrorDiffusion,
                        null,
                        0.0,
                        BitmapPaletteType.MedianCut);


                    // Allocate DataStream to receive the WIC image pixels
                    int rowStride = PixelFormat.GetStride(formatConverter.PixelFormat, formatConverter.Size.Width);
                    byte[] dataBuffer = new byte[formatConverter.Size.Height * rowStride];

                    // Copy the content of the WIC to the buffer
                    formatConverter.CopyPixels(dataBuffer, rowStride);

                    if (rectangle != Rectangle.Empty)
                    {
                        width = rectangle.Width;
                        height = rectangle.Height;
                        buffer = ReadRectangle(dataBuffer, formatConverter.Size.Width, rowStride, rectangle);
                    }
                    else
                    {
                        width = formatConverter.Size.Width;
                        height = formatConverter.Size.Height;
                        buffer = dataBuffer;
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// Reads a rectangle from a image byte buffer
        /// </summary>
        /// <param name="buffer">Image buffer</param>
        /// <param name="sourceWidth">Source width</param>
        /// <param name="sourceRowStride">Source row stride</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns a new buffer</returns>
        private static byte[] ReadRectangle(byte[] buffer, int sourceWidth, int sourceRowStride, Rectangle rectangle)
        {
            int stride = sourceRowStride / sourceWidth;

            int dstRowStride = rectangle.Width * stride;
            byte[] dstBuffer = new byte[rectangle.Height * dstRowStride];

            int dstOffset = 0;
            for (int i = 0; i < rectangle.Height; i++)
            {
                int offset = ((rectangle.Y + i) * sourceRowStride) + (rectangle.X * stride);

                Buffer.BlockCopy(buffer, offset, dstBuffer, dstOffset, dstRowStride);
                dstOffset += dstRowStride;
            }

            return dstBuffer;
        }

        /// <summary>
        /// Gets the next cicle bounds
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        private static void GetMipMapBounds(ref int width, ref int height, ref int depth)
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
        /// <param name="bitmap">Bitmap</param>
        private TextureData(int width, int height, byte[] buffer)
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
        private TextureData(DdsHeader header, DdsHeaderDX10? header10, byte[] bitData, int offset)
        {
            bool validFile = DdsHeader.ValidateTexture(
                header, header10,
                out int depth, out Format format, out _, out int arraySize, out bool isCubeMap);
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
                throw new EngineException("Bad DDS File");
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
        /// Gets a data box
        /// </summary>
        /// <param name="slice">Array slice</param>
        /// <param name="mip">Mip index</param>
        /// <returns>Returns a databox</returns>
        public DataBox GetDataBox(int slice, int mip)
        {
            GetDataOffset(slice, mip, out int offset, out int size, out int stride);

            var bytes = new byte[size];
            Array.Copy(data, offset, bytes, 0, bytes.Length);

            var stream = DataStream.Create(bytes, true, true);

            return new DataBox(stream.DataPointer, stride, stride);
        }
        /// <summary>
        /// Gets a data box array for the specified slice
        /// </summary>
        /// <param name="slice">Array slice</param>
        /// <returns>Returns a databox array</returns>
        public DataBox[] GetDataBoxes(int slice)
        {
            var res = new DataBox[MipMaps];

            for (int i = 0; i < MipMaps; i++)
            {
                res[i] = GetDataBox(slice, i);
            }

            return res;
        }
        /// <summary>
        /// Gets the complete data box array
        /// </summary>
        /// <returns>Returns a databox array</returns>
        public DataBox[] GetDataBoxes()
        {
            var res = new DataBox[ArraySize * MipMaps];

            int index = 0;
            for (int j = 0; j < ArraySize; j++)
            {
                for (int i = 0; i < MipMaps; i++)
                {
                    res[index++] = GetDataBox(j, i);
                }
            }

            return res;
        }

        /// <summary>
        /// Get data offset for the specifies slice and mip index
        /// </summary>
        /// <param name="slice">Array slice</param>
        /// <param name="mip">Mip index</param>
        /// <param name="offset">Returns the slice and mip offset</param>
        /// <param name="size">Returns the slice and mip size</param>
        /// <param name="stride">Returns the slice and mip stride</param>
        /// <returns>Returns true if the slice and mip were located</returns>
        private void GetDataOffset(
            int slice, int mip,
            out int offset, out int size, out int stride)
        {
            offset = 0;
            size = 0;
            stride = 0;

            for (int j = 0; j < ArraySize; j++)
            {
                int index = 0;
                int width = Width;
                int height = Height;
                int depth = Depth;

                for (int i = 0; i < MipMaps; i++)
                {
                    DdsPixelFormat.GetSurfaceInfo(
                        width,
                        height,
                        Format,
                        out int numBytes,
                        out int rowBytes,
                        out _);

                    if (slice == j && index == mip)
                    {
                        size = numBytes;
                        stride = rowBytes;

                        return;
                    }

                    offset += numBytes * depth;

                    if (offset > data.Length)
                    {
                        throw new EngineException("File too short");
                    }

                    GetMipMapBounds(ref width, ref height, ref depth);

                    index++;
                }
            }
        }
    }
}
