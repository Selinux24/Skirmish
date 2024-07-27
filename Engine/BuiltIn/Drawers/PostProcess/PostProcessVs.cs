using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.PostProcess
{
    /// <summary>
    /// Post-process vertex shader
    /// </summary>
    public class PostProcessVs : IShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PostProcessVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<PostProcessVs>("main", PostProcessResources.PostProcess_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetVertexShaderConstantBuffer(0, BuiltInShaders.GetPerFrameConstantBuffer());
        }
    }
}
