
namespace Engine.Common
{
    using Vortice.Direct3D11;

    /// <summary>
    /// Compute shader description
    /// </summary>
    public class EngineComputeShader : EngineShader<ID3D11ComputeShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="computeShader">Compute shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineComputeShader(string name, ID3D11ComputeShader computeShader, byte[] byteCode) : base(name, computeShader, byteCode) { }
    }
}
