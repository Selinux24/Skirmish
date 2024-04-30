
namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Directional light drawer
    /// </summary>
    public class BuiltInLightDirectional : BuiltInDrawer
    {
        /// <summary>
        /// Pixel shader
        /// </summary>
        private readonly DeferredLightDirectionalPs pixelShader;
        /// <summary>
        /// Directional light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<BuiltInShaders.BufferLightDirectional> cbDirectional;
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
            pixelShader = SetPixelShader<DeferredLightDirectionalPs>(false);

            cbDirectional = BuiltInShaders.GetConstantBuffer<BuiltInShaders.BufferLightDirectional>(false);

            pointSampler = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        public void UpdateGeometryMap(EngineShaderResourceView[] geometryMap)
        {
            pixelShader.SetDeferredBuffer(geometryMap);
            pixelShader.SetPointSampler(pointSampler);
        }
        /// <summary>
        /// Updates per light buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="light">Light constant buffer</param>
        public void UpdatePerLight(IEngineDeviceContext dc, ISceneLightDirectional light)
        {
            dc.UpdateConstantBuffer(cbDirectional, BuiltInShaders.BufferLightDirectional.Build(light));

            pixelShader.SetPerLightConstantBuffer(cbDirectional);
        }
    }
}
