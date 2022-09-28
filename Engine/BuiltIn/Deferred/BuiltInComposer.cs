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
        /// <param name="graphics">Graphics</param>
        public BuiltInComposer(Graphics graphics) : base(graphics)
        {
            SetVertexShader<DeferredLightVs>();
            SetPixelShader<DeferredComposerPs>();

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
