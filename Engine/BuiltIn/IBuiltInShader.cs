using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in shader interface
    /// </summary>
    public interface IBuiltInShader<T> : IDisposable where T : class, IDisposable
    {
        /// <summary>
        /// Shader
        /// </summary>
        T Shader { get; }

        /// <summary>
        /// Sets the shader resources
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetShaderResources(IEngineDeviceContext dc);
    }
}
