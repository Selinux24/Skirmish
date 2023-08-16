using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Decals
{
    using Engine.Common;

    /// <summary>
    /// Decals pixel shader
    /// </summary>
    public class DecalsPs : IBuiltInShader<EnginePixelShader>
    {
        /// <summary>
        /// Per decal constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerDecal;
        /// <summary>
        /// Texture array resource view
        /// </summary>
        private EngineShaderResourceView textureArray;
        /// <summary>
        /// Decals sampler
        /// </summary>
        private readonly EngineSamplerState samplerDecals;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DecalsPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<DecalsPs>("main", ForwardRenderingResources.Decal_ps);

            var samplerDesc = EngineSamplerStateDescription.Default();
            samplerDesc.Filter = Filter.MinMagMipPoint;
            samplerDesc.AddressU = TextureAddressMode.Clamp;
            samplerDesc.AddressV = TextureAddressMode.Clamp;

            samplerDecals = BuiltInShaders.GetSamplerCustom($"{nameof(DecalsPs)}.DecalsSampler", samplerDesc);
        }

        /// <summary>
        /// Sets per decal constant buffer
        /// </summary>
        public void SetPerDecalConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerDecal = constantBuffer;
        }
        /// <summary>
        /// Sets the texture array
        /// </summary>
        /// <param name="textureArray">Texture array</param>
        public void SetTextureArray(EngineShaderResourceView textureArray)
        {
            this.textureArray = textureArray;
        }

        /// <inheritdoc/>
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            dc.SetPixelShaderConstantBuffer(0, cbPerDecal);

            dc.SetPixelShaderResourceView(0, textureArray);

            dc.SetPixelShaderSampler(0, samplerDecals);
        }
    }
}
