using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace Engine.Helpers.DDS
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// DDS Header DXT10
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct DDSHeaderDX10
    {
        const int DDS_HEADER_FLAGS_TEXTURE = 0x00001007;// DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT
        const int DDS_HEADER_FLAGS_MIPMAP = 0x00020000;// DDSD_MIPMAPCOUNT
        const int DDS_HEADER_FLAGS_VOLUME = 0x00800000;// DDSD_DEPTH
        const int DDS_HEADER_FLAGS_PITCH = 0x00000008;// DDSD_PITCH
        const int DDS_HEADER_FLAGS_LINEARSIZE = 0x00080000;// DDSD_LINEARSIZE

        const int DDS_HEIGHT = 0x00000002;// DDSD_HEIGHT
        const int DDS_WIDTH = 0x00000004;// DDSD_WIDTH

        public readonly static int StructSize = Marshal.SizeOf(new DDSHeaderDX10());

        public Format DXGIFormat;
        public ResourceDimension Dimension;
        public ResourceOptionFlags MiscFlag;
        public int ArraySize;
        public int Reserved;

        public bool Validate(int flags, ref int width, ref int height, ref int depth, out Format format, out ResourceDimension resDim, out int arraySize, out bool isCubeMap)
        {
            format = Format.Unknown;
            resDim = ResourceDimension.Unknown;
            arraySize = 1;
            isCubeMap = false;

            arraySize = this.ArraySize;
            if (arraySize == 0)
            {
                return false;
            }

            if (DDSPixelFormat.BitsPerPixel(this.DXGIFormat) == 0)
            {
                return false;
            }

            format = this.DXGIFormat;

            switch (this.Dimension)
            {
                case ResourceDimension.Texture1D:
                    // D3DX writes 1D textures with a fixed Height of 1
                    if ((flags & DDS_HEIGHT) > 0 && height != 1)
                    {
                        return false;
                    }
                    height = depth = 1;
                    break;

                case ResourceDimension.Texture2D:
                    if (this.MiscFlag.HasFlag(ResourceOptionFlags.TextureCube))
                    {
                        arraySize *= 6;
                        isCubeMap = true;
                    }
                    depth = 1;
                    break;

                case ResourceDimension.Texture3D:
                    if (!((flags & DDS_HEADER_FLAGS_VOLUME) > 0))
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

            resDim = this.Dimension;

            return true;
        }
    }
}
