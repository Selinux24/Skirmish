using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine hull shader interface
    /// </summary>
    public interface IEngineHullShader : IDisposable
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
