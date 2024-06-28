
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Hull shader description
    /// </summary>
    public class EngineHullShader : EngineShader<HullShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="hullShader">Hull shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineHullShader(string name, HullShader hullShader, byte[] byteCode) : base(name, hullShader, byteCode) { }
    }
}
