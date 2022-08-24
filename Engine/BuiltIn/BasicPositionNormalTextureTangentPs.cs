using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal texture tangent pixel shader
    /// </summary>
    public class BasicPositionNormalTextureTangentPs : IBuiltInPixelShader
    {
        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public BasicPositionNormalTextureTangentPs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionNormalTextureTangent_Cso == null;
            var bytes = Resources.Ps_PositionNormalTextureTangent_Cso ?? Resources.Ps_PositionNormalTextureTangent;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(BasicPositionNormalTextureTangentPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(BasicPositionNormalTextureTangentPs), bytes);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionNormalTextureTangentPs()
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
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPSPerFrame(),
                BuiltInShaders.GetPSHemispheric(),
                BuiltInShaders.GetPSDirectionals(),
                BuiltInShaders.GetPSSpots(),
                BuiltInShaders.GetPSPoints(),
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetPSPerFrameLitShadowMapDir(),
                BuiltInShaders.GetPSPerFrameLitShadowMapSpot(),
                BuiltInShaders.GetPSPerFrameLitShadowMapPoint(),
                diffuseMapArray,
                normalMapArray,
            };

            Graphics.SetPixelShaderResourceViews(0, rv);

            var ss = new[]
            {
                samplerDiffuse,
                samplerNormal,
            };

            Graphics.SetPixelShaderSamplers(0, ss);
        }
    }
}
