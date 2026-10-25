
namespace Engine.Common
{
    using Vortice.Direct3D11;

    /// <summary>
    /// Pixel shader description
    /// </summary>
    public class EnginePixelShader : EngineShader<ID3D11PixelShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="pixelShader">Pixel shader</param>
        internal EnginePixelShader(string name, ID3D11PixelShader pixelShader, byte[] byteCode) : base(name, pixelShader, byteCode) { }
    }
}
