
namespace Engine.Helpers.DDS
{
    /// <summary>
    /// DDS Flags
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/es-es/library/windows/desktop/bb943982(v=vs.85).aspx</remarks>
    enum DDSFlagTypes
    {
        /// <summary>
        /// DDSD_CAPS: Required in every .dds file.
        /// </summary>
        Caps = 0x1,
        /// <summary>
        /// DDSD_HEIGHT: Required in every .dds file.
        /// </summary>
        Height = 0x2,
        /// <summary>
        /// DDSD_WIDTH: Required in every .dds file.
        /// </summary>
        Width = 0x4,
        /// <summary>
        /// DDSD_PITCH: Required when pitch is provided for an uncompressed texture.
        /// </summary>
        Pitch = 0x8,
        /// <summary>
        /// DDSD_PIXELFORMAT: Required in every .dds file.
        /// </summary>
        PixelFormat = 0x1000,
        /// <summary>
        /// DDSD_MIPMAPCOUNT: Required in a mipmapped texture.
        /// </summary>
        MipmapCount = 0x20000,
        /// <summary>
        /// DDSD_LINEARSIZE: Required when pitch is provided for a compressed texture.
        /// </summary>
        LinearSize = 0x80000,
        /// <summary>
        /// DDSD_DEPTH: Required in a depth texture.
        /// </summary>
        Depth = 0x800000,
    }
}
