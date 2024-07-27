using Engine.Common;

namespace Engine
{
    /// <summary>
    /// Shader interface
    /// </summary>
    public interface IShader<out T> where T : IEngineShader
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
