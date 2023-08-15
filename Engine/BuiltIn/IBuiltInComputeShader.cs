using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in compute shader interface
    /// </summary>
    public interface IBuiltInComputeShader : IDisposable
    {
        /// <summary>
        /// Compute shader
        /// </summary>
        EngineComputeShader Shader { get; }

        /// <summary>
        /// Sets the shader resources
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetShaderResources(IEngineDeviceContext dc);
    }
}
