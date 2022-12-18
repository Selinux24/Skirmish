using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine compute shader interface
    /// </summary>
    public interface IEngineComputeShader : IDisposable
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
