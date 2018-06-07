using SharpDX;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.IO;

namespace Engine.Helpers
{
    using Engine.Helpers.DDS;
    using SharpDX.Direct3D11;

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
        /// Reads a bitmap a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <returns>Returns the bitmap source</returns>
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
        /// <summary>
        /// Reads a bitmap from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the bitmap source</returns>
        private static BitmapSource ReadBitmap(Stream stream)
        {
            using (var factory = new ImagingFactory2())
            {
                stream.Seek(0, SeekOrigin.Begin);

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
        /// <summary>
        /// Reads a texture data from a byte buffer
        /// </summary>
        /// <param name="buffer">Byte buffer</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(byte[] buffer)
        {
            if (DDSHeader.GetInfo(buffer, out DDSHeader header, out DDSHeaderDX10? header10, out int offset))
            {
                return new TextureData(header, header10, buffer, offset, 0);
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
        /// <summary>
        /// Reads a texture data from a file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(string filename)
        {
            if (DDSHeader.GetInfo(filename, out DDSHeader header, out DDSHeaderDX10? header10, out int offset, out byte[] buffer))
            {
                return new TextureData(header, header10, buffer, offset, 0);
            }
            else
            {
                using (var bitmap = ReadBitmap(filename))
                {
                    return new TextureData(bitmap);
                }
            }
        }
        /// <summary>
        /// Reads a texture data from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the texture data</returns>
        public static TextureData ReadTexture(MemoryStream stream)
        {
            if (DDSHeader.GetInfo(stream, out DDSHeader header, out DDSHeaderDX10? header10, out int offset, out byte[] buffer))
            {
                return new TextureData(header, header10, buffer, offset, 0);
            }
            else
            {
                using (var bitmap = ReadBitmap(stream))
                {
                    return new TextureData(bitmap);
                }
            }
        }
        /// <summary>
        /// Reads a texture data list from a file list
        /// </summary>
        /// <param name="filenames">File name list</param>
        /// <returns>Returns the texture data list</returns>
        public static TextureData[] ReadTexture(string[] filenames)
        {
            TextureData[] textureList = new TextureData[filenames.Length];

            for (int i = 0; i < filenames.Length; i++)
            {
                textureList[i] = ReadTexture(filenames[i]);
            }

            return textureList;
        }
        /// <summary>
        /// Reads a texture data list from a stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <returns>Returns the texture data list</returns>
        public static TextureData[] ReadTexture(MemoryStream[] streams)
        {
            TextureData[] textureList = new TextureData[streams.Length];

            for (int i = 0; i < streams.Length; i++)
            {
                textureList[i] = ReadTexture(streams[i]);
            }

            return textureList;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        private TextureData(BitmapSource bitmap)
        {
            this.Width = bitmap.Size.Width;
            this.Height = bitmap.Size.Height;
            this.Depth = 1;
            this.Format = Format.R8G8B8A8_UNorm;
            this.ArraySize = 1;
            this.IsCubeMap = false;
            this.MipMaps = 1;

            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmap.Size.Width * 4;

            // Copy the content of the WIC to the buffer
            var bytes = new byte[bitmap.Size.Height * stride];
            bitmap.CopyPixels(bytes, stride);

            this.data = bytes;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="header">DDS Header</param>
        /// <param name="header10">DDSDX10 Header</param>
        /// <param name="bitData">Bit data</param>
        /// <param name="offset">Offset</param>
        /// <param name="maxsize">Maximum size</param>
        private TextureData(DDSHeader header, DDSHeaderDX10? header10, byte[] bitData, int offset, int maxsize)
        {
            bool validFile = DDSHeader.ValidateTexture(
                header, header10,
                out int depth, out Format format, out ResourceDimension resDim, out int arraySize, out bool isCubeMap);
            if (validFile)
            {
                this.Width = header.Width;
                this.Height = header.Height;
                this.Depth = depth;
                this.Format = format;
                this.ArraySize = arraySize;
                this.IsCubeMap = isCubeMap;
                this.MipMaps = header.MipMapCount == 0 ? 1 : header.MipMapCount;

                var bytes = new byte[bitData.Length - offset];
                Array.Copy(bitData, offset, bytes, 0, bytes.Length);

                this.data = bytes;
            }
            else
            {
                throw new EngineException("Bad DDS File");
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
            this.GetDataOffset(slice, mip, out int offset, out int size, out int stride);

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
            var res = new DataBox[this.MipMaps];

            for (int i = 0; i < this.MipMaps; i++)
            {
                res[i] = this.GetDataBox(slice, i);
            }

            return res;
        }
        /// <summary>
        /// Gets the complete data box array
        /// </summary>
        /// <returns>Returns a databox array</returns>
        public DataBox[] GetDataBoxes()
        {
            var res = new DataBox[this.ArraySize * this.MipMaps];

            int index = 0;
            for (int j = 0; j < this.ArraySize; j++)
            {
                for (int i = 0; i < this.MipMaps; i++)
                {
                    res[index++] = this.GetDataBox(j, i);
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
        private bool GetDataOffset(
            int slice, int mip,
            out int offset, out int size, out int stride)
        {
            offset = 0;
            size = 0;
            stride = 0;

            int numBytes = 0;
            int rowBytes = 0;
            int numRows = 0;

            for (int j = 0; j < this.ArraySize; j++)
            {
                int index = 0;
                int width = this.Width;
                int height = this.Height;
                int depth = this.Depth;

                for (int i = 0; i < this.MipMaps; i++)
                {
                    DDSPixelFormat.GetSurfaceInfo(
                        width,
                        height,
                        this.Format,
                        out numBytes,
                        out rowBytes,
                        out numRows);

                    if (slice == j && index == mip)
                    {
                        size = numBytes;
                        stride = rowBytes;

                        return true;
                    }

                    offset += numBytes * depth;

                    if (offset > this.data.Length)
                    {
                        throw new Exception("File too short");
                    }

                    width = width >> 1;
                    height = height >> 1;
                    depth = depth >> 1;
                    if (width == 0) width = 1;
                    if (height == 0) height = 1;
                    if (depth == 0) depth = 1;

                    index++;
                }
            }

            return false;
        }

        /// <summary>
        /// Resource dispose
        /// </summary>
        public void Dispose()
        {
            this.data = null;
        }
    }
}
