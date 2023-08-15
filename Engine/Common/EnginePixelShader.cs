
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Pixel shader description
    /// </summary>
    public class EnginePixelShader : EngineShader<PixelShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="pixelShader">Pixel shader</param>
        internal EnginePixelShader(string name, PixelShader pixelShader, byte[] byteCode) : base(name, pixelShader, byteCode) { }
    }
}
