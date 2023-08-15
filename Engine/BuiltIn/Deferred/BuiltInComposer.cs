using System.Collections.Generic;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Composer drawer
    /// </summary>
    public class BuiltInComposer : BuiltInDrawer
    {
        /// <summary>
        /// Point sampler
        /// </summary>
        private readonly EngineSamplerState pointSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInComposer() : base()
        {
            SetVertexShader<DeferredLightOrthoVs>(false);
            SetPixelShader<DeferredComposerPs>(false);

            pointSampler = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        public void UpdateGeometryMap(IEnumerable<EngineShaderResourceView> geometryMap, EngineShaderResourceView lightMap)
        {
            var pixelShader = GetPixelShader<DeferredComposerPs>();
            pixelShader?.SetDeferredBuffer(geometryMap);
            pixelShader?.SetLightMap(lightMap);
            pixelShader?.SetPointSampler(pointSampler);
        }
    }
}
