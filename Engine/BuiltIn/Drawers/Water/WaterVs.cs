using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Water
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Water vertex shader
    /// </summary>
    public class WaterVs : IBuiltInShader<EngineVertexShader>
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
