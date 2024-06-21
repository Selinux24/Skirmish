using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Shadows
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Position instanced vertex shader
    /// </summary>
    public class PositionVsI : IBuiltInShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionVsI()
        {
            Shader = BuiltInShaders.CompileVertexShader<PositionVsI>("main", ShadowRenderingResources.PositionI_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            // No shader resources
        }
    }
}
