using Shaders.Properties;
using System;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position normal color pixel shader
    /// </summary>
    public class BasicPositionNormalColorPs : IBuiltInPixelShader
    {
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
        public BasicPositionNormalColorPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(BasicPositionNormalColorPs), "main", ShaderDefaultBasicResources.PositionNormalColor_ps, HelperShaders.PSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionNormalColorPs()
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
            };

            Graphics.SetPixelShaderResourceViews(0, rv);

            var ss = new[]
            {
                BuiltInShaders.GetSamplerComparisonLessEqualBorder(),
                BuiltInShaders.GetSamplerComparisonLessEqualClamp(),
            };

            Graphics.SetPixelShaderSamplers(10, ss);
        }
    }
}
