using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Fonts
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Fonts pixel shader
    /// </summary>
    public class FontsPs : IBuiltInPixelShader
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
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public FontsPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(FontsPs), "main", UIRenderingResources.Font_ps, HelperShaders.PSProfile);

            var samplerPointDesc = EngineSamplerStateDescription.Default();
            samplerPointDesc.Filter = Filter.MinMagMipPoint;
            samplerPointDesc.AddressU = TextureAddressMode.Clamp;
            samplerPointDesc.AddressV = TextureAddressMode.Clamp;
            samplerPoint = EngineSamplerState.Create(graphics, $"{nameof(FontsPs)}.Point", samplerPointDesc);

            var samplerLinearDesc = EngineSamplerStateDescription.Default();
            samplerLinearDesc.Filter = Filter.MinMagMipLinear;
            samplerLinearDesc.AddressU = TextureAddressMode.Clamp;
            samplerLinearDesc.AddressV = TextureAddressMode.Clamp;
            samplerLinear = EngineSamplerState.Create(graphics, $"{nameof(FontsPs)}.Linear", samplerLinearDesc);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FontsPs()
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

                samplerPoint?.Dispose();
                samplerLinear?.Dispose();
            }
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
        public void SetShaderResources(EngineDeviceContext context)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerFont,
            };

            context.SetPixelShaderConstantBuffers(0, cb);

            context.SetPixelShaderResourceView(0, texture);

            context.SetPixelShaderSamplers(0, new[] { samplerPoint, samplerLinear });
        }
    }
}
