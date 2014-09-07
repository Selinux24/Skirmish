
namespace Common.Utils
{
    /// <summary>
    /// BackBuffer available formats
    /// </summary>
    public enum BackBufferFormats
    {
        /// <summary>
        /// Specifies a 32-bit floating-point depth buffer, with 8-bits (unsigned integer) reserved for the stencil buffer mapped to the [0, 255] range and 24-bits not used for padding
        /// </summary>
        D32_Float_S8X24_UInt = SharpDX.DXGI.Format.D32_Float_S8X24_UInt,
        /// <summary>
        /// Specifies a 32-bit floating-point depth buffer.
        /// </summary>
        D32_Float = SharpDX.DXGI.Format.D32_Float,
        /// <summary>
        /// Specifies an unsigned 24-bit depth buffer mapped to the [0, 1] range with 8-bits (unsigned integer) reserved for the stencil buffer mapped to the [0, 255] range.
        /// </summary>
        D24_UNorm_S8_UInt = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
        /// <summary>
        /// Specifies an unsigned 16-bit depth buffer mapped to the [0, 1] range.
        /// </summary>
        D16_UNorm = SharpDX.DXGI.Format.D16_UNorm,
    }
}
