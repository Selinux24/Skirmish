using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Clouds
{
    /// <summary>
    /// Clouds vertex shader
    /// </summary>
    public class CloudsVs : IShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CloudsVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<CloudsVs>("main", ForwardRenderingResources.Clouds_vs);
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
