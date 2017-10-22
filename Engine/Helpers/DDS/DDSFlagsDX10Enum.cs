
namespace Engine.Helpers.DDS
{
    /// <summary>
    /// DDS Header DX10 Misc flags 2 enumeration
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/es-es/library/windows/desktop/bb943983(v=vs.85).aspx</remarks>
    enum DDSFlagsDX10Enum : int
    {
        /// <summary>
        /// DDS_ALPHA_MODE_UNKNOWN: Alpha channel content is unknown. This is the value for legacy files, which typically is assumed to be 'straight' alpha.
        /// </summary>
        AlphaModeUnknown = 0x0,
        /// <summary>
        /// DDS_ALPHA_MODE_STRAIGHT: Any alpha channel content is presumed to use straight alpha.
        /// </summary>
        AlphaModeStraight = 0x1,
        /// <summary>
        /// DDS_ALPHA_MODE_PREMULTIPLIED: Any alpha channel content is using premultiplied alpha.The only legacy file formats that indicate this information are 'DX2' and 'DX4'.
        /// </summary>
        AlphaModePremultiplied = 0x2,
        /// <summary>
        /// DDS_ALPHA_MODE_OPAQUE: Any alpha channel content is all set to fully opaque. 
        /// </summary>
        AlphaModeOpaque = 0x3,
        /// <summary>
        /// DDS_ALPHA_MODE_CUSTOM: Any alpha channel content is being used as a 4th channel and is not intended to represent transparency (straight or premultiplied).
        /// </summary>
        AlphaModeCustom = 0x4,
    }
}
