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

            return ch0 | (ch1 << 8) | (ch2 << 16) | (ch3 << 24);
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
            int numBytes;
            int rowBytes;
            int numRows;

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
            return fmt switch
            {
                Format.R32G32B32A32_Typeless or Format.R32G32B32A32_Float or Format.R32G32B32A32_UInt or Format.R32G32B32A32_SInt => 128,
                Format.R32G32B32_Typeless or Format.R32G32B32_Float or Format.R32G32B32_UInt or Format.R32G32B32_SInt => 96,
                Format.R16G16B16A16_Typeless or Format.R16G16B16A16_Float or Format.R16G16B16A16_UNorm or Format.R16G16B16A16_UInt or Format.R16G16B16A16_SNorm or Format.R16G16B16A16_SInt or Format.R32G32_Typeless or Format.R32G32_Float or Format.R32G32_UInt or Format.R32G32_SInt or Format.R32G8X24_Typeless or Format.D32_Float_S8X24_UInt or Format.R32_Float_X8X24_Typeless or Format.X32_Typeless_G8X24_UInt => 64,
                Format.R10G10B10A2_Typeless or Format.R10G10B10A2_UNorm or Format.R10G10B10A2_UInt or Format.R11G11B10_Float or Format.R8G8B8A8_Typeless or Format.R8G8B8A8_UNorm or Format.R8G8B8A8_UNorm_SRgb or Format.R8G8B8A8_UInt or Format.R8G8B8A8_SNorm or Format.R8G8B8A8_SInt or Format.R16G16_Typeless or Format.R16G16_Float or Format.R16G16_UNorm or Format.R16G16_UInt or Format.R16G16_SNorm or Format.R16G16_SInt or Format.R32_Typeless or Format.D32_Float or Format.R32_Float or Format.R32_UInt or Format.R32_SInt or Format.R24G8_Typeless or Format.D24_UNorm_S8_UInt or Format.R24_UNorm_X8_Typeless or Format.X24_Typeless_G8_UInt or Format.R9G9B9E5_Sharedexp or Format.R8G8_B8G8_UNorm or Format.G8R8_G8B8_UNorm or Format.B8G8R8A8_UNorm or Format.B8G8R8X8_UNorm or Format.R10G10B10_Xr_Bias_A2_UNorm or Format.B8G8R8A8_Typeless or Format.B8G8R8A8_UNorm_SRgb or Format.B8G8R8X8_Typeless or Format.B8G8R8X8_UNorm_SRgb => 32,
                Format.R8G8_Typeless or Format.R8G8_UNorm or Format.R8G8_UInt or Format.R8G8_SNorm or Format.R8G8_SInt or Format.R16_Typeless or Format.R16_Float or Format.D16_UNorm or Format.R16_UNorm or Format.R16_UInt or Format.R16_SNorm or Format.R16_SInt or Format.B5G6R5_UNorm or Format.B5G5R5A1_UNorm or Format.B4G4R4A4_UNorm => 16,
                Format.R8_Typeless or Format.R8_UNorm or Format.R8_UInt or Format.R8_SNorm or Format.R8_SInt or Format.A8_UNorm => 8,
                Format.R1_UNorm => 1,
                Format.BC1_Typeless or Format.BC1_UNorm or Format.BC1_UNorm_SRgb or Format.BC4_Typeless or Format.BC4_UNorm or Format.BC4_SNorm => 4,
                Format.BC2_Typeless or Format.BC2_UNorm or Format.BC2_UNorm_SRgb or Format.BC3_Typeless or Format.BC3_UNorm or Format.BC3_UNorm_SRgb or Format.BC5_Typeless or Format.BC5_UNorm or Format.BC5_SNorm or Format.BC6H_Typeless or Format.BC6H_Uf16 or Format.BC6H_Sf16 or Format.BC7_Typeless or Format.BC7_UNorm or Format.BC7_UNorm_SRgb => 8,
                _ => 0,
            };
        }

        /// <summary>
        /// Structure size; set to 32 (bytes)
        /// </summary>
        public int Size;
        /// <summary>
        /// Values which indicate what type of data is in the surface.
        /// </summary>
        public DdsPixelFormats Flags;
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
        public readonly bool IsDX10()
        {
            return
                Flags.HasFlag(DdsPixelFormats.DDPF_FOURCC) &&
                (MakeFourCC("DX10") == FourCC);
        }
        /// <summary>
        /// Gets the equivalent DXGI format
        /// </summary>
        /// <returns>Returns the equivalent DXGI format</returns>
        public readonly Format GetDXGIFormat()
        {
            if (Flags.HasFlag(DdsPixelFormats.DDPF_RGB))
            {
                return GetFormatDDPFRGB();
            }
            else if (Flags.HasFlag(DdsPixelFormats.DDPF_LUMINANCE))
            {
                return GetFormatDDPFLuminance();
            }
            else if (Flags.HasFlag(DdsPixelFormats.DDPF_ALPHA))
            {
                return GetFormatDDPFAlpha();
            }
            else if (Flags.HasFlag(DdsPixelFormats.DDPF_FOURCC))
            {
                return GetFormatDDPFFOURCC();
            }

            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF RGB Format (32, 24, 16 bits)
        /// </summary>
        /// <returns>Returns the DDPF RGB Format</returns>
        private readonly Format GetFormatDDPFRGB()
        {
            // Note that sRGB formats are written using the "DX10" extended header
            return RGBBitCount switch
            {
                32 => GetFormatDDPFRGB32(),
                24 => GetFormatDDPFRGB24(),
                16 => GetFormatDDPFRGB16(),
                _ => Format.Unknown,
            };
        }
        /// <summary>
        /// Gets the DDPF RGB Format (32 bits)
        /// </summary>
        /// <returns>Returns the DDPF RGB Format</returns>
        private readonly Format GetFormatDDPFRGB32()
        {
            if (IsBitMask(0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000))
            {
                return Format.R8G8B8A8_UNorm;
            }

            if (IsBitMask(0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000))
            {
                return Format.B8G8R8A8_UNorm;
            }

            if (IsBitMask(0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000))
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
            if (IsBitMask(0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000))
            {
                return Format.R10G10B10A2_UNorm;
            }

            // No DXGI format maps to ISBITMASK(0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000) aka D3DFMT_A2R10G10B10

            if (IsBitMask(0x0000ffff, 0xffff0000, 0x00000000, 0x00000000))
            {
                return Format.R16G16_UNorm;
            }

            if (IsBitMask(0xffffffff, 0x00000000, 0x00000000, 0x00000000))
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
        private static Format GetFormatDDPFRGB24()
        {
            // No 24bpp DXGI formats aka D3DFMT_R8G8B8
            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF RGB Format (16 bits)
        /// </summary>
        /// <returns>Returns the DDPF RGB Format</returns>
        private readonly Format GetFormatDDPFRGB16()
        {
            if (IsBitMask(0x7c00, 0x03e0, 0x001f, 0x8000))
            {
                return Format.B5G5R5A1_UNorm;
            }
            if (IsBitMask(0xf800, 0x07e0, 0x001f, 0x0000))
            {
                return Format.B5G6R5_UNorm;
            }

            // No DXGI format maps to ISBITMASK(0x7c00, 0x03e0, 0x001f, 0x0000) aka D3DFMT_X1R5G5B5
            if (IsBitMask(0x0f00, 0x00f0, 0x000f, 0xf000))
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
        private readonly Format GetFormatDDPFLuminance()
        {
            if (8 == RGBBitCount && IsBitMask(0x000000ff, 0x00000000, 0x00000000, 0x00000000))
            {
                return Format.R8_UNorm; // D3DX10/11 writes this out as DX10 extension
            }
            // No DXGI format maps to ISBITMASK(0x0f, 0x00, 0x00, 0xf0) aka D3DFMT_A4L4

            if (16 == RGBBitCount)
            {
                if (IsBitMask(0x0000ffff, 0x00000000, 0x00000000, 0x00000000))
                {
                    return Format.R16_UNorm; // D3DX10/11 writes this out as DX10 extension
                }
                if (IsBitMask(0x000000ff, 0x00000000, 0x00000000, 0x0000ff00))
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
        private readonly Format GetFormatDDPFAlpha()
        {
            if (8 == RGBBitCount)
            {
                return Format.A8_UNorm;
            }

            return Format.Unknown;
        }
        /// <summary>
        /// Gets the DDPF Four CC Format
        /// </summary>
        /// <returns>Returns the DDPF Four CC Format</returns>
        private readonly Format GetFormatDDPFFOURCC()
        {
            if (MakeFourCC("DXT1") == FourCC)
            {
                return Format.BC1_UNorm;
            }
            if (MakeFourCC("DXT3") == FourCC)
            {
                return Format.BC2_UNorm;
            }
            if (MakeFourCC("DXT5") == FourCC)
            {
                return Format.BC3_UNorm;
            }

            // While pre-mulitplied alpha isn't directly supported by the DXGI formats,
            // they are basically the same as these BC formats so they can be mapped
            if (MakeFourCC("DXT2") == FourCC)
            {
                return Format.BC2_UNorm;
            }
            if (MakeFourCC("DXT4") == FourCC)
            {
                return Format.BC3_UNorm;
            }

            if (MakeFourCC("ATI1") == FourCC)
            {
                return Format.BC4_UNorm;
            }
            if (MakeFourCC("BC4U") == FourCC)
            {
                return Format.BC4_UNorm;
            }
            if (MakeFourCC("BC4S") == FourCC)
            {
                return Format.BC4_SNorm;
            }

            if (MakeFourCC("ATI2") == FourCC)
            {
                return Format.BC5_UNorm;
            }
            if (MakeFourCC("BC5U") == FourCC)
            {
                return Format.BC5_UNorm;
            }
            if (MakeFourCC("BC5S") == FourCC)
            {
                return Format.BC5_SNorm;
            }

            // BC6H and BC7 are written using the "DX10" extended header

            if (MakeFourCC("RGBG") == FourCC)
            {
                return Format.R8G8_B8G8_UNorm;
            }
            if (MakeFourCC("GRGB") == FourCC)
            {
                return Format.G8R8_G8B8_UNorm;
            }

            // Check for D3DFORMAT enums being set here
            return FourCC switch
            {
                // D3DFMT_A16B16G16R16
                36 => Format.R16G16B16A16_UNorm,
                // D3DFMT_Q16W16V16U16
                110 => Format.R16G16B16A16_SNorm,
                // D3DFMT_R16F
                111 => Format.R16_Float,
                // D3DFMT_G16R16F
                112 => Format.R16G16_Float,
                // D3DFMT_A16B16G16R16F
                113 => Format.R16G16B16A16_Float,
                // D3DFMT_R32F
                114 => Format.R32_Float,
                // D3DFMT_G32R32F
                115 => Format.R32G32_Float,
                // D3DFMT_A32B32G32R32F
                116 => Format.R32G32B32A32_Float,
                _ => Format.Unknown,
            };
        }
        /// <summary>
        /// Gets if the specified color components were the bit mask
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        /// <returns>Returns true if the specified color components were the bit mask</returns>
        private readonly bool IsBitMask(uint r, uint g, uint b, uint a)
        {
            return
                RBitMask == r &&
                GBitMask == g &&
                BBitMask == b &&
                ABitMask == a;
        }
    };
}
