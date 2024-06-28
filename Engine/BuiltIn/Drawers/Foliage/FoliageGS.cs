using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Foliage
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Foliage geometry shader
    /// </summary>
    public class FoliageGS : IBuiltInShader<EngineGeometryShader>
    {
        /// <summary>
        /// Per patch constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerPatch;
        /// <summary>
        /// Random texture
        /// </summary>
        private EngineShaderResourceView randomTexture;
        /// <summary>
        /// Sampler point
        /// </summary>
        private readonly EngineSamplerState samplerPoint;

        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FoliageGS()
        {
            Shader = BuiltInShaders.CompileGeometryShader<FoliageGS>("main", ForwardRenderingResources.Foliage_gs);

            samplerPoint = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Sets per patch constant buffer
        /// </summary>
        public void SetPerPatchConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerPatch = constantBuffer;
        }
        /// <summary>
        /// Sets the random texture resource view
        /// </summary>
        /// <param name="randomTexture">Random texture</param>
        public void SetRandomTexture(EngineShaderResourceView randomTexture)
        {
            this.randomTexture = randomTexture;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerPatch,
            };

            dc.SetGeometryShaderConstantBuffers(0, cb);

            dc.SetGeometryShaderResourceView(0, randomTexture);

            dc.SetGeometryShaderSampler(0, samplerPoint);
        }
    }
}
