using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Water
{
    /// <summary>
    /// Water vertex shader
    /// </summary>
    public class WaterVs : IShader<EngineVertexShader>
    {
        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public WaterVs()
        {
            Shader = BuiltInShaders.CompileVertexShader<WaterVs>("main", ForwardRenderingResources.Water_vs);
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
