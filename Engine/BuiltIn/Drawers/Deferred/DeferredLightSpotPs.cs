using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Deferred
{
    /// <summary>
    /// Deferred spot light pixel shader
    /// </summary>
    public class DeferredLightSpotPs : IShader<EnginePixelShader>
    {
        /// <summary>
        /// Light constant buffer
        /// </summary>
        private IEngineConstantBuffer perLightBuffer;
        /// <summary>
        /// Deferred buffer
        /// </summary>
        private EngineShaderResourceView[] deferredBuffer;
        /// <summary>
        /// Point sampler
        /// </summary>
        private EngineSamplerState samplerPoint;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DeferredLightSpotPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<DeferredLightSpotPs>("main", DeferredRenderingResources.DeferredLightSpot_ps);
        }

        /// <summary>
        /// Sets per-light constant buffer
        /// </summary>
        /// <param name="perLightBuffer">Constant buffer</param>
        public void SetPerLightConstantBuffer(IEngineConstantBuffer perLightBuffer)
        {
            this.perLightBuffer = perLightBuffer;
        }
        /// <summary>
        /// Sets the deferred buffer
        /// </summary>
        /// <param name="deferredBuffer">Deferred buffer</param>
        public void SetDeferredBuffer(EngineShaderResourceView[] deferredBuffer)
        {
            this.deferredBuffer = deferredBuffer;
        }
        /// <summary>
        /// Sets the point sampler state
        /// </summary>
        /// <param name="samplerPoint">Point sampler</param>
        public void SetPointSampler(EngineSamplerState samplerPoint)
        {
            this.samplerPoint = samplerPoint;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            if (deferredBuffer.Length <= 0)
            {
                return;
            }

            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                perLightBuffer,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            dc.SetPixelShaderResourceViews(0, deferredBuffer);
            dc.SetPixelShaderResourceView(deferredBuffer.Length, BuiltInShaders.GetShadowMapSpotResourceView());

            dc.SetPixelShaderSampler(0, samplerPoint);

            var ss = new[]
            {
                BuiltInShaders.GetSamplerComparisonLessEqualBorder(),
                BuiltInShaders.GetSamplerComparisonLessEqualClamp(),
            };

            dc.SetPixelShaderSamplers(10, ss);
        }
    }
}
