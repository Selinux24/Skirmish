using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Foliage
{
    /// <summary>
    /// Foliage pixel shader
    /// </summary>
    public class FoliagePs : IShader<EnginePixelShader>
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMaterial;
        /// <summary>
        /// Per patch constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerPatch;
        /// <summary>
        /// Texture array resource view
        /// </summary>
        private EngineShaderResourceView textureArray;
        /// <summary>
        /// Normal map array resource view
        /// </summary>
        private EngineShaderResourceView normalMapArray;
        /// <summary>
        /// Foliage sampler
        /// </summary>
        private readonly EngineSamplerState samplerFoliage;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FoliagePs()
        {
            Shader = BuiltInShaders.CompilePixelShader<FoliagePs>("main", ForwardRenderingResources.Foliage_ps);

            samplerFoliage = BuiltInShaders.GetSamplerLinear();
        }

        /// <summary>
        /// Sets per material constant buffer
        /// </summary>
        public void SetPerMaterialConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerMaterial = constantBuffer;
        }
        /// <summary>
        /// Sets per patch constant buffer
        /// </summary>
        public void SetPerPatchConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerPatch = constantBuffer;
        }
        /// <summary>
        /// Sets the texture array
        /// </summary>
        /// <param name="textureArray">Texture array</param>
        public void SetTextureArray(EngineShaderResourceView textureArray)
        {
            this.textureArray = textureArray;
        }
        /// <summary>
        /// Sets the normal map array
        /// </summary>
        /// <param name="normalMapArray">Mormal map array</param>
        public void SetNormalMapArray(EngineShaderResourceView normalMapArray)
        {
            this.normalMapArray = normalMapArray;
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
                cbPerMaterial,
                cbPerPatch,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetShadowMapDirResourceView(),
                BuiltInShaders.GetShadowMapSpotResourceView(),
                BuiltInShaders.GetShadowMapPointResourceView(),
                textureArray,
                normalMapArray,
            };

            dc.SetPixelShaderResourceViews(0, rv);

            dc.SetPixelShaderSampler(0, samplerFoliage);
        }
    }
}
