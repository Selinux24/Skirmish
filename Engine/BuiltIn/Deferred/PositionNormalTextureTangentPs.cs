using Engine.Shaders.Properties;
using System;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position normal texture tangent pixel shader
    /// </summary>
    public class PositionNormalTextureTangentPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;
        /// <summary>
        /// Diffuse sampler
        /// </summary>
        private EngineSamplerState samplerDiffuse;
        /// <summary>
        /// Normal map resource view
        /// </summary>
        private EngineShaderResourceView normalMapArray;
        /// <summary>
        /// Normal sampler
        /// </summary>
        private EngineSamplerState samplerNormal;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionNormalTextureTangentPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(PositionNormalTextureTangentPs), "main", DeferredRenderingResources.PositionNormalTextureTangent_ps, HelperShaders.PSProfile);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
            samplerNormal = BuiltInShaders.GetSamplerLinear();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalTextureTangentPs()
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
        /// Sets the diffuse map array
        /// </summary>
        /// <param name="diffuseMapArray">Diffuse map array</param>
        public void SetDiffuseMap(EngineShaderResourceView diffuseMapArray)
        {
            this.diffuseMapArray = diffuseMapArray;
        }
        /// <summary>
        /// Sets the diffuse sampler state
        /// </summary>
        /// <param name="samplerDiffuse">Diffuse sampler</param>
        public void SetDiffseSampler(EngineSamplerState samplerDiffuse)
        {
            this.samplerDiffuse = samplerDiffuse;
        }
        /// <summary>
        /// Sets the normal map array
        /// </summary>
        /// <param name="normalMapArray">Normal map array</param>
        public void SetNormalMap(EngineShaderResourceView normalMapArray)
        {
            this.normalMapArray = normalMapArray;
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
        public void SetShaderResources(EngineDeviceContext dc)
        {
            var rv = new[]
            {
                diffuseMapArray,
                normalMapArray,
            };

            dc.SetPixelShaderResourceViews(0, rv);

            var ss = new[]
            {
                samplerDiffuse,
                samplerNormal,
            };

            dc.SetPixelShaderSamplers(0, ss);
        }
    }
}
