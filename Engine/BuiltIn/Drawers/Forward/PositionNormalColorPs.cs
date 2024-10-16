﻿using Engine.Shaders.Properties;

namespace Engine.BuiltIn.Drawers.Forward
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Position normal color pixel shader
    /// </summary>
    public class PositionNormalColorPs : IBuiltInShader<EnginePixelShader>
    {
        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PositionNormalColorPs()
        {
            Shader = BuiltInShaders.CompilePixelShader<PositionNormalColorPs>("main", ForwardRenderingResources.PositionNormalColor_ps);
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
            };

            dc.SetPixelShaderResourceViews(0, rv);

            var ss = new[]
            {
                BuiltInShaders.GetSamplerComparisonLessEqualBorder(),
                BuiltInShaders.GetSamplerComparisonLessEqualClamp(),
            };

            dc.SetPixelShaderSamplers(10, ss);
        }
    }
}
