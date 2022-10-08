using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Foliage
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Foliage geometry shader
    /// </summary>
    public class FoliageGS : IBuiltInGeometryShader
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

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EngineGeometryShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public FoliageGS(Graphics graphics)
        {
            Graphics = graphics;

            Shader = Graphics.CompileGeometryShader(nameof(FoliageGS), "main", ForwardRenderingResources.Foliage_gs, HelperShaders.GSProfile);

            samplerPoint = BuiltInShaders.GetSamplerPoint();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FoliageGS()
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
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                cbPerPatch,
            };

            Graphics.SetGeometryShaderConstantBuffers(0, cb);

            Graphics.SetGeometryShaderResourceView(0, randomTexture);

            Graphics.SetGeometryShaderSampler(0, samplerPoint);
        }
    }
}
