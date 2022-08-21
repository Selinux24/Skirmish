﻿using System;

namespace Engine.Common
{
    /// <summary>
    /// Engine pixel shader interface
    /// </summary>
    public interface IEnginePixelShader : IDisposable
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