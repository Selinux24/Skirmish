
namespace Engine.Common
{
    using SharpDX.Direct3D11;

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
