using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Fonts
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Fonts pixel shader
    /// </summary>
    public class FontsPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Per font constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerFont;
        /// <summary>
        /// Texture resource view
        /// </summary>
        private EngineShaderResourceView texture;
        /// <summary>
        /// Font point sampler
        /// </summary>
        private readonly EngineSamplerState samplerPoint;
        /// <summary>
        /// Font linear sampler
        /// </summary>
        private readonly EngineSamplerState samplerLinear;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FontsPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<FontsPs>("main", UIRenderingResources.Font_ps);

            var samplerPointDesc = EngineSamplerStateDescription.Default();
            samplerPointDesc.Filter = Filter.MinMagMipPoint;
            samplerPointDesc.AddressU = TextureAddressMode.Clamp;
            samplerPointDesc.AddressV = TextureAddressMode.Clamp;
            samplerPoint = BuiltInShaders.GetSamplerCustom($"{nameof(FontsPs)}.Point", samplerPointDesc);

            var samplerLinearDesc = EngineSamplerStateDescription.Default();
            samplerLinearDesc.Filter = Filter.MinMagMipLinear;
            samplerLinearDesc.AddressU = TextureAddressMode.Clamp;
            samplerLinearDesc.AddressV = TextureAddressMode.Clamp;
            samplerLinear = BuiltInShaders.GetSamplerCustom($"{nameof(FontsPs)}.Linear", samplerLinearDesc);
        }

        /// <summary>
        /// Sets per font constant buffer
        /// </summary>
        public void SetPerFontConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerFont = constantBuffer;
        }
        /// <summary>
        /// Sets the texture
        /// </summary>
        /// <param name="textureArray">Texture array</param>
        public void SetTextureArray(EngineShaderResourceView texture)
        {
            this.texture = texture;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerFont,
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            dc.SetPixelShaderResourceView(0, texture);

            dc.SetPixelShaderSamplers(0, new[] { samplerPoint, samplerLinear });
        }
    }
}
