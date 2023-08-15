using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Decals
{
    using Engine.Common;
    using Engine.Helpers;

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
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public DecalsPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(DecalsPs), "main", ForwardRenderingResources.Decal_ps, HelperShaders.PSProfile);

            var samplerDesc = EngineSamplerStateDescription.Default();
            samplerDesc.Filter = Filter.MinMagMipPoint;
            samplerDesc.AddressU = TextureAddressMode.Clamp;
            samplerDesc.AddressV = TextureAddressMode.Clamp;

            samplerDecals = EngineSamplerState.Create(graphics, $"{nameof(DecalsPs)}.DecalsSampler", samplerDesc);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~DecalsPs()
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

                samplerDecals?.Dispose();
            }
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
