
namespace Engine.Common
{
    using ComputeShader = Vortice.Direct3D11.ID3D11ComputeShader;

    /// <summary>
    /// Compute shader description
    /// </summary>
    public class EngineComputeShader : EngineShader<ComputeShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="computeShader">Compute shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineComputeShader(string name, ComputeShader computeShader, byte[] byteCode) : base(name, computeShader, byteCode) { }
    }
}
