
namespace Engine.Common
{
    using Vortice.Direct3D11;

    /// <summary>
    /// Hull shader description
    /// </summary>
    public class EngineHullShader : EngineShader<ID3D11HullShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="hullShader">Hull shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineHullShader(string name, ID3D11HullShader hullShader, byte[] byteCode) : base(name, hullShader, byteCode) { }
    }
}
