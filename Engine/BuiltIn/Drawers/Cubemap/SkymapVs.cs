using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Cubemap
{
    /// <summary>
    /// Skymap vertex shader
    /// </summary>
    public class SkymapVs : IShader<EngineVertexShader>
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
