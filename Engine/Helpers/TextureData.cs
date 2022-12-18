using SharpDX;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Helpers
{
    using Engine.Helpers.DDS;

    /// <summary>
    /// Texture data
    /// </summary>
    class TextureData : IDisposable
    {
        /// <summary>
        /// BitmapData
        /// </summary>
        struct BitmapData
        {
            /// <summary>
            /// Bitmap width
            /// </summary>
            public int Width { get; set; }
            /// <summary>
            /// Bitmap height
            /// </summary>
            public int Height { get; set; }
            /// <summary>
            /// Buffer
            /// </summary>
            public byte[] Buffer { get; set; }
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
            return ReadTextureArray(filename, new Rectangle[] { }).FirstOrDefault();
        }
        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(string filename, Rectangle rectangle)
        {
            return ReadTextureArray(filename, new Rectangle[] { rectangle }).FirstOrDefault();
        }
        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(MemoryStream stream)
        {
            return ReadTextureArray(stream, new Rectangle[] { }).FirstOrDefault();
        }
        /// <summary>
        /// Reads a texture data from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(MemoryStream stream, Rectangle rectangle)
        {
            return ReadTextureArray(stream, new Rectangle[] { rectangle }).FirstOrDefault();
        }
        /// <summary>
        /// Reads a texture data list from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="rectangles">Crop rectangles</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(string filename, IEnumerable<Rectangle> rectangles)
        {
            List<TextureData> result = new List<TextureData>();

            if (DdsHeader.GetInfo(filename, out DdsHeader header, out DdsHeaderDX10? header10, out int offset, out byte[] buffer))
            {
                if (rectangles.Any())
                {
                    Logger.WriteWarning(nameof(TextureData), $"{nameof(ReadTextureArray)} -> Texture format not suitable for rectangle cropping. {filename}");
                }

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
        public static IEnumerable<TextureData> ReadTextureArray(MemoryStream stream, IEnumerable<Rectangle> rectangles)
        {
            List<TextureData> result = new List<TextureData>();

            if (DdsHeader.GetInfo(stream, out DdsHeader header, out DdsHeaderDX10? header10, out int offset, out byte[] buffer))
            {
                if (rectangles.Any())
                {
                    Logger.WriteWarning(nameof(TextureData), $"{nameof(ReadTextureArray)} -> Texture format not suitable for rectangle cropping.");
                }

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
            List<TextureData> textureList = new List<TextureData>();

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
        /// Reads a texture data list from a file list
        /// </summary>
        /// <param name="filenames">File name list</param>
        /// <param name="rectangles">Crop rectangle list</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<string> filenames, IEnumerable<Rectangle> rectangles)
        {
            List<TextureData> textureList = new List<TextureData>();

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
            List<TextureData> textureList = new List<TextureData>();

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
        /// Reads a texture data list from a stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <param name="rectangles">Crop rectangle list</param>
        /// <returns>Returns the texture data list</returns>
        public static IEnumerable<TextureData> ReadTextureArray(IEnumerable<MemoryStream> streams, IEnumerable<Rectangle> rectangles)
        {
            List<TextureData> textureList = new List<TextureData>();

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
        public static IEnumerable<TextureData> ReadTextureCubic(string filename, IEnumerable<Rectangle> faces)
        {
            if (faces == null)
            {
                throw new ArgumentNullException(nameof(faces));
            }

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
        public static IEnumerable<TextureData> ReadTextureCubic(MemoryStream stream, IEnumerable<Rectangle> faces)
        {
            if (faces == null)
            {
                throw new ArgumentNullException(nameof(faces));
            }

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
        private static IEnumerable<BitmapData> ReadBitmap(string filename, IEnumerable<Rectangle> rectangles)
        {
            using (var factory = new ImagingFactory2())
            using (var bitmapDecoder = new BitmapDecoder(factory, filename, DecodeOptions.CacheOnLoad))
            {
                return ReadBitmap(factory, bitmapDecoder, PixelFormat.Format32bppPRGBA, rectangles);
            }
        }
        /// <summary>
        /// Reads a bitmap from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="rectangles">Crop rectangles</param>
        /// <returns>Returns a bitmap data list</returns>
        private static IEnumerable<BitmapData> ReadBitmap(Stream stream, IEnumerable<Rectangle> rectangles)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using (var factory = new ImagingFactory2())
            using (var bitmapDecoder = new BitmapDecoder(factory, stream, DecodeOptions.CacheOnLoad))
            {
                return ReadBitmap(factory, bitmapDecoder, PixelFormat.Format32bppPRGBA, rectangles);
            }
        }
        /// <summary>
        /// Reads a bitmap from a decoder
        /// </summary>
        /// <param name="factory">Imaging factory</param>
        /// <param name="bitmapDecoder">Bitmap decoder</param>
        /// <param name="format">Target format</param>
        /// <param name="rectangles">Crop rectangles</param>
        /// <returns>Returns the readed bitmap data</returns>
        private static IEnumerable<BitmapData> ReadBitmap(ImagingFactory factory, BitmapDecoder bitmapDecoder, Guid format, IEnumerable<Rectangle> rectangles)
        {
            List<BitmapData> result = new List<BitmapData>();

            var frame = bitmapDecoder.GetFrame(0);

            if (frame.PixelFormat == format)
            {
                int rowStride = PixelFormat.GetStride(frame.PixelFormat, frame.Size.Width);

                // Allocate DataStream to receive the WIC image pixels
                byte[] dataBuffer = new byte[frame.Size.Height * rowStride];
                // Copy the content of the WIC to the buffer
                frame.CopyPixels(dataBuffer, rowStride);

                if (rectangles?.Any() != true)
                {
                    result.Add(new BitmapData
                    {
                        Width = frame.Size.Width,
                        Height = frame.Size.Height,
                        Buffer = dataBuffer,
                    });
                }
                else
                {
                    var bitmaps = CropBuffer(dataBuffer, frame.Size.Width, frame.Size.Height, rowStride, rectangles);
                    result.AddRange(bitmaps);
                }
            }
            else
            {
                using (var formatConverter = new FormatConverter(factory))
                {
                    if (!formatConverter.CanConvert(frame.PixelFormat, format))
                    {
                        return new BitmapData[] { };
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

                    if (rectangles?.Any() != true)
                    {
                        result.Add(new BitmapData
                        {
                            Width = formatConverter.Size.Width,
                            Height = formatConverter.Size.Height,
                            Buffer = dataBuffer,
                        });
                    }
                    else
                    {
                        var bitmaps = CropBuffer(dataBuffer, formatConverter.Size.Width, formatConverter.Size.Height, rowStride, rectangles);
                        result.AddRange(bitmaps);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Reads the specified crop rectangle list from a data buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="sourceWidth">Source width</param>
        /// <param name="sourceHeight">Source height</param>
        /// <param name="sourceRowStride">Row stride in bytes</param>
        /// <param name="rectangles">Crop rectangle list</param>
        /// <returns>Returns the readed bitmap data</returns>
        private static IEnumerable<BitmapData> CropBuffer(byte[] buffer, int sourceWidth, int sourceHeight, int sourceRowStride, IEnumerable<Rectangle> rectangles)
        {
            List<BitmapData> result = new List<BitmapData>();

            foreach (var rectangle in rectangles)
            {
                if (rectangle == Rectangle.Empty)
                {
                    result.Add(new BitmapData
                    {
                        Width = sourceWidth,
                        Height = sourceHeight,
                        Buffer = buffer,
                    });
                }
                else
                {
                    var rectBuffer = CropBuffer(buffer, sourceWidth, sourceRowStride, rectangle);
                    result.Add(new BitmapData
                    {
                        Width = rectangle.Width,
                        Height = rectangle.Height,
                        Buffer = rectBuffer,
                    });
                }
            }

            return result.ToArray();
        }
        /// <summary>
        /// Reads a rectangle from a image byte buffer
        /// </summary>
        /// <param name="buffer">Image buffer</param>
        /// <param name="sourceWidth">Source width</param>
        /// <param name="sourceRowStride">Source row stride</param>
        /// <param name="rectangle">Crop rectangle</param>
        /// <returns>Returns a new buffer</returns>
        private static byte[] CropBuffer(byte[] buffer, int sourceWidth, int sourceRowStride, Rectangle rectangle)
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
