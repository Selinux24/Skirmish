
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Domain shader description
    /// </summary>
    public class EngineDomainShader : EngineShader<DomainShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="domainShader">Domain shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineDomainShader(string name, DomainShader domainShader, byte[] byteCode) : base(name, domainShader, byteCode) { }
    }
}
