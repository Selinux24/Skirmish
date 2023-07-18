﻿using Engine.Shaders.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Deferred spot light pixel shader
    /// </summary>
    public class DeferredLightSpotPs : IBuiltInPixelShader
    {
        /// <summary>
        /// Light constant buffer
        /// </summary>
        private IEngineConstantBuffer perLightBuffer;
        /// <summary>
        /// Deferred buffer
        /// </summary>
        private IEnumerable<EngineShaderResourceView> deferredBuffer;
        /// <summary>
        /// Point sampler
        /// </summary>
        private EngineSamplerState samplerPoint;

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
        public DeferredLightSpotPs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(DeferredLightSpotPs), "main", DeferredRenderingResources.DeferredLightSpot_ps, HelperShaders.PSProfile);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~DeferredLightSpotPs()
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
        /// Sets per-light constant buffer
        /// </summary>
        /// <param name="perLightBuffer">Constant buffer</param>
        public void SetPerLightConstantBuffer(IEngineConstantBuffer perLightBuffer)
        {
            this.perLightBuffer = perLightBuffer;
        }
        /// <summary>
        /// Sets the deferred buffer
        /// </summary>
        /// <param name="deferredBuffer">Deferred buffer</param>
        public void SetDeferredBuffer(IEnumerable<EngineShaderResourceView> deferredBuffer)
        {
            this.deferredBuffer = deferredBuffer;
        }
        /// <summary>
        /// Sets the point sampler state
        /// </summary>
        /// <param name="samplerPoint">Point sampler</param>
        public void SetPointSampler(EngineSamplerState samplerPoint)
        {
            this.samplerPoint = samplerPoint;
        }

        /// <inheritdoc/>
        public void SetShaderResources(EngineDeviceContext context)
        {
            if (deferredBuffer?.Any() != true)
            {
                return;
            }

            var cb = new[]
            {
                BuiltInShaders.GetPerFrameConstantBuffer(),
                perLightBuffer,
            };

            context.SetPixelShaderConstantBuffers(0, cb);

            context.SetPixelShaderResourceViews(0, deferredBuffer);
            context.SetPixelShaderResourceView(deferredBuffer.Count(), BuiltInShaders.GetShadowMapSpotResourceView());
                
            context.SetPixelShaderSampler(0, samplerPoint);

            var ss = new[]
            {
                BuiltInShaders.GetSamplerComparisonLessEqualBorder(),
                BuiltInShaders.GetSamplerComparisonLessEqualClamp(),
            };

            context.SetPixelShaderSamplers(10, ss);
        }
    }
}
