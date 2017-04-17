using SharpDX.DXGI;

namespace Engine.Helpers
{
    /// <summary>
    /// Depth buffer available formats
    /// </summary>
    public static class DepthBufferFormats
    {
        /// <summary>
        /// Specifies a 32-bit floating-point depth buffer, with 8-bits (unsigned integer) reserved for the stencil buffer mapped to the [0, 255] range and 24-bits not used for padding
        /// </summary>
        public static readonly Format D32_Float_S8X24_UInt = Format.D32_Float_S8X24_UInt;
        /// <summary>
        /// Specifies a 32-bit floating-point depth buffer.
        /// </summary>
        public static readonly Format D32_Float = Format.D32_Float;
        /// <summary>
        /// Specifies an unsigned 24-bit depth buffer mapped to the [0, 1] range with 8-bits (unsigned integer) reserved for the stencil buffer mapped to the [0, 255] range.
        /// </summary>
        public static readonly Format D24_UNorm_S8_UInt = Format.D24_UNorm_S8_UInt;
        /// <summary>
        /// Specifies an unsigned 16-bit depth buffer mapped to the [0, 1] range.
        /// </summary>
        public static readonly Format D16_UNorm = Format.D16_UNorm;
    }
}
