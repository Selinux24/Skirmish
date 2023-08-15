using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in hull shader interface
    /// </summary>
    public interface IBuiltInHullShader : IDisposable
    {
        /// <summary>
        /// Hull shader
        /// </summary>
        EngineHullShader Shader { get; }

        /// <summary>
        /// Sets the shader resources
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetShaderResources(IEngineDeviceContext dc);
    }
}
