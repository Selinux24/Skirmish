using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in pixel shader interface
    /// </summary>
    public interface IBuiltInPixelShader : IDisposable
    {
        /// <summary>
        /// Pixel shader
        /// </summary>
        EnginePixelShader Shader { get; }

        /// <summary>
        /// Sets the shader resources
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetShaderResources(IEngineDeviceContext dc);
    }
}
