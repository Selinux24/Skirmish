using Engine.Common;

namespace Engine.BuiltIn.Drawers
{
    /// <summary>
    /// Empty shader
    /// </summary>
    public class Empty<T> : IShader<T> where T : IEngineShader
    {
        /// <inheritdoc/>
        public T Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Empty()
        {
            Shader = default;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            // Empty shader
        }
    }
}
