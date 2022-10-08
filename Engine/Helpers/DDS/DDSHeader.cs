using SharpDX.DXGI;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Engine.Helpers.DDS
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// DDS Header
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct DdsHeader
    {
        /// <summary>
        /// DDS_MAGIC
        /// </summary>
        const int DDSMagic = 0x20534444;

        /// <summary>
        /// Size of DDS Header
        /// </summary>
        public readonly static int StructSize = Marshal.SizeOf(new DdsHeader());

        /// <summary>
        /// Gets info from byte data
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="header">Resulting Header</param>
        /// <param name="header10">Resulting Header DX10</param>
        /// <param name="offset">Resulting Offset</param>
        /// <returns>Returns true if the byte data contains a DDS Header</returns>
        public static bool GetInfo(byte[] data, out DdsHeader header, out DdsHeaderDX10? header10, out int offset)
        {
            // Validate DDS file in memory
            header = new DdsHeader();
            header10 = null;
            offset = 0;

            if (data.Length < (sizeof(uint) + DdsHeader.StructSize))
            {
                return false;
            }

            // First is magic number
            int dwMagicNumber = BitConverter.ToInt32(data, 0);
            if (dwMagicNumber != DdsHeader.DDSMagic)
            {
                return false;
            }

            header = data.ToStructure<DdsHeader>(4, DdsHeader.StructSize);

            // Verify header to validate DDS file
            if (header.Size != DdsHeader.StructSize ||
                header.PixelFormat.Size != DdsPixelFormat.StructSize)
            {
                return false;
            }

            // Check for DX10 extension
            if (header.PixelFormat.IsDX10())
            {
                var h10Offset = 4 + DdsHeader.StructSize + DdsHeaderDX10.StructSize;

                // Must be long enough for both headers and magic value
                if (data.Length < h10Offset)
                {
                    return false;
                }

                header10 = data.ToStructure<DdsHeaderDX10>(4, DdsHeaderDX10.StructSize);

                offset = h10Offset;
            }
            else
            {
                offset = 4 + DdsHeader.StructSize;
            }

            return true;
        }
        /// <summary>
        /// Gets info from file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="header">Resulting Header</param>
        /// <param name="header10">Resulting Header DX10</param>
        /// <param name="offset">Resulting Offset</param>
        /// <param name="buffer">Readed byte buffer</param>
        /// <returns>Returns true if the file contains a DDS Header</returns>
        public static bool GetInfo(string filename, out DdsHeader header, out DdsHeaderDX10? header10, out int offset, out byte[] buffer)
        {
            buffer = File.ReadAllBytes(filename);
            return GetInfo(buffer, out header, out header10, out offset);
        }
        /// <summary>
        /// Gets info from stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="header">Resulting Header</param>
        /// <param name="header10">Resulting Header DX10</param>
        /// <param name="offset">Resulting Offset</param>
        /// <param name="buffer">Readed byte buffer</param>
        /// <returns>Returns true if the stream contains a DDS Header</returns>
        public static bool GetInfo(MemoryStream stream, out DdsHeader header, out DdsHeaderDX10? header10, out int offset, out byte[] buffer)
        {
            buffer = stream.GetBuffer();
            return GetInfo(buffer, out header, out header10, out offset);
        }
        /// <summary>
        /// Validates the texture
        /// </summary>
        /// <param name="header">DDS Header</param>
        /// <param name="header10">DDS header DX10</param>
        /// <param name="depth">Returns the texture depth</param>
        /// <param name="format">Returns the texture format</param>
        /// <param name="resDim">Returns the texture dimension</param>
        /// <param name="arraySize">Returns the texture array size</param>
        /// <param name="isCubeMap">Returns true if the texture is a cube map</param>
        /// <returns>Returns true if the texture is valid</returns>
        public static bool ValidateTexture(DdsHeader header, DdsHeaderDX10? header10, out int depth, out Format format, out ResourceDimension resDim, out int arraySize, out bool isCubeMap)
        {
            if (header10.HasValue)
            {
                return ValidateTexture(header10.Value, header.Flags, out depth, out format, out resDim, out arraySize, out isCubeMap);
            }
            else
            {
                return ValidateTexture(header, out depth, out format, out resDim, out arraySize, out isCubeMap);
            }
        }
        /// <summary>
        /// Validates the texture
        /// </summary>
        /// <param name="header">DDS Header</param>
        /// <param name="depth">Returns the texture depth</param>
        /// <param name="format">Returns the texture format</param>
        /// <param name="resDim">Returns the texture dimension</param>
        /// <param name="arraySize">Returns the texture array size</param>
        /// <param name="isCubeMap">Returns true if the texture is a cube map</param>
        /// <returns>Returns true if the texture is valid</returns>
        private static bool ValidateTexture(DdsHeader header, out int depth, out Format format, out ResourceDimension resDim, out int arraySize, out bool isCubeMap)
        {
            depth = 0;
            resDim = ResourceDimension.Unknown;
            arraySize = 1;
            isCubeMap = false;

            format = header.PixelFormat.GetDXGIFormat();
            if (format == Format.Unknown)
            {
                return false;
            }

            if (header.Flags.HasFlag(DdsFlagTypes.Depth))
            {
                resDim = ResourceDimension.Texture3D;
            }
            else
            {
                if (header.Caps2.HasFlag(DdsCaps2Types.Cubemap))
                {
                    // We require all six faces to be defined
                    if ((header.Caps2 & DdsCaps2Types.AllFaces) != DdsCaps2Types.AllFaces)
                    {
                        return false;
                    }

                    arraySize = 6;
                    isCubeMap = true;
                }

                depth = 1;
                resDim = ResourceDimension.Texture2D;
            }

            return true;
        }
        /// <summary>
        /// Validates the texture
        /// </summary>
        /// <param name="header">DDS DX10 header</param>
        /// <param name="flags">Flags</param>
        /// <param name="depth">Returns the texture depth</param>
        /// <param name="format">Returns the texture format</param>
        /// <param name="resDim">Returns the texture dimension</param>
        /// <param name="arraySize">Returns the texture array size</param>
        /// <param name="isCubeMap">Returns true if the texture is a cube map</param>
        /// <returns>Returns true if the texture is valid</returns>
        private static bool ValidateTexture(DdsHeaderDX10 header, DdsFlagTypes flags, out int depth, out Format format, out ResourceDimension resDim, out int arraySize, out bool isCubeMap)
        {
            depth = 0;
            format = Format.Unknown;
            resDim = ResourceDimension.Unknown;
            isCubeMap = false;

            arraySize = header.ArraySize;
            if (arraySize == 0)
            {
                return false;
            }

            if (header.MiscFlag2 != DdsFlagsDX10.AlphaModeUnknown)
            {
                return false;
            }

            if (DdsPixelFormat.BitsPerPixel(header.DXGIFormat) == 0)
            {
                return false;
            }

            format = header.DXGIFormat;

            switch (header.Dimension)
            {
                case ResourceDimension.Texture1D:
                    depth = 1;
                    break;

                case ResourceDimension.Texture2D:
                    if (header.MiscFlag.HasFlag(ResourceOptionFlags.TextureCube))
                    {
                        arraySize *= 6;
                        isCubeMap = true;
                    }
                    depth = 1;
                    break;

                case ResourceDimension.Texture3D:
                    if (!flags.HasFlag(DdsFlagTypes.Depth))
                    {
                        return false;
                    }

                    if (arraySize > 1)
                    {
                        return false;
                    }
                    break;

                default:
                    return false;
            }

            resDim = header.Dimension;

            return true;
        }

        /// <summary>
        /// Struct size (Must be 124)
        /// </summary>
        public int Size;
        /// <summary>
        /// Flags to indicate which members contain valid data.
        /// </summary>
        public DdsFlagTypes Flags;
        /// <summary>
        /// Surface height (in pixels).
        /// </summary>
        public int Height;
        /// <summary>
        /// Surface width (in pixels).
        /// </summary>
        public int Width;
        /// <summary>
        /// The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top level texture for a compressed texture.
        /// </summary>
        public int PitchOrLinearSize;
        /// <summary>
        /// Depth of a volume texture (in pixels), otherwise unused.
        /// </summary>
        public int Depth;
        /// <summary>
        /// Number of mipmap levels, otherwise unused.
        /// </summary>
        public int MipMapCount;
        /// <summary>
        /// Unused.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public int[] Reserved1;
        /// <summary>
        /// The pixel format
        /// </summary>
        public DdsPixelFormat PixelFormat;
        /// <summary>
        /// Specifies the complexity of the surfaces stored.
        /// </summary>
        public DdsCapsTypes Caps;
        /// <summary>
        /// Additional detail about the surfaces stored.
        /// </summary>
        public DdsCaps2Types Caps2;
        /// <summary>
        /// Unused.
        /// </summary>
        public int Caps3;
        /// <summary>
        /// Unused.
        /// </summary>
        public int Caps4;
        /// <summary>
        /// Unused.
        /// </summary>
        public int Reserved2;
    }
}
