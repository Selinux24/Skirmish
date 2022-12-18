using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine domain shader interface
    /// </summary>
    public interface IEngineDomainShader : IDisposable
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the shader byte code
        /// </summary>
        byte[] GetShaderBytecode();
    }
}
