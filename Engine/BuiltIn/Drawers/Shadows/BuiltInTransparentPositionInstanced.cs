
namespace Engine.BuiltIn.Drawers.Shadows
{
    using Engine.BuiltIn.Drawers;
    using Engine.Common;

    /// <summary>
    /// Transparent point shadow position instanced drawer
    /// </summary>
    public class BuiltInTransparentPositionInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCastingLight> cbPerLight;
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState linear;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInTransparentPositionInstanced(Game game) : base(game)
        {
            SetVertexShader<PositionTextureVsI>();
            SetGeometryShader<ShadowsTransparentGs>();
            SetPixelShader<TransparentPs>();

            cbPerLight = BuiltInShaders.GetConstantBuffer<PerCastingLight>();

            linear = BuiltInShaders.GetSamplerLinear();
        }

        /// <inheritdoc/>
        public override void UpdateCastingLight(DrawContextShadows context)
        {
            context.DeviceContext.UpdateConstantBuffer(cbPerLight, PerCastingLight.Build(context));

            var geometryShader = GetGeometryShader<ShadowsTransparentGs>();
            geometryShader?.SetPerCastingLightConstantBuffer(cbPerLight);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(IEngineDeviceContext dc, BuiltInDrawerMaterialState state)
        {
            var pixelShader = GetPixelShader<TransparentPs>();
            pixelShader?.SetDiffuseMap(state.Material?.DiffuseTexture);
            pixelShader?.SetDiffuseSampler(linear);
        }
    }
}
