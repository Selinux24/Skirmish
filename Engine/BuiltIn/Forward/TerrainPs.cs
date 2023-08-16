using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Forward
{
    using Engine.Common;

    /// <summary>
    /// Terrain pixel shader
    /// </summary>
    public class TerrainPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Per terrain constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerTerrain;
        /// <summary>
        /// Alpha map texture
        /// </summary>
        private EngineShaderResourceView alphaMap;
        /// <summary>
        /// Normal map
        /// </summary>
        private EngineShaderResourceView normalMap;
        /// <summary>
        /// Color texture
        /// </summary>
        private EngineShaderResourceView colorTexture;
        /// <summary>
        /// Low resolution texture
        /// </summary>
        private EngineShaderResourceView lowResolutionTexture;
        /// <summary>
        /// High resolution texture
        /// </summary>
        private EngineShaderResourceView highResolutionTexture;
        /// <summary>
        /// Diffuse sampler
        /// </summary>
        private EngineSamplerState samplerDiffuse;
        /// <summary>
        /// Normal sampler
        /// </summary>
        private EngineSamplerState samplerNormal;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TerrainPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<TerrainPs>("main", ForwardRenderingResources.Terrain_ps);
        }

        /// <summary>
        /// Sets per terrain constant buffer
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        public void SetPerTerrainConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerTerrain = constantBuffer;
        }

        /// <summary>
        /// Sets the alpha map texture
        /// </summary>
        /// <param name="alphaMap">Alpha map texture</param>
        public void SetAlphaMap(EngineShaderResourceView alphaMap)
        {
            this.alphaMap = alphaMap;
        }
        /// <summary>
        /// Sets the normal map texture
        /// </summary>
        /// <param name="normalMap">Normal map</param>
        public void SetNormalMap(EngineShaderResourceView normalMap)
        {
            this.normalMap = normalMap;
        }
        /// <summary>
        /// Sets the color texture
        /// </summary>
        /// <param name="colorTexture">Color texture</param>
        public void SetColorTexture(EngineShaderResourceView colorTexture)
        {
            this.colorTexture = colorTexture;
        }
        /// <summary>
        /// Sets the low resolution texture
        /// </summary>
        /// <param name="lowResolutionTexture">Low resolution texture</param>
        public void SetLowResolutionTexture(EngineShaderResourceView lowResolutionTexture)
        {
            this.lowResolutionTexture = lowResolutionTexture;
        }
        /// <summary>
        /// Sets the high resolution texture
        /// </summary>
        /// <param name="highResolutionTexture">High resolution texture</param>
        public void SetHighResolutionTexture(EngineShaderResourceView highResolutionTexture)
        {
            this.highResolutionTexture = highResolutionTexture;
        }

        /// <summary>
        /// Sets the diffuse sampler state
        /// </summary>
        /// <param name="samplerDiffuse">Diffuse sampler</param>
        public void SetDiffuseSampler(EngineSamplerState samplerDiffuse)
        {
            this.samplerDiffuse = samplerDiffuse;
        }
        /// <summary>
        /// Sets the normal sampler state
        /// </summary>
        /// <param name="samplerNormal">Normal sampler</param>
        public void SetNormalSampler(EngineSamplerState samplerNormal)
        {
            this.samplerNormal = samplerNormal;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                BuiltInShaders.GetHemisphericConstantBuffer(),
                BuiltInShaders.GetDirectionalsConstantBuffer(),
                BuiltInShaders.GetSpotsConstantBuffer(),
                BuiltInShaders.GetPointsConstantBuffer(),
                cbPerTerrain,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            var vr = new[]
            {
                BuiltInShaders.GetShadowMapDirResourceView(),
                BuiltInShaders.GetShadowMapSpotResourceView(),
                BuiltInShaders.GetShadowMapPointResourceView(),
                alphaMap,
                normalMap,
                colorTexture,
                lowResolutionTexture,
                highResolutionTexture,
            };

            dc.SetPixelShaderResourceViews(0, vr);

            var ss = new[]
            {
                samplerDiffuse,
                samplerNormal,
            };

            dc.SetPixelShaderSamplers(0, ss);
        }
    }
}
