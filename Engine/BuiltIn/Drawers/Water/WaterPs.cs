using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Water
{
    /// <summary>
    /// Water pixel shader
    /// </summary>
    public class WaterPs : IShader<EnginePixelShader>
    {
        /// <summary>
        /// Per water constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerWater;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public WaterPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<WaterPs>("main", ForwardRenderingResources.Water_ps);
        }

        /// <summary>
        /// Sets per water constant buffer
        /// </summary>
        public void SetPerWaterConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerWater = constantBuffer;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                BuiltInShaders.GetDirectionalsConstantBuffer(),
                cbPerWater,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);
        }
    }
}
