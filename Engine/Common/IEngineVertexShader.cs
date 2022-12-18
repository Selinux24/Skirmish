using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine vertex shader interface
    /// </summary>
    public interface IEngineVertexShader : IDisposable
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
