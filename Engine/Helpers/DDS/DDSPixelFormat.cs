using SharpDX.DXGI;
using System;
using System.Runtime.InteropServices;

namespace Engine.Helpers.DDS
{
    /// <summary>
    /// DDS Pixel Format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct DdsPixelFormat
    {
        /// <summary>
        /// Size of structure
        /// </summary>
        public readonly static int StructSize = Marshal.SizeOf(new DdsPixelFormat());

        /// <summary>
        /// Make four CC
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Returns the integer result</returns>
        public static int MakeFourCC(string text)
        {
            byte ch0 = (byte)text[0];
            byte ch1 = (byte)text[1];
            byte ch2 = (byte)text[2];
            byte ch3 = (byte)text[3];

            return (ch0 | (ch1 << 8) | (ch2 << 16) | (ch3 << 24));
        }
        /// <summary>
        /// Gets the surface info
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="fmt">Format</param>
        /// <param name="outNumBytes">Resulting number of bytes</param>
        /// <param name="outRowBytes">Resulting number of bytes in a row</param>
        /// <param name="outNumRows">Resulting number of rows</param>
        public static void GetSurfaceInfo(int width, int height, Format fmt, out int outNumBytes, out int outRowBytes, out int outNumRows)
        {
            int numBytes = 0;
            int rowBytes = 0;
            int numRows = 0;

            bool bc = false;
            bool packed = false;
            int bcnumBytesPerBlock = 0;
            switch (fmt)
            {
                case Format.BC1_Typeless:
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                case Format.BC4_Typeless:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                    bc = true;
                    bcnumBytesPerBlock = 8;
                    break;

                case Format.BC2_Typeless:
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_Typeless:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                case Format.BC5_Typeless:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                case Format.BC6H_Typeless:
                case Format.BC6H_Uf16:
                case Format.BC6H_Sf16:
                case Format.BC7_Typeless:
                case Format.BC7_UNorm:
                case Format.BC7_UNorm_SRgb:
                    bc = true;
                    bcnumBytesPerBlock = 16;
                    break;

                case Format.R8G8_B8G8_UNorm:
                case Format.G8R8_G8B8_UNorm:
                    packed = true;
                    break;
            }

            if (bc)
            {
                int numBlocksWide = 0;
                if (width > 0)
                {
                    numBlocksWide = Math.Max(1, (width + 3) / 4);
                }
                int numBlocksHigh = 0;
                if (height > 0)
                {
                    numBlocksHigh = Math.Max(1, (height + 3) / 4);
                }
                rowBytes = numBlocksWide * bcnumBytesPerBlock;
                numRows = numBlocksHigh;
            }
            else if (packed)
            {
                rowBytes = ((width + 1) >> 1) * 4;
                numRows = height;
            }
            else
            {
                int bpp = BitsPerPixel(fmt);
                rowBytes = (width * bpp + 7) / 8; // round up to nearest byte
                numRows = height;
            }

            numBytes = rowBytes * numRows;

            outNumBytes = numBytes;
            outRowBytes = rowBytes;
            outNumRows = numRows;
        }
        /// <summary>
        /// Gets the number of bits per pixel for the specified format
        /// </summary>
        /// <param name="fmt">Format</param>
        /// <returns>Returns the bits per pixel</returns>
        public static int BitsPerPixel(Format fmt)
        {
            switch (fmt)
            {
                case Format.R32G32B32A32_Typeless:
                case Format.R32G32B32A32_Float:
                case Format.R32G32B32A32_UInt:
                case Format.R32G32B32A32_SInt:
                    return 128;

                case Format.R32G32B32_Typeless:
                case Format.R32G32B32_Float:
                case Format.R32G32B32_UInt:
                case Format.R32G32B32_SInt:
                    return 96;

                case Format.R16G16B16A16_Typeless:
                case Format.R16G16B16A16_Float:
                case Format.R16G16B16A16_UNorm:
                case Format.R16G16B16A16_UInt:
                case Format.R16G16B16A16_SNorm:
                case Format.R16G16B16A16_SInt:
                case Format.R32G32_Typeless:
                case Format.R32G32_Float:
                case Format.R32G32_UInt:
                case Format.R32G32_SInt:
                case Format.R32G8X24_Typeless:
                case Format.D32_Float_S8X24_UInt:
                case Format.R32_Float_X8X24_Typeless:
                case Format.X32_Typeless_G8X24_UInt:
                    return 64;

                case Format.R10G10B10A2_Typeless:
                case Format.R10G10B10A2_UNorm:
                case Format.R10G10B10A2_UInt:
                case Format.R11G11B10_Float:
                case Format.R8G8B8A8_Typeless:
                case Format.R8G8B8A8_UNorm:
                case Format.R8G8B8A8_UNorm_SRgb:
                case Format.R8G8B8A8_UInt:
                case Format.R8G8B8A8_SNorm:
                case Format.R8G8B8A8_SInt:
                case Format.R16G16_Typeless:
                case Format.R16G16_Float:
                case Format.R16G16_UNorm:
                case Format.R16G16_UInt:
                case Format.R16G16_SNorm:
                case Format.R16G16_SInt:
                case Format.R32_Typeless:
                case Format.D32_Float:
                case Format.R32_Float:
                case Format.R32_UInt:
                case Format.R32_SInt:
                case Format.R24G8_Typeless:
                case Format.D24_UNorm_S8_UInt:
                case Format.R24_UNorm_X8_Typeless:
                case Format.X24_Typeless_G8_UInt:
                case Format.R9G9B9E5_Sharedexp:
                case Format.R8G8_B8G8_UNorm:
                case Format.G8R8_G8B8_UNorm:
                case Format.B8G8R8A8_UNorm:
                case Format.B8G8R8X8_UNorm:
                case Format.R10G10B10_Xr_Bias_A2_UNorm:
                case Format.B8G8R8A8_Typeless:
                case Format.B8G8R8A8_UNorm_SRgb:
                case Format.B8G8R8X8_Typeless:
                case Format.B8G8R8X8_UNorm_SRgb:
                    return 32;

                case Format.R8G8_Typeless:
                case Format.R8G8_UNorm:
                case Format.R8G8_UInt:
                case Format.R8G8_SNorm:
                case Format.R8G8_SInt:
                case Format.R16_Typeless:
                case Format.R16_Float:
                case Format.D16_UNorm:
                case Format.R16_UNorm:
                case Format.R16_UInt:
                case Format.R16_SNorm:
                case Format.R16_SInt:
                case Format.B5G6R5_UNorm:
                case Format.B5G5R5A1_UNorm:
                case Format.B4G4R4A4_UNorm:
                    return 16;

                case Format.R8_Typeless:
                case Format.R8_UNorm:
                case Format.R8_UInt:
                case Format.R8_SNorm:
                case Format.R8_SInt:
                case Format.A8_UNorm:
                    return 8;

                case Format.R1_UNorm:
                    return 1;

                case Format.BC1_Typeless:
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                case Format.BC4_Typeless:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                    return 4;

                case Format.BC2_Typeless:
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_Typeless:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                case Format.BC5_Typeless:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                case Format.BC6H_Typeless:
                case Format.BC6H_Uf16:
                case Format.BC6H_Sf16:
                case Format.BC7_Typeless:
                case Format.BC7_UNorm:
                case Format.BC7_UNorm_SRgb:
                    return 8;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Structure size; set to 32 (bytes)
        /// </summary>
        public int Size;
        /// <summary>
        /// Values which indicate what type of data is in the surface.
        /// </summary>
        public DDSPixelFormats Flags;
        /// <summary>
        /// Four-character codes for specifying compressed or custom formats. Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5. A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, and the dxgiFormat member of that structure indicates the true format. When using a four-character code, dwFlags must include DDPF_FOURCC.
        /// </summary>
        public int FourCC;
        /// <summary>
        /// Number of bits in an RGB (possibly including alpha) format. Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.
        /// </summary>
        public int RGBBitCount;
        /// <summary>
        /// Red (or lumiannce or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
        /// </summary>
        public uint RBitMask;
        /// <summary>
        /// Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
        /// </summary>
        public uint GBitMask;
        /// <summary>
        /// Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
        /// </summary>
        public uint BBitMask;
        /// <summary>
        /// Alpha mask for reading alpha data. dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
        /// </summary>
        public uint ABitMask;

        /// <summary>
        /// Gets wether this pixel format has a DDS_HEADER_DXT10 structure
        /// </summary>
        public bool IsDX10()
        {
            return
                (this.Flags.HasFlag(DDSPixelFormats.DDPF_FOURCC)) &&
                (MakeFourCC("DX10") == this.FourCC);
        }
        /// <summary>
        /// Gets the equivalent DXGI format
        /// </summary>
        /// <returns>Returns the equivalent DXGI format</returns>
        public Format GetDXGIFormat()
        {
            if (this.Flags.HasFlag(DDSPixelFormats.DDPF_RGB))
            {
                return GetFormatDDPFRGB();
            }
            else if (this.Flags.HasFlag(DDSPixelFormats.DDPF_LUMINANCE))
            {
                return GetFormatDDPFLuminance();
            }
            else if (this.Flags.HasFlag(DDSPixelFormats.DDPF_ALPHA))
            {
                return GetFormatDDPFAlpha();
            }
            else if (this.Flags.HasFlag(DDSPixelFormats.DDPF_FOURCC))
            {
                return GetFormatDDPFFOURCC();
            }

            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF RGB Format (32, 24, 16 bits)
        /// </summary>
        /// <returns>Returns the DDPF RGB Format</returns>
        private Format GetFormatDDPFRGB()
        {
            // Note that sRGB formats are written using the "DX10" extended header
            switch (this.RGBBitCount)
            {
                case 32:
                    return GetFormatDDPFRGB32();
                case 24:
                    return GetFormatDDPFRGB24();
                case 16:
                    return GetFormatDDPFRGB16();
            }

            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF RGB Format (32 bits)
        /// </summary>
        /// <returns>Returns the DDPF RGB Format</returns>
        private Format GetFormatDDPFRGB32()
        {
            if (this.IsBitMask(0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000))
            {
                return Format.R8G8B8A8_UNorm;
            }

            if (this.IsBitMask(0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000))
            {
                return Format.B8G8R8A8_UNorm;
            }

            if (this.IsBitMask(0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000))
            {
                return Format.B8G8R8X8_UNorm;
            }

            // No DXGI format maps to ISBITMASK(0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000) aka D3DFMT_X8B8G8R8

            // Note that many common DDS reader/writers (including D3DX) swap the
            // the RED/BLUE masks for 10:10:10:2 formats. We assumme
            // below that the 'backwards' header mask is being used since it is most
            // likely written by D3DX. The more robust solution is to use the 'DX10'
            // header extension and specify the DXGI_FORMAT_R10G10B10A2_UNORM format directly

            // For 'correct' writers, this should be 0x000003ff, 0x000ffc00, 0x3ff00000 for RGB data
            if (this.IsBitMask(0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000))
            {
                return Format.R10G10B10A2_UNorm;
            }

            // No DXGI format maps to ISBITMASK(0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000) aka D3DFMT_A2R10G10B10

            if (this.IsBitMask(0x0000ffff, 0xffff0000, 0x00000000, 0x00000000))
            {
                return Format.R16G16_UNorm;
            }

            if (this.IsBitMask(0xffffffff, 0x00000000, 0x00000000, 0x00000000))
            {
                // Only 32-bit color channel format in D3D9 was R32F
                return Format.R32_Float; // D3DX writes this out as a FourCC of 114
            }

            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF RGB Format (24 bits)
        /// </summary>
        /// <returns>Returns the DDPF RGB Format</returns>
        private Format GetFormatDDPFRGB24()
        {
            // No 24bpp DXGI formats aka D3DFMT_R8G8B8
            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF RGB Format (16 bits)
        /// </summary>
        /// <returns>Returns the DDPF RGB Format</returns>
        private Format GetFormatDDPFRGB16()
        {
            if (this.IsBitMask(0x7c00, 0x03e0, 0x001f, 0x8000))
            {
                return Format.B5G5R5A1_UNorm;
            }
            if (this.IsBitMask(0xf800, 0x07e0, 0x001f, 0x0000))
            {
                return Format.B5G6R5_UNorm;
            }

            // No DXGI format maps to ISBITMASK(0x7c00, 0x03e0, 0x001f, 0x0000) aka D3DFMT_X1R5G5B5
            if (this.IsBitMask(0x0f00, 0x00f0, 0x000f, 0xf000))
            {
                return Format.B4G4R4A4_UNorm;
            }

            // No DXGI format maps to ISBITMASK(0x0f00, 0x00f0, 0x000f, 0x0000) aka D3DFMT_X4R4G4B4

            // No 3:3:2, 3:3:2:8, or paletted DXGI formats aka D3DFMT_A8R3G3B2, D3DFMT_R3G3B2, D3DFMT_P8, D3DFMT_A8P8, etc.
            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF Luminance Format
        /// </summary>
        /// <returns>Returns the DDPF Luminance Format</returns>
        private Format GetFormatDDPFLuminance()
        {
            if (8 == this.RGBBitCount && this.IsBitMask(0x000000ff, 0x00000000, 0x00000000, 0x00000000))
            {
                return Format.R8_UNorm; // D3DX10/11 writes this out as DX10 extension
            }
            // No DXGI format maps to ISBITMASK(0x0f, 0x00, 0x00, 0xf0) aka D3DFMT_A4L4

            if (16 == this.RGBBitCount)
            {
                if (this.IsBitMask(0x0000ffff, 0x00000000, 0x00000000, 0x00000000))
                {
                    return Format.R16_UNorm; // D3DX10/11 writes this out as DX10 extension
                }
                if (this.IsBitMask(0x000000ff, 0x00000000, 0x00000000, 0x0000ff00))
                {
                    return Format.R8G8_UNorm; // D3DX10/11 writes this out as DX10 extension
                }
            }

            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF Alpha Format
        /// </summary>
        /// <returns>Returns the DDPF Alpha Format</returns>
        private Format GetFormatDDPFAlpha()
        {
            if (8 == this.RGBBitCount)
            {
                return Format.A8_UNorm;
            }

            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF Four CC Format
        /// </summary>
        /// <returns>Returns the DDPF Four CC Format</returns>
        private Format GetFormatDDPFFOURCC()
        {
            if (MakeFourCC("DXT1") == this.FourCC)
            {
                return Format.BC1_UNorm;
            }
            if (MakeFourCC("DXT3") == this.FourCC)
            {
                return Format.BC2_UNorm;
            }
            if (MakeFourCC("DXT5") == this.FourCC)
            {
                return Format.BC3_UNorm;
            }

            // While pre-mulitplied alpha isn't directly supported by the DXGI formats,
            // they are basically the same as these BC formats so they can be mapped
            if (MakeFourCC("DXT2") == this.FourCC)
            {
                return Format.BC2_UNorm;
            }
            if (MakeFourCC("DXT4") == this.FourCC)
            {
                return Format.BC3_UNorm;
            }

            if (MakeFourCC("ATI1") == this.FourCC)
            {
                return Format.BC4_UNorm;
            }
            if (MakeFourCC("BC4U") == this.FourCC)
            {
                return Format.BC4_UNorm;
            }
            if (MakeFourCC("BC4S") == this.FourCC)
            {
                return Format.BC4_SNorm;
            }

            if (MakeFourCC("ATI2") == this.FourCC)
            {
                return Format.BC5_UNorm;
            }
            if (MakeFourCC("BC5U") == this.FourCC)
            {
                return Format.BC5_UNorm;
            }
            if (MakeFourCC("BC5S") == this.FourCC)
            {
                return Format.BC5_SNorm;
            }

            // BC6H and BC7 are written using the "DX10" extended header

            if (MakeFourCC("RGBG") == this.FourCC)
            {
                return Format.R8G8_B8G8_UNorm;
            }
            if (MakeFourCC("GRGB") == this.FourCC)
            {
                return Format.G8R8_G8B8_UNorm;
            }

            // Check for D3DFORMAT enums being set here
            switch (this.FourCC)
            {
                case 36: // D3DFMT_A16B16G16R16
                    return Format.R16G16B16A16_UNorm;

                case 110: // D3DFMT_Q16W16V16U16
                    return Format.R16G16B16A16_SNorm;

                case 111: // D3DFMT_R16F
                    return Format.R16_Float;

                case 112: // D3DFMT_G16R16F
                    return Format.R16G16_Float;

                case 113: // D3DFMT_A16B16G16R16F
                    return Format.R16G16B16A16_Float;

                case 114: // D3DFMT_R32F
                    return Format.R32_Float;

                case 115: // D3DFMT_G32R32F
                    return Format.R32G32_Float;

                case 116: // D3DFMT_A32B32G32R32F
                    return Format.R32G32B32A32_Float;
            }

            return Format.Unknown;
        }
        /// <summary>
        /// Gets if the specified color components were the bit mask
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        /// <returns>Returns true if the specified color components were the bit mask</returns>
        private bool IsBitMask(uint r, uint g, uint b, uint a)
        {
            return (this.RBitMask == r && this.GBitMask == g && this.BBitMask == b && this.ABitMask == a);
        }
    };
}
