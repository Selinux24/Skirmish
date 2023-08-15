﻿using System.Collections.Generic;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Directional light drawer
    /// </summary>
    public class BuiltInLightDirectional : BuiltInDrawer
    {
        /// <summary>
        /// Point sampler
        /// </summary>
        private readonly EngineSamplerState pointSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInLightDirectional() : base()
        {
            SetVertexShader<DeferredLightOrthoVs>(false);
            SetPixelShader<DeferredLightDirectionalPs>(false);

            pointSampler = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        public void UpdateGeometryMap(IEnumerable<EngineShaderResourceView> geometryMap)
        {
            var pixelShader = GetPixelShader<DeferredLightDirectionalPs>();
            pixelShader?.SetDeferredBuffer(geometryMap);
            pixelShader?.SetPointSampler(pointSampler);
        }
        /// <summary>
        /// Updates per light buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="light">Light constant buffer</param>
        public void UpdatePerLight(IEngineDeviceContext dc, ISceneLightDirectional light)
        {
            var cbDirectional = BuiltInShaders.GetConstantBuffer<BuiltInShaders.BufferLightDirectional>();
            dc.UpdateConstantBuffer(cbDirectional, BuiltInShaders.BufferLightDirectional.Build(light));

            var pixelShader = GetPixelShader<DeferredLightDirectionalPs>();
            pixelShader?.SetPerLightConstantBuffer(cbDirectional);
        }
    }
}
