using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace Engine.Helpers.DDS
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// DDS Header
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct DDSHeader
    {
        public const int DDS_MAGIC = 0x20534444;

        const int DDS_FOURCC = 0x00000004;// DDPF_FOURCC

        const int DDS_HEADER_FLAGS_TEXTURE = 0x00001007;// DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT
        const int DDS_HEADER_FLAGS_MIPMAP = 0x00020000;// DDSD_MIPMAPCOUNT
        const int DDS_HEADER_FLAGS_VOLUME = 0x00800000;// DDSD_DEPTH
        const int DDS_HEADER_FLAGS_PITCH = 0x00000008;// DDSD_PITCH
        const int DDS_HEADER_FLAGS_LINEARSIZE = 0x00080000;// DDSD_LINEARSIZE

        const int DDS_SURFACE_FLAGS_TEXTURE = 0x00001000;// DDSCAPS_TEXTURE
        const int DDS_SURFACE_FLAGS_MIPMAP = 0x00400008;// DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
        const int DDS_SURFACE_FLAGS_CUBEMAP = 0x00000008;// DDSCAPS_COMPLEX

        const int DDS_FLAGS_VOLUME = 0x00200000;// DDSCAPS2_VOLUME

        const int DDS_CUBEMAP = 0x00000200;// DDSCAPS2_CUBEMAP

        const int DDS_CUBEMAP_POSITIVEX = 0x00000600;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
        const int DDS_CUBEMAP_NEGATIVEX = 0x00000a00;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
        const int DDS_CUBEMAP_POSITIVEY = 0x00001200;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
        const int DDS_CUBEMAP_NEGATIVEY = 0x00002200;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
        const int DDS_CUBEMAP_POSITIVEZ = 0x00004200;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
        const int DDS_CUBEMAP_NEGATIVEZ = 0x00008200;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ

        const int DDS_CUBEMAP_ALLFACES = (DDS_CUBEMAP_POSITIVEX | DDS_CUBEMAP_NEGATIVEX | DDS_CUBEMAP_POSITIVEY | DDS_CUBEMAP_NEGATIVEY | DDS_CUBEMAP_POSITIVEZ | DDS_CUBEMAP_NEGATIVEZ);

        public readonly static int StructSize = Marshal.SizeOf(new DDSHeader());

        public int Size;
        public int Flags;
        public int Height;
        public int Width;
        public int PitchOrLinearSize;
        public int Depth; // only if DDS_HEADER_FLAGS_VOLUME is set in flags
        public int MipMapCount;
        //===11
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public int[] Reserved1;

        public DDSPixelFormat PixelFormat;
        public int Caps;
        public int Caps2;
        public int Caps3;
        public int Caps4;
        public int Reserved2;

        public bool IsDX10
        {
            get
            {
                var pf = this.PixelFormat;

                return
                    ((pf.Flags & DDS_FOURCC) > 0) &&
                    (DDSPixelFormat.MakeFourCC('D', 'X', '1', '0') == pf.FourCC);
            }
        }
        public bool Validate(ref int width, ref int height, ref int depth, out Format format, out ResourceDimension resDim, out int arraySize, out bool isCubeMap)
        {
            format = Format.Unknown;
            resDim = ResourceDimension.Unknown;
            arraySize = 1;
            isCubeMap = false;

            format = this.PixelFormat.GetDXGIFormat();

            if (format == Format.Unknown)
            {
                return false;
            }

            if ((this.Flags & DDS_HEADER_FLAGS_VOLUME) > 0)
            {
                resDim = ResourceDimension.Texture3D;
            }
            else
            {
                if ((this.Caps2 & DDS_CUBEMAP) > 0)
                {
                    // We require all six faces to be defined
                    if ((this.Caps2 & DDS_CUBEMAP_ALLFACES) != DDS_CUBEMAP_ALLFACES)
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
    }
}
