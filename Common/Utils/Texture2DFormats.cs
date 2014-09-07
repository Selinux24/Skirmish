
namespace Common.Utils
{
    /// <summary>
    /// Texture2D available formats
    /// </summary>
    public enum Texture2DFormats
    {
        /// <summary>
        /// Each element has three 32-bit floating-point components.
        /// </summary>
        R32G32B32_Float = SharpDX.DXGI.Format.R32G32B32_Float,
        /// <summary>
        /// Each element has four 16-bit components mapped to the [0, 1] range.
        /// </summary>
        R16G16B16A16_UNorm = SharpDX.DXGI.Format.R16G16B16A16_UNorm,
        /// <summary>
        /// Each element has two 32-bit unsigned integer components.
        /// </summary>
        R32G32_UInt = SharpDX.DXGI.Format.R32G32_UInt,
        /// <summary>
        /// Each element has four 8-bit unsigned components mapped to the [0, 1] range.
        /// </summary>
        R8G8B8A8_UNorm = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
        /// <summary>
        /// Each element has four 8-bit signed components mapped to the [−1, 1] range.
        /// </summary>
        R8G8B8A8_SNorm = SharpDX.DXGI.Format.R8G8B8A8_SNorm,
        /// <summary>
        /// Each element has four 8-bit signed integer components mapped to the [−128, 127] range.
        /// </summary>
        R8G8B8A8_SInt = SharpDX.DXGI.Format.R8G8B8A8_SInt,
        /// <summary>
        /// Each element has four 8-bit unsigned integer components mapped to the [0, 255] range.
        /// </summary>
        R8G8B8A8_UInt = SharpDX.DXGI.Format.R8G8B8A8_UInt,
    }
}
