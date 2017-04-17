using SharpDX.DXGI;

namespace Engine.Helpers
{
    /// <summary>
    /// Back buffer available formats
    /// </summary>
    public static class BackBufferFormats
    {
        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits per channel including alpha.
        /// </summary>
        public static readonly Format R8G8B8A8_UNorm = Format.R8G8B8A8_UNorm;
        /// <summary>
        /// A four-component, 32-bit unsigned-normalized integer sRGB format that supports 8 bits per channel including alpha.
        /// </summary>
        public static readonly Format R8G8B8A8_UNorm_SRgb = Format.R8G8B8A8_UNorm_SRgb;
        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8-bit alpha.
        /// </summary>
        public static readonly Format B8G8R8A8_UNorm = Format.B8G8R8A8_UNorm;
        /// <summary>
        /// A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each channel including alpha.
        /// </summary>
        public static readonly Format B8G8R8A8_UNorm_SRgb = Format.B8G8R8A8_UNorm_SRgb;
        /// <summary>
        /// A four-component, 32-bit unsigned-normalized-integer format that supports 10 bits for each color and 2 bits for alpha.
        /// </summary>
        public static readonly Format R10G10B10A2_UNorm = Format.R10G10B10A2_UNorm;
        /// <summary>
        /// A four-component, 64-bit floating-point format that supports 16 bits per channel including alpha.
        /// </summary>
        public static readonly Format R16G16B16A16_Float = Format.R16G16B16A16_Float;
    }
}
