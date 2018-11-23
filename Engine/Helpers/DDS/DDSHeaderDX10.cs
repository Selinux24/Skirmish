using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace Engine.Helpers.DDS
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// DDS Header DXT10
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct DdsHeaderDX10
    {
        /// <summary>
        /// Size of structure
        /// </summary>
        public readonly static int StructSize = Marshal.SizeOf(new DdsHeaderDX10());

        /// <summary>
        /// The surface pixel format
        /// </summary>
        public Format DXGIFormat;
        /// <summary>
        /// Identifies the type of resource
        /// </summary>
        public ResourceDimension Dimension;
        /// <summary>
        /// Identifies other, less common options for resources
        /// </summary>
        public ResourceOptionFlags MiscFlag;
        /// <summary>
        /// The number of elements in the array.
        /// </summary>
        public int ArraySize;
        /// <summary>
        /// Contains additional metadata (formerly was reserved). The lower 3 bits indicate the alpha mode of the associated resource. The upper 29 bits are reserved and are typically 0.
        /// </summary>
        public DdsFlagsDX10 MiscFlag2;
    }
}
