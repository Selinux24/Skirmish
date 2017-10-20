using SharpDX;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;

namespace Engine.Helpers
{
    using Engine.Helpers.DDS;
    using SharpDX.Direct3D11;

    class TextureData : IDisposable
    {
        private DataStream stream;

        public Format Format { get; private set; }
        public int Stride { get; private set; }
        public bool IsCubeMap { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TextureData(BitmapSource bitmap)
        {
            this.IsCubeMap = false;

            this.Width = bitmap.Size.Width;
            this.Height = bitmap.Size.Height;

            this.Format = Format.R8G8B8A8_UNorm;

            // Allocate DataStream to receive the WIC image pixels
            this.Stride = bitmap.Size.Width * 4;

            this.stream = new DataStream(bitmap.Size.Height * this.Stride, true, true);

            // Copy the content of the WIC to the buffer
            bitmap.CopyPixels(this.Stride, this.stream);
        }
        public TextureData(DDSHeader header, DDSHeaderDX10? header10, byte[] bitData, int offset, int maxsize)
        {
            this.IsCubeMap = false;

            bool validFile = false;

            int width = header.Width;
            int height = header.Height;
            int depth = header.Depth;
            ResourceDimension resDim;
            int arraySize;
            Format format;
            bool isCubeMap;

            if (header.IsDX10)
            {
                validFile = header10.Value.Validate(
                    header.Flags,
                    ref width, ref height, ref depth,
                    out format, out resDim, out arraySize, out isCubeMap);
            }
            else
            {
                validFile = header.Validate(
                    ref width, ref height, ref depth,
                    out format, out resDim, out arraySize, out isCubeMap);
            }

            if (validFile)
            {
                int mipCount = header.MipMapCount;
                if (0 == mipCount)
                {
                    mipCount = 1;
                }

                int numBytes = 0;
                int numRowBytes = 0;
                int numRows = 0;
                DDSPixelFormat.GetSurfaceInfo(
                    width, height, format,
                    out numBytes, out numRowBytes, out numRows);

                var bytes = new byte[numBytes];
                Array.Copy(bitData, offset, bytes, 0, numBytes);

                this.Width = width;
                this.Height = height;
                this.Format = format;
                this.IsCubeMap = IsCubeMap;
                this.Stride = numRowBytes;
                this.stream = DataStream.Create(bytes, true, true);
            }
            else
            {
                throw new EngineException("Bad DDS File");
            }
        }

        public DataBox GetDataBox()
        {
            return new DataBox(this.stream.DataPointer, this.Stride, this.Stride);
        }

        public void Dispose()
        {
            Helper.Dispose(this.stream);
        }
    }
}
