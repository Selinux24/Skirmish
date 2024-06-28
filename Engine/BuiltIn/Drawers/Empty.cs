
namespace Engine.BuiltIn.Drawers
{
    using Engine.Common;

    /// <summary>
    /// Empty shader
    /// </summary>
    public class Empty<T> : IBuiltInShader<T> where T : IEngineShader
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
