﻿using Engine.Common;
using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Forward
{
    /// <summary>
    /// Position normal texture tangent pixel shader
    /// </summary>
    public class PositionNormalTextureTangentPs : IShader<EnginePixelShader>
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

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionNormalTextureTangentPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<PositionNormalTextureTangentPs>("main", ForwardRenderingResources.PositionNormalTextureTangent_ps);

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
            samplerNormal = BuiltInShaders.GetSamplerLinear();
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
        public void SetShaderResources(IEngineDeviceContext dc)
        {
            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                BuiltInShaders.GetHemisphericConstantBuffer(),
                BuiltInShaders.GetDirectionalsConstantBuffer(),
                BuiltInShaders.GetSpotsConstantBuffer(),
                BuiltInShaders.GetPointsConstantBuffer(),
            };

            dc.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetShadowMapDirResourceView(),
                BuiltInShaders.GetShadowMapSpotResourceView(),
                BuiltInShaders.GetShadowMapPointResourceView(),
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

            ss =
            [
                BuiltInShaders.GetSamplerComparisonLessEqualBorder(),
                BuiltInShaders.GetSamplerComparisonLessEqualClamp(),
            ];

            dc.SetPixelShaderSamplers(10, ss);
        }
    }
}
