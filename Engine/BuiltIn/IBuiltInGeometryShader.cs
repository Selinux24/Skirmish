using System;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in geometry shader interface
    /// </summary>
    public interface IBuiltInGeometryShader : IDisposable
    {
        /// <summary>
        /// Geometry shader
        /// </summary>
        EngineGeometryShader Shader { get; }

        /// <summary>
        /// Sets the shader resources
        /// </summary>
        void SetShaderResources();
    }
}
