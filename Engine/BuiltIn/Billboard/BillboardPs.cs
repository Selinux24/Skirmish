using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Billboard
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Billboards pixel shader
    /// </summary>
    public class BillboardPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Per billboard constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerBillboard;
        /// <summary>
        /// Random texture resource view
        /// </summary>
        private EngineShaderResourceView randomTexture;
        /// <summary>
        /// Texture array resource view
        /// </summary>
        private EngineShaderResourceView textureArray;
        /// <summary>
        /// Normal map array resource view
        /// </summary>
        private EngineShaderResourceView normalMapArray;
        /// <summary>
        /// Billboard sampler
        /// </summary>
        private readonly EngineSamplerState samplerBillboards;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public BillboardPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(BillboardPs), "main", ForwardRenderingResources.Billboard_ps, HelperShaders.PSProfile);

            var samplerDesc = EngineSamplerStateDescription.Default();
            samplerDesc.Filter = Filter.MinMagMipPoint;
            samplerDesc.AddressU = TextureAddressMode.Clamp;
            samplerDesc.AddressV = TextureAddressMode.Clamp;

            samplerBillboards = EngineSamplerState.Create(graphics, $"{nameof(BillboardPs)}.Sampler", samplerDesc);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BillboardPs()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shader?.Dispose();
                Shader = null;

                samplerBillboards?.Dispose();
            }
        }

        /// <summary>
        /// Sets per billboard constant buffer
        /// </summary>
        public void SetPerBillboardConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerBillboard = constantBuffer;
        }
        /// <summary>
        /// Sets the random texture
        /// </summary>
        /// <param name="randomTexture">Random texture</param>
        public void SetRandomTexture(EngineShaderResourceView randomTexture)
        {
            this.randomTexture = randomTexture;
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
        public void SetShaderResources(EngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                BuiltInShaders.GetHemisphericConstantBuffer(),
                BuiltInShaders.GetDirectionalsConstantBuffer(),
                BuiltInShaders.GetSpotsConstantBuffer(),
                BuiltInShaders.GetPointsConstantBuffer(),
                cbPerBillboard,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                randomTexture,
                BuiltInShaders.GetShadowMapDirResourceView(),
                BuiltInShaders.GetShadowMapSpotResourceView(),
                BuiltInShaders.GetShadowMapPointResourceView(),
                textureArray,
                normalMapArray,
            };

            dc.SetPixelShaderResourceViews(0, rv);

            dc.SetPixelShaderSampler(0, samplerBillboards);
        }
    }
}
