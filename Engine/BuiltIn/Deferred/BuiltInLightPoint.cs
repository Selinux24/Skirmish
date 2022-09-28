using System.Collections.Generic;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Point light drawer
    /// </summary>
    public class BuiltInLightPoint : BuiltInDrawer
    {
        /// <summary>
        /// Point sampler
        /// </summary>
        private readonly EngineSamplerState pointSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInLightPoint(Graphics graphics) : base(graphics)
        {
            SetVertexShader<DeferredLightVs>();
            SetPixelShader<DeferredLightPointPs>();

            pointSampler = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        public void UpdateGeometryMap(IEnumerable<EngineShaderResourceView> geometryMap)
        {
            var pixelShader = GetPixelShader<DeferredLightPointPs>();
            pixelShader?.SetDeferredBuffer(geometryMap);
            pixelShader?.SetPointSampler(pointSampler);
        }
        /// <summary>
        /// Updates per light buffer
        /// </summary>
        /// <param name="light">Light constant buffer</param>
        public void UpdatePerLight(ISceneLightPoint light)
        {

        }
    }
}
