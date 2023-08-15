using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in domain shader interface
    /// </summary>
    public interface IBuiltInDomainShader : IDisposable
    {
        /// <summary>
        /// Domain shader
        /// </summary>
        EngineDomainShader Shader { get; }

        /// <summary>
        /// Sets the shader resources
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetShaderResources(IEngineDeviceContext dc);
    }
}
