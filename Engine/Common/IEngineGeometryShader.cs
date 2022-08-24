using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine geometry shader interface
    /// </summary>
    public interface IEngineGeometryShader : IDisposable
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
