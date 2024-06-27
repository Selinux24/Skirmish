using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Cubemap
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Skymap vertex shader
    /// </summary>
    public class SkymapVs : IBuiltInShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkymapVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<SkymapVs>("main", ForwardRenderingResources.Skymap_vs);
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
            };

            dc.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
