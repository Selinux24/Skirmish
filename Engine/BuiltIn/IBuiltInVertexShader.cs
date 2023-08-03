﻿using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in vertex shader interface
    /// </summary>
    public interface IBuiltInVertexShader : IDisposable
    {
        /// <summary>
        /// Vertex shader
        /// </summary>
        EngineVertexShader Shader { get; }

        /// <summary>
        /// Sets the shader resources
        /// </summary>
        /// <param name="dc">Device context</param>
        void SetShaderResources(IEngineDeviceContext dc);
    }
}
