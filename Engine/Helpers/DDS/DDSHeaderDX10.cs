using System.Runtime.InteropServices;

namespace Engine.Helpers.DDS
{
    using SharpDX.DXGI;

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
        public TextureDimension Dimension;
        /// <summary>
        /// Identifies other, less common options for resources
        /// </summary>
        public int MiscFlag;
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
