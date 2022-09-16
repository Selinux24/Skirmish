using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Foliage
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Foliage pixel shader
    /// </summary>
    public class FoliageShadowsPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private IEngineConstantBuffer cbPerMaterial;
        /// <summary>
        /// Texture array resource view
        /// </summary>
        private EngineShaderResourceView textureArray;
        /// <summary>
        /// Foliage sampler
        /// </summary>
        private readonly EngineSamplerState samplerFoliage;

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
        public FoliageShadowsPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(FoliageShadowsPs), "main", ShadowRenderingResources.Foliage_ps, HelperShaders.PSProfile);

            samplerFoliage = BuiltInShaders.GetSamplerLinear();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FoliageShadowsPs()
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
            }
        }

        /// <summary>
        /// Sets per material constant buffer
        /// </summary>
        public void SetPerMaterialConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            cbPerMaterial = constantBuffer;
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
        public void SetShaderResources()
        {
            var cb = new[]
            {
                cbPerMaterial,
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                textureArray,
            };

            Graphics.SetPixelShaderResourceViews(0, rv);

            Graphics.SetPixelShaderSampler(0, samplerFoliage);
        }
    }
}
