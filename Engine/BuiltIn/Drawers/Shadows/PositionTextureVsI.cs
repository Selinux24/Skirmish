using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Shadows
{
    /// <summary>
    /// Position texture instanced vertex shader
    /// </summary>
    public class PositionTextureVsI : IShader<EngineVertexShader>
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
