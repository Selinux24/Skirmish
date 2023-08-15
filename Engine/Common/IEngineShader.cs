using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine shader interface
    /// </summary>
    public interface IEngineShader : IDisposable
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
