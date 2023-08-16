using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

    /// <summary>
    /// Position texture instanced vertex shader
    /// </summary>
    public class PositionTextureVsI : IBuiltInShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionTextureVsI()
        {
            Shader = BuiltInShaders.CompileVertexShader<PositionTextureVsI>("main", ShadowRenderingResources.PositionTextureI_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            // No shader resources
        }
    }
}
