
namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Composer drawer
    /// </summary>
    public class BuiltInComposer : BuiltInDrawer
    {
        /// <summary>
        /// Composer pixel shader
        /// </summary>
        private readonly DeferredComposerPs pixelShader;
        /// <summary>
        /// Point sampler
        /// </summary>
        private readonly EngineSamplerState pointSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInComposer(Game game) : base(game)
        {
            SetVertexShader<DeferredLightOrthoVs>(false);
            pixelShader = SetPixelShader<DeferredComposerPs>(false);

            pointSampler = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        public void UpdateGeometryMap(EngineShaderResourceView[] geometryMap, EngineShaderResourceView lightMap)
        {
            pixelShader.SetDeferredBuffer(geometryMap);
            pixelShader.SetLightMap(lightMap);
            pixelShader.SetPointSampler(pointSampler);
        }
    }
}
