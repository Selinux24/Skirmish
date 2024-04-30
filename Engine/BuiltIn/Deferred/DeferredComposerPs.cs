using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Deferred composer pixel shader
    /// </summary>
    public class DeferredComposerPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Deferred buffer
        /// </summary>
        private EngineShaderResourceView[] deferredBuffer;
        /// <summary>
        /// Light map buffer
        /// </summary>
        private EngineShaderResourceView lightMap;
        /// <summary>
        /// Point sampler
        /// </summary>
        private EngineSamplerState samplerPoint;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DeferredComposerPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<DeferredComposerPs>("main", DeferredRenderingResources.DeferredComposer_ps);
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
        /// Sets the light map
        /// </summary>
        /// <param name="lightMap">Light map</param>
        public void SetLightMap(EngineShaderResourceView lightMap)
        {
            this.lightMap = lightMap;
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
                BuiltInShaders.GetHemisphericConstantBuffer(),
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            dc.SetPixelShaderResourceViews(0, deferredBuffer);
            dc.SetPixelShaderResourceView(deferredBuffer.Length, lightMap);

            dc.SetPixelShaderSampler(0, samplerPoint);
        }
    }
}
